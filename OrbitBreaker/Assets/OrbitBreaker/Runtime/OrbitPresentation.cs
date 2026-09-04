using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed class OrbitCameraRig : MonoBehaviour
    {
        private Camera targetCamera;
        private Vector3 velocity;
        private float targetY;
        private float targetX;

        public float CameraY => targetCamera != null ? targetCamera.transform.position.y : 0f;

        public void Initialize(Camera camera)
        {
            targetCamera = camera;
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = 6.45f;
            targetCamera.backgroundColor = new Color(0.018f, 0.045f, 0.09f, 1f);
            targetCamera.clearFlags = CameraClearFlags.SolidColor;
            targetCamera.transform.rotation = Quaternion.identity;
        }

        public void Snap(Vector2 focus)
        {
            targetY = Mathf.Max(0f, focus.y + 2.25f);
            targetX = focus.x * 0.12f;
            targetCamera.transform.position = new Vector3(targetX, targetY, -10f);
            velocity = Vector3.zero;
        }

        public void SetTarget(Vector2 playerPosition, Vector2 anchorPosition)
        {
            float desiredY = Mathf.Max(0f, Mathf.Max(playerPosition.y, anchorPosition.y) + 2.15f);
            targetY = desiredY >= targetY ? desiredY : Mathf.MoveTowards(targetY, desiredY, 5.5f * Time.deltaTime);
            targetX = Mathf.Lerp(playerPosition.x, anchorPosition.x, 0.65f) * 0.12f;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            Vector3 destination = new Vector3(targetX, targetY, -10f);
            targetCamera.transform.position = Vector3.SmoothDamp(targetCamera.transform.position, destination, ref velocity, 0.28f, 18f, Time.unscaledDeltaTime);
        }
    }

    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            Apply();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height) Apply();
        }

        private void Apply()
        {
            if (rectTransform == null || Screen.width <= 0 || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            rectTransform.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rectTransform.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
    }

    public sealed class OrbitHud : MonoBehaviour
    {
        private Text scoreText;
        private Text bestText;
        private Text comboText;
        private Text multiplierText;
        private GameObject multiplierBadge;
        private Image multiplierFill;
        private Text stuntText;
        private Text titleText;
        private Text instructionText;
        private Text gameOverTitle;
        private Text gameOverDistance;
        private Text gameOverOrbits;
        private Text gameOverRecord;
        private GameObject gameOverPanel;
        private GameObject settingsPanel;
        private GameObject settingsButton;
        private GameObject pauseButton;
        private GameObject pausePanel;
        private GameObject tutorialTips;
        private RectTransform safeRect;
        private RectTransform floatingHud;
        private CanvasGroup hintGroup;
        private bool gameOverVisible;
        private float stuntShownAt = -10f;

        public bool SettingsOpen => settingsPanel != null && settingsPanel.activeSelf;
        public bool IsPaused => pausePanel != null && pausePanel.activeSelf;

        public void Initialize(OrbitFeedback audio)
        {
            if (EventSystem.current == null)
            {
                new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
            var canvasObject = new GameObject("Game HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var safeObject = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safeObject.transform.SetParent(canvasObject.transform, false);
            RectTransform safe = safeObject.GetComponent<RectTransform>();
            safeRect = safe;
            safe.anchorMin = Vector2.zero;
            safe.anchorMax = Vector2.one;
            safe.offsetMin = Vector2.zero;
            safe.offsetMax = Vector2.zero;

            var floatingObject = new GameObject("Player HUD", typeof(RectTransform));
            floatingObject.transform.SetParent(safe, false);
            floatingHud = floatingObject.GetComponent<RectTransform>();
            floatingHud.anchorMin = floatingHud.anchorMax = Vector2.one * 0.5f;
            floatingHud.pivot = new Vector2(0.5f, 0f);
            floatingHud.sizeDelta = new Vector2(390f, 190f);

            scoreText = CreateText(floatingHud, "Distance", "0 m", 76, TextAnchor.MiddleCenter, FontStyle.Bold);
            scoreText.color = new Color(0.9f, 1f, 1f, 1f);
            scoreText.gameObject.AddComponent<Outline>().effectColor = new Color(0.03f, 0.12f, 0.2f, 0.95f);
            SetRect(scoreText.rectTransform, new Vector2(0f, 0.46f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            multiplierBadge = new GameObject("Flight Readout", typeof(RectTransform));
            multiplierBadge.transform.SetParent(floatingHud, false);
            SetRect(multiplierBadge.GetComponent<RectTransform>(), new Vector2(0.12f, 0f), new Vector2(0.88f, 0.5f), Vector2.zero, Vector2.zero);

            Image dangerTrack = CreateImage(multiplierBadge.transform, "Void Timer", new Color(0.05f, 0.15f, 0.25f, 0.8f));
            ApplyRounded(dangerTrack);
            SetRect(dangerTrack.rectTransform, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.2f), Vector2.zero, Vector2.zero);
            multiplierFill = CreateImage(dangerTrack.transform, "Danger Fill", new Color(0.2f, 0.88f, 1f, 0.9f));
            ApplyRounded(multiplierFill);
            SetRect(multiplierFill.rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

            multiplierText = CreateText(multiplierBadge.transform, "Multiplier", string.Empty, 54, TextAnchor.MiddleCenter, FontStyle.Bold);
            multiplierText.color = new Color(1f, 0.72f, 0.24f, 1f);
            multiplierText.gameObject.AddComponent<Outline>().effectColor = new Color(0.04f, 0.08f, 0.16f, 0.95f);
            SetRect(multiplierText.rectTransform, new Vector2(0f, 0.22f), Vector2.one, Vector2.zero, Vector2.zero);

            bestText = CreateText(safe, "Best", "BEST 0", 30, TextAnchor.UpperRight, FontStyle.Bold);
            bestText.color = new Color(0.45f, 0.72f, 0.9f, 0.8f);
            SetRect(bestText.rectTransform, new Vector2(0.55f, 0.87f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero);

            comboText = CreateText(safe, "Combo", string.Empty, 34, TextAnchor.UpperLeft, FontStyle.Bold);
            comboText.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(comboText.rectTransform, new Vector2(0.06f, 0.87f), new Vector2(0.45f, 0.97f), Vector2.zero, Vector2.zero);

            stuntText = CreateText(floatingHud, "Stunt", string.Empty, 34, TextAnchor.MiddleLeft, FontStyle.Bold);
            stuntText.color = new Color(1f, 0.72f, 0.24f, 1f);
            stuntText.gameObject.AddComponent<Outline>().effectColor = new Color(0.03f, 0.08f, 0.15f, 1f);
            SetRect(stuntText.rectTransform, new Vector2(0.88f, 0.25f), new Vector2(1.85f, 0.72f), new Vector2(16f, 0f), Vector2.zero);

            titleText = CreateText(safe, "Title", "ORBIT\nBREAKER", 78, TextAnchor.MiddleCenter, FontStyle.Bold);
            titleText.color = new Color(0.76f, 0.98f, 1f, 1f);
            titleText.lineSpacing = 0.75f;
            SetRect(titleText.rectTransform, new Vector2(0.12f, 0.53f), new Vector2(0.88f, 0.73f), Vector2.zero, Vector2.zero);

            var hintObject = new GameObject("Launch Hint", typeof(RectTransform), typeof(CanvasGroup));
            hintObject.transform.SetParent(safe, false);
            hintGroup = hintObject.GetComponent<CanvasGroup>();
            RectTransform hintRect = hintObject.GetComponent<RectTransform>();
            SetRect(hintRect, new Vector2(0.1f, 0.17f), new Vector2(0.9f, 0.26f), Vector2.zero, Vector2.zero);
            instructionText = CreateText(hintRect, "Instruction", "TOUCHE POUR TE PROPULSER", 30, TextAnchor.MiddleCenter, FontStyle.Bold);
            instructionText.color = new Color(0.67f, 0.84f, 0.95f, 0.95f);
            SetRect(instructionText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Image tipsPanel = CreateImage(safe, "Tips Panel", new Color(0.025f, 0.075f, 0.14f, 0.9f));
            tutorialTips = tipsPanel.gameObject;
            ApplyRounded(tipsPanel);
            tipsPanel.raycastTarget = false;
            SetRect(tipsPanel.rectTransform, new Vector2(0.1f, 0.035f), new Vector2(0.9f, 0.165f), Vector2.zero, Vector2.zero);
            Text tipsLabel = CreateText(tipsPanel.transform, "Tips Label", "TIPS", 22, TextAnchor.MiddleLeft, FontStyle.Bold);
            tipsLabel.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(tipsLabel.rectTransform, new Vector2(0.06f, 0.6f), new Vector2(0.25f, 0.94f), Vector2.zero, Vector2.zero);
            Text tipsText = CreateText(tipsPanel.transform, "Tips", "SAUTE PLUSIEURS ORBITES POUR BOOSTER LA DISTANCE\nREVIENS SUR UNE ORBITE VISITÉE POUR CHANGER TA ROUTE\nAPRÈS UNE CAPTURE, TU ES BRIÈVEMENT PROTÉGÉ", 18, TextAnchor.MiddleLeft, FontStyle.Normal);
            tipsText.color = new Color(0.68f, 0.88f, 1f, 0.95f);
            tipsText.lineSpacing = 1.15f;
            SetRect(tipsText.rectTransform, new Vector2(0.06f, 0.06f), new Vector2(0.95f, 0.66f), Vector2.zero, Vector2.zero);

            gameOverPanel = new GameObject("Game Over Panel", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(safe, false);
            RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
            SetRect(panelRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image gameOverBackground = gameOverPanel.GetComponent<Image>();
            gameOverBackground.color = new Color(0.01f, 0.02f, 0.055f, 0.84f);
            gameOverBackground.raycastTarget = false;

            Image deathCard = CreateImage(panelRect, "Death Card", new Color(0.025f, 0.07f, 0.13f, 0.97f));
            ApplyRounded(deathCard);
            deathCard.raycastTarget = false;
            SetRect(deathCard.rectTransform, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.75f), Vector2.zero, Vector2.zero);

            gameOverTitle = CreateText(deathCard.rectTransform, "Game Over", "PERDU DANS L'ESPACE", 53, TextAnchor.MiddleCenter, FontStyle.Bold);
            gameOverTitle.color = new Color(1f, 0.3f, 0.43f, 1f);
            SetRect(gameOverTitle.rectTransform, new Vector2(0.07f, 0.65f), new Vector2(0.93f, 0.94f), Vector2.zero, Vector2.zero);

            gameOverDistance = CreateStatRow(deathCard.transform, "Distance", RuntimeAssets.LocationIcon, 0.54f);
            gameOverOrbits = CreateStatRow(deathCard.transform, "Orbits", RuntimeAssets.PlanetIcon, 0.39f);
            gameOverRecord = CreateStatRow(deathCard.transform, "Record", RuntimeAssets.TrophyIcon, 0.24f);

            Text retry = CreateText(deathCard.rectTransform, "Retry", "TOUCHE POUR RECOMMENCER", 28, TextAnchor.MiddleCenter, FontStyle.Bold);
            retry.color = new Color(1f, 0.76f, 0.28f, 1f);
            SetRect(retry.rectTransform, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.24f), Vector2.zero, Vector2.zero);
            gameOverPanel.SetActive(false);

            settingsButton = CreateIconButton(safe, "Settings Button", RuntimeAssets.SettingsIcon, ToggleSettings);
            SetRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.855f, 0.465f), new Vector2(0.965f, 0.535f), Vector2.zero, Vector2.zero);

            pauseButton = CreateIconButton(safe, "Pause Button", RuntimeAssets.PauseIcon, PauseGame);
            SetRect(pauseButton.GetComponent<RectTransform>(), new Vector2(0.045f, 0.89f), new Vector2(0.145f, 0.95f), Vector2.zero, Vector2.zero);
            pauseButton.SetActive(false);

            pausePanel = CreatePausePanel(safe);
            pausePanel.SetActive(false);

            settingsPanel = CreateSettingsPanel(safe, audio);
            settingsPanel.SetActive(false);
        }

        public void ShowPlaying(int distance, int best, bool tutorial)
        {
            UpdateProgress(distance, best);
            stuntText.text = string.Empty;
            titleText.gameObject.SetActive(tutorial);
            hintGroup.gameObject.SetActive(tutorial);
            tutorialTips.SetActive(tutorial);
            settingsButton.SetActive(tutorial);
            pauseButton.SetActive(!tutorial);
            settingsPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            gameOverVisible = false;
        }

        public void UpdateProgress(int distance, int best)
        {
            scoreText.text = distance + " m";
            bestText.text = "RECORD " + best + " m";
            comboText.text = string.Empty;
        }

        public void UpdateFlightDisplay(Vector3 worldPosition, float multiplier, float danger01, bool flying)
        {
            multiplierBadge.SetActive(flying);
            if (safeRect != null && Camera.main != null)
            {
                Vector2 screen = Camera.main.WorldToScreenPoint(worldPosition);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRect, screen, null, out Vector2 local))
                {
                    Rect bounds = safeRect.rect;
                    if (local.x > 0f)
                        SetRect(stuntText.rectTransform, new Vector2(-0.85f, 0.25f), new Vector2(0.12f, 0.72f), Vector2.zero, new Vector2(-16f, 0f));
                    else
                        SetRect(stuntText.rectTransform, new Vector2(0.88f, 0.25f), new Vector2(1.85f, 0.72f), new Vector2(16f, 0f), Vector2.zero);
                    stuntText.alignment = local.x > 0f ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                    local.x = Mathf.Clamp(local.x, bounds.xMin + 205f, bounds.xMax - 205f);
                    local.y = Mathf.Clamp(local.y + 105f, bounds.yMin + 100f, bounds.yMax - 235f);
                    floatingHud.anchoredPosition = Vector2.Lerp(floatingHud.anchoredPosition, local, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 13f));
                }
            }
            if (!string.IsNullOrEmpty(stuntText.text))
            {
                float pop = Mathf.Clamp01((Time.unscaledTime - stuntShownAt) / 0.16f);
                stuntText.transform.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, 1f - Mathf.Pow(1f - pop, 3f));
            }
            if (!flying) return;
            multiplierText.text = "x" + multiplier.ToString("0.0");
            float intensity = Mathf.Clamp01(danger01);
            multiplierText.color = Color.Lerp(new Color(0.25f, 0.92f, 1f), new Color(1f, 0.32f, 0.55f), intensity);
            multiplierBadge.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 8f) * (0.018f + intensity * 0.035f));
            multiplierFill.rectTransform.anchorMax = new Vector2(intensity, 1f);
            multiplierFill.color = Color.Lerp(new Color(0.2f, 0.88f, 1f, 0.35f), new Color(1f, 0.2f, 0.5f, 0.55f), intensity);
        }

        public void ShowLanding(int distance, int best, int gainedDistance, float multiplier, int skippedAnchors, bool backtrack, bool revisited)
        {
            UpdateProgress(distance, best);
            multiplierBadge.SetActive(false);
            settingsButton.SetActive(false);
            pauseButton.SetActive(true);
            string label = backtrack ? "RETOUR ORBITAL" : revisited ? "CHECKPOINT" : skippedAnchors > 0 ? "SKIP x" + (skippedAnchors + 1) : multiplier >= 2f ? "LONG VOL" : string.Empty;
            string delta = gainedDistance > 0 ? "+" + gainedDistance : gainedDistance < 0 ? gainedDistance.ToString() : string.Empty;
            stuntText.text = string.IsNullOrEmpty(label) ? string.Empty : label + (string.IsNullOrEmpty(delta) ? string.Empty : "  " + delta + " m");
            stuntShownAt = Time.unscaledTime;
            CancelInvoke(nameof(ClearStunt));
            Invoke(nameof(ClearStunt), 1.15f);
        }

        public void HideTutorial()
        {
            titleText.gameObject.SetActive(false);
            hintGroup.gameObject.SetActive(false);
            tutorialTips.SetActive(false);
            settingsButton.SetActive(false);
            settingsPanel.SetActive(false);
            pauseButton.SetActive(true);
        }

        public void ShowGameOver(int distance, int best, int anchors, DeathReason reason)
        {
            scoreText.text = distance + " m";
            multiplierBadge.SetActive(false);
            bestText.text = "RECORD " + best + " m";
            gameOverDistance.text = "DISTANCE     " + distance + " m";
            gameOverOrbits.text = "ORBITES      " + anchors;
            gameOverRecord.text = "RECORD       " + best + " m";
            gameOverTitle.text = reason == DeathReason.Breaker ? "VOTRE VAISSEAU\nA EXPLOSÉ" : "VOUS VOUS ÊTES PERDU\nDANS L'ESPACE";
            gameOverTitle.color = reason == DeathReason.Breaker ? new Color(1f, 0.28f, 0.38f) : new Color(0.45f, 0.82f, 1f);
            gameOverPanel.SetActive(true);
            settingsButton.SetActive(true);
            pauseButton.SetActive(false);
            gameOverVisible = true;
        }

        private void ClearStunt()
        {
            stuntText.text = string.Empty;
            stuntText.transform.localScale = Vector3.one;
        }

        private void ToggleSettings()
        {
            bool opening = !settingsPanel.activeSelf;
            settingsPanel.SetActive(opening);
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        public void PauseGame()
        {
            if (pausePanel.activeSelf) return;
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            pauseButton.SetActive(false);
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (pauseButton != null && !gameOverVisible && !titleText.gameObject.activeSelf) pauseButton.SetActive(true);
        }

        private GameObject CreatePausePanel(Transform safe)
        {
            var panel = new GameObject("Pause Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image background = panel.GetComponent<Image>();
            background.color = new Color(0.008f, 0.025f, 0.06f, 0.78f);
            background.raycastTarget = false;

            Image card = CreateImage(panel.transform, "Pause Card", new Color(0.025f, 0.075f, 0.14f, 0.97f));
            ApplyRounded(card);
            card.raycastTarget = false;
            SetRect(card.rectTransform, new Vector2(0.14f, 0.29f), new Vector2(0.86f, 0.65f), Vector2.zero, Vector2.zero);

            Image icon = CreateImage(card.transform, "Pause Icon", Color.white);
            icon.sprite = RuntimeAssets.PauseIcon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            SetRect(icon.rectTransform, new Vector2(0.39f, 0.57f), new Vector2(0.61f, 0.87f), Vector2.zero, Vector2.zero);
            Text heading = CreateText(card.transform, "Paused", "PAUSE", 62, TextAnchor.MiddleCenter, FontStyle.Bold);
            heading.color = new Color(0.72f, 0.97f, 1f, 1f);
            SetRect(heading.rectTransform, new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.59f), Vector2.zero, Vector2.zero);
            Text resume = CreateText(card.transform, "Resume Hint", "APPUYER POUR REPRENDRE", 27, TextAnchor.MiddleCenter, FontStyle.Bold);
            resume.color = new Color(0.42f, 0.72f, 0.9f, 0.9f);
            SetRect(resume.rectTransform, new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.3f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private GameObject CreateSettingsPanel(Transform safe, OrbitFeedback audio)
        {
            var panel = new GameObject("Settings Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.8f), Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.025f, 0.065f, 0.12f, 0.98f);
            ApplyRounded(panel.GetComponent<Image>());

            Text heading = CreateText(panel.transform, "Settings Title", "AUDIO", 54, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(heading.rectTransform, new Vector2(0.1f, 0.79f), new Vector2(0.9f, 0.96f), Vector2.zero, Vector2.zero);
            CreateVolumeRow(panel.transform, "GLOBAL", 0.62f, audio.MasterVolume, audio.SetMasterVolume);
            CreateVolumeRow(panel.transform, "MUSIQUE", 0.43f, audio.MusicVolume, audio.SetMusicVolume);
            CreateVolumeRow(panel.transform, "EFFETS", 0.24f, audio.EffectsVolume, audio.SetEffectsVolume);

            GameObject close = CreateButton(panel.transform, "Close Settings", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleSettings);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.25f, 0.05f), new Vector2(0.75f, 0.15f), Vector2.zero, Vector2.zero);
            return panel;
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

        private static GameObject CreateButton(Transform parent, string name, string label, Color color, UnityEngine.Events.UnityAction callback)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            instance.transform.SetParent(parent, false);
            Image image = instance.GetComponent<Image>();
            image.color = color;
            ApplyRounded(image);
            Button button = instance.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(callback);
            Text text = CreateText(instance.transform, "Label", label, 25, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return instance;
        }

        private static GameObject CreateIconButton(Transform parent, string name, Sprite iconSprite, UnityEngine.Events.UnityAction callback)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            instance.transform.SetParent(parent, false);
            Image background = instance.GetComponent<Image>();
            background.color = new Color(0.025f, 0.085f, 0.15f, 0.94f);
            background.sprite = RuntimeAssets.CircleSprite;
            Outline outline = instance.GetComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.78f, 0.94f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);
            Button button = instance.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(callback);
            Image icon = CreateImage(instance.transform, "Icon", new Color(0.76f, 0.98f, 1f, 1f));
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            SetRect(icon.rectTransform, new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.78f), Vector2.zero, Vector2.zero);
            return instance;
        }

        private static Text CreateStatRow(Transform parent, string name, Sprite iconSprite, float bottom)
        {
            Image icon = CreateImage(parent, name + " Icon", new Color(0.32f, 0.9f, 1f, 1f));
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            SetRect(icon.rectTransform, new Vector2(0.12f, bottom), new Vector2(0.23f, bottom + 0.13f), Vector2.zero, Vector2.zero);
            Text value = CreateText(parent, name + " Value", string.Empty, 31, TextAnchor.MiddleLeft, FontStyle.Bold);
            value.color = new Color(0.76f, 0.92f, 1f, 1f);
            SetRect(value.rectTransform, new Vector2(0.28f, bottom), new Vector2(0.9f, bottom + 0.13f), Vector2.zero, Vector2.zero);
            return value;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(Image));
            instance.transform.SetParent(parent, false);
            Image image = instance.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void ApplyRounded(Image image)
        {
            image.sprite = RuntimeAssets.RoundedRectSprite;
            image.type = Image.Type.Sliced;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, TextAnchor alignment, FontStyle style)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(Text));
            instance.transform.SetParent(parent, false);
            Text text = instance.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.fontStyle = style;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }

    public sealed class OrbitFeedback : MonoBehaviour
    {
        private const string MasterVolumeKey = "OrbitBreaker.Audio.Master";
        private const string MusicVolumeKey = "OrbitBreaker.Audio.Music";
        private const string EffectsVolumeKey = "OrbitBreaker.Audio.Effects";
        private AudioSource audioSource;
        private AudioSource musicSource;
        private AudioSource chargeSource;
        private AudioSource skipSource;
        private AudioClip launchClip;
        private AudioClip captureClip;
        private AudioClip perfectClip;
        private AudioClip deathClip;
        private AudioClip skipClip;

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
            musicSource.clip = RuntimeAssets.CreateChiptuneLoop();
            chargeSource = gameObject.AddComponent<AudioSource>();
            chargeSource.playOnAwake = false;
            chargeSource.loop = true;
            chargeSource.clip = RuntimeAssets.CreateChargeLoop();
            skipSource = gameObject.AddComponent<AudioSource>();
            skipSource.playOnAwake = false;
            launchClip = RuntimeAssets.CreateTone("Launch", 340f, 0.09f, 0.32f);
            captureClip = RuntimeAssets.CreateTone("Capture", 620f, 0.12f, 0.34f);
            perfectClip = RuntimeAssets.CreateTone("Perfect", 880f, 0.16f, 0.34f);
            deathClip = RuntimeAssets.CreateTone("Break", 115f, 0.32f, 0.44f);
            skipClip = RuntimeAssets.CreateSkipStinger();
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
            const int count = 12;
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
