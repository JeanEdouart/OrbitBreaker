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

        public PlayerOrbitState State { get; private set; }
        public OrbitAnchor CurrentAnchor { get; private set; }
        public int LastSequence { get; private set; }
        public Vector2 Velocity => velocity;
        public float FlightMultiplier => State == PlayerOrbitState.Flying ? Mathf.Min(GameTuning.MaxDistanceMultiplier, GameTuning.FlightMultiplier(flightTime) + nearMissBoost) : 1f;
        public float FlightDanger01 => State == PlayerOrbitState.Flying ? GameTuning.FlightDanger01(flightTime) : 0f;

        public event Action<CaptureResult> Captured;
        public event Action<DeathReason> Died;
        public event Action<NearMissResult> NearMissed;

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
            trail.Clear();
            trail.emitting = false;
            body.color = Color.white;
            body.enabled = true;
            fuelTrack.enabled = true;
            fuelFill.enabled = true;
            SetEngine(false);
            SetFuel(1f);
            ApplyStyle(GameProgression.SelectedStyle);
            Capture(anchor);
        }

        public void SetScore(int value)
        {
            score = Mathf.Max(0, value);
        }

        public bool Launch()
        {
            if (State != PlayerOrbitState.Orbiting || CurrentAnchor == null) return false;

            Vector2 radial = ((Vector2)transform.position - (Vector2)CurrentAnchor.transform.position).normalized;
            Vector2 tangent = CurrentAnchor.Direction > 0
                ? new Vector2(-radial.y, radial.x)
                : new Vector2(radial.y, -radial.x);

            velocity = tangent * GameTuning.LaunchSpeed(score);
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
            ApplyStyle(GameProgression.SelectedStyle);
            return true;
        }

        public void Tick(float deltaTime, IReadOnlyList<OrbitAnchor> anchors, IReadOnlyList<OrbitHazard> hazards, IReadOnlyList<FreeDebris> freeDebris, float cameraY)
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

            if (CheckHazards(hazards, freeDebris))
            {
                Die(DeathReason.Breaker);
            }
            else if (transform.position.y < cameraY - GameTuning.DeathDistanceBelowCamera || Mathf.Abs(transform.position.x) > GameTuning.HorizontalLimit)
            {
                Die(DeathReason.LostInSpace);
            }
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
                bool inSynchronizationZone = GameTuning.IsWithinSynchronizationZone(radial, best.Sequence, best.SynchronizationAngle);
                bool synchronized = GameTuning.IsSynchronizedCapture(radial, velocity, best.Direction, best.Sequence, best.SynchronizationAngle);
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

            if (flightTime >= GameTuning.MaxFlightTime)
            {
                Die(DeathReason.LostInSpace);
            }
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
            shieldEndsAt = capturedAt + GameTuning.CaptureGraceDuration(anchor.Sequence);
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
            for (int i = 0; i < hazards.Count; i++)
            {
                OrbitHazard hazard = hazards[i];
                if (!hazard.gameObject.activeInHierarchy) continue;
                if (CurrentAnchor != null && hazard.Sequence == CurrentAnchor.Sequence && Time.time - capturedAt < GameTuning.CaptureGraceDuration(CurrentAnchor.Sequence)) continue;
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
            float danger = GameTuning.FlightDanger01(flightTime);
            float multiplier01 = Mathf.InverseLerp(1f, GameTuning.MaxDistanceMultiplier, FlightMultiplier);
            float flicker = 1f + Mathf.Sin(Time.unscaledTime * 42f) * 0.09f;
            outerFlame.transform.localScale = new Vector3(0.24f, Mathf.Lerp(0.38f, 0.9f, multiplier01) * flicker, 1f);
            innerFlame.transform.localScale = new Vector3(0.52f, 0.62f, 1f);
            outerFlame.color = Color.Lerp(new Color(0.18f, 0.92f, 1f, 0.9f), new Color(1f, 0.2f, 0.62f, 0.96f), multiplier01);
            SetFuel(1f - danger);
        }

        public void ApplyStyle(int style)
        {
            Color accent = GameProgression.TrailColor(style);
            if (trail != null)
            {
                trail.startColor = new Color(accent.r, accent.g, accent.b, 0.9f);
                trail.endColor = new Color(accent.r, accent.g, accent.b, 0f);
            }
            if (outerFlame != null) outerFlame.color = new Color(accent.r, accent.g, accent.b, 0.94f);
            if (body != null && State != PlayerOrbitState.Dead) body.color = Color.Lerp(Color.white, accent, 0.13f);
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
            float remaining = shieldEndsAt - Time.time;
            if (!GamePreferences.Shield || remaining <= 0f || State != PlayerOrbitState.Orbiting)
            {
                SetShield(false);
                return;
            }
            shield.enabled = true;

            float alpha = remaining <= 0.36f
                ? Mathf.Lerp(0.08f, 0.9f, (Mathf.Sin(Time.unscaledTime * 38f) + 1f) * 0.5f)
                : 0.68f;
            Color color = new Color(0.2f, 0.96f, 1f, alpha);
            shield.startColor = color;
            shield.endColor = color;
            shield.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.035f);

        }

        private void SetShield(bool active)
        {
            if (shield == null) return;
            shield.enabled = active && GamePreferences.Shield;
        }
    }
}
