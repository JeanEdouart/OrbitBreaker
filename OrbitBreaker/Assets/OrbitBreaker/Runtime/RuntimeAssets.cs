using System;
using UnityEngine;

namespace OrbitBreaker
{
    internal static class RuntimeAssets
    {
        private static Sprite circleSprite;
        private static Sprite squareSprite;
        private static Material spriteMaterial;
        private static Sprite settingsIcon;
        private static Sprite pauseIcon;
        private static Sprite roundedRectSprite;
        private static Sprite locationIcon;
        private static Sprite planetIcon;
        private static Sprite trophyIcon;
        private static Sprite leaderboardIcon;
        private static Sprite statisticsIcon;
        private static Sprite rocketSprite;
        private static Sprite flameSprite;
        private static Sprite[] planetSprites;
        private static Sprite[] debrisSprites;
        private static Sprite spaceBackgroundSprite;
        private static Sprite[] cosmeticRockets;
        private static Sprite[] cosmeticPlanets;
        private static Sprite[] cosmeticBackgrounds;
        private static Sprite materialCrystalSprite;
        private static Sprite[] powerUpIcons;
        private static Sprite powerUpUpgradeIcon;
        private static Sprite auroraRocket;
        private static Sprite auroraBackground;
        private static Sprite musicIcon;
        private static Sprite discordIcon;
        private static Sprite[] auroraPlanets;
        private static Sprite[] generatedRockets;
        private static Sprite[] generatedPlanetsA;
        private static Sprite[] generatedPlanetsB;
        private static Sprite[] generatedBackgrounds;
        private static Sprite coquineRocket;
        private static Sprite herbalRocket;
        private static Sprite[] herbalPlanets;
        private static Sprite herbalBackground;
        private static readonly Sprite[] dailyRockets = new Sprite[3];

        public static Sprite CircleSprite
        {
            get
            {
                if (circleSprite == null)
                {
                    const int size = 128;
                    var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                    {
                        name = "RuntimeCircle",
                        filterMode = FilterMode.Bilinear,
                        wrapMode = TextureWrapMode.Clamp,
                        hideFlags = HideFlags.HideAndDontSave
                    };

                    var pixels = new Color32[size * size];
                    Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
                    float radius = size * 0.47f;
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float distance = Vector2.Distance(new Vector2(x, y), center);
                            float alpha = Mathf.Clamp01(radius - distance + 1f);
                            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }

                    texture.SetPixels32(pixels);
                    texture.Apply(false, true);
                    circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
                    circleSprite.name = "RuntimeCircle";
                }

                return circleSprite;
            }
        }

        public static Sprite SquareSprite
        {
            get
            {
                if (squareSprite == null)
                {
                    var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        name = "RuntimeSquare",
                        filterMode = FilterMode.Bilinear,
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                    texture.Apply(false, true);
                    squareSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f, 2f);
                    squareSprite.name = "RuntimeSquare";
                }

                return squareSprite;
            }
        }

        public static Sprite RoundedRectSprite
        {
            get
            {
                if (roundedRectSprite != null) return roundedRectSprite;
                const int size = 96;
                const float radius = 22f;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeRoundedRect",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                var pixels = new Color32[size * size];
                Vector2 half = Vector2.one * (size * 0.5f - radius - 1f);
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 p = new Vector2(Mathf.Abs(x + 0.5f - size * 0.5f), Mathf.Abs(y + 0.5f - size * 0.5f));
                        Vector2 q = new Vector2(Mathf.Max(p.x - half.x, 0f), Mathf.Max(p.y - half.y, 0f));
                        float alpha = Mathf.Clamp01(radius - q.magnitude + 0.75f);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                roundedRectSprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size, 0, SpriteMeshType.FullRect, new Vector4(24f, 24f, 24f, 24f));
                roundedRectSprite.name = "RuntimeRoundedRect";
                return roundedRectSprite;
            }
        }

        public static Material SpriteMaterial
        {
            get
            {
                if (spriteMaterial == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (shader == null)
                    {
                        shader = Shader.Find("Sprites/Default");
                    }

                    spriteMaterial = new Material(shader)
                    {
                        name = "RuntimeSpriteMaterial",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                }

                return spriteMaterial;
            }
        }

        public static Sprite SettingsIcon => settingsIcon != null ? settingsIcon : settingsIcon = CreateIcon("SettingsIcon", (x, y) =>
        {
            float distance = Mathf.Sqrt(x * x + y * y);
            float angle = Mathf.Atan2(y, x);
            float teeth = 0.37f + (Mathf.Cos(angle * 8f) > 0.38f ? 0.13f : 0f);
            return distance < teeth && distance > 0.21f;
        });

        public static Sprite PauseIcon => pauseIcon != null ? pauseIcon : pauseIcon = CreateIcon("PauseIcon", (x, y) =>
            Mathf.Abs(y) < 0.46f && (Mathf.Abs(x - 0.2f) < 0.11f || Mathf.Abs(x + 0.2f) < 0.11f));

        public static Sprite LocationIcon => locationIcon != null ? locationIcon : locationIcon = CreateIcon("LocationIcon", (x, y) =>
        {
            float head = Mathf.Sqrt(x * x + (y - 0.12f) * (y - 0.12f));
            bool ring = head < 0.31f && head > 0.14f;
            bool point = y < 0.08f && y > -0.43f && Mathf.Abs(x) < (y + 0.43f) * 0.48f;
            return ring || point;
        });

        public static Sprite PlanetIcon => planetIcon != null ? planetIcon : planetIcon = CreateIcon("PlanetIcon", (x, y) =>
        {
            x *= 1.6f;
            y *= 1.6f;
            float body = Mathf.Sqrt(x * x + y * y);
            float ring = Mathf.Abs((x * 0.72f + y * 0.28f) * (x * 0.72f + y * 0.28f) / 0.25f + (-x * 0.28f + y * 0.72f) * (-x * 0.28f + y * 0.72f) / 0.035f - 1f);
            return body < 0.28f || (ring < 0.22f && body > 0.24f);
        });

        public static Sprite TrophyIcon => trophyIcon != null ? trophyIcon : trophyIcon = CreateIcon("TrophyIcon", (x, y) =>
        {
            bool cup = y > -0.05f && y < 0.39f && Mathf.Abs(x) < Mathf.Lerp(0.18f, 0.32f, (y + 0.05f) / 0.44f);
            bool handles = y > 0.08f && y < 0.34f && Mathf.Abs(x) > 0.24f && Mathf.Abs(x) < 0.43f;
            bool stem = y > -0.3f && y <= -0.05f && Mathf.Abs(x) < 0.08f;
            bool basePlate = y > -0.4f && y < -0.28f && Mathf.Abs(x) < 0.27f;
            return cup || handles || stem || basePlate;
        });

        public static Sprite LeaderboardIcon => leaderboardIcon != null ? leaderboardIcon : leaderboardIcon = CreateIcon("LeaderboardIcon", (x, y) =>
        {
            bool left = x > -0.43f && x < -0.18f && y > -0.4f && y < 0.06f;
            bool center = Mathf.Abs(x) < 0.125f && y > -0.4f && y < 0.42f;
            bool right = x > 0.18f && x < 0.43f && y > -0.4f && y < 0.22f;
            return left || center || right;
        });

        public static Sprite StatisticsIcon => statisticsIcon != null ? statisticsIcon : statisticsIcon = CreateIcon("StatisticsIcon", (x, y) =>
        {
            bool axis = (x > -0.4f && x < -0.33f && y > -0.4f && y < 0.4f)
                || (y > -0.4f && y < -0.33f && x > -0.4f && x < 0.42f);
            bool bars = (x > -0.25f && x < -0.08f && y > -0.34f && y < -0.08f)
                || (x > 0f && x < 0.17f && y > -0.34f && y < 0.16f)
                || (x > 0.25f && x < 0.42f && y > -0.34f && y < 0.38f);
            return axis || bars;
        });

        public static Sprite RocketSprite => rocketSprite != null ? rocketSprite : rocketSprite = LoadSingleSprite("Art/rocket", "Player Rocket");
        public static Sprite SpaceBackgroundSprite => spaceBackgroundSprite != null ? spaceBackgroundSprite : spaceBackgroundSprite = LoadSingleSprite("Art/space-background", "Space Background");
        public static Sprite GetTrailSprite(int index) => index >= 6 ? ExpandedCosmetics.TrailSprite(index) : FlameSprite;
        public static Sprite MusicIcon => musicIcon != null ? musicIcon : musicIcon = CreateIcon("Music Icon", (x,y) =>
            ((x+.18f)*(x+.18f)/.025f+(y+.22f)*(y+.22f)/.017f<1f)
            || (x > -.09f && x < -.01f && y > -.22f && y < .4f)
            || (x > -.09f && x < .34f && y > .26f && y < .39f));

        public static Sprite DiscordIcon => discordIcon != null ? discordIcon : discordIcon = CreateIcon("Discord Icon", (x, y) =>
        {
            // Silhouette générique "bulle à oreilles" évoquant une messagerie communautaire,
            // dessinée en interne comme les autres icônes (pas de logo tiers importé).
            float bx = x / 0.40f, by = (y - 0.03f) / 0.32f;
            bool body = Mathf.Pow(Mathf.Abs(bx), 4f) + Mathf.Pow(Mathf.Abs(by), 4f) < 1f;
            bool earL = (x + 0.27f) * (x + 0.27f) / 0.028f + (y - 0.24f) * (y - 0.24f) / 0.024f < 1f;
            bool earR = (x - 0.27f) * (x - 0.27f) / 0.028f + (y - 0.24f) * (y - 0.24f) / 0.024f < 1f;
            bool eyeL = (x + 0.145f) * (x + 0.145f) / 0.016f + (y - 0.02f) * (y - 0.02f) / 0.026f < 1f;
            bool eyeR = (x - 0.145f) * (x - 0.145f) / 0.016f + (y - 0.02f) * (y - 0.02f) / 0.026f < 1f;
            return (body || earL || earR) && !(eyeL || eyeR);
        });

        public static Sprite GetRocketSprite(int index)
        {
            if (index == 30)
                return herbalRocket != null ? herbalRocket : herbalRocket = LoadSingleSprite("Art/herbal-rocket", "Joint Stellaire");
            if (index == 26)
                return coquineRocket != null ? coquineRocket : coquineRocket = LoadSingleSprite("Art/coquine-rocket", "Fusée Coquine");
            if (index >= 27)
            {
                int dailyVariant = Mathf.Clamp(index - 27, 0, 2);
                if (dailyRockets[dailyVariant] == null)
                {
                    string[] paths = { "Art/daily-solar", "Art/daily-crown", "Art/daily-eclipse" };
                    dailyRockets[dailyVariant] = LoadSingleSprite(paths[dailyVariant], "Daily Rocket " + dailyVariant);
                }
                return dailyRockets[dailyVariant];
            }
            if (index >= 11)
            {
                if (generatedRockets == null) generatedRockets = LoadGridSprites("Art/expanded-rockets-generated", 4, 4, "Generated Rocket");
                int variant = Mathf.Clamp(index - 11, 0, 15);
                int generatedAtlasIndex = (3 - variant / 4) * 4 + variant % 4;
                return generatedRockets.Length > generatedAtlasIndex ? generatedRockets[generatedAtlasIndex] : ExpandedCosmetics.Rocket(variant);
            }
            if (index == 10) return auroraRocket != null ? auroraRocket : auroraRocket = LoadSingleSprite("Art/aurora-rocket", "Aurore");
            if (index <= 0) return RocketSprite;
            if (cosmeticRockets == null) cosmeticRockets = LoadGridSprites("Art/cosmetics-rockets-atlas", 5, 2, "Rocket Cosmetic");
            // Sprite.Create uses texture coordinates from the bottom row upward.
            // Catalog order: interceptor, miner, retro, crystal, bio, banana, stealth, lunar, gold.
            int[] atlasByCatalog = { 0, 6, 7, 8, 9, 0, 1, 2, 3, 4 };
            int atlasIndex = atlasByCatalog[Mathf.Clamp(index, 0, atlasByCatalog.Length - 1)];
            return cosmeticRockets.Length > atlasIndex ? cosmeticRockets[atlasIndex] : RocketSprite;
        }

        public static Sprite MaterialCrystalSprite => materialCrystalSprite != null ? materialCrystalSprite : materialCrystalSprite = CreateFacetedCrystal();

        // Three rising bars and an upward arrow: upgrades, not a sixth power-up.
        public static Sprite PowerUpUpgradeIcon => powerUpUpgradeIcon != null ? powerUpUpgradeIcon : powerUpUpgradeIcon = CreateIcon("Bonus Upgrades", (x, y) =>
        {
            bool bars = y > -0.4f && ((x > -0.4f && x < -0.24f && y < -0.13f)
                || (x > -0.1f && x < 0.06f && y < 0.02f) || (x > 0.2f && x < 0.36f && y < 0.15f));
            bool shaft = Mathf.Abs(x + 0.19f) < 0.05f && y > 0.04f && y < 0.37f;
            bool arrow = y > 0.23f && y < 0.44f && Mathf.Abs(x + 0.19f) < (0.44f - y);
            return bars || shaft || arrow;
        }, 128, 2);

        public static Sprite GetPowerUpIcon(PowerUpType type)
        {
            if (powerUpIcons == null)
            {
                powerUpIcons = new Sprite[5];
                powerUpIcons[0] = CreateIcon("Wormhole Icon", (x, y) =>
                {
                    float ellipse = Mathf.Sqrt((x - 0.12f) * (x - 0.12f) / 0.075f + y * y / 0.1764f);
                    bool portal = ellipse > 0.76f && ellipse < 1f && !(x < 0.12f && Mathf.Abs(y) < 0.13f);
                    bool shaft = x > -0.43f && x < 0.12f && Mathf.Abs(y) < 0.05f;
                    bool arrow = x > -0.03f && x < 0.2f && Mathf.Abs(y) < (0.2f - x) * 0.85f;
                    return portal || shaft || arrow;
                }, 128, 2);
                powerUpIcons[1] = CreateIcon("Orbit Magnet Icon", (x, y) =>
                {
                    float r = Mathf.Sqrt(x * x + (y + 0.04f) * (y + 0.04f));
                    bool curve = y < -0.04f && r < 0.35f && r > 0.18f;
                    bool arms = Mathf.Abs(x) > 0.18f && Mathf.Abs(x) < 0.35f && y >= -0.04f && y < 0.32f && !(y > 0.16f && y < 0.21f);
                    return curve || arms;
                }, 128, 2);
                powerUpIcons[2] = CreateIcon("Shield Power Icon", (x, y) =>
                {
                    float ax = Mathf.Abs(x);
                    float width = y < -0.08f ? (y + 0.44f) * 0.95f : 0.34f;
                    float top = 0.4f - ax * 0.3f;
                    bool shell = y > -0.44f && y < top && ax < width;
                    bool inset = y > -0.29f && y < top - 0.09f && ax < width - 0.09f;
                    bool crest = Mathf.Abs(x) < 0.04f && y > -0.12f && y < 0.16f;
                    return (shell && !inset) || crest;
                }, 128, 2);
                Vector2[] bolt = { new(-0.03f, 0.44f), new(-0.31f, -0.05f), new(-0.06f, -0.05f), new(-0.15f, -0.44f), new(0.33f, 0.12f), new(0.06f, 0.12f), new(0.2f, 0.44f) };
                powerUpIcons[3] = CreateIcon("Ion Overdrive Icon", (x, y) => IconContains(bolt, x, y), 128, 2);
                powerUpIcons[4] = CreateIcon("Quantum Extractor Icon", (x, y) =>
                {
                    float diamond = Mathf.Abs(x) / 0.19f + Mathf.Abs(y) / 0.31f;
                    bool crystal = diamond < 1f && !(Mathf.Abs(x) < 0.022f && y > -0.16f && y < 0.17f);
                    float ax = Mathf.Abs(x);
                    bool arrows = ax > 0.24f && ax < 0.4f && Mathf.Abs(y) < (ax - 0.24f) * 0.8f;
                    bool tails = ax > 0.33f && ax < 0.46f && Mathf.Abs(y) < 0.035f;
                    return crystal || arrows || tails;
                }, 128, 2);
            }
            return powerUpIcons[Mathf.Clamp((int)type, 0, powerUpIcons.Length - 1)];
        }

        public static Sprite GetBackgroundSprite(int index)
        {
            if (index == 15)
                return herbalBackground != null ? herbalBackground : herbalBackground = LoadSingleSprite("Art/herbal-background", "Brume Herbal");
            if (index >= 4)
            {
                if (generatedBackgrounds == null) generatedBackgrounds = LoadGridSprites("Art/expanded-backgrounds-generated", 4, 3, "Generated Background");
                int variant = Mathf.Clamp(index - 4, 0, 10);
                int atlasIndex = (2 - variant / 4) * 4 + variant % 4;
                return generatedBackgrounds.Length > atlasIndex ? generatedBackgrounds[atlasIndex] : ExpandedCosmetics.Background(variant);
            }
            if (index == 3) return auroraBackground != null ? auroraBackground : auroraBackground = LoadSingleSprite("Art/aurora-background", "Voile Boreal");
            if (index <= 0) return SpaceBackgroundSprite;
            if (cosmeticBackgrounds == null) cosmeticBackgrounds = LoadGridSprites("Art/cosmetics-backgrounds-atlas", 2, 1, "Background Cosmetic");
            return cosmeticBackgrounds.Length > 0 ? cosmeticBackgrounds[Mathf.Clamp(index - 1, 0, cosmeticBackgrounds.Length - 1)] : SpaceBackgroundSprite;
        }

        public static Sprite FlameSprite => flameSprite != null ? flameSprite : flameSprite = CreateIcon("Engine Flame", (x, y) =>
        {
            float width = Mathf.Lerp(0.08f, 0.3f, Mathf.Clamp01(y + 0.5f));
            return y > -0.48f && y < 0.46f && Mathf.Abs(x) < width;
        });

        public static Sprite GetPlanetSprite(int sequence)
        {
            return GetPlanetPackSprite(MetaProgression.Selected(CosmeticKind.PlanetPack), sequence);
        }

        public static Sprite GetPlanetPackSprite(int pack, int sequence)
        {
            if (pack == 15)
            {
                if (herbalPlanets == null) herbalPlanets = LoadGridSprites("Art/herbal-planets", 3, 2, "Monde Herbal");
                int[] atlasByVariant = { 3, 4, 5, 0, 1 };
                int atlasIndex = atlasByVariant[Mathf.Abs(sequence % atlasByVariant.Length)];
                return herbalPlanets.Length > atlasIndex ? herbalPlanets[atlasIndex] : CircleSprite;
            }
            if (pack >= 4)
            {
                int variant = Mathf.Abs(sequence % 5);
                int packOffset = pack - 4;
                if (packOffset < 6)
                {
                    if (generatedPlanetsA == null) generatedPlanetsA = LoadGridSprites("Art/expanded-planets-a-generated", 5, 6, "Generated Planet A");
                    int atlasIndex = (5 - packOffset) * 5 + variant;
                    return generatedPlanetsA.Length > atlasIndex ? generatedPlanetsA[atlasIndex] : ExpandedCosmetics.Planet(packOffset, sequence);
                }
                if (generatedPlanetsB == null) generatedPlanetsB = LoadGridSprites("Art/expanded-planets-b-generated", 5, 5, "Generated Planet B");
                int row = packOffset - 6;
                int generatedIndex = (4 - row) * 5 + variant;
                return generatedPlanetsB.Length > generatedIndex ? generatedPlanetsB[generatedIndex] : ExpandedCosmetics.Planet(packOffset, sequence);
            }
            if (pack == 3)
            {
                if (auroraPlanets == null) auroraPlanets = LoadGridSprites("Art/aurora-planets", 2, 2, "Mondes Aurore");
                if (auroraPlanets.Length == 4) return auroraPlanets[Mathf.Abs(sequence % 4)];
            }
            if (pack > 0)
            {
                if (cosmeticPlanets == null) cosmeticPlanets = LoadGridSprites("Art/cosmetics-planets-atlas", 4, 2, "Planet Cosmetic", true);
                if (cosmeticPlanets.Length > 0)
                {
                    int offset = pack == 1 ? 4 : 0;
                    return cosmeticPlanets[offset + Mathf.Abs(sequence) % 4];
                }
            }
            if (planetSprites == null) planetSprites = LoadGridSprites("Art/planets-sheet", 3, 2, "Planet");
            return planetSprites.Length > 0 ? planetSprites[Mathf.Abs(sequence) % planetSprites.Length] : CircleSprite;
        }

        public static Sprite GetDebrisSprite(int sequence)
        {
            if (debrisSprites == null) debrisSprites = LoadGridSprites("Art/debris-sheet", 3, 2, "Debris");
            return debrisSprites.Length > 0 ? debrisSprites[Mathf.Abs(sequence * 5 + 3) % debrisSprites.Length] : SquareSprite;
        }

        private static Sprite LoadSingleSprite(string resourcePath, string name)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return CircleSprite;
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.one * 0.5f, texture.height, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        private static Sprite[] LoadGridSprites(string resourcePath, int columns, int rows, string prefix, bool removeLightEdgeBackdrop = false)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return Array.Empty<Sprite>();
            float cellWidth = texture.width / (float)columns;
            float cellHeight = texture.height / (float)rows;
            var sprites = new Sprite[columns * rows];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = row * columns + column;
                    Rect rect = new Rect(column * cellWidth, row * cellHeight, cellWidth, cellHeight);
                    if (removeLightEdgeBackdrop)
                    {
                        int width = Mathf.RoundToInt(cellWidth); int height = Mathf.RoundToInt(cellHeight);
                        Color32[] pixels = texture.GetPixels32(0);
                        int originX = Mathf.RoundToInt(column * cellWidth); int originY = Mathf.RoundToInt(row * cellHeight);
                        // Jupiter and Saturn overlap the mathematically even cell boundary.
                        // Move that one boundary left so Jupiter cannot inherit a ring fragment
                        // and Saturn keeps the complete left side of its rings.
                        if (rows == 2 && columns == 4 && row == 1 && column == 3)
                        {
                            originX = Mathf.RoundToInt(texture.width * 0.735f);
                            width = texture.width - originX;
                        }
                        var cellPixels = new Color32[width * height];
                        for (int y = 0; y < height; y++) Array.Copy(pixels, (originY + y) * texture.width + originX, cellPixels, y * width, width);
                        RemoveConnectedLightBackdrop(cellPixels, width, height);
                        if (index == 6) KeepCenterConnectedSubject(cellPixels, width, height);
                        var cellTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = prefix + " Texture " + index, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                        cellTexture.SetPixels32(cellPixels); cellTexture.Apply(false, true);
                        sprites[index] = Sprite.Create(cellTexture, new Rect(0f, 0f, width, height), Vector2.one * 0.5f, height, 0, SpriteMeshType.FullRect);
                    }
                    else sprites[index] = Sprite.Create(texture, rect, Vector2.one * 0.5f, cellHeight, 0, SpriteMeshType.FullRect);
                    sprites[index].name = prefix + " " + index;
                }
            }
            return sprites;
        }

        private static void RemoveConnectedLightBackdrop(Color32[] pixels, int width, int height)
        {
            var visited = new bool[pixels.Length];
            var queue = new System.Collections.Generic.Queue<int>();
            void Seed(int index) { if (!visited[index] && IsLightBackdrop(pixels[index])) { visited[index] = true; queue.Enqueue(index); } }
            for (int x = 0; x < width; x++) { Seed(x); Seed((height - 1) * width + x); }
            for (int y = 0; y < height; y++) { Seed(y * width); Seed(y * width + width - 1); }
            while (queue.Count > 0)
            {
                int index = queue.Dequeue(); Color32 color = pixels[index]; color.a = 0; pixels[index] = color;
                int x = index % width; int y = index / width;
                if (x > 0) Seed(index - 1); if (x + 1 < width) Seed(index + 1);
                if (y > 0) Seed(index - width); if (y + 1 < height) Seed(index + width);
            }
        }

        private static bool IsLightBackdrop(Color32 color)
        {
            int maximum = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            int minimum = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            return minimum >= 210 && maximum - minimum <= 20;
        }

        private static void KeepCenterConnectedSubject(Color32[] pixels, int width, int height)
        {
            var kept = new bool[pixels.Length];
            var queue = new System.Collections.Generic.Queue<int>();
            int center = (height / 2) * width + width / 2;
            if (pixels[center].a == 0) return;
            kept[center] = true; queue.Enqueue(center);
            void Visit(int index) { if (!kept[index] && pixels[index].a > 0) { kept[index] = true; queue.Enqueue(index); } }
            while (queue.Count > 0)
            {
                int index = queue.Dequeue(); int x = index % width; int y = index / width;
                if (x > 0) Visit(index - 1); if (x + 1 < width) Visit(index + 1);
                if (y > 0) Visit(index - width); if (y + 1 < height) Visit(index + width);
            }
            for (int i = 0; i < pixels.Length; i++)
                if (!kept[i]) { Color32 color = pixels[i]; color.a = 0; pixels[i] = color; }
        }


        private static bool IconContains(Vector2[] polygon, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i], b = polygon[j];
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside;
        }

        private static Sprite CreateIcon(string name, Func<float, float, bool> sample, int size = 64, int samples = 1)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float coverage = 0f;
                    for (int sy = 0; sy < samples; sy++)
                    for (int sx = 0; sx < samples; sx++)
                        if (sample((x + (sx + 0.5f) / samples) / size - 0.5f, (y + (sy + 0.5f) / samples) / size - 0.5f)) coverage++;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, coverage / (samples * samples));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            sprite.name = name;
            return sprite;
        }

        private static Sprite CreateFacetedCrystal()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Faceted Material Crystal",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f) / size - 0.5f;
                float py = (y + 0.5f) / size - 0.5f;
                float halfWidth = py >= 0.12f ? Mathf.Lerp(0.3f, 0.08f, Mathf.InverseLerp(0.12f, 0.48f, py))
                    : Mathf.Lerp(0.055f, 0.3f, Mathf.InverseLerp(-0.48f, 0.12f, py));
                if (Mathf.Abs(px) > halfWidth) { pixels[y * size + x] = Color.clear; continue; }
                float edge = Mathf.Clamp01((halfWidth - Mathf.Abs(px)) * 24f);
                float facet = px < -0.04f ? 0.5f : px > 0.11f ? 0.7f : 1f;
                float band = py > 0.1f ? 1f : py > -0.15f ? 0.82f : 0.62f;
                byte shade = (byte)Mathf.RoundToInt(255f * facet * band * Mathf.Lerp(0.72f, 1f, edge));
                pixels[y * size + x] = new Color32(shade, shade, shade, 255);
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            sprite.name = "Faceted Material Crystal";
            return sprite;
        }

        public static AudioClip CreateTone(string name, float frequency, float duration, float volume)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                float envelope = Mathf.Pow(1f - i / (float)sampleCount, 2.2f);
                float fundamental = Mathf.Sin(2f * Mathf.PI * frequency * time);
                float harmonic = Mathf.Sin(2f * Mathf.PI * frequency * 2f * time) * 0.18f;
                samples[i] = (fundamental + harmonic) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }


        public static AudioClip CreateChargeLoop()
        {
            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float phase = i / (float)sampleCount;
                float pulse = 0.45f + Mathf.Sin(phase * Mathf.PI * 2f) * 0.12f;
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 245f * t));
                float overtone = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 490f * t)) * 0.22f;
                samples[i] = (square + overtone) * pulse * 0.075f;
            }
            AudioClip clip = AudioClip.Create("Multiplier Charge", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public static AudioClip CreateSkipStinger()
        {
            const int sampleRate = 44100;
            const float duration = 0.42f;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            var samples = new float[sampleCount];
            float[] notes = { 659.25f, 830.61f, 987.77f, 1318.51f };
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                int step = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(time / 0.085f));
                float stepPhase = Mathf.Repeat(time, 0.085f) / 0.085f;
                float envelope = Mathf.Exp(-stepPhase * 3.8f) * Mathf.Clamp01((duration - time) / 0.06f);
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * notes[step] * time));
                float octave = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * notes[step] * 2f * time)) * 0.18f;
                float finalChord = time > 0.3f
                    ? (Mathf.Sin(2f * Mathf.PI * 1318.51f * time) + Mathf.Sin(2f * Mathf.PI * 1661.22f * time)) * 0.12f * Mathf.Clamp01((duration - time) / 0.12f)
                    : 0f;
                samples[i] = Mathf.Clamp((square + octave) * envelope * 0.16f + finalChord, -0.8f, 0.8f);
            }
            AudioClip clip = AudioClip.Create("Skip Stinger", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
