using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed class OrbitFeedback : MonoBehaviour
    {
        private const string MasterVolumeKey = "OrbitBreaker.Audio.Master";
        private const string MusicVolumeKey = "OrbitBreaker.Audio.Music";
        private const string EffectsVolumeKey = "OrbitBreaker.Audio.Effects";
        private AudioSource audioSource;
        private AudioSource musicSource;
        public int MusicTrack { get; private set; }
        private Coroutine musicTransition;
        private Coroutine musicPreview;

        public void SelectMusic(int index)
        {
            if (!MusicLibrary.IsOwned(index)) return;
            MusicTrack = index;
            StopMusicPreview();
        }

        public void PreviewMusic(int index)
        {
            if (musicPreview != null) StopCoroutine(musicPreview);
            ChangeMusic(index);
            musicPreview = StartCoroutine(EndMusicPreview());
        }

        private IEnumerator EndMusicPreview()
        {
            float time = 0f;
            while (time < 8f) { time += Time.unscaledDeltaTime; yield return null; }
            musicPreview = null;
            ChangeMusic(MusicTrack);
        }

        public void StopMusicPreview()
        {
            if (musicPreview != null) { StopCoroutine(musicPreview); musicPreview = null; }
            ChangeMusic(MusicTrack);
        }

        private void ChangeMusic(int index)
        {
            var clip = MusicLibrary.Load(index);
            if (musicTransition != null) StopCoroutine(musicTransition);
            musicTransition = null;
            if (clip == null || musicSource.clip == clip) { musicSource.volume = MusicVolume; return; }
            musicTransition = StartCoroutine(FadeMusic(clip));
        }

        private IEnumerator FadeMusic(AudioClip clip)
        {
            if(clip.loadState==AudioDataLoadState.Unloaded)clip.LoadAudioData();
            while(clip.loadState==AudioDataLoadState.Loading)yield return null;
            if(clip.loadState==AudioDataLoadState.Failed)yield break;
            float initial = musicSource.volume;
            for (float t = 0f; t < 0.15f; t += Time.unscaledDeltaTime)
            { musicSource.volume = initial * (1f - t / 0.15f); yield return null; }
            musicSource.clip = clip; musicSource.Play();
            MusicLibrary.ReleaseUnused(clip);
            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            { musicSource.volume = MusicVolume * t / 0.2f; yield return null; }
            musicSource.volume = MusicVolume; musicTransition = null;
        }
        private AudioSource chargeSource;
        private AudioSource skipSource;
        private AudioSource warpSource;
        private AudioClip launchClip;
        private AudioClip captureClip;
        private AudioClip perfectClip;
        private AudioClip synchronizationMissClip;
        private AudioClip deathClip;
        private AudioClip skipClip;
        private AudioClip nearMissClip;
        private AudioClip materialClip;
        private AudioClip challengeCompleteClip;
        private AudioClip challengeRewardClip;
        private AudioClip powerUpClip;
        private AudioClip reviveClip;
        private AudioClip reviveIntroClip;
        private AudioClip dailyRewardTickClip;
        private AudioClip dailyRewardRevealClip;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float EffectsVolume { get; private set; }

        public void Initialize()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            MusicTrack = MusicLibrary.Equipped;
            musicSource.clip = MusicLibrary.Load(MusicTrack);
            chargeSource = gameObject.AddComponent<AudioSource>();
            chargeSource.playOnAwake = false;
            chargeSource.loop = true;
            chargeSource.clip = RuntimeAssets.CreateChargeLoop();
            skipSource = gameObject.AddComponent<AudioSource>();
            skipSource.playOnAwake = false;
            warpSource = gameObject.AddComponent<AudioSource>();
            warpSource.playOnAwake = false;
            warpSource.loop = true;
            warpSource.clip = RuntimeAssets.CreateChargeLoop();
            launchClip = RuntimeAssets.CreateTone("Launch", 340f, 0.09f, 0.32f);
            captureClip = RuntimeAssets.CreateTone("Capture", 620f, 0.12f, 0.34f);
            perfectClip = RuntimeAssets.CreateTone("Perfect", 880f, 0.16f, 0.34f);
            synchronizationMissClip = RuntimeAssets.CreateTone("Synchronization Miss", 245f, 0.12f, 0.22f);
            deathClip = RuntimeAssets.CreateTone("Break", 115f, 0.32f, 0.44f);
            skipClip = RuntimeAssets.CreateSkipStinger();
            nearMissClip = RuntimeAssets.CreateTone("Near Miss", 1120f, 0.11f, 0.3f);
            materialClip = RuntimeAssets.CreateTone("Material", 1320f, 0.13f, 0.32f);
            challengeCompleteClip = RuntimeAssets.CreateTone("Challenge Complete", 940f, 0.2f, 0.3f);
            challengeRewardClip = RuntimeAssets.CreateSkipStinger();
            powerUpClip = RuntimeAssets.CreateTone("Power Up", 740f, 0.22f, 0.34f);
            reviveClip = RuntimeAssets.CreateReviveFanfare();
            reviveIntroClip = RuntimeAssets.CreateEpicRevive();
            dailyRewardTickClip = RuntimeAssets.CreateTone("Daily Reward Tick", 520f, 0.055f, 0.18f);
            dailyRewardRevealClip = RuntimeAssets.CreateSkipStinger();
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.85f);
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.55f);
            EffectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, 0.8f);
            ApplyVolumes();
            musicSource.Play();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            ApplyVolumes();
        }

        public void UpdateWarpAudio(float intensity)
        {
            if (warpSource == null) return;
            if (intensity <= 0.001f) { warpSource.Stop(); return; }
            UpdateCharge(1f, false);
            warpSource.volume = EffectsVolume * intensity * 0.3f;
            warpSource.pitch = Mathf.Lerp(0.55f, 1.8f, intensity);
            if (!warpSource.isPlaying) warpSource.Play();
        }

        public void SetMusicVolume(float value)
        {
            MusicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
            ApplyVolumes();
        }

        public void SetEffectsVolume(float value)
        {
            EffectsVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(EffectsVolumeKey, EffectsVolume);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            AudioListener.volume = MasterVolume;
            if (musicSource != null) musicSource.volume = MusicVolume;
            if (audioSource != null) audioSource.volume = EffectsVolume;
            if (chargeSource != null) chargeSource.volume = EffectsVolume * 0.42f;
            if (skipSource != null) skipSource.volume = EffectsVolume * 0.9f;
        }

        public void Launch(Vector2 position)
        {
            audioSource.PlayOneShot(launchClip);
            StartCoroutine(Pulse(position, new Color(0.25f, 0.9f, 1f, 0.8f), 0.28f, 0.7f));
        }

        public void Capture(Vector2 position, bool perfect, int skippedAnchors)
        {
            UpdateCharge(1f, false);
            audioSource.PlayOneShot(perfect ? perfectClip : captureClip);
            if (skippedAnchors > 0)
            {
                skipSource.pitch = Mathf.Clamp(0.96f + (skippedAnchors - 1) * 0.08f, 0.96f, 1.28f);
                skipSource.PlayOneShot(skipClip);
                if (skippedAnchors >= 2) StartCoroutine(DoubleSkipHaptic());
                else TriggerHaptic(46L, 115);
            }
            else TriggerHaptic(22L, 42);
            StartCoroutine(Pulse(position, perfect ? new Color(1f, 0.72f, 0.2f, 0.9f) : new Color(0.2f, 1f, 0.75f, 0.85f), 0.4f, perfect ? 1.5f : 1.05f));
        }

        public void SynchronizationMiss(Vector2 position)
        {
            audioSource.PlayOneShot(synchronizationMissClip, 0.72f);
            StartCoroutine(Pulse(position, new Color(0.35f, 0.65f, 1f, 0.58f), 0.22f, 0.72f));
        }

        public void UpdateCharge(float multiplier, bool flying)
        {
            if (chargeSource == null) return;
            if (!flying)
            {
                if (chargeSource.isPlaying) chargeSource.Stop();
                return;
            }
            float normalized = Mathf.InverseLerp(1f, GameTuning.MaxDistanceMultiplier, multiplier);
            chargeSource.pitch = Mathf.Lerp(0.82f, 1.85f, normalized);
            chargeSource.volume = EffectsVolume * Mathf.Lerp(0.18f, 0.48f, normalized);
            if (!chargeSource.isPlaying) chargeSource.Play();
        }

        public void NearMiss(Vector2 position, int chain)
        {
            audioSource.pitch = Mathf.Clamp(1f + (chain - 1) * 0.08f, 1f, 1.35f);
            audioSource.PlayOneShot(nearMissClip);
            audioSource.pitch = 1f;
            TriggerHaptic(18L, 55);
            StartCoroutine(Pulse(position, new Color(1f, 0.7f, 0.2f, 0.9f), 0.24f, 0.85f));
        }

        public void Material(Vector2 position, int value)
        {
            audioSource.pitch = value >= 7 ? 1.35f : value >= 3 ? 1.16f : 1f;
            audioSource.PlayOneShot(materialClip);
            audioSource.pitch = 1f;
            TriggerHaptic(value >= 7 ? 45L : 20L, value >= 7 ? 120 : 52);
            StartCoroutine(Pulse(position, value >= 7 ? new Color(1f, 0.72f, 0.2f, 0.95f) : new Color(0.25f, 0.95f, 1f, 0.9f), 0.24f, value >= 7 ? 1.25f : 0.8f));
        }

        public void ChallengeCompleted()
        {
            audioSource.pitch = 1.08f;
            audioSource.PlayOneShot(challengeCompleteClip, 0.7f);
            audioSource.pitch = 1f;
            TriggerHaptic(28L, 70);
        }

        public void ChallengeRewardClaimed()
        {
            audioSource.pitch = 1.22f;
            audioSource.PlayOneShot(challengeRewardClip, 0.78f);
            audioSource.pitch = 1f;
            TriggerHaptic(48L, 120);
        }

        public void PowerUp(Vector2 position, PowerUpType type, bool success)
        {
            PowerUpDefinition definition = PowerUpProgression.Definition(type);
            audioSource.pitch = success ? 1f + (int)type * 0.08f : 0.7f;
            audioSource.PlayOneShot(powerUpClip, success ? 0.9f : 0.45f);
            audioSource.pitch = 1f;
            if (success) TriggerHaptic(30L, 85);
            StartCoroutine(Pulse(position, success ? definition.Color : new Color(1f, 0.28f, 0.3f), 0.3f, success ? 1.15f : 0.6f));
        }

        public void SixtySevenReviveBegin()
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reviveIntroClip, 0.55f);
        }

        public void SixtySevenRevive(Vector2 position)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(reviveClip, 1f);
            StartCoroutine(Pulse(position, Color.white, 0.7f, 2.6f));
            StartCoroutine(PulseDelayed(position, new Color(1f, 0.25f, 0.85f, 0.7f), 0.12f));
            StartCoroutine(PulseDelayed(position, new Color(0.25f, 0.95f, 1f, 0.6f), 0.24f));
            TriggerHaptic(70L, 180);
        }

        public void DailyRewardTick(float progress)
        {
            if (audioSource == null || dailyRewardTickClip == null) return;
            audioSource.pitch = Mathf.Lerp(.82f, 1.72f, Mathf.Clamp01(progress));
            audioSource.PlayOneShot(dailyRewardTickClip, Mathf.Lerp(.28f, .62f, progress));
            audioSource.pitch = 1f;
        }

        public void DailyRewardReveal(DailyChestRarity rarity)
        {
            if (audioSource == null || dailyRewardRevealClip == null) return;
            audioSource.pitch = rarity == DailyChestRarity.Rainbow ? 1.32f : rarity == DailyChestRarity.Gold ? 1.18f : rarity == DailyChestRarity.Violet ? 1.08f : 1f;
            audioSource.PlayOneShot(dailyRewardRevealClip, rarity >= DailyChestRarity.Gold ? 1f : .82f);
            audioSource.pitch = 1f;
            TriggerHaptic(rarity >= DailyChestRarity.Gold ? 85L : 52L, rarity == DailyChestRarity.Rainbow ? 220 : 150);
        }

        public void DailyRewardHaptic(DailyChestRarity rarity)
        {
            TriggerHaptic(rarity >= DailyChestRarity.Gold ? 70L : 35L, rarity == DailyChestRarity.Rainbow ? 190 : 110);
        }

        public void Death(Vector2 position, DeathReason reason)
        {
            UpdateCharge(1f, false);
            audioSource.PlayOneShot(deathClip);
            if (reason == DeathReason.Breaker)
            {
                StartCoroutine(Pulse(position, new Color(1f, 0.15f, 0.35f, 0.92f), 0.58f, 1.8f));
                StartCoroutine(Explosion(position));
            }
            else
            {
                StartCoroutine(Pulse(position, new Color(0.22f, 0.68f, 1f, 0.82f), 0.85f, 3.1f));
                StartCoroutine(PulseDelayed(position, new Color(0.55f, 0.3f, 1f, 0.55f), 0.16f));
            }
            TriggerHaptic(135L, 210);
        }

        private IEnumerator DoubleSkipHaptic()
        {
            TriggerHaptic(38L, 125);
            float elapsed = 0f;
            while (elapsed < 0.095f)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            TriggerHaptic(58L, 175);
        }

        private static void TriggerHaptic(long durationMilliseconds, int amplitude)
        {
            if (!GamePreferences.Haptics) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                int sdk = version.GetStatic<int>("SDK_INT");
                if (sdk >= 26)
                {
                    using var vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    using AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", durationMilliseconds, Mathf.Clamp(amplitude, 1, 255));
                    vibrator.Call("vibrate", effect);
                }
                else vibrator.Call("vibrate", durationMilliseconds);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Haptic feedback unavailable: " + exception.Message);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private IEnumerator PulseDelayed(Vector2 position, Color color, float delay)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            yield return Pulse(position, color, 0.9f, 4.2f);
        }

        private IEnumerator Explosion(Vector2 position)
        {
            int count = GamePreferences.EnhancedEffects ? 12 : 6;
            var shards = new GameObject[count];
            var renderers = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                float angle = i / (float)count * Mathf.PI * 2f + 0.13f;
                shards[i] = new GameObject("Explosion Shard");
                shards[i].transform.position = position;
                shards[i].transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 45f);
                shards[i].transform.localScale = new Vector3(0.08f, 0.22f, 1f);
                renderers[i] = shards[i].AddComponent<SpriteRenderer>();
                renderers[i].sprite = RuntimeAssets.SquareSprite;
                renderers[i].sortingOrder = 22;
            }
            float elapsed = 0f;
            const float duration = 0.62f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                for (int i = 0; i < count; i++)
                {
                    float angle = i / (float)count * Mathf.PI * 2f + 0.13f;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    shards[i].transform.position = position + direction * (2.2f * (1f - Mathf.Pow(1f - t, 2f)));
                    shards[i].transform.Rotate(0f, 0f, 420f * Time.unscaledDeltaTime);
                    renderers[i].color = new Color(1f, Mathf.Lerp(0.75f, 0.12f, t), 0.2f, 1f - t);
                }
                yield return null;
            }
            for (int i = 0; i < count; i++) Destroy(shards[i]);
        }

        private IEnumerator Pulse(Vector2 position, Color color, float duration, float finalScale)
        {
            var instance = new GameObject("Feedback Pulse");
            instance.transform.position = position;
            var line = instance.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 48;
            line.widthMultiplier = 0.08f;
            line.sharedMaterial = RuntimeAssets.SpriteMaterial;
            line.sortingOrder = 20;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i / (float)line.positionCount * Mathf.PI * 2f;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.34f);
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                instance.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, finalScale, 1f - Mathf.Pow(1f - t, 3f));
                Color faded = new Color(color.r, color.g, color.b, color.a * (1f - t));
                line.startColor = faded;
                line.endColor = faded;
                yield return null;
            }
            Destroy(instance);
        }
    }
}
