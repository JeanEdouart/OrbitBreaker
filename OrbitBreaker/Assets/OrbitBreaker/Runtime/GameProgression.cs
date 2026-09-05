using System;
using UnityEngine;

namespace OrbitBreaker
{
    public enum DailyMissionType { Distance, Synchronizations, NearMisses }

    public static class GameProgression
    {
        private const string Prefix = "OrbitBreaker.Progress.";
        private static readonly int[] UnlockDistances = { 0, 500, 1800, 4500 };

        public static int LifetimeDistance => PlayerPrefs.GetInt(Prefix + "LifetimeDistance", 0);
        public static int Runs => PlayerPrefs.GetInt(Prefix + "Runs", 0);
        public static int SelectedStyle => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Style", 0), 0, UnlockedStyleCount - 1);
        public static int UnlockedStyleCount
        {
            get
            {
                int count = 1;
                for (int i = 1; i < UnlockDistances.Length; i++) if (LifetimeDistance >= UnlockDistances[i]) count++;
                return count;
            }
        }

        public static DailyMissionType MissionType => (DailyMissionType)(TodayId() % 3);
        public static int MissionTarget => MissionType == DailyMissionType.Distance ? 700 : MissionType == DailyMissionType.Synchronizations ? 5 : 3;
        public static int MissionProgress
        {
            get { EnsureMissionDate(); return PlayerPrefs.GetInt(Prefix + "MissionProgress", 0); }
        }

        public static string MissionLabel
        {
            get
            {
                switch (MissionType)
                {
                    case DailyMissionType.Synchronizations: return "MISSION  SYNCHRONISATIONS";
                    case DailyMissionType.NearMisses: return "MISSION  FRÔLEMENTS";
                    default: return "MISSION  DISTANCE";
                }
            }
        }

        public static void RecordRun(int distance, int synchronizations, int nearMisses)
        {
            EnsureMissionDate();
            PlayerPrefs.SetInt(Prefix + "LifetimeDistance", LifetimeDistance + Mathf.Max(0, distance));
            PlayerPrefs.SetInt(Prefix + "Runs", Runs + 1);
            int contribution = MissionType == DailyMissionType.Distance ? distance
                : MissionType == DailyMissionType.Synchronizations ? synchronizations : nearMisses;
            PlayerPrefs.SetInt(Prefix + "MissionProgress", Mathf.Min(MissionTarget, MissionProgress + Mathf.Max(0, contribution)));
            PlayerPrefs.Save();
        }

        public static int CycleStyle()
        {
            int next = (SelectedStyle + 1) % UnlockedStyleCount;
            PlayerPrefs.SetInt(Prefix + "Style", next);
            PlayerPrefs.Save();
            return next;
        }

        public static bool SelectStyle(int style)
        {
            if (style < 0 || style >= UnlockedStyleCount) return false;
            PlayerPrefs.SetInt(Prefix + "Style", style);
            PlayerPrefs.Save();
            return true;
        }

        public static int UnlockDistanceForStyle(int style)
        {
            return UnlockDistances[Mathf.Clamp(style, 0, UnlockDistances.Length - 1)];
        }

        public static Color TrailColor(int style)
        {
            Color[] colors = { new Color(0.25f, 0.9f, 1f), new Color(1f, 0.33f, 0.65f), new Color(0.54f, 1f, 0.4f), new Color(1f, 0.72f, 0.18f) };
            return colors[Mathf.Clamp(style, 0, colors.Length - 1)];
        }

        public static string StyleName(int style)
        {
            string[] names = { "CYAN", "NOVA", "ION", "SOLAIRE" };
            return names[Mathf.Clamp(style, 0, names.Length - 1)];
        }

        private static int TodayId() => (int)(DateTime.UtcNow.Date - new DateTime(2025, 1, 1)).TotalDays;

        private static void EnsureMissionDate()
        {
            int today = TodayId();
            if (PlayerPrefs.GetInt(Prefix + "MissionDate", -1) == today) return;
            PlayerPrefs.SetInt(Prefix + "MissionDate", today);
            PlayerPrefs.SetInt(Prefix + "MissionProgress", 0);
        }
    }
}
