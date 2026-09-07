using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class TrajectoryChallengeTests
    {
        [Test]
        public void ExpandedChallenges_KeepEveryLegacyIdStable()
        {
            for (int id = 0; id < 100; id++)
            {
                ChallengeDefinition challenge = MetaProgression.Challenge(id);
                Assert.That(challenge.Id, Is.EqualTo(id));
                Assert.That((int)challenge.Kind, Is.EqualTo(id % 8));
                Assert.That(challenge.Reward, Is.EqualTo(35 + id / 8 * 10 + id % 8 * 3));
            }
            var kinds = new HashSet<ChallengeKind>();
            for (int id = 100; id < 112; id++)
            {
                ChallengeDefinition challenge = MetaProgression.Challenge(id);
                Assert.That(challenge.Id, Is.EqualTo(id));
                Assert.That(challenge.Target, Is.GreaterThan(0));
                kinds.Add(challenge.Kind);
            }
            Assert.That(kinds.Count, Is.EqualTo(3));
        }

        [Test]
        public void SingleRunChallenges_UseBestAttemptAndProjectWithoutDoubleCounting()
        {
            const string prefix = "OrbitBreaker.Meta.";
            var old = new Dictionary<string, int?>();
            for (int slot = 0; slot < 3; slot++)
                foreach (string field in new[] { "Challenge.", "Progress.", "Claimed." })
                {
                    string key = prefix + field + slot;
                    old[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
                }
            try
            {
                for (int slot = 0; slot < 3; slot++)
                {
                    PlayerPrefs.SetInt(prefix + "Challenge." + slot, 109 + slot);
                    PlayerPrefs.SetInt(prefix + "Progress." + slot, 0);
                    PlayerPrefs.SetInt(prefix + "Claimed." + slot, 0);
                }
                MetaProgression.RecordRun(0, 0, 0, 0, 0, 20, 1f, 2, 1);
                MetaProgression.RecordRun(0, 0, 0, 0, 0, 15, 1f, 1, 1);
                Assert.That(MetaProgression.ChallengeProgress(0), Is.EqualTo(20));
                Assert.That(MetaProgression.ChallengeProgress(1), Is.EqualTo(2));
                Assert.That(MetaProgression.ChallengeProgress(2), Is.EqualTo(1));
                Assert.That(MetaProgression.ProjectedProgress(0, 0, 0, 0, 0, 0, 30, 1f, 3, 2), Is.EqualTo(30));
                Assert.That(MetaProgression.ProjectedProgress(1, 0, 0, 0, 0, 0, 30, 1f, 3, 2), Is.EqualTo(3));
                Assert.That(MetaProgression.ProjectedProgress(2, 0, 0, 0, 0, 0, 30, 1f, 3, 2), Is.EqualTo(2));
                MetaProgression.RecordRun(0, 0, 0, 0, 0, 30, 1f, 3, 2);
                Assert.That(MetaProgression.ChallengeProgress(0), Is.EqualTo(30));
                Assert.That(MetaProgression.ChallengeProgress(1), Is.EqualTo(3));
                Assert.That(MetaProgression.ChallengeProgress(2), Is.EqualTo(2));
            }
            finally
            {
                foreach (var entry in old)
                    if (entry.Value.HasValue) PlayerPrefs.SetInt(entry.Key, entry.Value.Value); else PlayerPrefs.DeleteKey(entry.Key);
                PlayerPrefs.Save();
            }
        }
    }
}
