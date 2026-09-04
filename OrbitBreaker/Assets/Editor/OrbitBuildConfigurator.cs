using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace OrbitBreaker.Editor
{
    public static class OrbitBuildConfigurator
    {
        private const string IconPath = "Assets/OrbitBreaker/Branding/orbit-breaker-icon.png";

        [MenuItem("Tools/Orbit Breaker/Apply Branding And Web Settings")]
        public static void ApplyBrandingAndWebSettings()
        {
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null) throw new System.InvalidOperationException("Orbit Breaker icon is missing at " + IconPath);

            int[] androidSizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
            if (androidSizes.Length > 0)
            {
                PlayerSettings.SetIconsForTargetGroup(
                    BuildTargetGroup.Android,
                    Enumerable.Repeat(icon, androidSizes.Length).ToArray());
            }

            NamedBuildTarget android = NamedBuildTarget.Android;
            foreach (PlatformIconKind kind in PlayerSettings.GetSupportedIconKinds(android))
            {
                PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(android, kind);
                foreach (PlatformIcon slot in icons)
                {
                    for (int layer = 0; layer < slot.maxLayerCount; layer++)
                    {
                        slot.SetTexture(icon, layer);
                    }
                }
                PlayerSettings.SetPlatformIcons(android, kind, icons);
            }

            PlayerSettings.WebGL.template = "PROJECT:OrbitBreakerPWA";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            AssetDatabase.SaveAssets();
            Debug.Log("[OrbitBreaker] Android icon and PWA settings applied.");
        }
    }
}
