using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public sealed class OrbitAnchor : MonoBehaviour
    {
        private LineRenderer ring;
        private SpriteRenderer core;
        private float pulseOffset;

        public int Sequence { get; private set; }
        public float Radius { get; private set; }
        public int Direction { get; private set; }
        public bool IsCurrent { get; private set; }

        public void Initialize(int sequence, Vector2 position, float radius, int direction)
        {
            Sequence = sequence;
            Radius = radius;
            Direction = direction;
            transform.position = position;
            gameObject.name = "Orbit Anchor " + sequence;
            gameObject.SetActive(true);
            pulseOffset = UnityEngine.Random.value * 10f;
            EnsureVisuals();
            DrawRing();
            SetCurrent(false);
        }

        public void SetCurrent(bool current)
        {
            IsCurrent = current;
            if (ring == null) return;

            Color color = current ? new Color(0.18f, 0.94f, 1f, 0.94f) : new Color(0.24f, 0.47f, 0.68f, 0.48f);
            ring.startColor = color;
            ring.endColor = color;
            ring.widthMultiplier = current ? 0.075f : 0.045f;
            core.color = current ? new Color(0.78f, 1f, 1f, 1f) : new Color(0.35f, 0.66f, 0.83f, 0.82f);
            core.transform.localScale = Vector3.one * (current ? 0.27f : 0.19f);
        }

        private void Update()
        {
            if (ring == null) return;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.4f + pulseOffset) * (IsCurrent ? 0.018f : 0.008f);
            ring.transform.localScale = Vector3.one * pulse;
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

        public int Sequence { get; private set; }
        public float CollisionRadius { get; private set; } = 0.34f;

        public void Initialize(int sequence, Vector2 position)
        {
            Sequence = sequence;
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            gameObject.name = "Breaker " + sequence;
            gameObject.SetActive(true);
            phase = UnityEngine.Random.value * 6f;
            EnsureVisuals();
        }

        private void Update()
        {
            float scale = 0.44f + Mathf.Sin(Time.time * 4.5f + phase) * 0.035f;
            transform.localScale = Vector3.one * scale;
            transform.Rotate(0f, 0f, 46f * Time.deltaTime);
        }

        private void EnsureVisuals()
        {
            if (diamond != null) return;
            diamond = gameObject.AddComponent<SpriteRenderer>();
            diamond.sprite = RuntimeAssets.SquareSprite;
            diamond.color = new Color(1f, 0.18f, 0.38f, 0.9f);
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
            lastPosition = new Vector2(0f, -2.1f);
            OrbitAnchor first = SpawnAnchor(lastPosition, 1.25f, 1);
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
                if (anchor.Sequence < currentSequence - 2 && anchor.transform.position.y < cameraY - 8f)
                {
                    anchors.RemoveAt(i);
                    Recycle(anchor);
                }
            }

            for (int i = hazards.Count - 1; i >= 0; i--)
            {
                OrbitHazard hazard = hazards[i];
                if (hazard.Sequence < currentSequence - 1 && hazard.transform.position.y < cameraY - 8f)
                {
                    hazards.RemoveAt(i);
                    Recycle(hazard);
                }
            }
        }

        private void GenerateNext()
        {
            int score = nextSequence;
            float gap = GameTuning.AnchorGap(score, NextFloat());
            float horizontalStep = Mathf.Lerp(-2.2f, 2.2f, NextFloat());
            float x = Mathf.Clamp(lastPosition.x + horizontalStep, -2.45f, 2.45f);
            lastPosition = new Vector2(x, lastPosition.y + gap);
            float radius = Mathf.Lerp(1.02f, 1.38f, NextFloat());
            int direction = NextFloat() > 0.5f ? 1 : -1;
            OrbitAnchor anchor = SpawnAnchor(lastPosition, radius, direction);

            float hazardChance = Mathf.Lerp(0.12f, 0.58f, GameTuning.Difficulty01(score));
            if (score >= 4 && NextFloat() < hazardChance)
            {
                float angle = Mathf.Lerp(0f, Mathf.PI * 2f, NextFloat());
                Vector2 hazardPosition = lastPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                hazards.Add(GetHazard(anchor.Sequence, hazardPosition));
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

        private OrbitHazard GetHazard(int sequence, Vector2 position)
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
            hazard.Initialize(sequence, position);
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
