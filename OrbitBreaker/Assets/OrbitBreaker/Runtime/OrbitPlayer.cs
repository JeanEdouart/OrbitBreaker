using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public readonly struct CaptureResult
    {
        public CaptureResult(OrbitAnchor anchor, float multiplier, int skippedAnchors, int fromSequence, SynchronizationResult synchronization, int nearMisses)
        {
            Anchor = anchor;
            Multiplier = multiplier;
            SkippedAnchors = skippedAnchors;
            FromSequence = fromSequence;
            Synchronization = synchronization;
            NearMisses = nearMisses;
        }

        public OrbitAnchor Anchor { get; }
        public float Multiplier { get; }
        public int SkippedAnchors { get; }
        public int FromSequence { get; }
        public bool IsBacktrack => Anchor.Sequence < FromSequence;
        public SynchronizationResult Synchronization { get; }
        public bool Synchronized => Synchronization == SynchronizationResult.Success;
        public int NearMisses { get; }
    }

    public enum SynchronizationResult { None, WrongDirection, Success }

    public readonly struct NearMissResult
    {
        public NearMissResult(Vector2 position, int chain) { Position = position; Chain = chain; }
        public Vector2 Position { get; }
        public int Chain { get; }
    }

    public enum PlayerOrbitState
    {
        Orbiting,
        Flying,
        Dead
    }

    public enum DeathReason
    {
        LostInSpace,
        Breaker
    }

    public sealed class OrbitPlayer : MonoBehaviour
    {
        private SpriteRenderer body;
        private LineRenderer shield;
        private TrailRenderer trail;
        private SpriteRenderer outerFlame;
        private SpriteRenderer innerFlame;
        private SpriteRenderer fuelTrack;
        private SpriteRenderer fuelFill;
        private float angleRadians;
        private Vector2 velocity;
        private float flightTime;
        private int score;
        private float capturedAt;
        private float shieldEndsAt;
        private float nearMissBoost;
        private float launchJuice;
        private float captureJuice;
        private int nearMissCount;
        private readonly HashSet<int> visitedSequences = new HashSet<int>();
        private readonly HashSet<int> nearbyFreeDebris = new HashSet<int>();
        private readonly HashSet<int> rewardedFreeDebris = new HashSet<int>();
        private float magnetEndsAt;
        private float shieldPowerEndsAt;
        private float overdriveEndsAt;
        private float extractorEndsAt;
        private float magnetStrength;
        private float extraFlightTime;
        private float extractorRadius;
        private int extractorMultiplier = 1;

        public PlayerOrbitState State { get; private set; }
        public OrbitAnchor CurrentAnchor { get; private set; }
        public int LastSequence { get; private set; }
        public Vector2 Velocity => velocity;
        public float FlightMultiplier => State == PlayerOrbitState.Flying ? Mathf.Min(GameTuning.MaxDistanceMultiplier, GameTuning.FlightMultiplier(flightTime) + nearMissBoost) : 1f;
        public float FlightDanger01 => State == PlayerOrbitState.Flying ? Mathf.Clamp01(flightTime / EffectiveMaxFlightTime) : 0f;
        public bool HasPowerShield => Time.time < shieldPowerEndsAt;
        public float PowerUpRemaining(PowerUpType type) => Mathf.Max(0f, type switch
        {
            PowerUpType.OrbitMagnet => magnetEndsAt - Time.time,
            PowerUpType.Shield => shieldPowerEndsAt - Time.time,
            PowerUpType.IonOverdrive => overdriveEndsAt - Time.time,
            PowerUpType.QuantumExtractor => extractorEndsAt - Time.time,
            _ => 0f
        });
        public float PowerUpDuration(PowerUpType type) => PowerUpProgression.Duration(type, PowerUpProgression.Level(type));
        private float EffectiveMaxFlightTime => GameTuning.MaxFlightTime + (Time.time < overdriveEndsAt ? extraFlightTime : 0f);

        public event Action<CaptureResult> Captured;
        public event Action<DeathReason> Died;
        public event Action<NearMissResult> NearMissed;
        public event Action<int, Vector2> MaterialCollected;
        public event Action<PowerUpType, Vector2> PowerUpCollected;

        public void Initialize()
        {
            gameObject.name = "Player Orb";
            EnsureVisuals();
        }

        public void ResetTo(OrbitAnchor anchor)
        {
            score = 0;
            visitedSequences.Clear();
            visitedSequences.Add(anchor.Sequence);
            LastSequence = anchor.Sequence;
            angleRadians = -Mathf.PI * 0.5f;
            flightTime = 0f;
            nearMissBoost = 0f;
            nearMissCount = 0;
            nearbyFreeDebris.Clear();
            rewardedFreeDebris.Clear();
            velocity = Vector2.zero;
            magnetEndsAt = shieldPowerEndsAt = overdriveEndsAt = extractorEndsAt = 0f;
            magnetStrength = extraFlightTime = extractorRadius = 0f;
            extractorMultiplier = 1;
            trail.Clear();
            trail.emitting = false;
            body.color = Color.white;
            body.enabled = true;
            fuelTrack.enabled = true;
            fuelFill.enabled = true;
            SetEngine(false);
            SetFuel(1f);
            ApplyCosmetics();
            Capture(anchor);
        }

        public void SetScore(int value)
        {
            score = Mathf.Max(score, Mathf.Clamp(value, 0, GameTuning.DifficultyCapDistance));
        }

        public bool Launch()
        {
            if (State != PlayerOrbitState.Orbiting || CurrentAnchor == null) return false;

            Vector2 radial = ((Vector2)transform.position - (Vector2)CurrentAnchor.transform.position).normalized;
            Vector2 tangent = CurrentAnchor.Direction > 0
                ? new Vector2(-radial.y, radial.x)
                : new Vector2(radial.y, -radial.x);

            velocity = tangent * GameTuning.LaunchSpeed(score);
            if (PowerUpRemaining(PowerUpType.IonOverdrive) > 0f)
                velocity *= PowerUpProgression.OverdriveSpeed(PowerUpProgression.Level(PowerUpType.IonOverdrive));
            CurrentAnchor.SetCurrent(false);
            CurrentAnchor = null;
            flightTime = 0f;
            nearMissBoost = 0f;
            nearMissCount = 0;
            nearbyFreeDebris.Clear();
            rewardedFreeDebris.Clear();
            State = PlayerOrbitState.Flying;
            trail.emitting = true;
            transform.up = velocity.normalized;
            SetEngine(true);
            SetShield(false);
            launchJuice = 0.16f;
            ApplyCosmetics();
            return true;
        }

        public void Tick(float deltaTime, IReadOnlyList<OrbitAnchor> anchors, IReadOnlyList<OrbitHazard> hazards, IReadOnlyList<FreeDebris> freeDebris, IReadOnlyList<MaterialPickup> materials, IReadOnlyList<PowerUpPickup> powerUps, float cameraY)
        {
            if (State == PlayerOrbitState.Dead) return;

            if (State == PlayerOrbitState.Orbiting)
            {
                TickOrbit(deltaTime);
            }
            else
            {
                TickFlight(deltaTime, anchors);
                if (State == PlayerOrbitState.Flying) CheckNearMisses(freeDebris);
            }

            UpdateShield();
            UpdateMotionJuice(deltaTime);
            CheckMaterials(materials);
            CheckPowerUps(powerUps);

            if (CheckHazards(hazards, freeDebris))
            {
                Die(DeathReason.Breaker);
            }
            else if (transform.position.y < cameraY - GameTuning.DeathDistanceBelowCamera || Mathf.Abs(transform.position.x) > GameTuning.HorizontalLimit)
            {
                Die(DeathReason.LostInSpace);
            }
        }

        private void CheckMaterials(IReadOnlyList<MaterialPickup> materials)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                MaterialPickup pickup = materials[i];
                if (!pickup.gameObject.activeInHierarchy) continue;
                float radius = pickup.Radius + GameTuning.PlayerCollisionRadius + (Time.time < extractorEndsAt ? extractorRadius : 0f);
                if (Vector2.Distance(transform.position, pickup.transform.position) > radius) continue;
                Vector2 position = pickup.transform.position;
                int value = pickup.Value * (Time.time < extractorEndsAt ? extractorMultiplier : 1);
                pickup.BeginCollection(transform, () => MaterialCollected?.Invoke(value, position));
            }
        }

        private void CheckPowerUps(IReadOnlyList<PowerUpPickup> powerUps)
        {
            for (int i = 0; i < powerUps.Count; i++)
            {
                PowerUpPickup pickup = powerUps[i];
                if (!pickup.gameObject.activeInHierarchy) continue;
                if (PowerUpProgression.StoredCount(pickup.Type) >= PowerUpProgression.MaxInventory) continue;
                if (Vector2.Distance(transform.position, pickup.transform.position) > PowerUpPickup.Radius + GameTuning.PlayerCollisionRadius) continue;
                Vector2 position = pickup.transform.position;
                PowerUpType type = pickup.Type;
                if (pickup.Collect()) PowerUpCollected?.Invoke(type, position);
            }
        }

        public void ActivateMagnet(int level)
        {
            magnetStrength = PowerUpProgression.MagnetStrength(level);
            magnetEndsAt = Time.time + PowerUpProgression.Duration(PowerUpType.OrbitMagnet, level);
        }

        public void ActivateShield(int level)
        {
            shieldPowerEndsAt = Time.time + PowerUpProgression.Duration(PowerUpType.Shield, level);
            SetShield(true);
        }

        public void ActivateOverdrive(int level)
        {
            if (State == PlayerOrbitState.Flying) velocity *= PowerUpProgression.OverdriveSpeed(level);
            extraFlightTime = PowerUpProgression.ExtraFlightTime(level);
            overdriveEndsAt = Time.time + PowerUpProgression.Duration(PowerUpType.IonOverdrive, level);
        }

        public void ActivateExtractor(int level)
        {
            extractorRadius = PowerUpProgression.ExtractorRadius(level);
            extractorMultiplier = PowerUpProgression.ExtractorMultiplier(level);
            extractorEndsAt = Time.time + PowerUpProgression.Duration(PowerUpType.QuantumExtractor, level);
        }

        public void WarpTo(OrbitAnchor anchor)
        {
            if (anchor == null || State == PlayerOrbitState.Dead) return;
            int fromSequence = LastSequence;
            transform.position = (Vector2)anchor.transform.position + Vector2.down * anchor.Radius;
            Capture(anchor);
            visitedSequences.Add(anchor.Sequence);
            Captured?.Invoke(new CaptureResult(anchor, 1f, Mathf.Max(0, anchor.Sequence - fromSequence - 1), fromSequence, SynchronizationResult.None, 0));
        }

        public void RefreshCaptureProtection()
        {
            if (CurrentAnchor == null) return;
            capturedAt = Time.time;
            shieldEndsAt = capturedAt + GameTuning.CaptureGraceDuration(score);
        }

        public void SetWarpEngine(float intensity)
        {
            SetEngine(intensity > 0f);
            trail.emitting = intensity > 0f;
            if (intensity <= 0f) { trail.Clear(); return; }
            SetShield(false);
            fuelTrack.enabled = fuelFill.enabled = false;
            outerFlame.transform.localScale = new Vector3(0.32f, 0.6f + intensity * 1.1f, 1f);
            outerFlame.color = new Color(0.45f, 0.8f, 1f, 0.95f);
        }

        public void Kill()
        {
            Die(DeathReason.LostInSpace);
        }

        private void TickOrbit(float deltaTime)
        {
            if (CurrentAnchor == null)
            {
                Die(DeathReason.LostInSpace);
                return;
            }

            float speedRadians = GameTuning.AngularSpeed(score) * Mathf.Deg2Rad;
            angleRadians += CurrentAnchor.Direction * speedRadians * deltaTime;
            Vector2 radial = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
            transform.position = (Vector2)CurrentAnchor.transform.position + radial * CurrentAnchor.Radius;
            Vector2 tangent = CurrentAnchor.Direction > 0 ? new Vector2(-radial.y, radial.x) : new Vector2(radial.y, -radial.x);
            transform.up = tangent;
            SetEngine(false);
            SetFuel(1f);
        }

        private void TickFlight(float deltaTime, IReadOnlyList<OrbitAnchor> anchors)
        {
            flightTime += deltaTime;
            if (Time.time < magnetEndsAt) ApplyOrbitMagnet(deltaTime, anchors);
            transform.position += (Vector3)(velocity * deltaTime);
            if (velocity.sqrMagnitude > 0.01f) transform.up = velocity.normalized;
            UpdateFlightVisuals();

            OrbitAnchor best = null;
            float bestError = float.MaxValue;
            for (int i = 0; i < anchors.Count; i++)
            {
                OrbitAnchor candidate = anchors[i];
                if (!candidate.gameObject.activeInHierarchy || candidate.Sequence == LastSequence) continue;
                if (candidate.Sequence < LastSequence && !visitedSequences.Contains(candidate.Sequence)) continue;

                float distance = Vector2.Distance(transform.position, candidate.transform.position);
                float error = Mathf.Abs(distance - candidate.Radius);
                if (error <= GameTuning.CaptureBand && error < bestError)
                {
                    best = candidate;
                    bestError = error;
                }
            }

            if (best != null)
            {
                Vector2 radial = ((Vector2)transform.position - (Vector2)best.transform.position).normalized;
                bool inSynchronizationZone = GameTuning.IsWithinSynchronizationZone(radial, best.DifficultyDistance, best.SynchronizationAngle);
                bool synchronized = GameTuning.IsSynchronizedCapture(radial, velocity, best.Direction, best.DifficultyDistance, best.SynchronizationAngle);
                SynchronizationResult synchronization = synchronized ? SynchronizationResult.Success
                    : inSynchronizationZone ? SynchronizationResult.WrongDirection : SynchronizationResult.None;
                float multiplier = Mathf.Min(GameTuning.MaxDistanceMultiplier,
                    FlightMultiplier + (synchronized ? GameTuning.SynchronizationMultiplierBonus : 0f));
                int fromSequence = LastSequence;
                int skippedAnchors = Mathf.Max(0, best.Sequence - LastSequence - 1);
                int completedNearMisses = nearMissCount;
                Capture(best);
                visitedSequences.Add(best.Sequence);
                Captured?.Invoke(new CaptureResult(best, multiplier, skippedAnchors, fromSequence, synchronization, completedNearMisses));
                return;
            }

            if (flightTime >= EffectiveMaxFlightTime)
            {
                Die(DeathReason.LostInSpace);
            }
        }

        private void ApplyOrbitMagnet(float deltaTime, IReadOnlyList<OrbitAnchor> anchors)
        {
            OrbitAnchor target = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < anchors.Count; i++)
            {
                OrbitAnchor candidate = anchors[i];
                if (!candidate.gameObject.activeInHierarchy || candidate.Sequence == LastSequence) continue;
                if (candidate.Sequence < LastSequence && !visitedSequences.Contains(candidate.Sequence)) continue;
                Vector2 to = (Vector2)candidate.transform.position - (Vector2)transform.position;
                if (Vector2.Dot(velocity, to) <= 0f || to.sqrMagnitude >= bestDistance) continue;
                bestDistance = to.sqrMagnitude;
                target = candidate;
            }
            if (target == null || velocity.sqrMagnitude < 0.01f) return;
            Vector2 desired = ((Vector2)target.transform.position - (Vector2)transform.position).normalized * velocity.magnitude;
            velocity = Vector2.Lerp(velocity, desired, Mathf.Clamp01(magnetStrength * deltaTime));
        }

        private void Capture(OrbitAnchor anchor)
        {
            CurrentAnchor?.SetCurrent(false);
            CurrentAnchor = anchor;
            anchor.SetVisited(true);
            LastSequence = anchor.Sequence;
            Vector2 radial = ((Vector2)transform.position - (Vector2)anchor.transform.position).normalized;
            if (radial.sqrMagnitude < 0.1f) radial = Vector2.down;
            angleRadians = Mathf.Atan2(radial.y, radial.x);
            transform.position = (Vector2)anchor.transform.position + radial * anchor.Radius;
            anchor.SetCurrent(true);
            velocity = Vector2.zero;
            flightTime = 0f;
            State = PlayerOrbitState.Orbiting;
            captureJuice = 0.18f;
            capturedAt = Time.time;
            shieldEndsAt = capturedAt + GameTuning.CaptureGraceDuration(score);
            SetShield(true);
            trail.emitting = false;
            SetEngine(false);
            SetFuel(1f);
        }

        private void CheckNearMisses(IReadOnlyList<FreeDebris> freeDebris)
        {
            for (int i = 0; i < freeDebris.Count; i++)
            {
                FreeDebris debris = freeDebris[i];
                if (!debris.gameObject.activeInHierarchy || rewardedFreeDebris.Contains(debris.Id)) continue;
                float distance = Vector2.Distance(transform.position, debris.transform.position);
                float nearRadius = debris.CollisionRadius + GameTuning.PlayerCollisionRadius + GameTuning.NearMissExtraRadius;
                bool isNear = distance <= nearRadius;
                if (isNear)
                {
                    nearbyFreeDebris.Add(debris.Id);
                }
                else if (nearbyFreeDebris.Remove(debris.Id))
                {
                    rewardedFreeDebris.Add(debris.Id);
                    nearMissCount++;
                    nearMissBoost = Mathf.Min(1.2f, nearMissBoost + GameTuning.NearMissMultiplierBonus);
                    NearMissed?.Invoke(new NearMissResult(debris.transform.position, nearMissCount));
                }
            }
        }

        private bool CheckHazards(IReadOnlyList<OrbitHazard> hazards, IReadOnlyList<FreeDebris> freeDebris)
        {
            if (Time.time < shieldPowerEndsAt) return false;
            for (int i = 0; i < hazards.Count; i++)
            {
                OrbitHazard hazard = hazards[i];
                if (!hazard.gameObject.activeInHierarchy) continue;
                if (CurrentAnchor != null && hazard.Sequence == CurrentAnchor.Sequence && Time.time < shieldEndsAt) continue;
                if (Vector2.Distance(transform.position, hazard.transform.position) <= hazard.CollisionRadius + GameTuning.PlayerCollisionRadius) return true;
            }
            for (int i = 0; i < freeDebris.Count; i++)
            {
                FreeDebris debris = freeDebris[i];
                if (debris.gameObject.activeInHierarchy && Vector2.Distance(transform.position, debris.transform.position) <= debris.CollisionRadius + GameTuning.PlayerCollisionRadius) return true;
            }
            return false;
        }

        private void Die(DeathReason reason)
        {
            if (State == PlayerOrbitState.Dead) return;
            CurrentAnchor?.SetCurrent(false);
            CurrentAnchor = null;
            State = PlayerOrbitState.Dead;
            velocity = Vector2.zero;
            trail.emitting = false;
            SetEngine(false);
            SetShield(false);
            SetFuel(0f);
            body.color = new Color(1f, 0.22f, 0.38f, 1f);
            if (reason == DeathReason.Breaker)
            {
                body.enabled = false;
                fuelTrack.enabled = false;
                fuelFill.enabled = false;
            }
            Died?.Invoke(reason);
        }

        private void EnsureVisuals()
        {
            body = gameObject.GetComponent<SpriteRenderer>();
            if (body == null) body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = RuntimeAssets.RocketSprite;
            body.color = Color.white;
            body.sortingOrder = 12;
            transform.localScale = Vector3.one * 0.72f;

            var shieldObject = new GameObject("Capture Shield");
            shieldObject.transform.SetParent(transform, false);
            shield = shieldObject.AddComponent<LineRenderer>();
            shield.useWorldSpace = false;
            shield.loop = true;
            shield.positionCount = 48;
            shield.widthMultiplier = 0.055f;
            shield.sharedMaterial = RuntimeAssets.SpriteMaterial;
            shield.sortingOrder = 16;
            for (int i = 0; i < shield.positionCount; i++)
            {
                float angle = i / (float)shield.positionCount * Mathf.PI * 2f;
                shield.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.58f);
            }
            SetShield(false);

            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.34f;
            trail.startWidth = 0.22f;
            trail.endWidth = 0.015f;
            trail.minVertexDistance = 0.035f;
            trail.numCornerVertices = 4;
            trail.sharedMaterial = RuntimeAssets.SpriteMaterial;
            trail.startColor = new Color(0.32f, 0.96f, 1f, 0.88f);
            trail.endColor = new Color(0.25f, 0.45f, 1f, 0f);
            trail.sortingOrder = 10;
            trail.emitting = false;

            var outerFlameObject = new GameObject("Outer Engine Flame");
            outerFlameObject.transform.SetParent(transform, false);
            outerFlameObject.transform.localPosition = new Vector3(0f, -0.58f, 0f);
            outerFlame = outerFlameObject.AddComponent<SpriteRenderer>();
            outerFlame.sprite = RuntimeAssets.FlameSprite;
            outerFlame.color = new Color(0.15f, 0.9f, 1f, 0.92f);
            outerFlame.sortingOrder = 11;

            var innerFlameObject = new GameObject("Inner Engine Flame");
            innerFlameObject.transform.SetParent(outerFlameObject.transform, false);
            innerFlameObject.transform.localPosition = new Vector3(0f, 0.13f, 0f);
            innerFlame = innerFlameObject.AddComponent<SpriteRenderer>();
            innerFlame.sprite = RuntimeAssets.FlameSprite;
            innerFlame.color = new Color(1f, 0.78f, 0.2f, 1f);
            innerFlame.sortingOrder = 12;

            var fuelTrackObject = new GameObject("Fuel Gauge Track");
            fuelTrackObject.transform.SetParent(transform, false);
            fuelTrackObject.transform.localPosition = new Vector3(0.2f, 0.02f, 0f);
            fuelTrackObject.transform.localScale = new Vector3(0.075f, 0.46f, 1f);
            fuelTrack = fuelTrackObject.AddComponent<SpriteRenderer>();
            fuelTrack.sprite = RuntimeAssets.SquareSprite;
            fuelTrack.color = new Color(0.015f, 0.04f, 0.08f, 0.92f);
            fuelTrack.sortingOrder = 14;

            var fuelFillObject = new GameObject("Fuel Gauge Fill");
            fuelFillObject.transform.SetParent(transform, false);
            fuelFill = fuelFillObject.AddComponent<SpriteRenderer>();
            fuelFill.sprite = RuntimeAssets.SquareSprite;
            fuelFill.sortingOrder = 15;
            SetEngine(false);
            SetFuel(1f);
        }

        private void UpdateFlightVisuals()
        {
            float danger = FlightDanger01;
            float multiplier01 = Mathf.InverseLerp(1f, GameTuning.MaxDistanceMultiplier, FlightMultiplier);
            float flicker = 1f + Mathf.Sin(Time.unscaledTime * 42f) * 0.09f;
            bool overdrive = Time.time < overdriveEndsAt;
            float boostScale = overdrive ? 1.55f : 1f;
            outerFlame.transform.localScale = new Vector3(0.24f * (overdrive ? 1.2f : 1f), Mathf.Lerp(0.38f, 0.9f, multiplier01) * flicker * boostScale, 1f);
            innerFlame.transform.localScale = new Vector3(0.52f, 0.62f, 1f);
            Color accent = GameProgression.TrailColor(MetaProgression.Selected(CosmeticKind.Trail));
            outerFlame.color = overdrive ? new Color(1f, 0.42f, 0.12f, 1f)
                : Color.Lerp(accent, Color.white, multiplier01 * 0.35f);
            innerFlame.color = Color.Lerp(accent, Color.white, 0.7f);
            SetFuel(1f - danger);
        }

        public void ApplyStyle(int style)
        {
            Color accent = GameProgression.TrailColor(style);
            if (trail != null)
            {
                trail.startColor = new Color(accent.r, accent.g, accent.b, 0.9f);
                trail.endColor = new Color(accent.r, accent.g, accent.b, 0f);
                trail.time = style == 4 ? 0.48f : style == 5 ? 0.42f : 0.34f;
                trail.startWidth = style == 4 ? 0.27f : style == 5 ? 0.17f : 0.22f;
            }
            if (outerFlame != null) outerFlame.color = new Color(accent.r, accent.g, accent.b, 0.94f);
            if (body != null && State != PlayerOrbitState.Dead) body.color = Color.Lerp(Color.white, accent, 0.13f);
        }

        public void ApplyCosmetics()
        {
            if (body != null) body.sprite = RuntimeAssets.GetRocketSprite(MetaProgression.Selected(CosmeticKind.Rocket));
            ApplyStyle(MetaProgression.Selected(CosmeticKind.Trail));
        }

        private void UpdateMotionJuice(float deltaTime)
        {
            launchJuice = Mathf.Max(0f, launchJuice - deltaTime);
            captureJuice = Mathf.Max(0f, captureJuice - deltaTime);
            Vector3 targetScale = Vector3.one * 0.72f;
            if (launchJuice > 0f)
            {
                float t = launchJuice / 0.16f;
                targetScale = new Vector3(0.62f, 0.86f + t * 0.08f, 1f);
            }
            else if (captureJuice > 0f)
            {
                float wave = Mathf.Sin((1f - captureJuice / 0.18f) * Mathf.PI);
                targetScale = new Vector3(0.72f + wave * 0.13f, 0.72f - wave * 0.08f, 1f);
            }
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 1f - Mathf.Exp(-deltaTime * 28f));
            if (body != null && State != PlayerOrbitState.Dead)
            {
                Color effect = Time.time < extractorEndsAt ? new Color(1f, 0.78f, 0.2f)
                    : Time.time < magnetEndsAt ? new Color(0.2f, 0.92f, 1f)
                    : Time.time < overdriveEndsAt ? new Color(1f, 0.42f, 0.14f)
                    : Time.time < shieldPowerEndsAt ? new Color(0.25f, 1f, 0.7f) : Color.white;
                float strength = effect == Color.white ? 0f : 0.18f + (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.06f;
                body.color = Color.Lerp(Color.white, effect, strength);
            }
        }

        private void SetEngine(bool active)
        {
            if (outerFlame != null) outerFlame.gameObject.SetActive(active);
        }

        private void SetFuel(float amount)
        {
            if (fuelFill == null) return;
            bool visible = GamePreferences.FlightGauges && State != PlayerOrbitState.Dead;
            fuelTrack.enabled = visible;
            fuelFill.enabled = visible;
            float fuel = Mathf.Clamp01(amount);
            const float height = 0.42f;
            float filledHeight = Mathf.Max(0.015f, height * fuel);
            fuelFill.transform.localScale = new Vector3(0.045f, filledHeight, 1f);
            fuelFill.transform.localPosition = new Vector3(0.2f, -0.21f + filledHeight * 0.5f, 0f);
            fuelFill.color = Color.Lerp(new Color(1f, 0.2f, 0.38f, 1f), new Color(0.2f, 1f, 0.78f, 1f), fuel);
        }

        private void UpdateShield()
        {
            if (shield == null) return;
            bool powerShield = Time.time < shieldPowerEndsAt;
            float remaining = powerShield ? shieldPowerEndsAt - Time.time : shieldEndsAt - Time.time;
            if (!GamePreferences.Shield || remaining <= 0f || (!powerShield && State != PlayerOrbitState.Orbiting))
            {
                SetShield(false);
                return;
            }
            shield.enabled = true;

            float alpha = remaining <= (powerShield ? 0.7f : 0.36f)
                ? Mathf.Lerp(0.08f, 0.9f, (Mathf.Sin(Time.unscaledTime * 38f) + 1f) * 0.5f)
                : 0.68f;
            Color baseColor = powerShield ? new Color(0.25f, 1f, 0.68f) : new Color(0.2f, 0.96f, 1f);
            Color color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            shield.startColor = color;
            shield.endColor = color;
            float pulse = (Mathf.Sin(Time.unscaledTime * (powerShield ? 10f : 7f)) + 1f) * 0.5f;
            shield.widthMultiplier = powerShield ? Mathf.Lerp(0.07f, 0.115f, pulse) : 0.055f;
            shield.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 7f) * (powerShield ? 0.065f : 0.035f));
            shield.transform.localRotation = Quaternion.Euler(0f, 0f, powerShield ? Time.unscaledTime * 42f : 0f);

        }

        private void SetShield(bool active)
        {
            if (shield == null) return;
            shield.enabled = active && GamePreferences.Shield;
        }
    }
}
