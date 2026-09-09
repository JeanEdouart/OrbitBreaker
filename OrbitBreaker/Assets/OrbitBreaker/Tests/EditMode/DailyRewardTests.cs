using System;
using System.Reflection;
using NUnit.Framework;

namespace OrbitBreaker.Tests
{
    public sealed class DailyRewardTests
    {
        [TestCase(0.00, DailyChestRarity.Green)]
        [TestCase(0.5999, DailyChestRarity.Green)]
        [TestCase(0.60, DailyChestRarity.Blue)]
        [TestCase(0.8399, DailyChestRarity.Blue)]
        [TestCase(0.84, DailyChestRarity.Violet)]
        [TestCase(0.9499, DailyChestRarity.Violet)]
        [TestCase(0.95, DailyChestRarity.Gold)]
        [TestCase(0.9919, DailyChestRarity.Gold)]
        [TestCase(0.992, DailyChestRarity.Rainbow)]
        public void RarityThresholds_AreStable(double roll, DailyChestRarity expected)
        {
            Assert.That(DailyRewards.RarityFromRoll(roll), Is.EqualTo(expected));
        }

        [Test]
        public void GeneratedRewards_RespectEveryTierRangeAndExclusiveRule()
        {
            MethodInfo generate = typeof(DailyRewards).GetMethod("Generate", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(generate, Is.Not.Null);
            var seen = new bool[5];
            for (int seed = 0; seed < 20000; seed++)
            {
                var reward = (DailyRewardResult)generate.Invoke(null, new object[] { new Random(seed) });
                seen[(int)reward.Rarity] = true;
                switch (reward.Rarity)
                {
                    case DailyChestRarity.Green: AssertRange(reward, 0, 500, 1, 3); break;
                    case DailyChestRarity.Blue: AssertRange(reward, 500, 1000, 3, 5); break;
                    case DailyChestRarity.Violet: AssertRange(reward, 1000, 1500, 5, 10); break;
                    case DailyChestRarity.Gold: AssertRange(reward, 1500, 3000, 10, 25); break;
                    case DailyChestRarity.Rainbow:
                        AssertRange(reward, 3000, 5000, 25, 25);
                        if(reward.Kind==DailyRewardKind.Gadgets)for (int i = 0; i < reward.Gadgets.Length; i++) Assert.That(reward.Gadgets[i], Is.EqualTo(5));
                        break;
                }
                if (reward.Cosmetic.HasValue)
                    Assert.That(DailyCourse.IsExclusiveRocket(reward.Cosmetic.Value.Id), Is.False);
            }
            Assert.That(seen, Is.All.True, "The deterministic sample should exercise all five rarity branches.");
        }

        private static void AssertRange(DailyRewardResult reward, int minMaterials, int maxMaterials, int minGadgets, int maxGadgets)
        {
            int categories=(reward.Materials>0?1:0)+(reward.TotalGadgets>0?1:0)+(reward.Cosmetic.HasValue||reward.CosmeticCompensation>0?1:0);
            Assert.That(categories,Is.EqualTo(1),"One chest must award exactly one category.");
            if(reward.Kind==DailyRewardKind.Materials)Assert.That(reward.Materials, Is.InRange(Math.Max(1,minMaterials), maxMaterials));
            if(reward.Kind==DailyRewardKind.Gadgets)Assert.That(reward.TotalGadgets, Is.InRange(minGadgets, maxGadgets));
        }
    }
}

