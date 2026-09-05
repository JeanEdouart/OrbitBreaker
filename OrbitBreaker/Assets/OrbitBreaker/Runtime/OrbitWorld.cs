using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public sealed class OrbitAnchor : MonoBehaviour
    {
        private LineRenderer ring;
        private SpriteRenderer core;
        private readonly List<SpriteRenderer> directionMarkers = new List<SpriteRenderer>();
        private float pulseOffset;
        private float markerPhase;

        public int Sequence { get; private set; }
        public float Radius { get; private set; }
        public int Direction { get; private set; }
        public bool IsCurrent { get; private set; }
        public bool IsVisited { get; private set; }

        public void Initialize(int sequence, Vector2 position, float radius, int direction)
        {
            Sequence = sequence;
            Radius = radius;
            Direction = direction;
            transform.position = position;
            gameObject.name = "Orbit Anchor " + sequence;
            gameObject.SetActive(true);
            pulseOffset = UnityEngine.Random.value * 10f;
            markerPhase = UnityEngine.Random.value;
            IsVisited = false;
            EnsureVisuals();
            core.sprite = RuntimeAssets.GetPlanetSprite(sequence);
            DrawRing();
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
        }

        public void SetVisited(bool visited)
        {
            IsVisited = visited;
            SetCurrent(IsCurrent);
        }

        private void Update()
        {
            if (ring == null) return;
            ring.enabled = GamePreferences.OrbitRings;
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
            CollisionRadius = GameTuning.HazardCollisionRadius(Sequence);
            activationTime = Time.time + 0.8f;
            EnsureVisuals();
            diamond.sprite = RuntimeAssets.GetDebrisSprite(Sequence);
        }

        private void Update()
        {
            if (anchor == null) return;
            orbitAngle += anchor.Direction * Mathf.Lerp(24f, 42f, GameTuning.Difficulty01(Sequence)) * Mathf.Deg2Rad * Time.deltaTime;
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

    public sealed class OrbitWorld : MonoBehaviour
    {
        private readonly List<OrbitAnchor> anchors = new List<OrbitAnchor>();
        private readonly List<OrbitHazard> hazards = new List<OrbitHazard>();
        private readonly Queue<OrbitAnchor> anchorPool = new Queue<OrbitAnchor>();
        private readonly Queue<OrbitHazard> hazardPool = new Queue<OrbitHazard>();
        private Transform activeRoot;
        private Transform poolRoot;
        private System.Random random;
        private int nextSequence;
        private Vector2 lastPosition;
        private OrbitAnchor lastAnchor;

        public IReadOnlyList<OrbitAnchor> Anchors => anchors;
        public IReadOnlyList<OrbitHazard> Hazards => hazards;

        public OrbitAnchor ResetWorld()
        {
            EnsureRoots();
            foreach (OrbitAnchor anchor in anchors) Recycle(anchor);
            foreach (OrbitHazard hazard in hazards) Recycle(hazard);
            anchors.Clear();
            hazards.Clear();
            random = new System.Random(Environment.TickCount);
            nextSequence = 0;
            lastPosition = new Vector2(0f, GameTuning.StartingHeight);
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
        }

        private void GenerateNext()
        {
            int score = nextSequence;
            Vector2 candidatePosition = default;
            float candidateRadius = 1.2f;
            int candidateDirection = 1;
            int reachableSamples = 0;

            for (int attempt = 0; attempt < GameTuning.GenerationAttempts; attempt++)
            {
                float gap = GameTuning.AnchorGap(score, NextFloat());
                float horizontalStep = Mathf.Lerp(-2.2f, 2.2f, NextFloat());
                float x = Mathf.Clamp(lastPosition.x + horizontalStep, -2.45f, 2.45f);
                candidatePosition = new Vector2(x, lastPosition.y + gap);
                candidateRadius = Mathf.Lerp(1.02f, 1.38f, NextFloat());
                candidateDirection = NextFloat() > 0.5f ? 1 : -1;
                reachableSamples = GameTuning.CountReachableLaunchSamples(
                    lastAnchor.transform.position,
                    lastAnchor.Radius,
                    lastAnchor.Direction,
                    candidatePosition,
                    candidateRadius,
                    score);

                if (reachableSamples >= GameTuning.MinimumReachableLaunchSamples(score)) break;
            }

            if (reachableSamples < GameTuning.MinimumReachableLaunchSamples(score))
            {
                // Deterministic fallback: a centered, generously sized orbit instead of an unfair roll.
                candidatePosition = new Vector2(
                    Mathf.Lerp(lastPosition.x, 0f, 0.35f),
                    lastPosition.y + GameTuning.AnchorGap(score, 0.25f));
                candidateRadius = 1.34f;
                candidateDirection = lastAnchor.Direction;
                reachableSamples = GameTuning.CountReachableLaunchSamples(
                    lastAnchor.transform.position,
                    lastAnchor.Radius,
                    lastAnchor.Direction,
                    candidatePosition,
                    candidateRadius,
                    score);
            }

            lastPosition = candidatePosition;
            OrbitAnchor anchor = SpawnAnchor(candidatePosition, candidateRadius, candidateDirection);
            lastAnchor = anchor;

            if (GameTuning.HasHazard(score) && GameTuning.CanAddHazardToLayout(reachableSamples, score))
            {
                float angle = Mathf.Repeat(score * 0.381966f, 1f) * Mathf.PI * 2f;
                hazards.Add(GetHazard(anchor, angle));
            }
        }

        private OrbitAnchor SpawnAnchor(Vector2 position, float radius, int direction)
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
            anchor.Initialize(nextSequence, position, radius, direction);
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
