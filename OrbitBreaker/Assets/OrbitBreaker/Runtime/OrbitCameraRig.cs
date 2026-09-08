using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed class OrbitCameraRig : MonoBehaviour
    {
        private Camera targetCamera;
        private Vector3 velocity;
        private Vector3 basePosition;
        private float targetY;
        private float targetX;
        private float impactShakeRemaining;
        private float impactShakeDuration;
        private float impactShakeStrength;
        private float flightShakeStrength;

        public float CameraY => targetCamera != null ? targetCamera.transform.position.y : 0f;
        public Vector3 CameraPosition => targetCamera != null ? targetCamera.transform.position : Vector3.zero;

        public void ApplyReplayPosition(Vector3 position)
        {
            if (targetCamera == null) return;
            targetX = position.x;
            targetY = position.y;
            basePosition = position;
            velocity = Vector3.zero;
            impactShakeRemaining = 0f;
            flightShakeStrength = 0f;
            targetCamera.transform.position = position;
        }

        public void Initialize(Camera camera)
        {
            targetCamera = camera;
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = 6.45f;
            targetCamera.backgroundColor = new Color(0.018f, 0.045f, 0.09f, 1f);
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.transform.rotation = Quaternion.identity;
        }

        public void Snap(Vector2 focus)
        {
            targetY = Mathf.Max(0f, focus.y + 2.25f);
            targetX = focus.x * 0.12f;
            basePosition = new Vector3(targetX, targetY, -10f);
            targetCamera.transform.position = basePosition;
            velocity = Vector3.zero;
            impactShakeRemaining = 0f;
            flightShakeStrength = 0f;
        }

        public void SetTarget(Vector2 playerPosition, Vector2 anchorPosition)
        {
            if (GamePreferences.FixedCamera)
            {
                // Stable mode keeps the playfield readable: vertical tracking remains
                // essential in an endless game, while lateral movement and shake are removed.
                float stableY = Mathf.Max(0f, playerPosition.y + 2.15f);
                targetY = Mathf.MoveTowards(targetY, stableY, 4.25f * Time.deltaTime);
                targetX = Mathf.MoveTowards(targetX, 0f, 2.5f * Time.deltaTime);
                return;
            }
            float desiredY = Mathf.Max(0f, Mathf.Max(playerPosition.y, anchorPosition.y) + 2.15f);
            targetY = desiredY >= targetY ? desiredY : Mathf.MoveTowards(targetY, desiredY, 5.5f * Time.deltaTime);
            targetX = Mathf.Lerp(playerPosition.x, anchorPosition.x, 0.65f) * 0.12f;
        }

        public void SetCinematicPosition(Vector2 position)
        {
            targetX = position.x;
            targetY = position.y;
            basePosition = new Vector3(targetX, targetY, -10f);
            targetCamera.transform.position = basePosition;
            velocity = Vector3.zero;
            impactShakeRemaining = flightShakeStrength = 0f;
        }

        public void ShakeCapture()
        {
            if (!GamePreferences.FixedCamera && GamePreferences.CaptureShake) TriggerImpactShake(0.13f, 0.065f);
        }

        public void ShakeExplosion()
        {
            if (!GamePreferences.FixedCamera && GamePreferences.ExplosionShake) TriggerImpactShake(0.38f, 0.19f);
        }

        public void SetFlightShake(float danger01, bool flying)
        {
            float desired = !GamePreferences.FixedCamera && GamePreferences.FlightShake && flying
                ? Mathf.InverseLerp(0.42f, 1f, danger01) * 0.045f
                : 0f;
            flightShakeStrength = Mathf.MoveTowards(flightShakeStrength, desired, Time.unscaledDeltaTime * 0.12f);
        }

        private void TriggerImpactShake(float duration, float strength)
        {
            impactShakeDuration = duration;
            impactShakeRemaining = duration;
            impactShakeStrength = strength;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            Vector3 destination = new Vector3(targetX, targetY, -10f);
            float smoothTime = GamePreferences.FixedCamera ? 0.42f : 0.28f;
            basePosition = Vector3.SmoothDamp(basePosition, destination, ref velocity, smoothTime, 18f, Time.unscaledDeltaTime);
            float impact = 0f;
            if (impactShakeRemaining > 0f)
            {
                impactShakeRemaining = Mathf.Max(0f, impactShakeRemaining - Time.unscaledDeltaTime);
                impact = impactShakeStrength * (impactShakeRemaining / Mathf.Max(0.01f, impactShakeDuration));
            }

            float strength = GamePreferences.FixedCamera ? 0f : impact + flightShakeStrength;
            float time = Time.unscaledTime;
            Vector3 shakeOffset = new Vector3(
                (Mathf.PerlinNoise(time * 31f, 2.7f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(7.1f, time * 37f) - 0.5f) * 2f,
                0f) * strength;
            targetCamera.transform.position = basePosition + shakeOffset;
        }
    }
}
