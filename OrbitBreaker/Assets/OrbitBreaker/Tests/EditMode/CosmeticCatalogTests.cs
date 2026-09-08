using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class CosmeticCatalogTests
    {
        [Test]
        public void Catalog_HasUniqueStableIdsAndContiguousVisualIndices()
        {
            var ids = new HashSet<string>();
            var indices = new Dictionary<CosmeticKind, HashSet<int>>();
            foreach (CosmeticDefinition item in MetaProgression.Catalog)
            {
                Assert.That(ids.Add(item.Id), Is.True, "Duplicate ID " + item.Id);
                Assert.That(item.Name, Is.Not.Null.And.Not.Empty);
                Assert.That(item.Price, Is.GreaterThanOrEqualTo(0), item.Id);
                Assert.That(item.VisualIndex, Is.GreaterThanOrEqualTo(0), item.Id);
                if (!indices.TryGetValue(item.Kind, out HashSet<int> values))
                    indices[item.Kind] = values = new HashSet<int>();
                Assert.That(values.Add(item.VisualIndex), Is.True, "Duplicate visual " + item.Kind + ":" + item.VisualIndex);
                if (item.VisualIndex == 0) Assert.That(item.Price, Is.Zero, "Default must remain free: " + item.Id);
            }
            Assert.That(MetaProgression.Catalog.Length, Is.GreaterThanOrEqualTo(90));
            foreach (CosmeticKind kind in Enum.GetValues(typeof(CosmeticKind)))
            {
                Assert.That(indices.ContainsKey(kind), Is.True, kind.ToString());
                for (int i = 0; i < indices[kind].Count; i++)
                    Assert.That(indices[kind].Contains(i), Is.True, "Gap in " + kind + " at " + i);
            }
        }

        [Test]
        public void ExistingCatalog_IdsAndVisualIndicesRemainSaveCompatible()
        {
            Check(CosmeticKind.Rocket, "rocket_default", "rocket_interceptor", "rocket_miner", "rocket_retro", "rocket_crystal", "rocket_bio", "rocket_banana", "rocket_stealth", "rocket_lander", "rocket_gold", "rocket_aurora");
            Check(CosmeticKind.Trail, "trail_cyan", "trail_plasma", "trail_toxic", "trail_solar", "trail_boreal", "trail_amethyst");
            Check(CosmeticKind.PlanetPack, "planets_default", "planets_solar", "planets_anime", "planets_aurora");
            Check(CosmeticKind.Background, "background_default", "background_ion", "background_inferno", "background_aurora");
        }

        static void Check(CosmeticKind kind, params string[] expected)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                bool found = false;
                foreach (CosmeticDefinition item in MetaProgression.Catalog)
                    if (item.Id == expected[index])
                    {
                        Assert.That(item.Kind, Is.EqualTo(kind), item.Id);
                        Assert.That(item.VisualIndex, Is.EqualTo(index), item.Id);
                        found = true;
                        break;
                    }
                Assert.That(found, Is.True, "Removed saved cosmetic " + expected[index]);
            }
        }

        [Test]
        public void ExpandedArtwork_IsPresentDistinctAndStaysInsideItsSourceTexture()
        {
            Type runtime = typeof(MetaProgression).Assembly.GetType("OrbitBreaker.RuntimeAssets", true);
            var unique = new HashSet<Sprite>();
            foreach (CosmeticDefinition item in MetaProgression.Catalog)
            {
                string method;
                object[] args;
                if (item.Kind == CosmeticKind.Rocket && item.VisualIndex >= 11)
                { method = "GetRocketSprite"; args = new object[] { item.VisualIndex }; }
                else if (item.Kind == CosmeticKind.Trail && item.VisualIndex >= 6)
                { method = "GetTrailSprite"; args = new object[] { item.VisualIndex }; }
                else if (item.Kind == CosmeticKind.Background && item.VisualIndex >= 4)
                { method = "GetBackgroundSprite"; args = new object[] { item.VisualIndex }; }
                else if (item.Kind == CosmeticKind.PlanetPack && item.VisualIndex >= 4)
                {
                    for (int variant = 0; variant < 5; variant++)
                        Validate((Sprite)runtime.GetMethod("GetPlanetPackSprite").Invoke(null, new object[] { item.VisualIndex, variant }), item.Id + ":" + variant, unique);
                    continue;
                }
                else continue;
                Sprite sprite = (Sprite)runtime.GetMethod(method).Invoke(null, args);
                Validate(sprite, item.Id, unique);
            }
        }

        static void Validate(Sprite sprite, string id, HashSet<Sprite> unique)
        {
            Assert.That(sprite, Is.Not.Null, id);
            Assert.That(unique.Add(sprite), Is.True, "Fallback or reused asset: " + id);
            Assert.That(sprite.texture, Is.Not.Null, id);
            Assert.That(sprite.rect.xMin, Is.GreaterThanOrEqualTo(0), id);
            Assert.That(sprite.rect.yMin, Is.GreaterThanOrEqualTo(0), id);
            Assert.That(sprite.rect.xMax, Is.LessThanOrEqualTo(sprite.texture.width), id);
            Assert.That(sprite.rect.yMax, Is.LessThanOrEqualTo(sprite.texture.height), id);
            Assert.That(sprite.rect.width, Is.GreaterThan(0), id);
            Assert.That(sprite.rect.height, Is.GreaterThan(0), id);
            Assert.That(sprite.bounds.size.x, Is.GreaterThan(0), id);
            Assert.That(sprite.bounds.size.y, Is.EqualTo(1f).Within(.001f), id);
        }

        [Test]
        public void SanitizedAtlases_KeepEverySubjectInsideATransparentSafeArea()
        {
            CheckAtlas("expanded-rockets-generated.png", 4, 4);
            CheckAtlas("expanded-planets-a-generated.png", 5, 6);
            CheckAtlas("expanded-planets-b-generated.png", 5, 5);
            CheckAtlas("cosmetics-planets-atlas.png", 4, 2);
        }

        private static void CheckAtlas(string fileName, int columns, int rows)
        {
            string path = Path.Combine(Application.dataPath, "OrbitBreaker", "Resources", "Art", fileName);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Assert.That(texture.LoadImage(File.ReadAllBytes(path), false), Is.True, fileName);
            Assert.That(texture.width, Is.EqualTo(columns * 256), fileName);
            Assert.That(texture.height, Is.EqualTo(rows * 256), fileName);
            Color32[] pixels = texture.GetPixels32();
            const int safeMargin = 16;
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int opaque = 0;
                for (int y = 0; y < 256; y++)
                for (int x = 0; x < 256; x++)
                {
                    byte alpha = pixels[(row * 256 + y) * texture.width + column * 256 + x].a;
                    if (alpha <= 8) continue;
                    opaque++;
                    Assert.That(x, Is.InRange(safeMargin, 255 - safeMargin), fileName + " cell " + (row * columns + column) + " touches a horizontal crop edge");
                    Assert.That(y, Is.InRange(safeMargin, 255 - safeMargin), fileName + " cell " + (row * columns + column) + " touches a vertical crop edge");
                }
                Assert.That(opaque, Is.GreaterThan(1000), fileName + " cell " + (row * columns + column) + " is empty");
            }
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}
