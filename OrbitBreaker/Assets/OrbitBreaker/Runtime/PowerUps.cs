using System;
using UnityEngine;

namespace OrbitBreaker
{
    public enum PowerUpType { Wormhole, OrbitMagnet, Shield, IonOverdrive, QuantumExtractor }

    public readonly struct PowerUpDefinition
    {
        public PowerUpDefinition(PowerUpType type, string name, string description, Color color, int basePrice)
        { Type = type; Name = name; Description = description; Color = color; BasePrice = basePrice; }
        public PowerUpType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public Color Color { get; }
        public int BasePrice { get; }
        public int UpgradePrice(int currentLevel) => currentLevel >= 5 ? 0 : Mathf.RoundToInt(BasePrice * Mathf.Pow(1.72f, currentLevel - 1) / 10f) * 10;
    }

    public static class PowerUpProgression
    {
        private const string Prefix = "OrbitBreaker.PowerUps.";
        public const int MaxInventory = 5;
        public static readonly PowerUpDefinition[] Catalog =
        {
            new(PowerUpType.Wormhole, "TROU DE VER", "Traverse l'hyperespace vers une orbite sûre et gagne de la distance.", new Color(0.72f, 0.3f, 1f), 220),
            new(PowerUpType.OrbitMagnet, "ATTRACTION", "Courbe doucement le vol vers les orbites accessibles.", new Color(0.2f, 0.92f, 1f), 150),
            new(PowerUpType.Shield, "BOUCLIER", "Bloque les collisions avec tous les débris.", new Color(0.25f, 1f, 0.7f), 180),
            new(PowerUpType.IonOverdrive, "SURCHARGE ION", "Accélère la fusée et prolonge son autonomie de vol.", new Color(1f, 0.45f, 0.18f), 170),
            new(PowerUpType.QuantumExtractor, "EXTRACTEUR", "Attire les cristaux proches et multiplie leur valeur.", new Color(1f, 0.78f, 0.2f), 140)
        };

        public static PowerUpDefinition Definition(PowerUpType type) => Catalog[(int)type];
        public static int StoredCount(PowerUpType type) => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + type + ".Stock", 0), 0, MaxInventory);
        public static int TotalStored()
        {
            int total = 0;
            for (int i = 0; i < Catalog.Length; i++) total += StoredCount((PowerUpType)i);
            return total;
        }
        public static bool TryStore(PowerUpType type)
        {
            if (StoredCount(type) >= MaxInventory) return false;
            PlayerPrefs.SetInt(Prefix + type + ".Stock", StoredCount(type) + 1);
            PlayerPrefs.Save();
            return true;
        }
        public static bool TryConsume(PowerUpType type)
        {
            int count = StoredCount(type);
            if (count <= 0) return false;
            PlayerPrefs.SetInt(Prefix + type + ".Stock", count - 1);
            PlayerPrefs.Save();
            return true;
        }
        public static int Level(PowerUpType type) => Mathf.Clamp(PlayerPrefs.GetInt(Prefix + type + ".Level", 1), 1, 5);
        public static bool Upgrade(PowerUpType type)
        {
            int level = Level(type);
            if (level >= 5) return false;
            int price = Definition(type).UpgradePrice(level);
            if (!MetaProgression.TrySpendMaterials(price)) return false;
            PlayerPrefs.SetInt(Prefix + type + ".Level", level + 1);
            PlayerPrefs.Save();
            return true;
        }

        public static float Duration(PowerUpType type, int level) => type switch
        {
            PowerUpType.OrbitMagnet => 4.5f + level * 1.1f,
            PowerUpType.Shield => 8f + level * 2f,
            PowerUpType.IonOverdrive => 5f + level * 1.2f,
            PowerUpType.QuantumExtractor => 7f + level * 1.6f,
            _ => 0f
        };
        public static float MagnetStrength(int level) => 0.75f + level * 0.22f;
        public static float OverdriveSpeed(int level) => 1.08f + level * 0.035f;
        public static float ExtraFlightTime(int level) => 0.3f + level * 0.16f;
        public static float ExtractorRadius(int level) => 0.55f + level * 0.24f;
        public static int ExtractorMultiplier(int level) => level >= 4 ? 3 : 2;
        public static int WormholeDistance(int level) => 80 + level * 20;
        public static int WormholeOrbitSkip(int level) => 5 + level;
        public static string Stats(PowerUpType type, int level) => type switch
        {
            PowerUpType.Wormhole => "+" + WormholeDistance(level) + " UA · " + WormholeOrbitSkip(level) + " orbites",
            PowerUpType.OrbitMagnet => Duration(type, level).ToString("0.0") + " s · force " + MagnetStrength(level).ToString("0.00"),
            PowerUpType.Shield => Duration(type, level).ToString("0") + " s d'immunité",
            PowerUpType.IonOverdrive => Duration(type, level).ToString("0.0") + " s · vitesse +" + Mathf.RoundToInt((OverdriveSpeed(level) - 1f) * 100f) + "%",
            PowerUpType.QuantumExtractor => Duration(type, level).ToString("0.0") + " s · rayon " + ExtractorRadius(level).ToString("0.00") + " · x" + ExtractorMultiplier(level),
            _ => string.Empty
        };
    }

    public sealed class PowerUpPickup : MonoBehaviour
    {
        private SpriteRenderer core;
        private SpriteRenderer halo;
        private float phase;
        public int Sequence { get; private set; }
        public PowerUpType Type { get; private set; }
        public const float Radius = 0.26f;

        public void Initialize(int sequence, Vector2 position, PowerUpType type)
        {
            Sequence = sequence; Type = type; phase = sequence * 0.61f + (int)type;
            gameObject.name = DefinitionName(type) + " Pickup (" + sequence + ")";
            gameObject.SetActive(true); transform.position = position; transform.localScale = Vector3.one * 0.52f;
            EnsureVisuals();
            Color color = PowerUpProgression.Definition(type).Color;
            core.sprite = RuntimeAssets.GetPowerUpIcon(type);
            core.color = color;
            halo.color = new Color(color.r, color.g, color.b, 0.17f);
        }

        public bool Collect() { if (!gameObject.activeSelf) return false; gameObject.SetActive(false); return true; }
        private void Update()
        {
            // Keep the pictogram upright so it can be recognised during a fast skip.
            float pulse = 1.18f + Mathf.Sin(Time.unscaledTime * 3.8f + phase) * 0.12f;
            halo.transform.localScale = Vector3.one * pulse;
        }
        private void EnsureVisuals()
        {
            if (core != null) return;
            halo = new GameObject("Energy Halo").AddComponent<SpriteRenderer>(); halo.transform.SetParent(transform, false);
            halo.sprite = RuntimeAssets.CircleSprite; halo.sortingOrder = 7;
            core = new GameObject("Power Icon").AddComponent<SpriteRenderer>(); core.transform.SetParent(transform, false);
            core.sortingOrder = 8;
        }
        private static string DefinitionName(PowerUpType type) => PowerUpProgression.Definition(type).Name;
    }
}
