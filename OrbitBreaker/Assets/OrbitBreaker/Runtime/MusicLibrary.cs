using UnityEngine;

namespace OrbitBreaker
{
    public static class MusicLibrary
    {
        private static readonly string[] Keys = { "neon_orbit", "starlight", "void_runner", "cosmic_disco", "lunar_lounge", "asteroid_funk", "binary_chase", "aurora_dream", "rusty_station", "solar_carnival", "deep_blue", "pocket_galaxy", "meow_sad", "herbal_reggae", "cyberpunk" };
        private static readonly AudioClip[] Clips = new AudioClip[15];
        private static readonly string[] Descriptions = {
            "132 BPM · ARCADE NÉON", "116 BPM · LUMINEUSE", "148 BPM · POURSUITE SOMBRE",
            "124 BPM · GROOVE COSMIQUE", "108 BPM · LOUNGE LUNAIRE", "126 BPM · BASSE FUNKY",
            "152 BPM · COURSE BINAIRE", "118 BPM · RÊVE BORÉAL", "112 BPM · STATION MÉTALLIQUE",
            "136 BPM · CARNAVAL SOLAIRE", "100 BPM · PROFONDEURS", "128 BPM · MÉLODIE DE POCHE", "BALLADE IA · MEOW MÉLANCOLIQUE",
            "REGGAE · AMBIANCE VERTE", "140 BPM · COURSE NÉON URBAINE"
        };
        public static string Description(int index) => Descriptions[Mathf.Clamp(index, 0, Descriptions.Length - 1)];
        public static bool IsOwned(int index)
        {
            foreach (var item in MetaProgression.Catalog)
                if (item.Kind == CosmeticKind.Music && item.VisualIndex == index) return MetaProgression.Owned(item);
            return false;
        }
        public static int Equipped => IsOwned(MetaProgression.Selected(CosmeticKind.Music)) ? MetaProgression.Selected(CosmeticKind.Music) : 0;
        public static AudioClip Load(int index)
        {
            index = Mathf.Clamp(index, 0, Keys.Length - 1);
            if (Clips[index] == null) Clips[index] = Resources.Load<AudioClip>("OrbitMusic/" + Keys[index]);
            return Clips[index];
        }

        public static void ReleaseUnused(AudioClip active)
        {
            foreach (var clip in Clips)
                if (clip != null && clip != active && clip.loadState == AudioDataLoadState.Loaded)
                    clip.UnloadAudioData();
        }
    }
}
