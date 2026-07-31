using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public enum PlayerOrbitState
    {
        Orbiting,
        Flying,
        Dead
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

        public PlayerOrbitState State { get; private set; }
        public OrbitAnchor CurrentAnchor { get; private set; }
        public int LastSequence { get; private set; }
        public Vector2 Velocity => velocity;

        public event Action<OrbitAnchor, float> Captured;
        public event Action Died;

        public void Initialize()
        {
            gameObject.name = "Player Orb";
            EnsureVisuals();
        }

        public void ResetTo(OrbitAnchor anchor)
        {
            score = 0;
            LastSequence = anchor.Sequence;
            angleRadians = -Mathf.PI * 0.5f;
            flightTime = 0f;
            velocity = Vector2.zero;
            trail.Clear();
            trail.emitting = false;
            body.color = Color.white;
            glow.color = new Color(0.2f, 0.95f, 1f, 0.24f);
            Capture(anchor, 0f, false);
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

            if (CheckHazards(hazards) || transform.position.y < cameraY - GameTuning.DeathDistanceBelowCamera || Mathf.Abs(transform.position.x) > GameTuning.HorizontalLimit)
            {
                Die();
            }
        }

        public void Kill()
        {
            Die();
        }

        private void TickOrbit(float deltaTime)
        {
            if (CurrentAnchor == null)
            {
                Die();
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
                if (!candidate.gameObject.activeInHierarchy || candidate.Sequence <= LastSequence) continue;

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
                Capture(best, bestError / GameTuning.CaptureBand, true);
                return;
            }

            if (flightTime >= GameTuning.MaxFlightTime)
            {
                Die();
            }
        }

        private void Capture(OrbitAnchor anchor, float normalizedError, bool notify)
        {
            CurrentAnchor?.SetCurrent(false);
            CurrentAnchor = anchor;
            LastSequence = anchor.Sequence;
            Vector2 radial = ((Vector2)transform.position - (Vector2)anchor.transform.position).normalized;
            if (radial.sqrMagnitude < 0.1f) radial = Vector2.down;
            angleRadians = Mathf.Atan2(radial.y, radial.x);
            transform.position = (Vector2)anchor.transform.position + radial * anchor.Radius;
            anchor.SetCurrent(true);
            velocity = Vector2.zero;
            flightTime = 0f;
            State = PlayerOrbitState.Orbiting;
            trail.emitting = false;
            if (notify) Captured?.Invoke(anchor, Mathf.Clamp01(normalizedError));
        }

        private bool CheckHazards(IReadOnlyList<OrbitHazard> hazards)
        {
            for (int i = 0; i < hazards.Count; i++)
            {
                OrbitHazard hazard = hazards[i];
                if (!hazard.gameObject.activeInHierarchy) continue;
                if (Vector2.Distance(transform.position, hazard.transform.position) <= hazard.CollisionRadius + 0.17f) return true;
            }
            return false;
        }

        private void Die()
        {
            if (State == PlayerOrbitState.Dead) return;
            CurrentAnchor?.SetCurrent(false);
            CurrentAnchor = null;
            State = PlayerOrbitState.Dead;
            velocity = Vector2.zero;
            trail.emitting = false;
            body.color = new Color(1f, 0.22f, 0.38f, 1f);
            glow.color = new Color(1f, 0.1f, 0.2f, 0.32f);
            Died?.Invoke();
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
