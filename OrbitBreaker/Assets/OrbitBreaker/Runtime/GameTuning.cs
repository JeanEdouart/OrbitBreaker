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
        public const float NearMissExtraRadius = 0.48f;
        public const float NearMissMultiplierBonus = 0.2f;
        public const float SynchronizationMultiplierBonus = 0.35f;

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

        public static bool TryFindSynchronizationGate(
            Vector2 fromCenter,
            float fromRadius,
            int fromDirection,
            Vector2 targetCenter,
            float targetRadius,
            int targetDirection,
            int sequence,
            out float arrivalAngle,
            out float bestAlignment)
        {
            arrivalAngle = 0f;
            bestAlignment = -1f;
            float maximumTravel = LaunchSpeed(sequence) * Mathf.Max(0.1f, MaxFlightTime - FlightTimeReserve);
            float captureRadius = targetRadius + CaptureBand;

            for (int sample = 0; sample < ReachabilitySamples; sample++)
            {
                float launchAngle = sample * Mathf.PI * 2f / ReachabilitySamples;
                Vector2 launchRadial = new Vector2(Mathf.Cos(launchAngle), Mathf.Sin(launchAngle));
                Vector2 origin = fromCenter + launchRadial * fromRadius;
                Vector2 flightDirection = fromDirection > 0
                    ? new Vector2(-launchRadial.y, launchRadial.x)
                    : new Vector2(launchRadial.y, -launchRadial.x);

                Vector2 toOrigin = origin - targetCenter;
                float b = Vector2.Dot(toOrigin, flightDirection);
                float c = toOrigin.sqrMagnitude - captureRadius * captureRadius;
                float discriminant = b * b - c;
                if (discriminant < 0f) continue;

                float travel = -b - Mathf.Sqrt(discriminant);
                if (travel < 0f) travel = -b + Mathf.Sqrt(discriminant);
                if (travel < 0f || travel > maximumTravel) continue;

                Vector2 capturePoint = origin + flightDirection * travel;
                if (Mathf.Abs(capturePoint.x) > HorizontalLimit - PlayerCollisionRadius) continue;
                Vector2 arrivalRadial = (capturePoint - targetCenter).normalized;
                Vector2 targetTangent = targetDirection > 0
                    ? new Vector2(-arrivalRadial.y, arrivalRadial.x)
                    : new Vector2(arrivalRadial.y, -arrivalRadial.x);
                float alignment = Vector2.Dot(flightDirection, targetTangent);
                if (alignment <= bestAlignment) continue;
                bestAlignment = alignment;
                arrivalAngle = Mathf.Atan2(arrivalRadial.y, arrivalRadial.x);
            }

            return bestAlignment >= SynchronizationAlignment(sequence);
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

        public static bool CanAddSkipChallenge(int reachableSamples, int sequence)
        {
            // A two-orbit jump is intentionally narrower than a normal transfer, but still
            // needs several viable launch timings before a moving challenge can be added.
            int minimumSkipSamples = Mathf.Max(4, MinimumReachableLaunchSamples(sequence) - 2);
            return sequence >= 11 && reachableSamples >= minimumSkipSamples;
        }

        public static bool TryFindSkipChallengePoint(
            Vector2 fromCenter,
            float fromRadius,
            int fromDirection,
            Vector2 bypassedCenter,
            float bypassedRadius,
            Vector2 targetCenter,
            float targetRadius,
            int sequence,
            out Vector2 challengePoint,
            out float bypassClearance)
        {
            challengePoint = Vector2.zero;
            bypassClearance = float.MinValue;
            float maximumTravel = LaunchSpeed(sequence) * Mathf.Max(0.1f, MaxFlightTime - FlightTimeReserve);
            float captureRadius = targetRadius + CaptureBand;
            bool found = false;

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
                if (Mathf.Abs(capturePoint.x) > HorizontalLimit - PlayerCollisionRadius) continue;

                float closestProgress = Mathf.Clamp01(Vector2.Dot(bypassedCenter - origin, direction) / travel);
                float routeClearance = Vector2.Distance(origin + direction * (travel * closestProgress), bypassedCenter) - bypassedRadius;
                if (routeClearance <= CaptureBand + 0.08f) continue;

                Vector2 point = Vector2.zero;
                float clearance = float.MinValue;
                float[] placementProgress = { 0.38f, 0.46f, 0.54f, 0.62f };
                for (int placement = 0; placement < placementProgress.Length; placement++)
                {
                    Vector2 candidatePoint = origin + direction * (travel * placementProgress[placement]);
                    float candidateClearance = Vector2.Distance(candidatePoint, bypassedCenter) - bypassedRadius;
                    if (candidateClearance <= clearance) continue;
                    point = candidatePoint;
                    clearance = candidateClearance;
                }
                float requiredClearance = CaptureBand + 0.46f + HazardCollisionRadius(sequence) + 0.12f;
                float sourceClearance = Vector2.Distance(point, fromCenter) - fromRadius;
                float targetClearance = Vector2.Distance(point, targetCenter) - targetRadius;
                if (clearance < requiredClearance || sourceClearance < requiredClearance || targetClearance < requiredClearance) continue;
                if (clearance <= bypassClearance) continue;
                challengePoint = point;
                bypassClearance = clearance;
                found = true;
            }
            return found;
        }

        public static bool IsBreatherOrbit(int sequence)
        {
            int phase = Mathf.Abs(sequence) % 12;
            return sequence >= HazardIntroductionSequence && phase >= 9;
        }

        public static float PatternHorizontalStep(int sequence, float random01)
        {
            int motif = Mathf.Abs(sequence / 4) % 4;
            float noise = Mathf.Lerp(-0.35f, 0.35f, Mathf.Clamp01(random01));
            if (IsBreatherOrbit(sequence)) return noise * 0.45f;
            switch (motif)
            {
                case 0: return (sequence % 2 == 0 ? 1.35f : -1.35f) + noise;
                case 1: return Mathf.Lerp(-1.75f, 1.75f, Mathf.Repeat(sequence * 0.37f, 1f)) + noise;
                case 2: return noise * 0.65f;
                default: return (sequence % 2 == 0 ? -0.9f : 0.9f) + noise;
            }
        }

        public static float SynchronizationHalfAngle(int sequence)
        {
            return Mathf.Lerp(42f, 27f, Difficulty01(sequence));
        }

        public static float SynchronizationAlignment(int sequence)
        {
            return Mathf.Lerp(0.52f, 0.68f, Difficulty01(sequence));
        }

        public static bool IsSynchronizedCapture(Vector2 radial, Vector2 velocity, int orbitDirection, int sequence, float zoneAngleRadians)
        {
            if (radial.sqrMagnitude < 0.01f || velocity.sqrMagnitude < 0.01f) return false;
            radial.Normalize();
            Vector2 desiredTangent = orbitDirection > 0
                ? new Vector2(-radial.y, radial.x)
                : new Vector2(radial.y, -radial.x);
            float alignment = Vector2.Dot(velocity.normalized, desiredTangent);
            return alignment >= SynchronizationAlignment(sequence) && IsWithinSynchronizationZone(radial, sequence, zoneAngleRadians);
        }

        public static bool IsWithinSynchronizationZone(Vector2 radial, int sequence, float zoneAngleRadians)
        {
            if (radial.sqrMagnitude < 0.01f) return false;
            float captureAngle = Mathf.Atan2(radial.y, radial.x);
            float angleError = Mathf.Abs(Mathf.DeltaAngle(captureAngle * Mathf.Rad2Deg, zoneAngleRadians * Mathf.Rad2Deg));
            return angleError <= SynchronizationHalfAngle(sequence);
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
