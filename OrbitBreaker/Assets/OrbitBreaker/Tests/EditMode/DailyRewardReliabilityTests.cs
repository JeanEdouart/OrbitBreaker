using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class DailyRewardReliabilityTests
    {
        private readonly Dictionary<string,string> strings = new Dictionary<string,string>();
        private readonly Dictionary<string,int> ints = new Dictionary<string,int>();
        private readonly HashSet<string> existing = new HashSet<string>();
        private const string Prefix="OrbitBreaker.DailyReward.";
        private static readonly DateTime Now=new DateTime(2026,9,9,12,0,0,DateTimeKind.Utc);
        [SetUp] public void Save()
        {
            foreach(string suffix in new[]{"LastClaimUtcTicks","Pending","Result"})
            {
                string key=Prefix+suffix;strings[key]=PlayerPrefs.GetString(key,"");if(PlayerPrefs.HasKey(key))existing.Add(key);PlayerPrefs.DeleteKey(key);
            }
            BackupInt("OrbitBreaker.Meta.Materials");
            for(int i=0;i<5;i++)BackupInt("OrbitBreaker.PowerUps."+(PowerUpType)i+".Stock");
            foreach(var item in MetaProgression.Catalog)BackupInt("OrbitBreaker.Meta.Owned."+item.Id);
        }
        private void BackupInt(string key){ints[key]=PlayerPrefs.GetInt(key);if(PlayerPrefs.HasKey(key))existing.Add(key);}
        [TearDown] public void Restore()
        {
            foreach(var entry in strings)if(existing.Contains(entry.Key))PlayerPrefs.SetString(entry.Key,entry.Value);else PlayerPrefs.DeleteKey(entry.Key);
            foreach(var entry in ints)if(existing.Contains(entry.Key))PlayerPrefs.SetInt(entry.Key,entry.Value);else PlayerPrefs.DeleteKey(entry.Key);
            PlayerPrefs.Save();strings.Clear();ints.Clear();existing.Clear();
        }
        [Test] public void ClaimPersistsReceiptAndRejectsDoubleTapUntilExact24Hours()
        {
            var reward=DailyRewards.TryClaim(Now);Assert.That(reward,Is.Not.Null);
            int balance=MetaProgression.Materials;
            Assert.That(DailyRewards.TryClaim(Now),Is.Null);Assert.That(MetaProgression.Materials,Is.EqualTo(balance));
            Assert.That(DailyRewards.IsReady(Now.AddHours(24).AddTicks(-1)),Is.False);
            Assert.That(DailyRewards.IsReady(Now.AddHours(24)),Is.True);
            Assert.That(DailyRewards.IsReady(Now.AddHours(-1)),Is.False);
            var receipt=DailyRewards.LastResult();Assert.That(receipt,Is.Not.Null);
            Assert.That(receipt.Materials,Is.EqualTo(reward.Materials));Assert.That(receipt.AddedGadgets,Is.EqualTo(reward.AddedGadgets));
            Assert.That(receipt.CosmeticId,Is.EqualTo(reward.CosmeticId));
        }
        [Test] public void FullInventoryConvertsExactlyTheUnstoredGadgets()
        {
            for(int i=0;i<5;i++)PlayerPrefs.SetInt("OrbitBreaker.PowerUps."+(PowerUpType)i+".Stock",5);
            PlayerPrefs.SetInt("OrbitBreaker.Meta.Materials",100);
            var reward=DailyRewards.TryClaim(Now);
            Assert.That(reward.AddedGadgets,Is.All.Zero);
            Assert.That(reward.OverflowMaterials,Is.EqualTo(reward.TotalGadgets*25));
            Assert.That(MetaProgression.Materials,Is.EqualTo(100+reward.Materials+reward.OverflowMaterials+reward.CosmeticCompensation));
            Assert.That(PowerUpProgression.TotalStored(),Is.EqualTo(25));
        }
        [Test] public void InterruptedTransactionReplaysAbsoluteTargetsExactlyOnce()
        {
            string journal="{\"ticks\":"+Now.Ticks+",\"wallet\":1234,\"stocks\":[1,2,3,4,5],\"reward\":{\"Rarity\":2,\"Materials\":1200,\"Gadgets\":[1,2,3,4,5],\"AddedGadgets\":[1,2,3,4,5]}}";
            PlayerPrefs.SetString(Prefix+"Pending",journal);
            DailyRewards.RecoverPending();DailyRewards.RecoverPending();
            Assert.That(MetaProgression.Materials,Is.EqualTo(1234));Assert.That(PowerUpProgression.TotalStored(),Is.EqualTo(15));
            Assert.That(DailyRewards.TryClaim(Now),Is.Null);Assert.That(PlayerPrefs.HasKey(Prefix+"Pending"),Is.False);
            Assert.That(DailyRewards.LastResult().Rarity,Is.EqualTo(DailyChestRarity.Violet));
        }
        [Test] public void InvalidTimestampCannotCrashMenu()
        {
            PlayerPrefs.SetString(Prefix+"LastClaimUtcTicks",long.MaxValue.ToString());
            Assert.DoesNotThrow(()=>DailyRewards.Remaining(Now));
        }
    }
}
