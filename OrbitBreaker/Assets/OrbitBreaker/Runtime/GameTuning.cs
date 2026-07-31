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
        public const float DeathDistanceBelowCamera = 7.2f;
        public const int AnchorsAhead = 9;

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

        public static int PointsForCapture(float normalizedAccuracy, int combo)
        {
            int precisionBonus = normalizedAccuracy <= 0.22f ? 25 : normalizedAccuracy <= 0.55f ? 10 : 0;
            return 100 + precisionBonus + Mathf.Clamp(combo, 0, 10) * 5;
        }
    }
}
