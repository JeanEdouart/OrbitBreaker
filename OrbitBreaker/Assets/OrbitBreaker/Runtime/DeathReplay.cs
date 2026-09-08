using System;
using UnityEngine;

namespace OrbitBreaker
{
    /// <summary>A hazard's or free debris's position/rotation at one recorded replay instant,
    /// keyed by its stable Id (OrbitHazard.Sequence or FreeDebris.Id) so the replay can find the
    /// same object across frames even though the live GameObject list order can shift.</summary>
    public readonly struct EntityReplaySnapshot
    {
        public EntityReplaySnapshot(int id, Vector2 position, float rotation)
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public int Id { get; }
        public Vector2 Position { get; }
        public float Rotation { get; }
    }

    /// <summary>A compact visual sample used by the local five-second death replay.</summary>
    public readonly struct DeathReplayFrame
    {
        public DeathReplayFrame(float time, Vector3 playerPosition, Quaternion playerRotation, Vector3 playerScale,
            Vector3 cameraPosition, bool bodyVisible, bool engineVisible, bool shieldVisible, float fuel, float warpIntensity,
            EntityReplaySnapshot[] hazardSnapshots, EntityReplaySnapshot[] debrisSnapshots)
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
            WarpIntensity = warpIntensity;
            HazardSnapshots = hazardSnapshots;
            DebrisSnapshots = debrisSnapshots;
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
        // 0 outside a wormhole warp; >0 mirrors the live hyperspace intensity so the death
        // replay can reproduce the tunnel/veil overlay and warp engine look, not just the ship pose.
        public float WarpIntensity { get; }
        // Where every orbiting hazard / drifting debris really was at this instant, so the replay
        // can move them along their true recorded path instead of leaving them frozen in place.
        public EntityReplaySnapshot[] HazardSnapshots { get; }
        public EntityReplaySnapshot[] DebrisSnapshots { get; }
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
