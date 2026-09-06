using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class PowerUpTests
    {
        private readonly int[] savedStock = new int[5];

        [SetUp]
        public void SaveAndClearStock()
        {
            for (int i = 0; i < savedStock.Length; i++)
            {
                string key = "OrbitBreaker.PowerUps." + (PowerUpType)i + ".Stock";
                savedStock[i] = PlayerPrefs.GetInt(key, 0);
                PlayerPrefs.SetInt(key, 0);
            }
        }

        [TearDown]
        public void RestoreStock()
        {
            for (int i = 0; i < savedStock.Length; i++)
                PlayerPrefs.SetInt("OrbitBreaker.PowerUps." + (PowerUpType)i + ".Stock", savedStock[i]);
            PlayerPrefs.Save();
        }
        [Test]
        public void Catalog_ContainsFiveDistinctPowerUps()
        {
            Assert.That(PowerUpProgression.Catalog, Has.Length.EqualTo(5));
            for (int i = 0; i < PowerUpProgression.Catalog.Length; i++)
                Assert.That((int)PowerUpProgression.Catalog[i].Type, Is.EqualTo(i));
        }

        [Test]
        public void EveryUpgradeHasFiveIncreasingUsefulLevels()
        {
            foreach (PowerUpDefinition definition in PowerUpProgression.Catalog)
            {
                int previous = 0;
                for (int level = 1; level < 5; level++)
                {
                    int price = definition.UpgradePrice(level);
                    Assert.That(price, Is.GreaterThan(previous));
                    previous = price;
                }
                Assert.That(definition.UpgradePrice(5), Is.Zero);
            }
            Assert.That(PowerUpProgression.Duration(PowerUpType.Shield, 5), Is.GreaterThan(PowerUpProgression.Duration(PowerUpType.Shield, 1)));
            Assert.That(PowerUpProgression.WormholeDistance(5), Is.GreaterThan(PowerUpProgression.WormholeDistance(1)));
        }

        [Test]
        public void EveryTypeHasFiveCharges()
        {
            Assert.That(PowerUpProgression.MaxInventory, Is.EqualTo(5));
        }

        [Test]
        public void PersistentInventoryCapsEachTypeIndependentlyAndConsumesExactType()
        {
            foreach (PowerUpDefinition definition in PowerUpProgression.Catalog)
            {
                for (int i = 0; i < 5; i++) Assert.That(PowerUpProgression.TryStore(definition.Type), Is.True);
                Assert.That(PowerUpProgression.TryStore(definition.Type), Is.False);
                Assert.That(PowerUpProgression.StoredCount(definition.Type), Is.EqualTo(5));
            }
            Assert.That(PowerUpProgression.TotalStored(), Is.EqualTo(25));
            Assert.That(PowerUpProgression.TryConsume(PowerUpType.OrbitMagnet), Is.True);
            Assert.That(PowerUpProgression.TotalStored(), Is.EqualTo(24));
            Assert.That(PowerUpProgression.StoredCount(PowerUpType.Shield), Is.EqualTo(5));
            Assert.That(PowerUpProgression.TryStore(PowerUpType.Shield), Is.False);
            Assert.That(PowerUpProgression.TryStore(PowerUpType.OrbitMagnet), Is.True);
        }

        [Test]
        public void EveryLevelExposesReadableStats()
        {
            foreach (PowerUpDefinition definition in PowerUpProgression.Catalog)
                for (int level = 1; level <= 5; level++)
                    Assert.That(PowerUpProgression.Stats(definition.Type, level), Is.Not.Empty);
        }
    }
}
