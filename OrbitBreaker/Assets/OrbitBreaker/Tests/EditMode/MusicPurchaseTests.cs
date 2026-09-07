using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public class MusicPurchaseTests
    {
        [Test]
        public void MusicPurchase_RequiresFundsChargesOnceAndPersistsSelection()
        {
            var item = MetaProgression.Catalog.First(x => x.Kind == CosmeticKind.Music && x.VisualIndex == 1);
            string[] keys = {"OrbitBreaker.Meta.Materials", "OrbitBreaker.Meta.Owned." + item.Id, "OrbitBreaker.Meta.Selected.Music"};
            var old = new Dictionary<string,int?>();
            foreach (string key in keys) old[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            try
            {
                PlayerPrefs.DeleteKey(keys[1]); PlayerPrefs.SetInt(keys[2],0);
                PlayerPrefs.SetInt(keys[0],item.Price-1);
                Assert.That(MetaProgression.BuyOrEquip(item), Is.False);
                Assert.That(MetaProgression.Materials, Is.EqualTo(item.Price-1));
                Assert.That(MetaProgression.Selected(CosmeticKind.Music), Is.Zero);
                PlayerPrefs.SetInt(keys[0],item.Price);
                Assert.That(MetaProgression.BuyOrEquip(item), Is.True);
                Assert.That(MetaProgression.Materials, Is.Zero);
                Assert.That(MusicLibrary.IsOwned(1), Is.True);
                Assert.That(MusicLibrary.Equipped, Is.EqualTo(1));
                Assert.That(MetaProgression.BuyOrEquip(item), Is.True);
                Assert.That(MetaProgression.Materials, Is.Zero);
            }
            finally
            {
                foreach (var pair in old) if(pair.Value.HasValue)PlayerPrefs.SetInt(pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey(pair.Key);
                PlayerPrefs.Save();
            }
        }
    }
}
