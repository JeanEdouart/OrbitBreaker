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
        private static Sprite rocketSprite;
        private static Sprite flameSprite;
        private static Sprite[] planetSprites;
        private static Sprite[] debrisSprites;
        private static Sprite spaceBackgroundSprite;

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

        public static Sprite RocketSprite => rocketSprite != null ? rocketSprite : rocketSprite = LoadSingleSprite("Art/rocket", "Player Rocket");
        public static Sprite SpaceBackgroundSprite => spaceBackgroundSprite != null ? spaceBackgroundSprite : spaceBackgroundSprite = LoadSingleSprite("Art/space-background", "Space Background");

        public static Sprite FlameSprite => flameSprite != null ? flameSprite : flameSprite = CreateIcon("Engine Flame", (x, y) =>
        {
            float width = Mathf.Lerp(0.08f, 0.3f, Mathf.Clamp01(y + 0.5f));
            return y > -0.48f && y < 0.46f && Mathf.Abs(x) < width;
        });

        public static Sprite GetPlanetSprite(int sequence)
        {
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

        private static Sprite[] LoadGridSprites(string resourcePath, int columns, int rows, string prefix)
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
                    sprites[index] = Sprite.Create(texture, rect, Vector2.one * 0.5f, cellHeight, 0, SpriteMeshType.FullRect);
                    sprites[index].name = prefix + " " + index;
                }
            }
            return sprites;
        }

        private static Sprite CreateIcon(string name, Func<float, float, bool> sample)
        {
            const int size = 64;
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
                    float px = (x + 0.5f) / size - 0.5f;
                    float py = (y + 0.5f) / size - 0.5f;
                    pixels[y * size + x] = sample(px, py) ? Color.white : Color.clear;
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
            sprite.name = name;
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

        public static AudioClip CreateChiptuneLoop()
        {
            const int sampleRate = 44100;
            const float beatDuration = 60f / 132f;
            const int beats = 64;
            int sampleCount = Mathf.CeilToInt(beats * beatDuration * sampleRate);
            var samples = new float[sampleCount];
            int[] melody = { 76, 79, 83, 79, 74, 76, 79, 71, 72, 76, 79, 76, 69, 72, 76, 67 };
            int[] roots = { 48, 45, 41, 43 };

            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)sampleRate;
                int beat = Mathf.FloorToInt(time / beatDuration);
                float beatPhase = Mathf.Repeat(time / beatDuration, 1f);
                int root = roots[(beat / 8) % roots.Length];
                int halfBeat = Mathf.FloorToInt(time / (beatDuration * 0.5f));
                int note = melody[halfBeat % melody.Length];
                float rootFrequency = 440f * Mathf.Pow(2f, (root - 69) / 12f);
                float noteFrequency = 440f * Mathf.Pow(2f, (note - 69) / 12f);
                float halfPhase = Mathf.Repeat(time / (beatDuration * 0.5f), 1f);
                float bass = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * rootFrequency * time)) * 0.065f * Mathf.Exp(-beatPhase * 3.2f);
                float leadEnvelope = Mathf.Exp(-halfPhase * 4f);
                float softSquare = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * noteFrequency * time)) * 0.045f * leadEnvelope;
                float sparkle = Mathf.Sin(2f * Mathf.PI * noteFrequency * 2f * time) * 0.018f * leadEnvelope;
                float kick = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(105f, 48f, beatPhase) * time) * Mathf.Exp(-beatPhase * 13f) * 0.13f;
                float eighthPhase = Mathf.Repeat(time / (beatDuration * 0.5f), 1f);
                float noise = Mathf.Repeat(Mathf.Sin(i * 12.9898f) * 43758.5453f, 2f) - 1f;
                float hat = noise * Mathf.Exp(-eighthPhase * 26f) * 0.025f;
                float backbeat = beat % 4 == 1 || beat % 4 == 3 ? noise * Mathf.Exp(-beatPhase * 18f) * 0.055f : 0f;
                samples[i] = Mathf.Clamp(bass + softSquare + sparkle + kick + hat + backbeat, -0.82f, 0.82f);
            }

            AudioClip clip = AudioClip.Create("Neon Orbit 132", sampleCount, 1, sampleRate, false);
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
