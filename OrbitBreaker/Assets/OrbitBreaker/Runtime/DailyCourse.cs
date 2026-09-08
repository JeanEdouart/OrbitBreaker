using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public readonly struct DailyCourseDefinition
    {
        public int DayKey { get; }
        public int Seed { get; }
        public int Tier { get; }
        public int RequiredCaptures { get; }
        public int DifficultyDistance { get; }
        public int MaterialReward { get; }

        internal DailyCourseDefinition(int dayKey, int tier)
        {
            DayKey = dayKey;
            Seed = dayKey;
            Tier = tier;
            RequiredCaptures = 10 + tier * 2;
            DifficultyDistance = tier switch { 1 => 240, 2 => 380, 3 => 520, 4 => 680, _ => 850 };
            MaterialReward = tier switch { 1 => 90, 2 => 130, 3 => 180, 4 => 240, _ => 320 };
        }
    }

    /// <summary>Counts actual unique captures, not sequence gaps or restored checkpoints.</summary>
    public sealed class DailyCourseProgress
    {
        private readonly HashSet<int> captured = new HashSet<int>();
        private readonly int startingSequence;
        public DailyCourseDefinition Definition { get; }
        public int Captures => captured.Count;
        public bool IsComplete => Captures >= Definition.RequiredCaptures && Definition.RequiredCaptures > 0;

        public DailyCourseProgress(DailyCourseDefinition definition, int startingSequence = 0)
        {
            if (definition.RequiredCaptures <= 0) throw new ArgumentException("A valid daily course is required.", nameof(definition));
            Definition = definition;
            this.startingSequence = startingSequence;
        }

        public bool RegisterCapture(int sequence)
        {
            if (IsComplete || sequence <= startingSequence) return false;
            return captured.Add(sequence);
        }
    }

    public readonly struct DailyCourseReward
    {
        public int Materials { get; }
        public string UnlockedRocketId { get; }
        public int CompletedDays { get; }
        internal DailyCourseReward(int materials, string unlockedRocketId, int completedDays)
        { Materials = materials; UnlockedRocketId = unlockedRocketId; CompletedDays = completedDays; }
    }

    /// <summary>Local daily completion ledger. Does not create or require a remote leaderboard.</summary>
    public static class DailyCourse
    {
        private const string Prefix = "OrbitBreaker.Daily.";
        private const string WalletKey = "OrbitBreaker.Meta.Materials";
        private const string OwnedPrefix = "OrbitBreaker.Meta.Owned.";
        public const string SolarRocketId = "rocket_daily_solar";
        public const string CrownRocketId = "rocket_daily_crown";
        public const string EclipseRocketId = "rocket_daily_eclipse";
        public static int CompletedDays => Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "CompletedDays", 0));
        public static int AttemptedDays => Mathf.Max(CompletedDays, Mathf.Max(0, PlayerPrefs.GetInt(Prefix + "AttemptedDays", 0)));

        public static DailyCourseDefinition ForDate(DateTime utcInstant)
        {
            // Integer-only mixing is stable between Mono, IL2CPP, WebGL and process launches.
            int dayKey = FrenchGameClock.ParisDayKey(utcInstant);
            uint mixed = unchecked((uint)dayKey * 2654435761u);
            mixed ^= mixed >> 16;
            return new DailyCourseDefinition(dayKey, 1 + (int)(mixed % 5u));
        }

        public static bool IsClaimed(int dayKey) => PlayerPrefs.GetInt(Prefix + "Claimed." + dayKey, 0) == 1;
        public static bool IsAttempted(int dayKey) => PlayerPrefs.GetInt(Prefix + "Attempted." + dayKey, 0) == 1;

        /// <summary>Consumes today's single attempt at the first real launch, not while browsing modes.</summary>
        public static bool TryBeginAttempt(int dayKey)
        {
            if (IsAttempted(dayKey)) return false;
            PlayerPrefs.SetInt(Prefix + "Attempted." + dayKey, 1);
            PlayerPrefs.SetInt(Prefix + "AttemptedDays", (int)Math.Min(int.MaxValue, (long)AttemptedDays + 1));
            PlayerPrefs.Save();
            return true;
        }

        public static bool IsExclusiveRocket(string id) => RequiredDaysForRocket(id) > 0;
        public static int RequiredDaysForRocket(string id) => id switch
        { SolarRocketId => 3, CrownRocketId => 7, EclipseRocketId => 14, _ => 0 };

        public static bool TryClaim(DailyCourseProgress progress, out DailyCourseReward reward)
        {
            reward = default;
            if (progress == null || !progress.IsComplete || IsClaimed(progress.Definition.DayKey)) return false;
            int total = (int)Math.Min(int.MaxValue, (long)CompletedDays + 1);
            int materialReward = progress.Definition.MaterialReward;
            PlayerPrefs.SetInt(WalletKey, (int)Math.Min(int.MaxValue, (long)MetaProgression.Materials + materialReward));
            PlayerPrefs.SetInt(Prefix + "Claimed." + progress.Definition.DayKey, 1);
            PlayerPrefs.SetInt(Prefix + "CompletedDays", total);
            // Existing purchases and equipment are never reset. This new ledger intentionally does
            // not infer completed courses from historical distance records, which prove no captures.
            string unlocked = string.Empty;
            foreach (string id in new[] { SolarRocketId, CrownRocketId, EclipseRocketId })
            {
                if (total < RequiredDaysForRocket(id) || PlayerPrefs.GetInt(OwnedPrefix + id, 0) == 1) continue;
                PlayerPrefs.SetInt(OwnedPrefix + id, 1);
                unlocked = id;
            }
            PlayerPrefs.Save();
            reward = new DailyCourseReward(materialReward, unlocked, total);
            return true;
        }
    }
}
