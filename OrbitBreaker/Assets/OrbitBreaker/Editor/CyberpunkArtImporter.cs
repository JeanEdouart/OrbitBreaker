using UnityEditor;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    public sealed class CyberpunkArtImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/OrbitBreaker/Resources/Art/cyberpunk-")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = assetPath.Contains("background") ? 2048 : 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
        }
    }
}
