using System;
using UnityEngine;

namespace OrbitBreaker
{
    /// <summary>A compact visual sample used by the local three-second death replay.</summary>
    public readonly struct DeathReplayFrame
    {
        public DeathReplayFrame(float time, Vector3 playerPosition, Quaternion playerRotation, Vector3 playerScale,
            Vector3 cameraPosition, bool bodyVisible, bool engineVisible, bool shieldVisible, float fuel)
        {
            Time = time;
            PlayerPosition = playerPosition;
            PlayerRotation = playerRotation;
            PlayerScale = playerScale;
            CameraPosition = cameraPosition;
            BodyVisible = bodyVisible;
            EngineVisible = engineVisible;
            ShieldVisible = shieldVisible;
            Fuel = fuel;
        }

        public float Time { get; }
        public Vector3 PlayerPosition { get; }
        public Quaternion PlayerRotation { get; }
        public Vector3 PlayerScale { get; }
        public Vector3 CameraPosition { get; }
        public bool BodyVisible { get; }
        public bool EngineVisible { get; }
        public bool ShieldVisible { get; }
        public float Fuel { get; }
    }

    /// <summary>Stable Europe/Paris civil-date conversion, including EU daylight-saving boundaries.</summary>
    public static class FrenchGameClock
    {
        public static DateTime ParisDate(DateTime utc)
        {
            DateTime normalized = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return normalized.AddHours(IsSummerTimeUtc(normalized) ? 2 : 1).Date;
        }

        public static int ParisDayKey(DateTime utc) => LocalRunStats.DailySeed(ParisDate(utc));

        private static bool IsSummerTimeUtc(DateTime utc)
        {
            DateTime start = LastSundayUtc(utc.Year, 3);
            DateTime end = LastSundayUtc(utc.Year, 10);
            return utc >= start && utc < end;
        }

        private static DateTime LastSundayUtc(int year, int month)
        {
            DateTime last = new DateTime(year, month, DateTime.DaysInMonth(year, month), 1, 0, 0, DateTimeKind.Utc);
            return last.AddDays(-(int)last.DayOfWeek);
        }
    }
}
