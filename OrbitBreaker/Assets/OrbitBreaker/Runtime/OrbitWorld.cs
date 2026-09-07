using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public sealed class OrbitAnchor : MonoBehaviour
    {
        private LineRenderer ring;
        private LineRenderer synchronizationArc;
        private SpriteRenderer synchronizationMarker;
        private SpriteRenderer core;
        private readonly List<SpriteRenderer> directionMarkers = new List<SpriteRenderer>();
        private float pulseOffset;
        private float markerPhase;

        public int Sequence { get; private set; }
        public int DifficultyDistance { get; private set; }
        public float Radius { get; private set; }
        public int Direction { get; private set; }
        public bool IsCurrent { get; private set; }
        public bool IsVisited { get; private set; }
        public float SynchronizationAngle { get; private set; }

        public void Initialize(int sequence, Vector2 position, float radius, int direction, float synchronizationAngle = 0f, int difficultyDistance = 0)
        {
            DifficultyDistance = difficultyDistance;
            Sequence = sequence;
            Radius = radius;
            Direction = direction;
            transform.position = position;
            gameObject.name = "Orbit Anchor " + sequence;
            gameObject.SetActive(true);
            pulseOffset = UnityEngine.Random.value * 10f;
            markerPhase = UnityEngine.Random.value;
            SynchronizationAngle = synchronizationAngle;
            IsVisited = false;
            EnsureVisuals();
            core.sprite = RuntimeAssets.GetPlanetSprite(sequence);
            DrawRing();
            DrawSynchronizationArc();
            SetCurrent(false);
        }

        public void SetCurrent(bool current)
        {
            IsCurrent = current;
            if (ring == null) return;

            Color color = current
                ? new Color(0.18f, 0.94f, 1f, 0.38f)
                : IsVisited ? new Color(0.58f, 0.4f, 1f, 0.3f) : new Color(0.24f, 0.47f, 0.68f, 0.24f);
            ring.startColor = color;
            ring.endColor = color;
            ring.widthMultiplier = current ? 0.075f : 0.045f;
            core.color = current ? new Color(0.78f, 1f, 1f, 1f) : IsVisited ? new Color(0.72f, 0.55f, 1f, 0.9f) : new Color(0.35f, 0.66f, 0.83f, 0.82f);
            core.transform.localScale = Vector3.one * (current ? 1.02f : 0.88f);
            for (int i = 0; i < directionMarkers.Count; i++)
            {
                directionMarkers[i].color = current
                    ? new Color(0.76f, 1f, 1f, 0.92f - i * 0.1f)
                    : new Color(0.34f, 0.68f, 0.88f, 0.5f - i * 0.055f);
            }
            if (synchronizationArc != null)
            {
                synchronizationArc.enabled = GamePreferences.OrbitRings && !current && !IsVisited;
                Color cyan = new Color(0.2f, 0.95f, 1f, current ? 0f : 0.9f);
                Color amber = new Color(1f, 0.68f, 0.2f, cyan.a);
                synchronizationArc.startColor = Direction > 0 ? cyan : amber;
                synchronizationArc.endColor = Direction > 0 ? amber : cyan;
            }
        }

        public void SetVisited(bool visited)
        {
            IsVisited = visited;
            SetCurrent(IsCurrent);
        }

        public void RefreshCosmetic()
        {
            if (core != null) core.sprite = RuntimeAssets.GetPlanetSprite(Sequence);
        }

        private void Update()
        {
            if (ring == null) return;
            ring.enabled = GamePreferences.OrbitRings;
            if (synchronizationArc != null) synchronizationArc.enabled = GamePreferences.OrbitRings && !IsCurrent && !IsVisited;
            if (synchronizationMarker != null)
            {
                synchronizationMarker.enabled = synchronizationArc != null && synchronizationArc.enabled;
                float halfArc = GameTuning.SynchronizationHalfAngle(DifficultyDistance) * Mathf.Deg2Rad;
                float travel = Mathf.Repeat(Time.unscaledTime * 0.72f + Sequence * 0.13f, 1f);
                float from = Direction > 0 ? -halfArc : halfArc;
                float to = -from;
                float syncAngle = SynchronizationAngle + Mathf.Lerp(from, to, travel);
                synchronizationMarker.transform.localPosition = new Vector3(Mathf.Cos(syncAngle), Mathf.Sin(syncAngle)) * Radius;
                synchronizationMarker.transform.localScale = Vector3.one * (0.11f + Mathf.Sin(travel * Mathf.PI) * 0.045f);
            }
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.4f + pulseOffset) * (IsCurrent ? 0.018f : 0.008f);
            ring.transform.localScale = Vector3.one * pulse;
            markerPhase = Mathf.Repeat(markerPhase + Direction * Time.deltaTime * (IsCurrent ? 0.28f : 0.17f), 1f);
            for (int i = 0; i < directionMarkers.Count; i++)
            {
                float progress = Mathf.Repeat(markerPhase - i * 0.075f * Direction, 1f);
                float angle = progress * Mathf.PI * 2f;
                Transform marker = directionMarkers[i].transform;
                directionMarkers[i].enabled = GamePreferences.OrbitRings && GamePreferences.RotationGuides;
                marker.localPosition = new Vector3(Mathf.Cos(angle) * Radius, Mathf.Sin(angle) * Radius, 0f);
                float tangentAngle = angle * Mathf.Rad2Deg + (Direction > 0 ? 90f : -90f);
                marker.localRotation = Quaternion.Euler(0f, 0f, tangentAngle - 45f);
            }
        }

        private void EnsureVisuals()
        {
            if (ring == null)
            {
                var ringObject = new GameObject("Capture Ring");
                ringObject.transform.SetParent(transform, false);
                ring = ringObject.AddComponent<LineRenderer>();
                ring.useWorldSpace = false;
                ring.loop = true;
                ring.positionCount = 72;
                ring.numCornerVertices = 3;
                ring.numCapVertices = 3;
                ring.textureMode = LineTextureMode.Stretch;
                ring.sharedMaterial = RuntimeAssets.SpriteMaterial;
                ring.sortingOrder = 0;

                var syncObject = new GameObject("Synchronization Window");
                syncObject.transform.SetParent(transform, false);
                synchronizationArc = syncObject.AddComponent<LineRenderer>();
                synchronizationArc.useWorldSpace = false;
                synchronizationArc.positionCount = 13;
                synchronizationArc.numCapVertices = 4;
                synchronizationArc.sharedMaterial = RuntimeAssets.SpriteMaterial;
                synchronizationArc.widthMultiplier = 0.115f;
                synchronizationArc.sortingOrder = 3;

                var syncMarkerObject = new GameObject("Synchronization Direction");
                syncMarkerObject.transform.SetParent(transform, false);
                synchronizationMarker = syncMarkerObject.AddComponent<SpriteRenderer>();
                synchronizationMarker.sprite = RuntimeAssets.CircleSprite;
                synchronizationMarker.color = new Color(0.92f, 1f, 1f, 0.98f);
                synchronizationMarker.sortingOrder = 4;
            }

            if (core == null)
            {
                var coreObject = new GameObject("Core");
                coreObject.transform.SetParent(transform, false);
                core = coreObject.AddComponent<SpriteRenderer>();
                core.sprite = RuntimeAssets.CircleSprite;
                core.sortingOrder = 1;

                for (int i = 0; i < 5; i++)
                {
                    var markerObject = new GameObject("Direction Marker " + (i + 1));
                    markerObject.transform.SetParent(transform, false);
                    markerObject.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, 0.07f, i / 4f);
                    SpriteRenderer marker = markerObject.AddComponent<SpriteRenderer>();
                    marker.sprite = RuntimeAssets.SquareSprite;
                    marker.sortingOrder = 2;
                    directionMarkers.Add(marker);
                }
            }
        }

        private void DrawRing()
        {
            for (int i = 0; i < ring.positionCount; i++)
            {
                float angle = i / (float)ring.positionCount * Mathf.PI * 2f;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * Radius, Mathf.Sin(angle) * Radius, 0f));
            }
        }

        private void DrawSynchronizationArc()
        {
            if (synchronizationArc == null) return;
            float halfArc = GameTuning.SynchronizationHalfAngle(DifficultyDistance) * Mathf.Deg2Rad;
            for (int i = 0; i < synchronizationArc.positionCount; i++)
            {
                float t = i / (float)(synchronizationArc.positionCount - 1);
                float angle = SynchronizationAngle + Mathf.Lerp(-halfArc, halfArc, t);
                synchronizationArc.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius);
            }
        }
    }

    public sealed class OrbitHazard : MonoBehaviour
    {
        private SpriteRenderer diamond;
        private LineRenderer outline;
        private float phase;
        private OrbitAnchor anchor;
        private float orbitAngle;
        private float activationTime;

        public int Sequence { get; private set; }
        public float CollisionRadius { get; private set; }

        public void Initialize(OrbitAnchor targetAnchor, float startAngle)
        {
            anchor = targetAnchor;
            Sequence = targetAnchor.Sequence;
            orbitAngle = startAngle;
            transform.position = PositionOnOrbit();
            transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            gameObject.name = "Breaker " + Sequence;
            gameObject.SetActive(true);
            phase = UnityEngine.Random.value * 6f;
            CollisionRadius = GameTuning.HazardCollisionRadius(anchor.DifficultyDistance);
            activationTime = Time.time + 0.8f;
            EnsureVisuals();
            diamond.sprite = RuntimeAssets.GetDebrisSprite(Sequence);
        }

        private void Update()
        {
            if (anchor == null) return;
            orbitAngle += anchor.Direction * Mathf.Lerp(24f, 42f, GameTuning.Difficulty01(anchor.DifficultyDistance)) * Mathf.Deg2Rad * Time.deltaTime;
            transform.position = PositionOnOrbit();
            float activation = Mathf.Clamp01((Time.time - activationTime) / 0.45f);
            float scale = (CollisionRadius * 4.1f + Mathf.Sin(Time.time * 4.5f + phase) * 0.055f) * Mathf.Lerp(0.35f, 1f, activation);
            transform.localScale = Vector3.one * scale;
            transform.Rotate(0f, 0f, 46f * Time.deltaTime);
            diamond.color = new Color(1f, Mathf.Lerp(0.55f, 1f, activation), Mathf.Lerp(0.62f, 1f, activation), Mathf.Lerp(0.25f, 1f, activation));
        }

        private Vector2 PositionOnOrbit()
        {
            return (Vector2)anchor.transform.position + new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * anchor.Radius;
        }

        private void EnsureVisuals()
        {
            if (diamond != null) return;
            diamond = gameObject.AddComponent<SpriteRenderer>();
            diamond.sprite = RuntimeAssets.GetDebrisSprite(Sequence);
            diamond.color = Color.white;
            diamond.sortingOrder = 3;

            var outlineObject = new GameObject("Warning Outline");
            outlineObject.transform.SetParent(transform, false);
            outline = outlineObject.AddComponent<LineRenderer>();
            outline.useWorldSpace = false;
            outline.loop = true;
            outline.positionCount = 4;
            outline.sharedMaterial = RuntimeAssets.SpriteMaterial;
            outline.widthMultiplier = 0.07f;
            outline.startColor = new Color(1f, 0.65f, 0.2f, 0.95f);
            outline.endColor = outline.startColor;
            outline.sortingOrder = 4;
            outline.enabled = false;
            outline.SetPosition(0, new Vector3(-0.7f, -0.7f));
            outline.SetPosition(1, new Vector3(-0.7f, 0.7f));
            outline.SetPosition(2, new Vector3(0.7f, 0.7f));
            outline.SetPosition(3, new Vector3(0.7f, -0.7f));
        }
    }

    public sealed class FreeDebris : MonoBehaviour
    {
        private SpriteRenderer body;
        private Vector2 origin;
        private Vector2 axis;
        private float amplitude;
        private float speed;
        private float phase;

        public int Id { get; private set; }
        public float CollisionRadius { get; private set; }

        public void Initialize(int id, Vector2 position, Vector2 movementAxis, float movementAmplitude, float movementSpeed, int difficultyDistance = 0)
        {
            Id = id;
            origin = position;
            axis = movementAxis.sqrMagnitude > 0.01f ? movementAxis.normalized : Vector2.right;
            amplitude = movementAmplitude;
            speed = movementSpeed;
            phase = Mathf.Repeat(id * 0.381966f, 1f) * Mathf.PI * 2f;
            CollisionRadius = Mathf.Lerp(0.2f, 0.29f, GameTuning.Difficulty01(difficultyDistance));
            gameObject.name = "Drifting Debris " + id;
            gameObject.SetActive(true);
            EnsureVisuals();
            body.sprite = RuntimeAssets.GetDebrisSprite(id + 11);
            transform.position = origin + axis * Mathf.Sin(phase) * amplitude;
        }

        private void Update()
        {
            phase += speed * Time.deltaTime;
            transform.position = origin + axis * Mathf.Sin(phase) * amplitude;
            transform.Rotate(0f, 0f, (65f + speed * 18f) * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f + Id) * 0.06f;
            transform.localScale = Vector3.one * CollisionRadius * 4.4f * pulse;
        }

        private void EnsureVisuals()
        {
            if (body != null) return;
            body = gameObject.AddComponent<SpriteRenderer>();
            body.sortingOrder = 6;
            body.color = new Color(1f, 0.76f, 0.42f, 0.96f);
        }
    }

    public sealed class MaterialPickup : MonoBehaviour
    {
        private SpriteRenderer glow;
        private SpriteRenderer crystal;
        private float phase;
        private Transform collectionTarget;
        private Action collectionCompleted;
        private float collectionProgress;
        private Vector3 collectionStart;
        private Vector3 collectionScale;
        public int Sequence { get; private set; }
        public int Value { get; private set; }
        public float Radius { get; private set; }

        public void Initialize(int sequence, Vector2 position, int value)
        {
            Sequence = sequence;
            Value = value;
            Radius = value >= 7 ? 0.3f : value >= 3 ? 0.23f : 0.17f;
            phase = sequence * 0.73f + value;
            gameObject.name = "Material " + value + " (" + sequence + ")";
            gameObject.SetActive(true);
            transform.position = position;
            collectionTarget = null;
            collectionCompleted = null;
            collectionProgress = 0f;
            EnsureVisuals();
            float scale = value >= 7 ? 0.44f : value >= 3 ? 0.34f : 0.26f;
            transform.localScale = Vector3.one * scale;
            crystal.color = value >= 7 ? new Color(1f, 0.66f, 0.18f) : value >= 3 ? new Color(0.68f, 0.3f, 1f) : new Color(0.2f, 0.95f, 1f);
            glow.color = new Color(crystal.color.r, crystal.color.g, crystal.color.b, 0.11f);
        }

        public bool Collect()
        {
            if (!gameObject.activeSelf) return false;
            gameObject.SetActive(false);
            return true;
        }

        public bool BeginCollection(Transform target, Action completed)
        {
            if (!gameObject.activeSelf || collectionTarget != null || target == null) return false;
            collectionTarget = target;
            collectionCompleted = completed;
            collectionStart = transform.position;
            collectionScale = transform.localScale;
            collectionProgress = 0f;
            return true;
        }

        private void Update()
        {
            if (collectionTarget != null)
            {
                collectionProgress = Mathf.Clamp01(collectionProgress + Time.deltaTime / 0.28f);
                float eased = collectionProgress * collectionProgress * (3f - 2f * collectionProgress);
                Vector3 target = collectionTarget.position;
                Vector3 arc = Vector3.right * Mathf.Sin(collectionProgress * Mathf.PI) * 0.22f;
                transform.position = Vector3.Lerp(collectionStart, target, eased) + arc;
                transform.localScale = collectionScale * Mathf.Lerp(1f, 0.12f, eased);
                transform.Rotate(0f, 0f, 520f * Time.deltaTime);
                if (collectionProgress >= 1f)
                {
                    Action callback = collectionCompleted;
                    collectionTarget = null;
                    collectionCompleted = null;
                    gameObject.SetActive(false);
                    callback?.Invoke();
                }
                return;
            }
            transform.Rotate(0f, 0f, 72f * Time.deltaTime);
            glow.transform.localScale = Vector3.one * (1.3f + Mathf.Sin(Time.unscaledTime * 4f + phase) * 0.1f);
        }

        private void EnsureVisuals()
        {
            if (crystal != null) return;
            var halo = new GameObject("Halo");
            halo.transform.SetParent(transform, false);
            glow = halo.AddComponent<SpriteRenderer>();
            glow.sprite = RuntimeAssets.CircleSprite;
            glow.sortingOrder = 7;
            var core = new GameObject("Crystal");
            core.transform.SetParent(transform, false);
            core.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            crystal = core.AddComponent<SpriteRenderer>();
            crystal.sprite = RuntimeAssets.MaterialCrystalSprite;
            crystal.sortingOrder = 8;
        }
    }

    public sealed class OrbitWorld : MonoBehaviour
    {
        private readonly List<OrbitAnchor> anchors = new List<OrbitAnchor>();
        private readonly List<OrbitHazard> hazards = new List<OrbitHazard>();
        private readonly List<FreeDebris> freeDebris = new List<FreeDebris>();
        private readonly List<MaterialPickup> materials = new List<MaterialPickup>();
        private readonly List<PowerUpPickup> powerUps = new List<PowerUpPickup>();
        private readonly Queue<OrbitAnchor> anchorPool = new Queue<OrbitAnchor>();
        private readonly Queue<OrbitHazard> hazardPool = new Queue<OrbitHazard>();
        private readonly Queue<FreeDebris> freeDebrisPool = new Queue<FreeDebris>();
        private readonly Queue<MaterialPickup> materialPool = new Queue<MaterialPickup>();
        private readonly Queue<PowerUpPickup> powerUpPool = new Queue<PowerUpPickup>();
        private Transform activeRoot;
        private Transform poolRoot;
        private System.Random random;
        private int nextSequence;
        private Vector2 lastPosition;
        private OrbitAnchor lastAnchor;
        private int lastPowerUpSequence = -10;
        private float hazardBudget;

        public IReadOnlyList<OrbitAnchor> Anchors => anchors;
        public IReadOnlyList<OrbitHazard> Hazards => hazards;
        public IReadOnlyList<FreeDebris> FreeDebris => freeDebris;
        public IReadOnlyList<MaterialPickup> Materials => materials;
        public IReadOnlyList<PowerUpPickup> PowerUps => powerUps;

        public void SetWarpVisible(bool visible)
        {
            EnsureRoots();
            activeRoot.gameObject.SetActive(visible);
        }

        public void RefreshCosmetics()
        {
            for (int i = 0; i < anchors.Count; i++) anchors[i].RefreshCosmetic();
        }

        private int difficultyDistance;

        public void SetDifficultyDistance(int distance) => difficultyDistance = Mathf.Max(difficultyDistance, Mathf.Clamp(distance, 0, GameTuning.DifficultyCapDistance));

        public OrbitAnchor ResetWorld()
        {
            EnsureRoots();
            foreach (OrbitAnchor anchor in anchors) Recycle(anchor);
            foreach (OrbitHazard hazard in hazards) Recycle(hazard);
            foreach (FreeDebris debris in freeDebris) Recycle(debris);
            foreach (MaterialPickup material in materials) Recycle(material);
            foreach (PowerUpPickup powerUp in powerUps) Recycle(powerUp);
            anchors.Clear();
            hazards.Clear();
            freeDebris.Clear();
            materials.Clear();
            powerUps.Clear();
            random = new System.Random(Environment.TickCount);
            nextSequence = 0;
            difficultyDistance = 0;
            hazardBudget = 0f;
            lastPosition = new Vector2(0f, GameTuning.StartingHeight);
            lastPowerUpSequence = -10;
            OrbitAnchor first = SpawnAnchor(lastPosition, 1.25f, 1);
            lastAnchor = first;
            EnsureAhead(0);
            return first;
        }

        public void EnsureAhead(int currentSequence)
        {
            int requiredSequence = currentSequence + GameTuning.AnchorsAhead;
            while (nextSequence <= requiredSequence) GenerateNext();
        }

        public void RecycleBehind(float cameraY, int currentSequence)
        {
            for (int i = anchors.Count - 1; i >= 0; i--)
            {
                OrbitAnchor anchor = anchors[i];
                if (anchor.Sequence < currentSequence - GameTuning.BackwardOrbitRetention && anchor.transform.position.y < cameraY - 18f)
                {
                    anchors.RemoveAt(i);
                    Recycle(anchor);
                }
            }

            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                OrbitHazard hazard = hazards[i];
                if (hazard.Sequence < currentSequence - GameTuning.BackwardOrbitRetention && hazard.transform.position.y < cameraY - 18f)
                {
                    hazards.RemoveAt(i);
                    Recycle(hazard);
                }
            }

            for (int i = freeDebris.Count - 1; i >= 0; i--)
            {
                FreeDebris debris = freeDebris[i];
                if (debris.Id < currentSequence - GameTuning.BackwardOrbitRetention && debris.transform.position.y < cameraY - 18f)
                {
                    freeDebris.RemoveAt(i);
                    Recycle(debris);
                }
            }
            for (int i = materials.Count - 1; i >= 0; i--)
            {
                MaterialPickup material = materials[i];
                if (material.Sequence < currentSequence - GameTuning.BackwardOrbitRetention && material.transform.position.y < cameraY - 18f)
                {
                    materials.RemoveAt(i);
                    Recycle(material);
                }
            }
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                PowerUpPickup powerUp = powerUps[i];
                if (powerUp.Sequence < currentSequence - GameTuning.BackwardOrbitRetention && powerUp.transform.position.y < cameraY - 18f)
                {
                    powerUps.RemoveAt(i);
                    Recycle(powerUp);
                }
            }
        }

        private void GenerateNext()
        {
            int score = nextSequence;
            OrbitAnchor previousAnchor = lastAnchor;
            Vector2 candidatePosition = default;
            float candidateRadius = 1.2f;
            int candidateDirection = 1;
            int reachableSamples = 0;
            float synchronizationAngle = 0f;

            for (int attempt = 0; attempt < GameTuning.GenerationAttempts; attempt++)
            {
                float gap = GameTuning.AnchorGap(difficultyDistance, GameTuning.IsBreatherOrbit(score) ? NextFloat() * 0.35f : NextFloat());
                float x = GameTuning.OrbitHorizontalPosition(score, NextFloat());
                candidatePosition = new Vector2(x, lastPosition.y + gap);
                candidateRadius = Mathf.Lerp(1.02f, 1.38f, NextFloat());
                candidateDirection = NextFloat() > 0.5f ? 1 : -1;
                reachableSamples = GameTuning.CountReachableLaunchSamples(
                    lastAnchor.transform.position,
                    lastAnchor.Radius,
                    lastAnchor.Direction,
                    candidatePosition,
                    candidateRadius,
                    difficultyDistance);

                if (reachableSamples >= GameTuning.MinimumReachableLaunchSamples(difficultyDistance)) break;
            }

            if (reachableSamples < GameTuning.MinimumReachableLaunchSamples(difficultyDistance))
            {
                // Deterministic fallback: a centered, generously sized orbit instead of an unfair roll.
                candidatePosition = new Vector2(
                    Mathf.Lerp(lastPosition.x, 0f, 0.35f),
                    lastPosition.y + GameTuning.AnchorGap(difficultyDistance, 0.25f));
                candidateRadius = 1.34f;
                candidateDirection = lastAnchor.Direction;
                reachableSamples = GameTuning.CountReachableLaunchSamples(
                    lastAnchor.transform.position,
                    lastAnchor.Radius,
                    lastAnchor.Direction,
                    candidatePosition,
                    candidateRadius,
                    difficultyDistance);
            }

            float positiveAngle;
            float positiveAlignment;
            float negativeAngle;
            float negativeAlignment;
            bool positiveGate = GameTuning.TryFindSynchronizationGate(
                lastAnchor.transform.position, lastAnchor.Radius, lastAnchor.Direction,
                candidatePosition, candidateRadius, 1, difficultyDistance,
                out positiveAngle, out positiveAlignment);
            bool negativeGate = GameTuning.TryFindSynchronizationGate(
                lastAnchor.transform.position, lastAnchor.Radius, lastAnchor.Direction,
                candidatePosition, candidateRadius, -1, difficultyDistance,
                out negativeAngle, out negativeAlignment);
            if (positiveGate || negativeGate)
            {
                candidateDirection = positiveAlignment >= negativeAlignment ? 1 : -1;
                synchronizationAngle = candidateDirection > 0 ? positiveAngle : negativeAngle;
            }
            else
            {
                // This should only happen on a heavily constrained fallback. Keep the gate on
                // the best simulated arrival instead of placing it at an unrelated random angle.
                candidateDirection = positiveAlignment >= negativeAlignment ? 1 : -1;
                synchronizationAngle = candidateDirection > 0 ? positiveAngle : negativeAngle;
            }

            lastPosition = candidatePosition;
            OrbitAnchor anchor = SpawnAnchor(candidatePosition, candidateRadius, candidateDirection, synchronizationAngle);
            lastAnchor = anchor;

            if (score >= 2 && NextFloat() < 0.7f)
            {
                Vector2 route = (Vector2)anchor.transform.position - (Vector2)previousAnchor.transform.position;
                Vector2 side = route.sqrMagnitude > 0.01f ? new Vector2(-route.y, route.x).normalized : Vector2.right;
                float offset = Mathf.Lerp(-0.48f, 0.48f, NextFloat());
                Vector2 position = Vector2.Lerp(previousAnchor.transform.position, anchor.transform.position, Mathf.Lerp(0.4f, 0.6f, NextFloat())) + side * offset;
                float roll = NextFloat();
                int value = roll < 0.68f ? 1 : roll < 0.94f ? 3 : 7;
                materials.Add(GetMaterial(score, position, value));
            }

            // Rare, readable pickups: never on consecutive transfers and always near
            // the safe centre of a route so collecting one remains a choice, not a trap.
            if (score >= 4 && score - lastPowerUpSequence >= 3 && NextFloat() < 0.22f)
            {
                if (GameTuning.TryFindTransferPickupPoint(previousAnchor.transform.position, previousAnchor.Radius, previousAnchor.Direction,
                    anchor.transform.position, anchor.Radius, difficultyDistance, out Vector2 position))
                {
                    PowerUpType type = (PowerUpType)Mathf.Clamp(Mathf.FloorToInt(NextFloat() * 5f), 0, 4);
                    powerUps.Add(GetPowerUp(score, position, type));
                    lastPowerUpSequence = score;
                }
            }

            bool orbitFullyVisible = GameTuning.IsOrbitFullyVisibleForHazard(anchor.transform.position.x, anchor.Radius);
            // Carry a small density budget past unsafe layouts instead of silently losing
            // almost every hazard roll on the wide lanes. Never relax the safety checks.
            hazardBudget = Mathf.Min(2f, hazardBudget + GameTuning.OrbitHazardChance(difficultyDistance));
            if (hazardBudget >= 1f && orbitFullyVisible && !GameTuning.IsBreatherOrbit(score) && GameTuning.CanAddHazardToLayout(reachableSamples, difficultyDistance))
            {
                hazardBudget -= 1f;
                float angle = Mathf.Repeat(score * 0.381966f, 1f) * Mathf.PI * 2f;
                hazards.Add(GetHazard(anchor, angle));
            }

            if (score >= 11 && !GameTuning.IsBreatherOrbit(score) && NextFloat() < GameTuning.SkipHazardChance(difficultyDistance))
            {
                OrbitAnchor skipSource = FindAnchor(score - 2);
                OrbitAnchor bypassed = FindAnchor(score - 1);
                if (skipSource != null && bypassed != null)
                {
                    int skipSamples = GameTuning.CountReachableLaunchSamples(
                        skipSource.transform.position, skipSource.Radius, skipSource.Direction,
                        anchor.transform.position, anchor.Radius, difficultyDistance);
                    if (GameTuning.CanAddSkipChallenge(skipSamples, score, difficultyDistance))
                    {
                        Vector2 challengePosition;
                        float bypassClearance;
                        if (GameTuning.TryFindSkipChallengePoint(
                            skipSource.transform.position, skipSource.Radius, skipSource.Direction,
                            bypassed.transform.position, bypassed.Radius,
                            anchor.transform.position, anchor.Radius, difficultyDistance,
                            out challengePosition, out bypassClearance)
                            && IsClearOfEveryOrbit(challengePosition, skipSource.Sequence, anchor.Sequence, score))
                        {
                            Vector2 skipRoute = (Vector2)anchor.transform.position - (Vector2)skipSource.transform.position;
                            Vector2 movementAxis = skipRoute.sqrMagnitude > 0.01f
                                ? new Vector2(-skipRoute.y, skipRoute.x).normalized : Vector2.right;
                            freeDebris.Add(GetFreeDebris(score, challengePosition, movementAxis));
                        }
                    }
                }
            }
        }

        private bool IsClearOfEveryOrbit(Vector2 point, int sourceSequence, int targetSequence, int sequence)
        {
            float requiredClearance = GameTuning.CaptureBand + 0.46f + GameTuning.HazardCollisionRadius(difficultyDistance) + 0.12f;
            for (int i = 0; i < anchors.Count; i++)
            {
                OrbitAnchor other = anchors[i];
                if (other.Sequence == sourceSequence || other.Sequence == targetSequence) continue;
                if (Vector2.Distance(point, other.transform.position) - other.Radius < requiredClearance) return false;
            }
            return true;
        }

        public OrbitAnchor FindAnchor(int sequence)
        {
            for (int i = anchors.Count - 1; i >= 0; i--)
                if (anchors[i].Sequence == sequence) return anchors[i];
            return null;
        }

        public OrbitAnchor PrepareSafeWarpTarget(int fromSequence, int orbitSkip)
        {
            int targetSequence = Mathf.Max(fromSequence + 2, fromSequence + orbitSkip);
            EnsureAhead(targetSequence);
            OrbitAnchor target = FindAnchor(targetSequence);
            if (target == null) return null;
            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                if (hazards[i].Sequence != targetSequence) continue;
                OrbitHazard hazard = hazards[i]; hazards.RemoveAt(i); Recycle(hazard);
            }
            return target;
        }

        private OrbitAnchor SpawnAnchor(Vector2 position, float radius, int direction, float synchronizationAngle = 0f)
        {
            OrbitAnchor anchor;
            if (anchorPool.Count > 0)
            {
                anchor = anchorPool.Dequeue();
                anchor.transform.SetParent(activeRoot, true);
            }
            else
            {
                var instance = new GameObject();
                instance.transform.SetParent(activeRoot, true);
                anchor = instance.AddComponent<OrbitAnchor>();
            }
            anchor.Initialize(nextSequence, position, radius, direction, synchronizationAngle, difficultyDistance);
            anchors.Add(anchor);
            nextSequence++;
            return anchor;
        }

        private OrbitHazard GetHazard(OrbitAnchor anchor, float startAngle)
        {
            OrbitHazard hazard;
            if (hazardPool.Count > 0)
            {
                hazard = hazardPool.Dequeue();
                hazard.transform.SetParent(activeRoot, true);
            }
            else
            {
                var instance = new GameObject();
                instance.transform.SetParent(activeRoot, true);
                hazard = instance.AddComponent<OrbitHazard>();
            }
            hazard.Initialize(anchor, startAngle);
            return hazard;
        }

        private FreeDebris GetFreeDebris(int id, Vector2 position, Vector2 axis)
        {
            FreeDebris debris;
            if (freeDebrisPool.Count > 0)
            {
                debris = freeDebrisPool.Dequeue();
                debris.transform.SetParent(activeRoot, true);
            }
            else
            {
                var instance = new GameObject();
                instance.transform.SetParent(activeRoot, true);
                debris = instance.AddComponent<FreeDebris>();
            }
            debris.Initialize(id, position, axis, 0.46f, Mathf.Lerp(0.72f, 1.28f, GameTuning.Difficulty01(difficultyDistance)), difficultyDistance);
            return debris;
        }

        private MaterialPickup GetMaterial(int sequence, Vector2 position, int value)
        {
            MaterialPickup pickup = materialPool.Count > 0 ? materialPool.Dequeue() : new GameObject().AddComponent<MaterialPickup>();
            pickup.transform.SetParent(activeRoot, true);
            pickup.Initialize(sequence, position, value);
            return pickup;
        }

        private PowerUpPickup GetPowerUp(int sequence, Vector2 position, PowerUpType type)
        {
            PowerUpPickup pickup = powerUpPool.Count > 0 ? powerUpPool.Dequeue() : new GameObject().AddComponent<PowerUpPickup>();
            pickup.transform.SetParent(activeRoot, true);
            pickup.Initialize(sequence, position, type);
            return pickup;
        }

        private void Recycle(OrbitAnchor anchor)
        {
            anchor.SetCurrent(false);
            anchor.gameObject.SetActive(false);
            anchor.transform.SetParent(poolRoot, false);
            anchorPool.Enqueue(anchor);
        }

        private void Recycle(OrbitHazard hazard)
        {
            hazard.gameObject.SetActive(false);
            hazard.transform.SetParent(poolRoot, false);
            hazardPool.Enqueue(hazard);
        }

        private void Recycle(FreeDebris debris)
        {
            debris.gameObject.SetActive(false);
            debris.transform.SetParent(poolRoot, false);
            freeDebrisPool.Enqueue(debris);
        }

        private void Recycle(MaterialPickup material)
        {
            material.gameObject.SetActive(false);
            material.transform.SetParent(poolRoot, false);
            materialPool.Enqueue(material);
        }

        private void Recycle(PowerUpPickup powerUp)
        {
            powerUp.gameObject.SetActive(false);
            powerUp.transform.SetParent(poolRoot, false);
            powerUpPool.Enqueue(powerUp);
        }

        private void EnsureRoots()
        {
            if (activeRoot != null) return;
            activeRoot = new GameObject("Active World").transform;
            activeRoot.SetParent(transform, false);
            poolRoot = new GameObject("Object Pool").transform;
            poolRoot.SetParent(transform, false);
            poolRoot.gameObject.SetActive(false);
        }

        private float NextFloat() => (float)random.NextDouble();
    }
}
