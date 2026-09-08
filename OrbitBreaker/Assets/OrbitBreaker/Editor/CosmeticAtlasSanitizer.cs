using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    /// <summary>Normalizes generated contact sheets into evenly padded, contamination-free runtime atlases.</summary>
    public static class CosmeticAtlasSanitizer
    {
        private const int CellSize = 256;

        [MenuItem("Orbit Breaker/Tools/Sanitize Cosmetic Atlases")]
        public static void SanitizeAll()
        {
            Sanitize("Art/expanded-rockets-generated", "Assets/OrbitBreaker/Resources/Art/expanded-rockets-generated.png", 4, 4, false);
            Sanitize("Art/expanded-planets-a-generated", "Assets/OrbitBreaker/Resources/Art/expanded-planets-a-generated.png", 5, 6, false);
            Sanitize("Art/expanded-planets-b-generated", "Assets/OrbitBreaker/Resources/Art/expanded-planets-b-generated.png", 5, 5, false);
            Sanitize("Art/cosmetics-planets-atlas", "Assets/OrbitBreaker/Resources/Art/cosmetics-planets-atlas.png", 4, 2, true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Orbit Breaker: cosmetic atlases sanitized with complete subjects and safe transparent margins.");
        }

        private static void Sanitize(string resourcePath, string assetPath, int columns, int rows, bool removeBackdrop)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
            Type runtimeAssets = typeof(MetaProgression).Assembly.GetType("OrbitBreaker.RuntimeAssets", true);
            MethodInfo loader = runtimeAssets.GetMethod("LoadGridSprites", BindingFlags.Static | BindingFlags.NonPublic);
            var sprites = (Sprite[])loader.Invoke(null, new object[] { resourcePath, columns, rows, "Sanitize", removeBackdrop, true });
            var atlas = new Texture2D(columns * CellSize, rows * CellSize, TextureFormat.RGBA32, false);
            atlas.SetPixels32(new Color32[atlas.width * atlas.height]);

            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                Texture2D source = sprites[row * columns + column].texture;
                float scale = Mathf.Min(CellSize * 0.82f / source.width, CellSize * 0.82f / source.height);
                int width = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
                int height = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));
                int offsetX = column * CellSize + (CellSize - width) / 2;
                int offsetY = row * CellSize + (CellSize - height) / 2;
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    atlas.SetPixel(offsetX + x, offsetY + y, source.GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height));
            }

            atlas.Apply(false, false);
            File.WriteAllBytes(Path.GetFullPath(assetPath), atlas.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(atlas);
            foreach (Sprite sprite in sprites)
            {
                Texture2D texture = sprite.texture;
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.isReadable = false;
            importer.SaveAndReimport();
        }
    }
}
