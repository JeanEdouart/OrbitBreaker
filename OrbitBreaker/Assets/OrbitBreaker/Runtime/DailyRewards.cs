using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    public enum DailyChestRarity { Green, Blue, Violet, Gold, Rainbow }
    public enum DailyRewardKind { Legacy, Materials, Gadgets, Cosmetic }

    [Serializable]
    public sealed class DailyRewardResult
    {
        public DailyChestRarity Rarity;
        public DailyRewardKind Kind;
        public int Materials;
        public int OverflowMaterials;
        public int CosmeticCompensation;
        public string CosmeticId;
        public CosmeticDefinition? Cosmetic;
        public int[] Gadgets = new int[5];
        public int[] AddedGadgets = new int[5];
        public int TotalGadgets { get { int n = 0; for (int i = 0; i < Gadgets.Length; i++) n += Gadgets[i]; return n; } }
    }

    /// <summary>Persistent 24-hour reward. The result is committed before its reveal animation.</summary>
    public static class DailyRewards
    {
        private const string LastClaimKey = "OrbitBreaker.DailyReward.LastClaimUtcTicks";
        private const string PendingKey = "OrbitBreaker.DailyReward.Pending";
        private const string ResultKey = "OrbitBreaker.DailyReward.Result";
        [Serializable] private sealed class Transaction
        {
            public long ticks;
            public int wallet;
            public int[] stocks;
            public DailyRewardResult reward;
        }
        public static readonly TimeSpan Cooldown = TimeSpan.FromHours(24);

        public static DateTime LastClaimUtc
        {
            get
            {
                string raw = PlayerPrefs.GetString(LastClaimKey, string.Empty);
                return long.TryParse(raw, out long ticks) && ticks > 0 && ticks <= DateTime.MaxValue.Ticks ? new DateTime(ticks, DateTimeKind.Utc) : DateTime.MinValue;
            }
        }

        public static TimeSpan Remaining(DateTime utcNow)
        {
            DateTime last = LastClaimUtc;
            if (last == DateTime.MinValue) return TimeSpan.Zero;
            TimeSpan elapsed = utcNow - last;
            if (elapsed < TimeSpan.Zero) return Cooldown; // Clock rollback must not duplicate a claim.
            TimeSpan remaining = Cooldown - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public static bool IsReady(DateTime utcNow) => Remaining(utcNow) == TimeSpan.Zero;

#if UNITY_EDITOR
        public static void ResetCooldownForEditor()
        {
            RecoverPending();
            PlayerPrefs.DeleteKey(LastClaimKey);
            PlayerPrefs.Save();
        }
#endif

        public static DailyChestRarity RarityFromRoll(double roll)
        {
            if (roll < 0.60) return DailyChestRarity.Green;
            if (roll < 0.84) return DailyChestRarity.Blue;
            if (roll < 0.95) return DailyChestRarity.Violet;
            if (roll < 0.992) return DailyChestRarity.Gold;
            return DailyChestRarity.Rainbow;
        }

        public static DailyRewardResult TryClaim(DateTime utcNow)
        {
            RecoverPending();
            if (!IsReady(utcNow)) return null;
            var result = Generate(new System.Random(Guid.NewGuid().GetHashCode()));
            var stocks = new int[5];
            for (int i = 0; i < stocks.Length; i++)
            {
                int current = PowerUpProgression.StoredCount((PowerUpType)i);
                result.AddedGadgets[i] = Math.Min(result.Gadgets[i], PowerUpProgression.MaxInventory - current);
                stocks[i] = current + result.AddedGadgets[i];
                result.OverflowMaterials += (result.Gadgets[i] - result.AddedGadgets[i]) * 25;
            }
            result.CosmeticId = result.Cosmetic.HasValue ? result.Cosmetic.Value.Id : string.Empty;
            var transaction = new Transaction { ticks = utcNow.Ticks, stocks = stocks, reward = result,
                wallet = (int)Math.Min(int.MaxValue, (long)MetaProgression.Materials + result.Materials + result.OverflowMaterials + result.CosmeticCompensation) };
            // A write-ahead journal stores absolute targets; recovery can apply it repeatedly without duplicating rewards.
            PlayerPrefs.SetString(PendingKey, JsonUtility.ToJson(transaction)); PlayerPrefs.Save();
            Apply(transaction);
            return result;
        }

        public static void RecoverPending()
        {
            string json = PlayerPrefs.GetString(PendingKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return;
            Transaction transaction;
            try { transaction = JsonUtility.FromJson<Transaction>(json); }
            catch (ArgumentException) { return; }
            if (transaction != null && transaction.reward != null && transaction.stocks != null && transaction.stocks.Length == 5) Apply(transaction);
        }

        private static void Apply(Transaction transaction)
        {
            PlayerPrefs.SetInt("OrbitBreaker.Meta.Materials", transaction.wallet);
            for (int i = 0; i < 5; i++) PlayerPrefs.SetInt("OrbitBreaker.PowerUps." + (PowerUpType)i + ".Stock", transaction.stocks[i]);
            if (!string.IsNullOrEmpty(transaction.reward.CosmeticId) && !DailyCourse.IsExclusiveRocket(transaction.reward.CosmeticId))
                PlayerPrefs.SetInt("OrbitBreaker.Meta.Owned." + transaction.reward.CosmeticId, 1);
            PlayerPrefs.SetString(LastClaimKey, transaction.ticks.ToString());
            PlayerPrefs.SetString(ResultKey, JsonUtility.ToJson(transaction.reward));
            PlayerPrefs.Save();
            PlayerPrefs.DeleteKey(PendingKey); PlayerPrefs.Save();
        }

        public static DailyRewardResult LastResult()
        {
            string json = PlayerPrefs.GetString(ResultKey, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;
            DailyRewardResult result;
            try { result = JsonUtility.FromJson<DailyRewardResult>(json); } catch (ArgumentException) { return null; }
            if (result == null) return null;
            foreach (var item in MetaProgression.Catalog) if (item.Id == result.CosmeticId) { result.Cosmetic = item; break; }
            return result;
        }

        internal static DailyRewardResult Generate(System.Random random)
        {
            var result = new DailyRewardResult { Rarity = RarityFromRoll(random.NextDouble()) };
            int minMaterials, maxMaterials, minGadgets, maxGadgets;
            double cosmeticChance;
            switch (result.Rarity)
            {
                case DailyChestRarity.Blue: minMaterials = 500; maxMaterials = 1000; minGadgets = 3; maxGadgets = 5; cosmeticChance = .22; break;
                case DailyChestRarity.Violet: minMaterials = 1000; maxMaterials = 1500; minGadgets = 5; maxGadgets = 10; cosmeticChance = .38; break;
                case DailyChestRarity.Gold: minMaterials = 1500; maxMaterials = 3000; minGadgets = 10; maxGadgets = 25; cosmeticChance = .62; break;
                case DailyChestRarity.Rainbow: minMaterials = 3000; maxMaterials = 5000; minGadgets = 25; maxGadgets = 25; cosmeticChance = 1; break;
                default: minMaterials = 0; maxMaterials = 500; minGadgets = 1; maxGadgets = 3; cosmeticChance = .10; break;
            }
            // Exactly one category. Rarity independently controls the value of that reward.
            double category = random.NextDouble();
            result.Kind = category < .5 ? DailyRewardKind.Materials : category < .8 ? DailyRewardKind.Gadgets : DailyRewardKind.Cosmetic;
            if (result.Kind == DailyRewardKind.Materials)
            {
                result.Materials = random.Next(Math.Max(1,minMaterials), maxMaterials + 1);
                return result;
            }
            if (result.Kind == DailyRewardKind.Cosmetic)
            {
                result.Cosmetic = PickCosmetic(random, result.Rarity);
                if (!result.Cosmetic.HasValue) result.CosmeticCompensation = result.Rarity == DailyChestRarity.Rainbow ? 3000 : maxMaterials;
                return result;
            }
            int gadgetCount = random.Next(minGadgets, maxGadgets + 1);
            if (result.Rarity == DailyChestRarity.Rainbow)
            {
                for (int i = 0; i < result.Gadgets.Length; i++) result.Gadgets[i] = PowerUpProgression.MaxInventory;
            }
            else for (int i = 0; i < gadgetCount; i++)
            {
                int type; do { type = random.Next(0, 5); } while (result.Gadgets[type] >= 5);
                result.Gadgets[type]++;
            }
            return result;
        }

        private static CosmeticDefinition? PickCosmetic(System.Random random, DailyChestRarity rarity)
        {
            var eligible = new List<CosmeticDefinition>();
            int min = rarity == DailyChestRarity.Green ? 1 : rarity == DailyChestRarity.Blue ? 501 : rarity == DailyChestRarity.Violet ? 1001 : rarity == DailyChestRarity.Gold ? 1501 : 3000;
            int max = rarity == DailyChestRarity.Green ? 500 : rarity == DailyChestRarity.Blue ? 1000 : rarity == DailyChestRarity.Violet ? 1500 : rarity == DailyChestRarity.Gold ? 3000 : int.MaxValue;
            foreach (CosmeticDefinition item in MetaProgression.Catalog)
                if (item.Price >= min && item.Price <= max && !MetaProgression.Owned(item) && !DailyCourse.IsExclusiveRocket(item.Id)) eligible.Add(item);
            return eligible.Count == 0 ? null : eligible[random.Next(eligible.Count)];
        }

        public static string RarityName(DailyChestRarity rarity) => rarity switch
        {
            DailyChestRarity.Blue => "BLEU",
            DailyChestRarity.Violet => "VIOLET",
            DailyChestRarity.Gold => "DORÉ",
            DailyChestRarity.Rainbow => "RAINBOW",
            _ => "VERT"
        };

        public static Color RarityColor(DailyChestRarity rarity) => rarity switch
        {
            DailyChestRarity.Blue => new Color(.2f, .65f, 1f),
            DailyChestRarity.Violet => new Color(.72f, .32f, 1f),
            DailyChestRarity.Gold => new Color(1f, .72f, .16f),
            DailyChestRarity.Rainbow => new Color(1f, .32f, .72f),
            _ => new Color(.25f, 1f, .55f)
        };
    }
}

