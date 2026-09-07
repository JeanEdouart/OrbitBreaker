using System;
using UnityEngine;

namespace OrbitBreaker
{
    // Keep numeric identifiers stable for existing mode references; retired Training was 1.
    public enum RunMode { Endless = 0, Daily = 2, Sprint = 3 }

    public static class LocalRunStats
    {
        private const string Prefix = "OrbitBreaker.Stats.";
        public static int CompletedRuns => PlayerPrefs.GetInt(Prefix + "Runs", 0);
        public static float AverageDuration => CompletedRuns == 0 ? 0f : PlayerPrefs.GetFloat(Prefix + "Seconds", 0f) / CompletedRuns;
        public static int BestSkip => PlayerPrefs.GetInt(Prefix + "BestSkip", 0);
        public static int BestChain => PlayerPrefs.GetInt(Prefix + "BestChain", 0);
        public static int Deaths(DeathReason reason) => PlayerPrefs.GetInt(Prefix + "Deaths." + reason, 0);
        public static int DailySeed(DateTime utcDate) => utcDate.Year * 10000 + utcDate.Month * 100 + utcDate.Day;
        private static string BestKey(RunMode mode, int dailySeed) => Prefix + "Best." + mode + (mode == RunMode.Daily ? "." + dailySeed : string.Empty);
        public static int Best(RunMode mode, int dailySeed) => PlayerPrefs.GetInt(BestKey(mode, dailySeed), 0);
        public static void RecordModeBest(RunMode mode, int dailySeed, int score)
        {
            PlayerPrefs.SetInt(BestKey(mode, dailySeed), Mathf.Max(Best(mode, dailySeed), score));
            PlayerPrefs.Save();
        }
        public static void Record(float duration, int bestSkip, int bestChain, DeathReason reason)
        {
            PlayerPrefs.SetInt(Prefix + "Runs", CompletedRuns + 1);
            PlayerPrefs.SetFloat(Prefix + "Seconds", PlayerPrefs.GetFloat(Prefix + "Seconds", 0f) + Mathf.Max(0f, duration));
            PlayerPrefs.SetInt(Prefix + "BestSkip", Mathf.Max(BestSkip, bestSkip));
            PlayerPrefs.SetInt(Prefix + "BestChain", Mathf.Max(BestChain, bestChain));
            PlayerPrefs.SetInt(Prefix + "Deaths." + reason, Deaths(reason) + 1);
            PlayerPrefs.Save();
        }
    }
}
