using UnityEngine;

namespace OrbitBreaker
{
    public static class GameTuning
    {
        public const float BaseAngularSpeed = 125f;
        public const float MaxAngularSpeed = 230f;
        public const float BaseLaunchSpeed = 7.4f;
        public const float MaxLaunchSpeed = 10.2f;
        public const float CaptureBand = 0.34f;
        public const float MaxFlightTime = 2.65f;
        public const float HorizontalLimit = 5.4f;
        public const float DeathDistanceBelowCamera = 12.5f;
        public const int AnchorsAhead = 9;
        public const int BackwardOrbitRetention = 8;
        public const int HazardIntroductionSequence = 7;
        public const float StartingHeight = -2.1f;
        public const float MultiplierStepDuration = 0.12f;
        public const float MaxDistanceMultiplier = 6f;
        public const int GenerationAttempts = 14;
        public const int ReachabilitySamples = 72;
        public const float FlightTimeReserve = 0.32f;
        public const float PlayerCollisionRadius = 0.17f;

        public static float Difficulty01(int score)
        {
            return 1f - Mathf.Exp(-Mathf.Max(0, score) / 22f);
        }

        public static float AngularSpeed(int score)
        {
            return Mathf.Lerp(BaseAngularSpeed, MaxAngularSpeed, Difficulty01(score));
        }

        public static float LaunchSpeed(int score)
        {
            return Mathf.Lerp(BaseLaunchSpeed, MaxLaunchSpeed, Difficulty01(score));
        }

        public static float AnchorGap(int score, float random01)
        {
            float minimum = Mathf.Lerp(3.1f, 3.65f, Difficulty01(score));
            float maximum = Mathf.Lerp(3.75f, 4.45f, Difficulty01(score));
            return Mathf.Lerp(minimum, maximum, Mathf.Clamp01(random01));
        }

        public static bool HasHazard(int sequence)
        {
            if (sequence < HazardIntroductionSequence) return false;
            if (sequence < 18) return sequence % 3 == 1;
            if (sequence < 34) return sequence % 2 == 0;
            return true;
        }

        public static float HazardCollisionRadius(int sequence)
        {
            return Mathf.Lerp(0.2f, 0.32f, Difficulty01(sequence - HazardIntroductionSequence));
        }

        public static float CaptureGraceDuration(int sequence)
        {
            return Mathf.Lerp(1.05f, 0.58f, Difficulty01(sequence));
        }

        public static int MinimumReachableLaunchSamples(int sequence)
        {
            return Mathf.RoundToInt(Mathf.Lerp(9f, 5f, Difficulty01(sequence)));
        }

        public static int CountReachableLaunchSamples(
            Vector2 fromCenter,
            float fromRadius,
            int fromDirection,
            Vector2 targetCenter,
            float targetRadius,
            int sequence)
        {
            int reachable = 0;
            float speed = LaunchSpeed(sequence);
            float maximumTravel = speed * Mathf.Max(0.1f, MaxFlightTime - FlightTimeReserve);
            float captureRadius = targetRadius + CaptureBand;

            for (int sample = 0; sample < ReachabilitySamples; sample++)
            {
                float angle = sample * Mathf.PI * 2f / ReachabilitySamples;
                Vector2 radial = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 origin = fromCenter + radial * fromRadius;
                Vector2 direction = fromDirection > 0
                    ? new Vector2(-radial.y, radial.x)
                    : new Vector2(radial.y, -radial.x);

                Vector2 toOrigin = origin - targetCenter;
                float b = Vector2.Dot(toOrigin, direction);
                float c = toOrigin.sqrMagnitude - captureRadius * captureRadius;
                float discriminant = b * b - c;
                if (discriminant < 0f) continue;

                float travel = -b - Mathf.Sqrt(discriminant);
                if (travel < 0f) travel = -b + Mathf.Sqrt(discriminant);
                if (travel < 0f || travel > maximumTravel) continue;

                Vector2 capturePoint = origin + direction * travel;
                if (Mathf.Abs(capturePoint.x) <= HorizontalLimit - PlayerCollisionRadius) reachable++;
            }

            return reachable;
        }

        public static bool IsAnchorReachable(
            Vector2 fromCenter,
            float fromRadius,
            int fromDirection,
            Vector2 targetCenter,
            float targetRadius,
            int sequence)
        {
            return CountReachableLaunchSamples(fromCenter, fromRadius, fromDirection, targetCenter, targetRadius, sequence)
                   >= MinimumReachableLaunchSamples(sequence);
        }

        public static bool CanAddHazardToLayout(int reachableSamples, int sequence)
        {
            // A dangerous landing must retain a wider timing window than an empty orbit.
            return reachableSamples >= MinimumReachableLaunchSamples(sequence) + 3;
        }

        public static float FlightMultiplier(float flightTime)
        {
            float steps = Mathf.Floor(Mathf.Max(0f, flightTime) / MultiplierStepDuration);
            return Mathf.Min(MaxDistanceMultiplier, 1f + steps * 0.1f);
        }

        public static float FlightDanger01(float flightTime)
        {
            return Mathf.Clamp01(Mathf.Max(0f, flightTime) / MaxFlightTime);
        }

        public static int BankedDistance(float startHeight, float endHeight, float multiplier)
        {
            float climbed = Mathf.Max(0f, endHeight - startHeight);
            return Mathf.Max(0, Mathf.RoundToInt(climbed * Mathf.Max(1f, multiplier)));
        }
    }
}
