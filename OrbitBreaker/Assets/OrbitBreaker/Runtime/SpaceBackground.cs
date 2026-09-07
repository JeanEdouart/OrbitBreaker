using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed class SpaceBackground : MonoBehaviour
    {
        private const float TileSize = 14.6f;
        private const float StarSpan = 17f;
        private Camera targetCamera;
        private readonly SpriteRenderer[] nebulaTiles = new SpriteRenderer[3];
        private readonly Transform[] stars = new Transform[52];
        private readonly float[] starSeedX = new float[52];
        private readonly float[] starSeedY = new float[52];
        private float tileWorldHeight = TileSize;
        private float hyperspaceIntensity;
        private float hyperspaceDrift;
        private Material sectorMaterial;
        private float sectorHue;
        private float targetSectorHue;

        public static int SectorForDistance(int distance) => Mathf.Max(0, distance) / 500;

        public void SetDistance(int distance, bool immediate = false)
        {
            targetSectorHue = (SectorForDistance(distance) % 6) * 60f;
            if (immediate) sectorHue = targetSectorHue;
        }

        public void SetHyperspace(float intensity)
        {
            hyperspaceIntensity = Mathf.Clamp01(intensity);
        }

        public void Initialize(Camera camera)
        {
            targetCamera = camera;
            Shader sectorShader = Resources.Load<Shader>("Shaders/BackgroundSector");
            if (sectorShader != null) sectorMaterial = new Material(sectorShader);
            for (int i = 0; i < nebulaTiles.Length; i++)
            {
                var tile = new GameObject("Nebula Tile " + (i + 1));
                tile.transform.SetParent(transform, false);
                nebulaTiles[i] = tile.AddComponent<SpriteRenderer>();
                nebulaTiles[i].sprite = RuntimeAssets.GetBackgroundSprite(MetaProgression.Selected(CosmeticKind.Background));
                nebulaTiles[i].color = new Color(0.72f, 0.78f, 0.92f, 0.72f);
                nebulaTiles[i].sortingOrder = -100;
                if (sectorMaterial != null) nebulaTiles[i].sharedMaterial = sectorMaterial;
            }
            ResizeTilesToCoverCamera();

            var random = new System.Random(7319);
            for (int i = 0; i < stars.Length; i++)
            {
                var star = new GameObject("Parallax Star " + (i + 1));
                star.transform.SetParent(transform, false);
                float scale = Mathf.Lerp(0.018f, 0.052f, (float)random.NextDouble());
                star.transform.localScale = Vector3.one * scale;
                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeAssets.CircleSprite;
                renderer.color = i % 5 == 0
                    ? new Color(0.72f, 0.42f, 1f, 0.78f)
                    : new Color(0.42f, 0.9f, 1f, 0.68f);
                renderer.sortingOrder = -90;
                stars[i] = star.transform;
                starSeedX[i] = Mathf.Lerp(-3.5f, 3.5f, (float)random.NextDouble());
                starSeedY[i] = Mathf.Lerp(-StarSpan * 0.5f, StarSpan * 0.5f, (float)random.NextDouble());
                star.transform.localPosition = new Vector3(starSeedX[i], starSeedY[i], 0f);
            }
            RefreshPositions();
        }

        public void ApplyCosmetics()
        {
            Sprite selected = RuntimeAssets.GetBackgroundSprite(MetaProgression.Selected(CosmeticKind.Background));
            for (int i = 0; i < nebulaTiles.Length; i++)
                if (nebulaTiles[i] != null) nebulaTiles[i].sprite = selected;
            ResizeTilesToCoverCamera();
        }

        private void ResizeTilesToCoverCamera()
        {
            if (targetCamera == null) return;
            float viewHeight = targetCamera.orthographicSize * 2f;
            float viewWidth = viewHeight * targetCamera.aspect;
            tileWorldHeight = TileSize;
            for (int i = 0; i < nebulaTiles.Length; i++)
            {
                Sprite sprite = nebulaTiles[i] != null ? nebulaTiles[i].sprite : null;
                if (sprite == null) continue;
                float scale = Mathf.Max(viewWidth / Mathf.Max(0.01f, sprite.bounds.size.x), viewHeight / Mathf.Max(0.01f, sprite.bounds.size.y)) * 1.04f;
                nebulaTiles[i].transform.localScale = Vector3.one * scale;
                tileWorldHeight = sprite.bounds.size.y * scale;
            }
        }

        private void LateUpdate()
        {
            if (GamePreferences.DynamicBackground) hyperspaceDrift += Time.deltaTime * hyperspaceIntensity * 12f;
            sectorHue = Mathf.MoveTowardsAngle(sectorHue, targetSectorHue, Time.deltaTime * 45f);
            if (sectorMaterial != null) sectorMaterial.SetFloat("_HueShift", sectorHue / 360f);
            RefreshPositions();
        }

        private void OnDestroy()
        {
            if (sectorMaterial != null) Destroy(sectorMaterial);
        }

        private void RefreshPositions()
        {
            if (targetCamera == null) return;
            float cameraY = targetCamera.transform.position.y;
            float cameraX = targetCamera.transform.position.x;
            Color biomeTint = new Color(0.72f, 0.78f, 0.92f, 0.72f);
            float nebulaOffset = GamePreferences.DynamicBackground
                ? Mathf.Repeat(cameraY * 0.2f + tileWorldHeight * 0.5f, tileWorldHeight) - tileWorldHeight * 0.5f
                : 0f;
            for (int i = 0; i < nebulaTiles.Length; i++)
            {
                nebulaTiles[i].transform.position = new Vector3(cameraX * 0.88f, cameraY + (i - 1) * tileWorldHeight - nebulaOffset, 2f);
                nebulaTiles[i].color = biomeTint;
            }

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(GamePreferences.EnhancedEffects);
                float drift = GamePreferences.DynamicBackground ? cameraY * 0.48f : 0f;
                drift += hyperspaceDrift * (0.7f + (i % 4) * 0.1f);
                float relativeY = Mathf.Repeat(starSeedY[i] - drift + StarSpan * 0.5f, StarSpan) - StarSpan * 0.5f;
                stars[i].position = new Vector3(cameraX + starSeedX[i], cameraY + relativeY, 1f);
                float width = Mathf.Lerp(1f, 0.42f, hyperspaceIntensity);
                float length = 1f;
                stars[i].localScale = new Vector3(width, length, 1f) * Mathf.Lerp(0.018f, 0.052f, (i % 13) / 12f);
            }
        }
    }
}
