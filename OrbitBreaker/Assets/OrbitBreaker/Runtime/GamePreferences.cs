using UnityEngine;

namespace OrbitBreaker
{
    public static class GamePreferences
    {
        private const string Prefix = "OrbitBreaker.Settings.";
        private static bool loaded;
        private static bool rotationGuides;
        private static bool orbitRings;
        private static bool flightGauges;
        private static bool shield;
        private static bool haptics;
        private static bool dynamicBackground;
        private static bool enhancedEffects;
        private static bool target60Fps;
        private static bool captureShake;
        private static bool explosionShake;
        private static bool flightShake;
        private static bool fixedCamera;
        private static int targetFrameRate;

        public static bool RotationGuides { get { EnsureLoaded(); return rotationGuides; } }
        public static bool OrbitRings { get { EnsureLoaded(); return orbitRings; } }
        public static bool FlightGauges { get { EnsureLoaded(); return flightGauges; } }
        public static bool Shield { get { EnsureLoaded(); return shield; } }
        public static bool Haptics { get { EnsureLoaded(); return haptics; } }
        public static bool DynamicBackground { get { EnsureLoaded(); return dynamicBackground; } }
        public static bool EnhancedEffects { get { EnsureLoaded(); return enhancedEffects; } }
        public static bool Target60Fps { get { EnsureLoaded(); return target60Fps; } }
        public static bool CaptureShake { get { EnsureLoaded(); return captureShake; } }
        public static bool ExplosionShake { get { EnsureLoaded(); return explosionShake; } }
        public static bool FlightShake { get { EnsureLoaded(); return flightShake; } }
        public static bool FixedCamera { get { EnsureLoaded(); return fixedCamera; } }
        public static int TargetFrameRate { get { EnsureLoaded(); return targetFrameRate; } }

        public static void ApplyRuntime()
        {
            EnsureLoaded();
            Application.targetFrameRate = targetFrameRate;
        }

        public static void SetRotationGuides(bool value) => Save(ref rotationGuides, "RotationGuides", value);
        public static void SetOrbitRings(bool value) => Save(ref orbitRings, "OrbitRings", value);
        public static void SetFlightGauges(bool value) => Save(ref flightGauges, "FlightGauges", value);
        public static void SetShield(bool value) => Save(ref shield, "Shield", value);
        public static void SetHaptics(bool value) => Save(ref haptics, "Haptics", value);
        public static void SetDynamicBackground(bool value) => Save(ref dynamicBackground, "DynamicBackground", value);
        public static void SetEnhancedEffects(bool value) => Save(ref enhancedEffects, "EnhancedEffects", value);
        public static void SetCaptureShake(bool value) => Save(ref captureShake, "CaptureShake", value);
        public static void SetExplosionShake(bool value) => Save(ref explosionShake, "ExplosionShake", value);
        public static void SetFlightShake(bool value) => Save(ref flightShake, "FlightShake", value);
        public static void SetFixedCamera(bool value) => Save(ref fixedCamera, "FixedCamera", value);

        public static void SetTargetFrameRate(int value)
        {
            EnsureLoaded();
            targetFrameRate = value >= 120 ? 120 : value >= 60 ? 60 : 30;
            target60Fps = targetFrameRate >= 60;
            PlayerPrefs.SetInt(Prefix + "TargetFrameRate", targetFrameRate);
            PlayerPrefs.SetInt(Prefix + "Target60Fps", target60Fps ? 1 : 0);
            PlayerPrefs.Save();
            Application.targetFrameRate = targetFrameRate;
        }

        public static void SetTarget60Fps(bool value)
        {
            Save(ref target60Fps, "Target60Fps", value);
            Application.targetFrameRate = value ? 60 : 30;
        }

        private static void EnsureLoaded()
        {
            if (loaded) return;
            rotationGuides = Load("RotationGuides", true);
            orbitRings = Load("OrbitRings", true);
            flightGauges = Load("FlightGauges", true);
            shield = Load("Shield", true);
            haptics = Load("Haptics", true);
            dynamicBackground = Load("DynamicBackground", true);
            enhancedEffects = Load("EnhancedEffects", true);
            target60Fps = Load("Target60Fps", true);
            captureShake = Load("CaptureShake", true);
            explosionShake = Load("ExplosionShake", true);
            flightShake = Load("FlightShake", true);
            fixedCamera = Load("FixedCamera", false);
            targetFrameRate = PlayerPrefs.HasKey(Prefix + "TargetFrameRate")
                ? Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "TargetFrameRate"), 30, 120)
                : target60Fps ? 60 : 30;
            loaded = true;
        }

        private static bool Load(string key, bool defaultValue) => PlayerPrefs.GetInt(Prefix + key, defaultValue ? 1 : 0) != 0;

        private static void Save(ref bool field, string key, bool value)
        {
            EnsureLoaded();
            field = value;
            PlayerPrefs.SetInt(Prefix + key, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
