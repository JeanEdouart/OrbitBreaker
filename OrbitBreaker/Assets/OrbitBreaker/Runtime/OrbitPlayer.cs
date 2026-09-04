using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public readonly struct CaptureResult
    {
        public CaptureResult(OrbitAnchor anchor, float multiplier, int skippedAnchors, int fromSequence)
        {
            Anchor = anchor;
            Multiplier = multiplier;
            SkippedAnchors = skippedAnchors;
            FromSequence = fromSequence;
        }

        public OrbitAnchor Anchor { get; }
        public float Multiplier { get; }
        public int SkippedAnchors { get; }
        public int FromSequence { get; }
        public bool IsBacktrack => Anchor.Sequence < FromSequence;
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
        private SpriteRenderer glow;
        private TrailRenderer trail;
        private float angleRadians;
        private Vector2 velocity;
        private float flightTime;
        private int score;
        private float capturedAt;
        private readonly HashSet<int> visitedSequences = new HashSet<int>();

        public PlayerOrbitState State { get; private set; }
        public OrbitAnchor CurrentAnchor { get; private set; }
        public int LastSequence { get; private set; }
        public Vector2 Velocity => velocity;
        public float FlightMultiplier => State == PlayerOrbitState.Flying ? GameTuning.FlightMultiplier(flightTime) : 1f;
        public float FlightDanger01 => State == PlayerOrbitState.Flying ? GameTuning.FlightDanger01(flightTime) : 0f;

        public event Action<CaptureResult> Captured;
        public event Action<DeathReason> Died;

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
            velocity = Vector2.zero;
            trail.Clear();
            trail.emitting = false;
            body.color = Color.white;
            glow.color = new Color(0.2f, 0.95f, 1f, 0.24f);
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
            State = PlayerOrbitState.Flying;
            trail.emitting = true;
            return true;
        }

        public void Tick(float deltaTime, IReadOnlyList<OrbitAnchor> anchors, IReadOnlyList<OrbitHazard> hazards, float cameraY)
        {
            if (State == PlayerOrbitState.Dead) return;

            if (State == PlayerOrbitState.Orbiting)
            {
                TickOrbit(deltaTime);
            }
            else
            {
                TickFlight(deltaTime, anchors);
            }

            if (CheckHazards(hazards))
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
            transform.Rotate(0f, 0f, -CurrentAnchor.Direction * 190f * deltaTime);
        }

        private void TickFlight(float deltaTime, IReadOnlyList<OrbitAnchor> anchors)
        {
            flightTime += deltaTime;
            transform.position += (Vector3)(velocity * deltaTime);
            transform.Rotate(0f, 0f, -320f * deltaTime);

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
                float multiplier = GameTuning.FlightMultiplier(flightTime);
                int fromSequence = LastSequence;
                int skippedAnchors = Mathf.Max(0, best.Sequence - LastSequence - 1);
                Capture(best);
                visitedSequences.Add(best.Sequence);
                Captured?.Invoke(new CaptureResult(best, multiplier, skippedAnchors, fromSequence));
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
            capturedAt = Time.time;
            trail.emitting = false;
        }

        private bool CheckHazards(IReadOnlyList<OrbitHazard> hazards)
        {
            for (int i = 0; i < hazards.Count; i++)
            {
                OrbitHazard hazard = hazards[i];
                if (!hazard.gameObject.activeInHierarchy) continue;
                if (CurrentAnchor != null && hazard.Sequence == CurrentAnchor.Sequence && Time.time - capturedAt < GameTuning.CaptureGraceDuration(CurrentAnchor.Sequence)) continue;
                if (Vector2.Distance(transform.position, hazard.transform.position) <= hazard.CollisionRadius + 0.17f) return true;
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
            body.color = new Color(1f, 0.22f, 0.38f, 1f);
            glow.color = new Color(1f, 0.1f, 0.2f, 0.32f);
            Died?.Invoke(reason);
        }

        private void EnsureVisuals()
        {
            body = gameObject.GetComponent<SpriteRenderer>();
            if (body == null) body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = RuntimeAssets.CircleSprite;
            body.color = Color.white;
            body.sortingOrder = 12;
            transform.localScale = Vector3.one * 0.38f;

            var glowObject = new GameObject("Glow");
            glowObject.transform.SetParent(transform, false);
            glowObject.transform.localScale = Vector3.one * 2.2f;
            glow = glowObject.AddComponent<SpriteRenderer>();
            glow.sprite = RuntimeAssets.CircleSprite;
            glow.color = new Color(0.2f, 0.95f, 1f, 0.24f);
            glow.sortingOrder = 11;

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
        }
    }
}
