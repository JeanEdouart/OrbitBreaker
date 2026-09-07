using UnityEngine;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed partial class OrbitHud
    {
        private GameObject CreateSettingsPanel(Transform safe, OrbitFeedback audio)
        {
            var panel = new GameObject("Settings Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.045f, 0.08f), new Vector2(0.955f, 0.9f), Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.065f, 0.12f, 0.98f);
            ApplyRounded(panel.GetComponent<Image>());

            Text heading = CreateText(panel.transform, "Settings Title", "RÉGLAGES", 48, TextAnchor.MiddleCenter, FontStyle.Bold);
            heading.color = new Color(0.76f, 0.98f, 1f, 1f);
            SetRect(heading.rectTransform, new Vector2(0.1f, 0.86f), new Vector2(0.9f, 0.98f), Vector2.zero, Vector2.zero);

            GameObject soundTab = CreateButton(panel.transform, "Sound Tab", "SON", new Color(0.08f, 0.3f, 0.42f, 1f), () => ShowSettingsTab(0));
            GameObject gameplayTab = CreateButton(panel.transform, "Gameplay Tab", "GAMEPLAY", new Color(0.055f, 0.16f, 0.25f, 1f), () => ShowSettingsTab(1));
            GameObject videoTab = CreateButton(panel.transform, "Video Tab", "VIDÉO", new Color(0.055f, 0.16f, 0.25f, 1f), () => ShowSettingsTab(2));
            SetRect(soundTab.GetComponent<RectTransform>(), new Vector2(0.07f, 0.76f), new Vector2(0.34f, 0.84f), Vector2.zero, Vector2.zero);
            SetRect(gameplayTab.GetComponent<RectTransform>(), new Vector2(0.365f, 0.76f), new Vector2(0.635f, 0.84f), Vector2.zero, Vector2.zero);
            SetRect(videoTab.GetComponent<RectTransform>(), new Vector2(0.66f, 0.76f), new Vector2(0.93f, 0.84f), Vector2.zero, Vector2.zero);
            settingsTabImages = new[] { soundTab.GetComponent<Image>(), gameplayTab.GetComponent<Image>(), videoTab.GetComponent<Image>() };

            settingsAudioPage = CreateSettingsPage(panel.transform, "Sound Page");
            CreateVolumeRow(settingsAudioPage.transform, "GLOBAL", 0.68f, audio.MasterVolume, audio.SetMasterVolume);
            CreateVolumeRow(settingsAudioPage.transform, "MUSIQUE", 0.44f, audio.MusicVolume, audio.SetMusicVolume);
            CreateVolumeRow(settingsAudioPage.transform, "EFFETS", 0.2f, audio.EffectsVolume, audio.SetEffectsVolume);
            Text musicHint = CreateText(settingsAudioPage.transform, "Music Hint", "CHOISIS TES MUSIQUES DANS LE HANGAR", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(musicHint.rectTransform, new Vector2(0.06f, 0.01f), new Vector2(0.94f, 0.11f), Vector2.zero, Vector2.zero);

            settingsGameplayPage = CreateSettingsPage(panel.transform, "Gameplay Page");
            Text gameplayNote = CreateText(settingsGameplayPage.transform, "Visual Options Note", "MASQUER UNE AIDE NE CHANGE PAS LE GAMEPLAY NI LES COLLISIONS", 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            gameplayNote.color = new Color(1f, 0.72f, 0.18f, 0.95f);
            gameplayNote.resizeTextForBestFit = true;
            gameplayNote.resizeTextMinSize = 11;
            gameplayNote.resizeTextMaxSize = 16;
            SetRect(gameplayNote.rectTransform, new Vector2(0.04f, 0.89f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
            CreateToggleRow(settingsGameplayPage.transform, "GUIDES DE ROTATION", 0.72f, GamePreferences.RotationGuides, GamePreferences.SetRotationGuides);
            CreateToggleRow(settingsGameplayPage.transform, "ANNEAUX D'ORBITE", 0.58f, GamePreferences.OrbitRings, GamePreferences.SetOrbitRings);
            CreateToggleRow(settingsGameplayPage.transform, "JAUGES DE VOL", 0.44f, GamePreferences.FlightGauges, GamePreferences.SetFlightGauges);
            CreateToggleRow(settingsGameplayPage.transform, "BOUCLIER D'IMMUNITÉ", 0.30f, GamePreferences.Shield, GamePreferences.SetShield);
            CreateToggleRow(settingsGameplayPage.transform, "VIBRATIONS", 0.16f, GamePreferences.Haptics, GamePreferences.SetHaptics);
            CreateToggleRow(settingsGameplayPage.transform, "CONTRASTE DES DÉBRIS", 0.02f, GamePreferences.HighContrastDebris, GamePreferences.SetHighContrastDebris);

            settingsVideoPage = CreateSettingsPage(panel.transform, "Video Page");
            CreateFrameRateSelector(settingsVideoPage.transform);
            CreateToggleRow(settingsVideoPage.transform, "FOND DYNAMIQUE", 0.60f, GamePreferences.DynamicBackground, GamePreferences.SetDynamicBackground);
            CreateToggleRow(settingsVideoPage.transform, "EFFETS RENFORCÉS", 0.48f, GamePreferences.EnhancedEffects, GamePreferences.SetEnhancedEffects);
            CreateToggleRow(settingsVideoPage.transform, "SECOUSSE À LA CAPTURE", 0.36f, GamePreferences.CaptureShake, GamePreferences.SetCaptureShake);
            CreateToggleRow(settingsVideoPage.transform, "SECOUSSE D'EXPLOSION", 0.24f, GamePreferences.ExplosionShake, GamePreferences.SetExplosionShake);
            CreateToggleRow(settingsVideoPage.transform, "TREMBLEMENT EN VOL", 0.12f, GamePreferences.FlightShake, GamePreferences.SetFlightShake);
            CreateToggleRow(settingsVideoPage.transform, "CAMÉRA STABLE", 0f, GamePreferences.FixedCamera, GamePreferences.SetFixedCamera);

            GameObject close = CreateButton(panel.transform, "Close Settings", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleSettings);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.27f, 0.035f), new Vector2(0.73f, 0.105f), Vector2.zero, Vector2.zero);
            ShowSettingsTab(0);
            return panel;
        }

        private static GameObject CreateSettingsPage(Transform parent, string name)
        {
            var page = new GameObject(name, typeof(RectTransform));
            page.transform.SetParent(parent, false);
            SetRect(page.GetComponent<RectTransform>(), new Vector2(0.055f, 0.13f), new Vector2(0.945f, 0.735f), Vector2.zero, Vector2.zero);
            return page;
        }

        private void ShowSettingsTab(int index)
        {
            if (settingsAudioPage == null) return;
            settingsAudioPage.SetActive(index == 0);
            settingsGameplayPage.SetActive(index == 1);
            settingsVideoPage.SetActive(index == 2);
            for (int i = 0; i < settingsTabImages.Length; i++)
            {
                settingsTabImages[i].color = i == index
                    ? new Color(0.08f, 0.42f, 0.56f, 1f)
                    : new Color(0.045f, 0.13f, 0.22f, 1f);
            }
        }

        private void CreateFrameRateSelector(Transform parent)
        {
            Text label = CreateText(parent, "Frame Rate Label", "FRÉQUENCE D'AFFICHAGE", 20, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.color = new Color(0.72f, 0.9f, 1f, 1f);
            SetRect(label.rectTransform, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.99f), Vector2.zero, Vector2.zero);

            int[] rates = { 30, 60, 120 };
            frameRateButtonImages = new Image[rates.Length];
            for (int i = 0; i < rates.Length; i++)
            {
                int rate = rates[i];
                GameObject button = CreateButton(parent, rate + " FPS", rate.ToString(), new Color(0.045f, 0.13f, 0.22f, 1f), () => SelectFrameRate(rate));
                float left = 0.12f + i * 0.265f;
                SetRect(button.GetComponent<RectTransform>(), new Vector2(left, 0.75f), new Vector2(left + 0.23f, 0.865f), Vector2.zero, Vector2.zero);
                frameRateButtonImages[i] = button.GetComponent<Image>();
            }
            RefreshFrameRateSelector();
        }

        private void SelectFrameRate(int frameRate)
        {
            GamePreferences.SetTargetFrameRate(frameRate);
            RefreshFrameRateSelector();
        }

        private void RefreshFrameRateSelector()
        {
            if (frameRateButtonImages == null) return;
            int selected = GamePreferences.TargetFrameRate >= 120 ? 2 : GamePreferences.TargetFrameRate >= 60 ? 1 : 0;
            for (int i = 0; i < frameRateButtonImages.Length; i++)
            {
                frameRateButtonImages[i].color = i == selected
                    ? new Color(0.08f, 0.62f, 0.72f, 1f)
                    : new Color(0.045f, 0.13f, 0.22f, 1f);
            }
        }

        private static void CreateVolumeRow(Transform parent, string label, float centerY, float value, UnityEngine.Events.UnityAction<float> callback)
        {
            Text text = CreateText(parent, label, label, 28, TextAnchor.MiddleLeft, FontStyle.Bold);
            SetRect(text.rectTransform, new Vector2(0.1f, centerY), new Vector2(0.42f, centerY + 0.1f), Vector2.zero, Vector2.zero);

            var sliderObject = new GameObject(label + " Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            SetRect(sliderObject.GetComponent<RectTransform>(), new Vector2(0.43f, centerY + 0.015f), new Vector2(0.88f, centerY + 0.085f), Vector2.zero, Vector2.zero);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            Image background = CreateImage(sliderObject.transform, "Track", new Color(0.12f, 0.24f, 0.35f, 1f));
            ApplyRounded(background);
            SetRect(background.rectTransform, new Vector2(0f, 0.37f), new Vector2(1f, 0.63f), Vector2.zero, Vector2.zero);
            Image fill = CreateImage(background.transform, "Fill", new Color(0.2f, 0.9f, 1f, 1f));
            ApplyRounded(fill);
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image handle = CreateImage(sliderObject.transform, "Handle", new Color(0.92f, 1f, 1f, 1f));
            handle.sprite = RuntimeAssets.CircleSprite;
            SetRect(handle.rectTransform, new Vector2(0f, 0.15f), new Vector2(0.08f, 0.85f), Vector2.zero, Vector2.zero);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.value = value;
            slider.onValueChanged.AddListener(callback);
        }

        private static void CreateToggleRow(Transform parent, string label, float bottom, bool value, UnityEngine.Events.UnityAction<bool> callback)
        {
            Text text = CreateText(parent, label + " Label", label, 22, TextAnchor.MiddleLeft, FontStyle.Bold);
            text.color = new Color(0.72f, 0.9f, 1f, 1f);
            SetRect(text.rectTransform, new Vector2(0.06f, bottom), new Vector2(0.72f, bottom + 0.12f), Vector2.zero, Vector2.zero);

            var toggleObject = new GameObject(label + " Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
            toggleObject.transform.SetParent(parent, false);
            SetRect(toggleObject.GetComponent<RectTransform>(), new Vector2(0.76f, bottom + 0.025f), new Vector2(0.94f, bottom + 0.095f), Vector2.zero, Vector2.zero);
            Image background = toggleObject.GetComponent<Image>();
            background.sprite = RuntimeAssets.RoundedRectSprite;
            background.type = Image.Type.Sliced;
            background.color = new Color(0.06f, 0.16f, 0.24f, 1f);

            Image check = CreateImage(toggleObject.transform, "Knob", new Color(0.9f, 1f, 1f, 1f));
            check.sprite = RuntimeAssets.CircleSprite;
            check.preserveAspect = true;
            check.raycastTarget = false;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.transition = Selectable.Transition.None;
            toggle.SetIsOnWithoutNotify(value);
            ToggleSwitchVisual visual = toggleObject.AddComponent<ToggleSwitchVisual>();
            visual.Initialize(background, check.rectTransform, value);
            toggle.onValueChanged.AddListener(visual.SetValue);
            toggle.onValueChanged.AddListener(callback);
        }
    }
}
