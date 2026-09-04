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
