using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class DailyCourseTests
    {
        [Test]
        public void DailyAttempt_CanOnlyBeginOnceAndPersists()
        {
            const int dayKey = 19020317;
            string key = "OrbitBreaker.Daily.Attempted." + dayKey;
            const string countKey = "OrbitBreaker.Daily.AttemptedDays";
            int previousCount = PlayerPrefs.GetInt(countKey, 0);
            int expectedBase = Math.Max(previousCount, DailyCourse.CompletedDays);
            PlayerPrefs.DeleteKey(key);
            try
            {
                Assert.That(DailyCourse.IsAttempted(dayKey), Is.False);
                Assert.That(DailyCourse.TryBeginAttempt(dayKey), Is.True);
                Assert.That(DailyCourse.IsAttempted(dayKey), Is.True);
                Assert.That(DailyCourse.TryBeginAttempt(dayKey), Is.False);
                Assert.That(DailyCourse.AttemptedDays, Is.EqualTo(expectedBase + 1));
            }
            finally { PlayerPrefs.DeleteKey(key); PlayerPrefs.SetInt(countKey, previousCount); }
        }

        [Test]
        public void LeaderboardRows_OnlyExposeTheSelectedMode()
        {
            var entry = new OrbitLeaderboardEntry(4, "Nova", 321, 999, 123, false);
            string endless = OnlineLeaderboard.FormatRow(entry, RunMode.Endless);
            string sprint = OnlineLeaderboard.FormatRow(entry, RunMode.Sprint);
            Assert.That(endless, Does.Contain("321 UA").And.Not.Contain("90 S").And.Not.Contain("\n"));
            Assert.That(sprint, Does.Contain("321 UA").And.Not.Contain("\n"));
        }

        [Test]
        public void DailyDefinition_IsStableAndUsesAllFiveBoundedTiers()
        {
            var tiers = new HashSet<int>();
            int[] difficulties = { 240, 380, 520, 680, 850 };
            int[] rewards = { 90, 130, 180, 240, 320 };
            for (int day = 0; day < 120; day++)
            {
                DateTime date = new DateTime(2026, 1, 1).AddDays(day);
                DailyCourseDefinition morning = DailyCourse.ForDate(date);
                DailyCourseDefinition evening = DailyCourse.ForDate(date.AddHours(23));
                Assert.That(evening.Seed, Is.EqualTo(morning.Seed));
                Assert.That(evening.Tier, Is.EqualTo(morning.Tier));
                Assert.That(morning.Tier, Is.InRange(1, 5));
                Assert.That(morning.RequiredCaptures, Is.EqualTo(10 + morning.Tier * 2));
                Assert.That(morning.DifficultyDistance, Is.EqualTo(difficulties[morning.Tier - 1]));
                Assert.That(morning.MaterialReward, Is.EqualTo(rewards[morning.Tier - 1]));
                tiers.Add(morning.Tier);
            }
            Assert.That(tiers.Count, Is.EqualTo(5));
        }

        [Test]
        public void DailyProgress_IgnoresRevisitsAndCountsASkipAsOneCapture()
        {
            var progress = new DailyCourseProgress(DailyCourse.ForDate(new DateTime(2026, 1, 1)));
            Assert.That(progress.RegisterCapture(0), Is.False);
            Assert.That(progress.RegisterCapture(-1), Is.False);
            Assert.That(progress.RegisterCapture(7), Is.True);
            Assert.That(progress.RegisterCapture(7), Is.False);
            Assert.That(progress.Captures, Is.EqualTo(1));
            for (int i = 1; i < progress.Definition.RequiredCaptures; i++)
                Assert.That(progress.RegisterCapture(7 + i * 3), Is.True);
            Assert.That(progress.IsComplete, Is.True);
            Assert.That(progress.RegisterCapture(1000), Is.False);
            Assert.That(progress.Captures, Is.EqualTo(progress.Definition.RequiredCaptures));
        }

        [TestCase(2, DailyCourse.SolarRocketId)]
        [TestCase(6, DailyCourse.CrownRocketId)]
        [TestCase(13, DailyCourse.EclipseRocketId)]
        public void DailyReward_IsClaimedOnlyOnceAndUnlocksMilestone(int completedBefore, string exclusive)
        {
            var definition = DailyCourse.ForDate(new DateTime(1902, 6, 15));
            string[] keys = {
                "OrbitBreaker.Meta.Materials", "OrbitBreaker.Daily.CompletedDays", "OrbitBreaker.Daily.Claimed." + definition.DayKey,
                "OrbitBreaker.Meta.Owned." + DailyCourse.SolarRocketId, "OrbitBreaker.Meta.Owned." + DailyCourse.CrownRocketId,
                "OrbitBreaker.Meta.Owned." + DailyCourse.EclipseRocketId
            };
            MetaProgression.FlushCollectedMaterials(true);
            var previous = new Dictionary<string, int?>();
            foreach (string key in keys) previous[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            try
            {
                PlayerPrefs.SetInt(keys[0], 100);
                PlayerPrefs.SetInt(keys[1], completedBefore);
                PlayerPrefs.DeleteKey(keys[2]);
                for (int i = 3; i < keys.Length; i++) PlayerPrefs.DeleteKey(keys[i]);
                var progress = new DailyCourseProgress(definition);
                Assert.That(DailyCourse.TryClaim(progress, out _), Is.False);
                for (int i = 1; i <= definition.RequiredCaptures; i++) progress.RegisterCapture(i);
                Assert.That(DailyCourse.TryClaim(progress, out DailyCourseReward reward), Is.True);
                Assert.That(reward.Materials, Is.EqualTo(definition.MaterialReward));
                Assert.That(reward.UnlockedRocketId, Is.EqualTo(exclusive));
                Assert.That(DailyCourse.CompletedDays, Is.EqualTo(completedBefore + 1));
                Assert.That(PlayerPrefs.GetInt("OrbitBreaker.Meta.Owned." + exclusive), Is.EqualTo(1));
                Assert.That(DailyCourse.TryClaim(progress, out _), Is.False);
                Assert.That(MetaProgression.Materials, Is.EqualTo(100 + definition.MaterialReward));
                // A newly reconstructed run on the same date still cannot claim again.
                var retry = new DailyCourseProgress(definition);
                for (int i = 1; i <= definition.RequiredCaptures; i++) retry.RegisterCapture(i);
                Assert.That(DailyCourse.TryClaim(retry, out _), Is.False);
            }
            finally
            {
                foreach (var item in previous)
                    if (item.Value.HasValue) PlayerPrefs.SetInt(item.Key, item.Value.Value); else PlayerPrefs.DeleteKey(item.Key);
                PlayerPrefs.Save();
            }
        }
    }
}
