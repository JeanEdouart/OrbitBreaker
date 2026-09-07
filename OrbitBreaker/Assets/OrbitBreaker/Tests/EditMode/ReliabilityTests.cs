using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class ReliabilityTests
    {
        [Test]
        public void Upgrade_ChangesLevelAndWalletTogetherAndRejectsInsufficientFunds()
        {
            const string walletKey = "OrbitBreaker.Meta.Materials";
            const string levelKey = "OrbitBreaker.PowerUps.Shield.Level";
            MetaProgression.FlushCollectedMaterials(true);
            bool hadWallet = PlayerPrefs.HasKey(walletKey), hadLevel = PlayerPrefs.HasKey(levelKey);
            int wallet = PlayerPrefs.GetInt(walletKey), level = PlayerPrefs.GetInt(levelKey);
            try
            {
                PlayerPrefs.SetInt(levelKey, 1);
                PlayerPrefs.SetInt(walletKey, PowerUpProgression.Definition(PowerUpType.Shield).UpgradePrice(1));
                Assert.That(PowerUpProgression.Upgrade(PowerUpType.Shield), Is.True);
                Assert.That(PowerUpProgression.Level(PowerUpType.Shield), Is.EqualTo(2));
                Assert.That(MetaProgression.Materials, Is.Zero);
                Assert.That(PowerUpProgression.Upgrade(PowerUpType.Shield), Is.False);
                Assert.That(PowerUpProgression.Level(PowerUpType.Shield), Is.EqualTo(2));
                Assert.That(MetaProgression.Materials, Is.Zero);
            }
            finally
            {
                if (hadWallet) PlayerPrefs.SetInt(walletKey, wallet); else PlayerPrefs.DeleteKey(walletKey);
                if (hadLevel) PlayerPrefs.SetInt(levelKey, level); else PlayerPrefs.DeleteKey(levelKey);
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void BatchedCollection_IsSpendableImmediatelyAndFlushDoesNotReplayIt()
        {
            const string key = "OrbitBreaker.Meta.Materials";
            MetaProgression.FlushCollectedMaterials(true);
            bool existed = PlayerPrefs.HasKey(key);
            int old = PlayerPrefs.GetInt(key);
            try
            {
                PlayerPrefs.SetInt(key, 100);
                MetaProgression.CollectMaterials(5);
                MetaProgression.CollectMaterials(7);
                Assert.That(MetaProgression.Materials, Is.EqualTo(112));
                Assert.That(MetaProgression.TrySpendMaterials(110), Is.True);
                MetaProgression.FlushCollectedMaterials(true);
                MetaProgression.FlushCollectedMaterials(true);
                Assert.That(MetaProgression.Materials, Is.EqualTo(2));
            }
            finally
            {
                MetaProgression.FlushCollectedMaterials(true);
                if (existed) PlayerPrefs.SetInt(key, old); else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void Claim_IsAwardedOnceAndDoesNotRollUntilAllThreeAreClaimed()
        {
            string[] keys = {
                "OrbitBreaker.Meta.Materials", "OrbitBreaker.Meta.Generation",
                "OrbitBreaker.Meta.Challenge.0", "OrbitBreaker.Meta.Progress.0", "OrbitBreaker.Meta.Claimed.0",
                "OrbitBreaker.Meta.Challenge.1", "OrbitBreaker.Meta.Progress.1", "OrbitBreaker.Meta.Claimed.1",
                "OrbitBreaker.Meta.Challenge.2", "OrbitBreaker.Meta.Progress.2", "OrbitBreaker.Meta.Claimed.2"
            };
            var old = new Dictionary<string, int?>();
            foreach (string key in keys) old[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            try
            {
                PlayerPrefs.SetInt(keys[0], 100);
                PlayerPrefs.SetInt(keys[1], 20);
                for (int slot = 0; slot < 3; slot++)
                {
                    PlayerPrefs.SetInt("OrbitBreaker.Meta.Challenge." + slot, slot);
                    PlayerPrefs.SetInt("OrbitBreaker.Meta.Progress." + slot, MetaProgression.Challenge(slot).Target);
                    PlayerPrefs.SetInt("OrbitBreaker.Meta.Claimed." + slot, 0);
                }
                Assert.That(MetaProgression.Claim(0), Is.True);
                Assert.That(MetaProgression.Claim(0), Is.False);
                Assert.That(MetaProgression.Materials, Is.EqualTo(100 + MetaProgression.Challenge(0).Reward));
                Assert.That(PlayerPrefs.GetInt(keys[1]), Is.EqualTo(20));
                Assert.That(MetaProgression.Claim(1), Is.True);
                Assert.That(MetaProgression.Claim(2), Is.True);
                Assert.That(PlayerPrefs.GetInt(keys[1]), Is.EqualTo(21));
                for (int slot = 0; slot < 3; slot++)
                {
                    Assert.That(MetaProgression.ActiveChallengeId(slot), Is.GreaterThan(2));
                    Assert.That(MetaProgression.ChallengeProgress(slot), Is.Zero);
                }
            }
            finally
            {
                foreach (var entry in old)
                    if (entry.Value.HasValue) PlayerPrefs.SetInt(entry.Key, entry.Value.Value); else PlayerPrefs.DeleteKey(entry.Key);
                PlayerPrefs.Save();
            }
        }

        [Test]
        public void DailySeed_IsStableWithinADayAndChangesAtMidnight()
        {
            Assert.That(LocalRunStats.DailySeed(new DateTime(2026, 9, 7, 0, 0, 0)),
                Is.EqualTo(LocalRunStats.DailySeed(new DateTime(2026, 9, 7, 23, 59, 59))));
            Assert.That(LocalRunStats.DailySeed(new DateTime(2026, 9, 8)), Is.Not.EqualTo(LocalRunStats.DailySeed(new DateTime(2026, 9, 7))));
        }

        [Test]
        public void ResetWorld_WithSameSeed_ReproducesOrbitGeometry()
        {
            var instance = new GameObject("Seed regression world");
            try
            {
                var world = instance.AddComponent<OrbitWorld>();
                world.ResetWorld(20260907);
                var positions = new List<Vector3>();
                var directions = new List<int>();
                foreach (OrbitAnchor anchor in world.Anchors) { positions.Add(anchor.transform.position); directions.Add(anchor.Direction); }
                world.ResetWorld(20260907);
                Assert.That(world.Anchors.Count, Is.EqualTo(positions.Count));
                for (int i = 0; i < positions.Count; i++)
                {
                    Assert.That(world.Anchors[i].transform.position, Is.EqualTo(positions[i]));
                    Assert.That(world.Anchors[i].Direction, Is.EqualTo(directions[i]));
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
    }
}
