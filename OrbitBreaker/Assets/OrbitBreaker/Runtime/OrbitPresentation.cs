using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitBreaker
{
    public sealed partial class OrbitHud : MonoBehaviour
    {
        private Text scoreText;
        private Text bestText;
        private Text sectorText;
        private Text comboText;
        private Text multiplierText;
        private GameObject multiplierBadge;
        private Image multiplierFill;
        private Text stuntText;
        private Text nearMissText;
        private GameObject materialToast;
        private Text materialToastText;
        private GameObject challengeToast;
        private Text challengeToastText;
        private OrbitFeedback hudFeedback;
        private Text titleText;
        private Text instructionText;
        private Text gameOverTitle;
        private Text gameOverDistance;
        private Text gameOverOrbits;
        private Text gameOverRecord;
        private Text gameOverSummary;
        private GameObject gameOverPanel;
        private GameObject deathReplayOverlay;
        private Text deathReplayLabel;
        private GameObject settingsPanel;
        private GameObject settingsButton;
        private GameObject infoButton;
        private GameObject styleButton;
        private GameObject missionsButton;
        private GameObject leaderboardButton;
        private GameObject powerUpButton;
        private GameObject leaderboardPanel;
        private GameObject playerNamePanel;
        private InputField playerNameInput;
        private Text playerNameStatus;
        private InputField leaderboardSearchInput;
        private Text leaderboardStatus;
        private Text leaderboardEndlessTab, leaderboardSprintTab;
        private Image leaderboardEndlessTabImage, leaderboardSprintTabImage;
        private Text leaderboardModeHint;
        private readonly Text[] leaderboardRows = new Text[100];
        private ScrollRect leaderboardScroll;
        private RectTransform leaderboardContent;
        private bool leaderboardLoading;
        private OnlineLeaderboard onlineLeaderboard;
        private Action identityAccepted;
        private GameObject missionsPanel;
        private GameObject hangarPanel;
        private GameObject powerUpPanel;
        private Text powerUpCurrencyText;
        private Text powerUpMenuStatus;
        private readonly Text[] powerUpLevelTexts = new Text[5];
        private readonly Text[] powerUpPriceTexts = new Text[5];
        private readonly Button[] powerUpUpgradeButtons = new Button[5];
        private readonly GameObject[] powerUpInventoryButtons = new GameObject[5];
        private readonly Button[] inventoryControls = new Button[5];
        private readonly Image[] inventoryIcons = new Image[5];
        private readonly int[] inventoryCounts = new int[5];
        private readonly Text[] powerUpInventoryCounts = new Text[5];
        private readonly GameObject[] activePowerRows = new GameObject[5];
        private readonly Image[] activePowerFills = new Image[5];
        private readonly Text[] activePowerTimes = new Text[5];
        private readonly Image[,] powerUpLevelPips = new Image[5, 5];
        private readonly Text[] powerUpStats = new Text[5];
        private readonly Text[] powerUpStockTexts = new Text[5];
        private GameObject powerUpToast;
        private Text powerUpToastText;
        private GameObject hyperspaceOverlay;
        private Image hyperspaceVeil;
        private QuantumTunnelGraphic hyperspaceTunnel;
        private readonly Text[] challengeLabels = new Text[3];
        private readonly Text[] challengeProgressTexts = new Text[3];
        private readonly Image[] challengeFills = new Image[3];
        private readonly Button[] challengeButtons = new Button[3];
        private readonly Text[] challengeButtonLabels = new Text[3];
        private Text missionCurrencyText;
        private Text hangarStatus;
        private Text hangarCurrencyText;
        private Text hangarItemName;
        private Text hangarItemPrice;
        private Image hangarPreview;
        private Button hangarActionButton;
        private Text hangarActionLabel;
        private readonly Button[] cosmeticCards = new Button[4];
        private readonly Text[] cosmeticCardLabels = new Text[4];
        private readonly Image[] cosmeticCardPreviews = new Image[4];
        private readonly Image[] hangarTabImages = new Image[5];
        private GameObject musicPreviewButton;
        private Text musicDescription;
        private Image hangarBackdrop;
        private CosmeticKind hangarCategory;
        private int hangarPage;
        private int selectedCosmeticIndex;
        private GameObject creditsPanel;
        private GameObject settingsAudioPage;
        private GameObject settingsGameplayPage;
        private GameObject settingsVideoPage;
        private Image[] settingsTabImages;
        private Image[] frameRateButtonImages;
        private GameObject pauseButton;
        private GameObject pausePanel;
        private GameObject tutorialTips;
        private RectTransform safeRect;
        private RectTransform floatingHud;
        private CanvasGroup hintGroup;
        private bool gameOverVisible;
        private float stuntShownAt = -10f;

        public bool SettingsOpen => (settingsPanel != null && settingsPanel.activeSelf)
            || (creditsPanel != null && creditsPanel.activeSelf)
            || (missionsPanel != null && missionsPanel.activeSelf)
            || (hangarPanel != null && hangarPanel.activeSelf)
            || (powerUpPanel != null && powerUpPanel.activeSelf)
            || (leaderboardPanel != null && leaderboardPanel.activeSelf)
            || (modePanel != null && modePanel.activeSelf)
            || (journalPanel != null && journalPanel.activeSelf)
            || (statisticsPanel != null && statisticsPanel.activeSelf)
            || (playerNamePanel != null && playerNamePanel.activeSelf);
        public bool IsPaused => pausePanel != null && pausePanel.activeSelf;
        public event Action CosmeticsChanged;
        public event Action<PowerUpType> PowerUpRequested;

        public void Initialize(OrbitFeedback audio, OnlineLeaderboard leaderboard)
        {
            hudFeedback = audio;
            onlineLeaderboard = leaderboard;
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
            scaler.matchWidthOrHeight = Screen.width / Mathf.Max(1f, Screen.height) <= 0.5625f ? 0f : 1f;
            canvasObject.AddComponent<ResponsiveCanvasScaler>();

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

            scoreText = CreateText(floatingHud, "Distance", "0 UA", 76, TextAnchor.MiddleCenter, FontStyle.Bold);
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
            sectorText = CreateText(safe, "Sector", string.Empty, 27, TextAnchor.MiddleCenter, FontStyle.Bold);
            sectorText.color = new Color(0.6f, 1f, 0.95f);
            SetRect(sectorText.rectTransform, new Vector2(0.15f, 0.855f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            comboText.color = new Color(1f, 0.72f, 0.24f, 1f);
            comboText.fontSize = 30;
            comboText.resizeTextForBestFit = false;
            comboText.gameObject.AddComponent<Outline>().effectColor = new Color(0.02f, 0.04f, 0.1f, 0.9f);
            SetRect(comboText.rectTransform, new Vector2(0.06f, 0.79f), new Vector2(0.65f, 0.85f), Vector2.zero, Vector2.zero);

            stuntText = CreateText(floatingHud, "Stunt", string.Empty, 34, TextAnchor.MiddleLeft, FontStyle.Bold);
            stuntText.color = new Color(1f, 0.72f, 0.24f, 1f);
            stuntText.gameObject.AddComponent<Outline>().effectColor = new Color(0.03f, 0.08f, 0.15f, 1f);
            stuntText.horizontalOverflow = HorizontalWrapMode.Overflow;
            stuntText.resizeTextForBestFit = true;
            stuntText.resizeTextMinSize = 18;
            stuntText.resizeTextMaxSize = 34;
            SetRect(stuntText.rectTransform, new Vector2(0.88f, 0.25f), new Vector2(1.85f, 0.72f), new Vector2(16f, 0f), Vector2.zero);

            nearMissText = CreateText(floatingHud, "Near Miss", string.Empty, 28, TextAnchor.MiddleCenter, FontStyle.Bold);
            nearMissText.color = new Color(1f, 0.78f, 0.25f, 1f);
            nearMissText.gameObject.AddComponent<Outline>().effectColor = new Color(0.08f, 0.035f, 0.01f, 1f);
            nearMissText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(nearMissText.rectTransform, new Vector2(-0.25f, -0.16f), new Vector2(1.25f, 0.1f), Vector2.zero, Vector2.zero);

            Image materialToastImage = CreateImage(safe, "Material Toast", new Color(0.025f, 0.12f, 0.18f, 0.94f));
            materialToast = materialToastImage.gameObject;
            materialToast.transform.SetParent(floatingHud, false);
            ApplyRounded(materialToastImage);
            materialToastImage.raycastTarget = false;
            SetRect(materialToastImage.rectTransform, new Vector2(0.02f, 1.1f), new Vector2(0.98f, 1.34f), Vector2.zero, Vector2.zero);
            materialToastText = CreateText(materialToast.transform, "Material Value", string.Empty, 23, TextAnchor.MiddleCenter, FontStyle.Bold);
            materialToastText.color = new Color(0.35f, 0.95f, 1f);
            SetRect(materialToastText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            materialToast.SetActive(false);

            Image challengeToastImage = CreateImage(safe, "Challenge Complete Toast", new Color(0.12f, 0.075f, 0.02f, 0.95f));
            challengeToast = challengeToastImage.gameObject;
            ApplyRounded(challengeToastImage);
            challengeToastImage.raycastTarget = false;
            SetRect(challengeToastImage.rectTransform, new Vector2(0.17f, 0.735f), new Vector2(0.83f, 0.795f), Vector2.zero, Vector2.zero);
            challengeToastText = CreateText(challengeToast.transform, "Challenge Complete", string.Empty, 21, TextAnchor.MiddleCenter, FontStyle.Bold);
            challengeToastText.color = new Color(1f, 0.76f, 0.28f);
            challengeToastText.resizeTextForBestFit = true;
            challengeToastText.resizeTextMinSize = 14;
            challengeToastText.resizeTextMaxSize = 21;
            SetRect(challengeToastText.rectTransform, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
            challengeToast.SetActive(false);

            Image powerToastImage = CreateImage(safe, "Power Up Toast", new Color(0.045f, 0.08f, 0.18f, 0.96f));
            powerUpToast = powerToastImage.gameObject; ApplyRounded(powerToastImage); powerToastImage.raycastTarget = false;
            SetRect(powerToastImage.rectTransform, new Vector2(0.24f, 0.665f), new Vector2(0.76f, 0.725f), Vector2.zero, Vector2.zero);
            powerUpToastText = CreateText(powerUpToast.transform, "Power Up Message", string.Empty, 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            powerUpToastText.color = new Color(0.72f, 0.98f, 1f); SetRect(powerUpToastText.rectTransform, new Vector2(0.05f, 0f), new Vector2(0.95f, 1f), Vector2.zero, Vector2.zero);
            powerUpToast.SetActive(false);

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
            Text tipsLabel = CreateText(tipsPanel.transform, "Tips Label", "TIPS", 20, TextAnchor.MiddleLeft, FontStyle.Bold);
            tipsLabel.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(tipsLabel.rectTransform, new Vector2(0.06f, 0.6f), new Vector2(0.25f, 0.94f), Vector2.zero, Vector2.zero);
            Text tipsText = CreateText(tipsPanel.transform, "Tips", "VISE LA PORTE COLORÉE DANS SON SENS POUR UN BONUS SYNCHRO\nELLE EST OPTIONNELLE : TOUTE L'ORBITE PEUT TE CAPTURER\nENCHAÎNE LES SKIPS : BONUS DE DISTANCE JUSQU'À ×2,5", 17, TextAnchor.MiddleLeft, FontStyle.Normal);
            tipsText.color = new Color(0.68f, 0.88f, 1f, 0.95f);
            tipsText.lineSpacing = 1.15f;
            tipsContent = tipsText;
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
            gameOverRecord = CreateStatRow(deathCard.transform, "Record", RuntimeAssets.TrophyIcon, 0.28f);
            gameOverSummary = CreateText(deathCard.transform, "Run Summary", string.Empty, 19, TextAnchor.MiddleCenter, FontStyle.Bold);
            gameOverSummary.color = new Color(1f, 0.72f, 0.24f, 0.95f);
            gameOverSummary.resizeTextForBestFit = true;
            gameOverSummary.resizeTextMinSize = 13;
            gameOverSummary.resizeTextMaxSize = 19;
            SetRect(gameOverSummary.rectTransform, new Vector2(0.07f, 0.15f), new Vector2(0.93f, 0.25f), Vector2.zero, Vector2.zero);

            Text retry = CreateText(deathCard.rectTransform, "Retry", "TOUCHE POUR RECOMMENCER", 28, TextAnchor.MiddleCenter, FontStyle.Bold);
            retry.color = new Color(1f, 0.76f, 0.28f, 1f);
            SetRect(retry.rectTransform, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.15f), Vector2.zero, Vector2.zero);
            gameOverPanel.SetActive(false);

            deathReplayOverlay = new GameObject("Death Replay Overlay", typeof(RectTransform), typeof(Image));
            deathReplayOverlay.transform.SetParent(safe, false);
            SetRect(deathReplayOverlay.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image replayVeil = deathReplayOverlay.GetComponent<Image>();
            replayVeil.color = new Color(0.005f, 0.015f, 0.045f, 0.16f);
            replayVeil.raycastTarget = false;
            deathReplayLabel = CreateText(deathReplayOverlay.transform, "Replay Label", "REPLAY  ·  APPUIE POUR PASSER", 25, TextAnchor.LowerCenter, FontStyle.Bold);
            deathReplayLabel.color = new Color(0.72f, 0.94f, 1f, 0.58f);
            deathReplayLabel.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0.03f, 0.08f, 0.45f);
            SetRect(deathReplayLabel.rectTransform, new Vector2(0.08f, 0.075f), new Vector2(0.92f, 0.16f), Vector2.zero, Vector2.zero);
            deathReplayOverlay.SetActive(false);

            settingsButton = CreateIconButton(safe, "Settings Button", RuntimeAssets.SettingsIcon, ToggleSettings);
            SetSquareRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.5f), 112f);

            infoButton = CreateRoundTextButton(safe, "Credits Button", "i", ToggleCredits);
            SetSquareRect(infoButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.5f), 76f);

            styleButton = CreateIconButton(safe, "Hangar Button", RuntimeAssets.RocketSprite, ToggleHangar);
            SetSquareRect(styleButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.405f), 92f);

            powerUpButton = CreateIconButton(safe, "Power Ups Button", RuntimeAssets.PowerUpUpgradeIcon, TogglePowerUps);
            SetSquareRect(powerUpButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.31f), 92f);

            missionsButton = CreateIconButton(safe, "Missions Button", RuntimeAssets.TrophyIcon, ToggleMissions);
            SetSquareRect(missionsButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.405f), 92f);

            leaderboardButton = CreateIconButton(safe, "Leaderboard Button", RuntimeAssets.LeaderboardIcon, ToggleLeaderboard);
            SetSquareRect(leaderboardButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.31f), 92f);

            pauseButton = CreateIconButton(safe, "Pause Button", RuntimeAssets.PauseIcon, PauseGame);
            SetSquareRect(pauseButton.GetComponent<RectTransform>(), new Vector2(0.095f, 0.92f), 104f);
            pauseButton.SetActive(false);

            for (int i = 0; i < powerUpInventoryButtons.Length; i++)
            {
                int index = i;
                PowerUpType type = (PowerUpType)i;
                GameObject button = CreateIconButton(safe, type + " Inventory", RuntimeAssets.GetPowerUpIcon(type), () => PowerUpRequested?.Invoke((PowerUpType)index));
                SetSquareRect(button.GetComponent<RectTransform>(), new Vector2(0.31f + i * 0.095f, 0.075f), 78f);
                Image icon = button.transform.Find("Icon").GetComponent<Image>(); icon.color = PowerUpProgression.Definition(type).Color;
                Text count = CreateText(button.transform, "Count", string.Empty, 21, TextAnchor.LowerRight, FontStyle.Bold);
                count.color = Color.white; count.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.9f);
                count.resizeTextForBestFit = false;
                count.horizontalOverflow = HorizontalWrapMode.Overflow;
                SetRect(count.rectTransform, new Vector2(0.1f, -0.13f), new Vector2(0.9f, 0.16f), Vector2.zero, Vector2.zero);
                count.alignment = TextAnchor.MiddleCenter;
                powerUpInventoryButtons[i] = button; powerUpInventoryCounts[i] = count; button.SetActive(false);
                inventoryControls[i] = button.GetComponent<Button>();
                inventoryIcons[i] = button.transform.Find("Icon").GetComponent<Image>();
            }

            for (int i = 1; i < activePowerRows.Length; i++)
            {
                PowerUpType type = (PowerUpType)i;
                Image row = CreateImage(safe, type + " Active Timer", new Color(0.018f, 0.07f, 0.13f, 0.92f)); ApplyRounded(row);
                SetRect(row.rectTransform, new Vector2(0.68f, 0.79f - (i - 1) * 0.055f), new Vector2(0.95f, 0.835f - (i - 1) * 0.055f), Vector2.zero, Vector2.zero);
                Image icon = CreateImage(row.transform, "Icon", PowerUpProgression.Definition(type).Color); icon.sprite = RuntimeAssets.GetPowerUpIcon(type); icon.preserveAspect = true;
                SetRect(icon.rectTransform, new Vector2(0.03f, 0.12f), new Vector2(0.2f, 0.88f), Vector2.zero, Vector2.zero);
                Image track = CreateImage(row.transform, "Timer Track", new Color(0.05f, 0.14f, 0.2f, 1f)); ApplyRounded(track);
                SetRect(track.rectTransform, new Vector2(0.23f, 0.17f), new Vector2(0.76f, 0.38f), Vector2.zero, Vector2.zero);
                Image fill = CreateImage(track.transform, "Timer Fill", PowerUpProgression.Definition(type).Color); ApplyRounded(fill);
                SetRect(fill.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                Text timer = CreateText(row.transform, "Time", string.Empty, 17, TextAnchor.MiddleRight, FontStyle.Bold);
                timer.color = Color.white; SetRect(timer.rectTransform, new Vector2(0.74f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
                activePowerRows[i] = row.gameObject; activePowerFills[i] = fill; activePowerTimes[i] = timer; row.gameObject.SetActive(false);
            }

            CreateHyperspaceOverlay(safe);

            pausePanel = CreatePausePanel(safe);
            pausePanel.SetActive(false);

            settingsPanel = CreateSettingsPanel(safe, audio);
            settingsPanel.SetActive(false);
            creditsPanel = CreateCreditsPanel(safe);
            creditsPanel.SetActive(false);
            missionsPanel = CreateMissionsPanel(safe);
            missionsPanel.SetActive(false);
            hangarPanel = CreateHangarPanel(safe);
            hangarPanel.SetActive(false);
            powerUpPanel = CreatePowerUpPanel(safe);
            powerUpPanel.SetActive(false);
            leaderboardPanel = CreateLeaderboardPanel(safe);
            leaderboardPanel.SetActive(false);
            playerNamePanel = CreatePlayerNamePanel(safe);
            playerNamePanel.SetActive(false);
            CreateExtraMenus(safe);
        }

        public void PreparePlayerIdentity(Action onAccepted)
        {
            identityAccepted = onAccepted;
            if (onlineLeaderboard == null || onlineLeaderboard.NeedsPlayerName)
            {
                playerNameStatus.text = onlineLeaderboard != null && !string.IsNullOrEmpty(onlineLeaderboard.LastError)
                    ? onlineLeaderboard.LastError + " · CHOISIS TON PSEUDO"
                    : "3 À 16 CARACTÈRES · LETTRES, CHIFFRES, _ OU -";
                playerNamePanel.SetActive(true);
                return;
            }
            identityAccepted?.Invoke();
            identityAccepted = null;
        }

        public void ShowPlaying(int distance, int best, bool tutorial)
        {
            CancelInvoke(nameof(ClearSector));
            ClearSector();
            UpdateProgress(distance, best);
            titleText.text = "ORBIT\nBREAKER";
            instructionText.text = "TOUCHE POUR TE PROPULSER";
            instructionText.color = new Color(0.67f, 0.84f, 0.95f, 0.95f);
            stuntText.text = string.Empty;
            nearMissText.text = string.Empty;
            titleText.gameObject.SetActive(tutorial);
            hintGroup.gameObject.SetActive(tutorial);
            tutorialTips.SetActive(tutorial);
            settingsButton.SetActive(tutorial);
            infoButton.SetActive(tutorial);
            styleButton.SetActive(tutorial);
            // Le bouton d'amélioration reste accessible depuis le menu principal;
            // seuls les boutons d'inventaire en partie sont masqués avant le lancement.
            powerUpButton.SetActive(true);
            missionsButton.SetActive(tutorial);
            leaderboardButton.SetActive(tutorial);
            pauseButton.SetActive(!tutorial);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            if (statisticsPanel != null) statisticsPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            gameOverVisible = false;
            if (deathReplayOverlay != null) deathReplayOverlay.SetActive(false);
            if (materialToast != null) materialToast.SetActive(false);
            if (challengeToast != null) challengeToast.SetActive(false);
            if (nearMissText != null) nearMissText.text = string.Empty;
            if (powerUpToast != null) powerUpToast.SetActive(false);
            for (int i = 1; i < activePowerRows.Length; i++) if (activePowerRows[i] != null) activePowerRows[i].SetActive(false);
            UpdatePowerUpInventory(new int[5], false);
        }

        public void ShowDailyAvailability(bool attempted, bool completed)
        {
            if (!attempted) return;
            titleText.text = completed ? "PARCOURS\nACCOMPLI" : "ESSAI\nTERMINÉ";
            instructionText.text = completed ? "PARCOURS DU JOUR DÉJÀ TERMINÉ" : "ESSAI DU JOUR DÉJÀ EFFECTUÉ";
            instructionText.color = completed ? new Color(0.42f, 1f, 0.72f) : new Color(1f, 0.72f, 0.24f);
        }

        public void UpdateProgress(int distance, int best)
        {
            lastDistance = distance; lastRecord = best;
            scoreText.text = distance + " UA";
            bestText.text = "RECORD " + best + " UA";
            comboText.text = string.Empty;
        }

        public void ShowSector(int sector)
        {
            sectorText.text = "SECTEUR " + (sector + 1) + "  ·  " + sector * 500 + " UA";
            CancelInvoke(nameof(ClearSector));
            Invoke(nameof(ClearSector), 2.4f);
        }

        private void ClearSector() { if (sectorText != null) sectorText.text = string.Empty; }

        public void UpdateFlightDisplay(Vector3 worldPosition, float multiplier, float danger01, bool flying)
        {
            if (materialToast != null && materialToast.activeSelf)
                materialToast.transform.localScale = Vector3.Lerp(materialToast.transform.localScale, Vector3.one, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 15f));
            if (challengeToast != null && challengeToast.activeSelf)
                challengeToast.transform.localScale = Vector3.Lerp(challengeToast.transform.localScale, Vector3.one, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 12f));
            multiplierBadge.SetActive(flying && GamePreferences.FlightGauges);
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
                    local.y = Mathf.Clamp(local.y + 105f, bounds.yMin + 100f, bounds.yMax - 370f);
                    floatingHud.anchoredPosition = Vector2.Lerp(floatingHud.anchoredPosition, local, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 13f));
                }
            }
            if (!string.IsNullOrEmpty(stuntText.text))
            {
                float pop = Mathf.Clamp01((Time.unscaledTime - stuntShownAt) / 0.16f);
                stuntText.transform.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, 1f - Mathf.Pow(1f - pop, 3f));
            }
            if (!string.IsNullOrEmpty(nearMissText.text))
                nearMissText.transform.localScale = Vector3.Lerp(nearMissText.transform.localScale, Vector3.one, 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f));
            if (!flying) return;
            multiplierText.text = "x" + multiplier.ToString("0.0");
            float intensity = Mathf.Clamp01(danger01);
            multiplierText.color = Color.Lerp(new Color(0.25f, 0.92f, 1f), new Color(1f, 0.32f, 0.55f), intensity);
            multiplierBadge.transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 8f) * (0.018f + intensity * 0.035f));
            multiplierFill.rectTransform.anchorMax = new Vector2(intensity, 1f);
            multiplierFill.color = Color.Lerp(new Color(0.2f, 0.88f, 1f, 0.35f), new Color(1f, 0.2f, 0.5f, 0.55f), intensity);
        }

        public void ShowLanding(int distance, int best, int gainedDistance, float multiplier, int skippedAnchors, bool backtrack, bool revisited, SynchronizationResult synchronization, int skipChain = 0)
        {
            UpdateProgress(distance, best);
            if (skipChain >= 2)
                comboText.text = "SÉRIE DE SKIPS " + skipChain + "\nBONUS ×" + GameTuning.SkipChainMultiplier(skipChain).ToString("0.##");
            if (skipChain >= 2)
            {
                comboText.transform.SetParent(floatingHud, false);
                SetRect(comboText.rectTransform, new Vector2(0f, 1.44f), new Vector2(1f, 1.84f), Vector2.zero, Vector2.zero);
                comboText.alignment = TextAnchor.MiddleCenter;
            }
            multiplierBadge.SetActive(false);
            settingsButton.SetActive(false);
            pauseButton.SetActive(true);
            string label = backtrack ? "RETOUR ORBITAL" : revisited ? "CHECKPOINT" : synchronization == SynchronizationResult.Success
                ? "SYNCHRO +0.4x" + (skippedAnchors > 0 ? "  SKIP x" + (skippedAnchors + 1) : string.Empty)
                : synchronization == SynchronizationResult.WrongDirection ? "ZONE ATTEINTE  MAUVAIS SENS"
                : skippedAnchors > 0 ? "SKIP x" + (skippedAnchors + 1) : multiplier >= 2f ? "LONG VOL" : string.Empty;
            string delta = gainedDistance > 0 ? "+" + gainedDistance : gainedDistance < 0 ? gainedDistance.ToString() : string.Empty;
            stuntText.text = string.IsNullOrEmpty(label) ? string.Empty : label + (string.IsNullOrEmpty(delta) ? string.Empty : "  " + delta + " UA");
            stuntShownAt = Time.unscaledTime;
            CancelInvoke(nameof(ClearStunt));
            Invoke(nameof(ClearStunt), 1.15f);
        }

        public void ShowNearMiss(int chain, float multiplier)
        {
            nearMissText.text = "FRÔLEMENT" + (chain > 1 ? " x" + chain : string.Empty) + "  ·  MULTI x" + multiplier.ToString("0.0");
            nearMissText.transform.localScale = Vector3.one * 1.18f;
            CancelInvoke(nameof(ClearNearMiss));
            Invoke(nameof(ClearNearMiss), 1.05f);
        }

        public void ShowMaterialPickup(Vector2 position, int value)
        {
            materialToastText.text = "+" + value + " MATÉRIAU" + (value > 1 ? "X" : string.Empty) + "   ·   TOTAL " + MetaProgression.Materials;
            materialToastText.color = value >= 7 ? new Color(1f, 0.72f, 0.2f) : value >= 3 ? new Color(0.78f, 0.55f, 1f) : new Color(0.35f, 0.95f, 1f);
            materialToast.SetActive(true);
            materialToast.transform.localScale = Vector3.one * 1.12f;
            CancelInvoke(nameof(ClearMaterialToast));
            Invoke(nameof(ClearMaterialToast), 1.15f);
        }

        public void RefreshMetaPanels()
        {
            if (missionsPanel != null && missionsPanel.activeSelf) RefreshMissions();
            if (hangarPanel != null && hangarPanel.activeSelf) RefreshHangar(string.Empty);
            if (powerUpPanel != null && powerUpPanel.activeSelf) RefreshPowerUps(string.Empty);
        }

        public void UpdatePowerUpInventory(int[] counts, bool playing)
        {
            for (int i = 0; i < powerUpInventoryButtons.Length; i++)
            {
                int count = counts != null && i < counts.Length ? counts[i] : 0;
                inventoryCounts[i] = count;
                powerUpInventoryButtons[i].SetActive(playing);
                SetSquareRect(powerUpInventoryButtons[i].GetComponent<RectTransform>(),
                    new Vector2(0.16f + i * 0.17f, tutorialTips.activeSelf ? 0.285f : 0.075f), 112f);
                inventoryControls[i].interactable = count > 0;
                inventoryIcons[i].color = count > 0
                    ? PowerUpProgression.Definition((PowerUpType)i).Color : new Color(0.3f, 0.38f, 0.44f, 0.6f);
                powerUpInventoryCounts[i].text = count + "/" + PowerUpProgression.MaxInventory;
            }
        }

        public void UpdateActivePowerUps(OrbitPlayer player)
        {
            if (player == null) return;
            for (int i = 1; i < activePowerRows.Length; i++)
            {
                if (activePowerRows[i] == null) continue;
                PowerUpType type = (PowerUpType)i;
                float remaining = player.PowerUpRemaining(type);
                bool active = remaining > 0.01f && player.State != PlayerOrbitState.Dead && !gameOverVisible;
                inventoryControls[i].interactable = !active && inventoryCounts[i] > 0 && !IsPaused && !SettingsOpen;
                activePowerRows[i].SetActive(active);
                if (!active) continue;
                float duration = Mathf.Max(0.01f, player.PowerUpDuration(type));
                RectTransform fill = activePowerFills[i].rectTransform;
                fill.anchorMax = new Vector2(Mathf.Clamp01(remaining / duration), 1f);
                fill.offsetMin = fill.offsetMax = Vector2.zero;
                activePowerTimes[i].text = remaining.ToString("0.0") + "s";
            }
        }

        public void ShowPowerUpPickup(PowerUpType type, int inventoryCount, bool collected)
        {
            PowerUpDefinition definition = PowerUpProgression.Definition(type);
            powerUpToastText.text = definition.Name + (collected ? "  AJOUTÉ · " : "  STOCK PLEIN · ") + inventoryCount + "/" + PowerUpProgression.MaxInventory;
            powerUpToastText.color = collected ? definition.Color : new Color(1f, 0.45f, 0.28f);
            powerUpToast.SetActive(true); powerUpToast.transform.localScale = Vector3.one * 1.13f;
            CancelInvoke(nameof(ClearPowerUpToast)); Invoke(nameof(ClearPowerUpToast), 1.35f);
        }

        public void ShowPowerUpActivated(PowerUpType type)
        {
            PowerUpDefinition definition = PowerUpProgression.Definition(type);
            powerUpToastText.text = definition.Name + "  ACTIVÉ"; powerUpToastText.color = definition.Color;
            powerUpToast.SetActive(true); powerUpToast.transform.localScale = Vector3.one * 1.18f;
            CancelInvoke(nameof(ClearPowerUpToast)); Invoke(nameof(ClearPowerUpToast), 1.2f);
        }

        public void ShowChallengeComplete(string objective)
        {
            challengeToastText.text = "DÉFI TERMINÉ  ·  " + objective;
            challengeToast.SetActive(true);
            challengeToast.transform.localScale = Vector3.one * 1.1f;
            CancelInvoke(nameof(ClearChallengeToast));
            Invoke(nameof(ClearChallengeToast), 2.1f);
        }

        public void HideTutorial()
        {
            if (modeButton != null) modeButton.SetActive(false);
            if (journalButton != null) journalButton.SetActive(false);
            if (statisticsButton != null) statisticsButton.SetActive(false);
            hudFeedback.StopMusicPreview();
            titleText.gameObject.SetActive(false);
            hintGroup.gameObject.SetActive(false);
            tutorialTips.SetActive(false);
            settingsButton.SetActive(false);
            infoButton.SetActive(false);
            styleButton.SetActive(false);
            powerUpButton.SetActive(false);
            missionsButton.SetActive(false);
            leaderboardButton.SetActive(false);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            if (statisticsPanel != null) statisticsPanel.SetActive(false);
            pauseButton.SetActive(true);
        }

        public void ShowGameOver(int distance, int best, int anchors, DeathReason reason, int synchronizations, int nearMisses, int bestSkip, float bestMultiplier, int runMaterials)
        {
            for (int i = 1; i < activePowerRows.Length; i++) if (activePowerRows[i] != null) activePowerRows[i].SetActive(false);
            if (materialToast != null) materialToast.SetActive(false);
            if (challengeToast != null) challengeToast.SetActive(false);
            if (nearMissText != null) nearMissText.text = string.Empty;
            scoreText.text = distance + " UA";
            multiplierBadge.SetActive(false);
            bestText.text = "RECORD " + best + " UA";
            gameOverDistance.text = "DISTANCE     " + distance + " UA";
            gameOverOrbits.text = "ORBITES      " + anchors;
            gameOverRecord.text = "RECORD       " + best + " UA";
            gameOverSummary.text = "MATÉRIAUX +" + runMaterials + "   ·   SYNCHRO " + synchronizations + "   ·   FRÔLEMENTS " + nearMisses + "   ·   SKIP " + bestSkip + "   ·   MAX x" + bestMultiplier.ToString("0.0");
            gameOverTitle.text = reason == DeathReason.Breaker ? "VOTRE VAISSEAU\nA EXPLOSÉ" : "VOUS VOUS ÊTES PERDU\nDANS L'ESPACE";
            if(game!=null)
            {
                if(game.LastRunTimedOut)gameOverTitle.text="SPRINT TERMINÉ";
                if(game.LastRunDailyCompleted)
                {
                    gameOverTitle.text="PARCOURS ACCOMPLI";
                    gameOverDistance.text="NIVEAU       "+game.DailyTier+" / 5";
                    gameOverOrbits.text="OBJECTIF     "+game.DailyTarget+" / "+game.DailyTarget;
                    gameOverRecord.text="JOURS RÉUSSIS "+DailyCourse.CompletedDays;
                    gameOverSummary.text=(game.LastDailyReward>0?"RÉCOMPENSE +"+game.LastDailyReward+" MAT":"RÉCOMPENSE DU JOUR DÉJÀ RÉCUPÉRÉE")+(string.IsNullOrEmpty(game.LastDailyUnlock)?string.Empty:"\nNOUVEAU VAISSEAU EXCLUSIF DÉBLOQUÉ");
                }
                else
                {
                    if(game.CurrentRunMode==RunMode.Daily){gameOverDistance.text="NIVEAU       "+game.DailyTier+" / 5";gameOverOrbits.text="OBJECTIF     "+game.DailyCaptures+" / "+game.DailyTarget;gameOverRecord.text="RÉCOMPENSE   "+DailyCourse.ForDate(DateTime.UtcNow).MaterialReward+" MAT";}
                    gameOverSummary.text=(game.CurrentRunMode==RunMode.Endless ? "MATÉRIAUX +"+runMaterials : game.CurrentRunMode==RunMode.Daily?"PARCOURS "+game.DailyCaptures+" / "+game.DailyTarget:"CLASSEMENT 90 S")+" · MEILLEURE SÉRIE "+game.LastRunBestChain+"\n"+(game.LastRunTimedOut?"TENTE UN SKIP DE PLUS AU PROCHAIN SPRINT":reason==DeathReason.Breaker?"OBSERVE LE PASSAGE DES DÉBRIS AVANT DE PARTIR":"VISE UNE ORBITE AVANT QUE LE CARBURANT S'ÉPUISE");
                }
            }
            gameOverTitle.color = reason == DeathReason.Breaker ? new Color(1f, 0.28f, 0.38f) : new Color(0.45f, 0.82f, 1f);
            gameOverPanel.SetActive(true);
            settingsButton.SetActive(true);
            infoButton.SetActive(true);
            styleButton.SetActive(true);
            powerUpButton.SetActive(true);
            missionsButton.SetActive(true);
            leaderboardButton.SetActive(true);
            statisticsButton.SetActive(true);
            pauseButton.SetActive(false);
            gameOverVisible = true;
        }

        public void ShowDeathReplay()
        {
            gameOverPanel.SetActive(false);
            gameOverVisible = false;
            titleText.gameObject.SetActive(false);
            hintGroup.gameObject.SetActive(false);
            tutorialTips.SetActive(false);
            pauseButton.SetActive(false);
            settingsButton.SetActive(false);
            infoButton.SetActive(false);
            styleButton.SetActive(false);
            powerUpButton.SetActive(false);
            missionsButton.SetActive(false);
            leaderboardButton.SetActive(false);
            for (int i = 0; i < powerUpInventoryButtons.Length; i++) powerUpInventoryButtons[i].SetActive(false);
            deathReplayOverlay.SetActive(true);
        }

        public void HideDeathReplay()
        {
            if (deathReplayOverlay != null) deathReplayOverlay.SetActive(false);
        }

        public void ShowDailyProgress(int captures, int target, int tier)
        {
            scoreText.text=captures+" / "+target;
            bestText.text="PARCOURS DU JOUR · NIVEAU "+tier;
        }

        private void ClearStunt()
        {
            stuntText.text = string.Empty;
            stuntText.transform.localScale = Vector3.one;
        }

        private void ClearNearMiss()
        {
            nearMissText.text = string.Empty;
            nearMissText.transform.localScale = Vector3.one;
        }

        private void ClearMaterialToast()
        {
            if (materialToast != null) materialToast.SetActive(false);
        }

        private void ClearChallengeToast()
        {
            if (challengeToast != null) challengeToast.SetActive(false);
        }

        private void ClearPowerUpToast() { if (powerUpToast != null) powerUpToast.SetActive(false); }

        private void ToggleSettings()
        {
            bool opening = !settingsPanel.activeSelf;
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            statisticsPanel.SetActive(false);
            settingsPanel.SetActive(opening);
            if (opening) ShowSettingsTab(0);
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void ToggleCredits()
        {
            bool opening = !creditsPanel.activeSelf;
            settingsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            statisticsPanel.SetActive(false);
            creditsPanel.SetActive(opening);
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void ToggleMissions()
        {
            bool opening = !missionsPanel.activeSelf;
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            statisticsPanel.SetActive(false);
            missionsPanel.SetActive(opening);
            if (opening) RefreshMission();
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void ToggleHangar()
        {
            hudFeedback.StopMusicPreview();
            bool opening = !hangarPanel.activeSelf;
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            statisticsPanel.SetActive(false);
            hangarPanel.SetActive(opening);
            if (opening) RefreshHangar(string.Empty);
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void TogglePowerUps()
        {
            bool opening = !powerUpPanel.activeSelf;
            settingsPanel.SetActive(false); creditsPanel.SetActive(false); missionsPanel.SetActive(false);
            hangarPanel.SetActive(false); leaderboardPanel.SetActive(false); statisticsPanel.SetActive(false); powerUpPanel.SetActive(opening);
            if (opening) RefreshPowerUps(string.Empty);
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private async void AcceptPlayerName()
        {
            if (onlineLeaderboard == null) return;
            playerNameStatus.text = "CONNEXION...";
            bool saved = await onlineLeaderboard.SavePlayerNameAsync(playerNameInput.text);
            if (!saved && onlineLeaderboard.NeedsPlayerName)
            {
                playerNameStatus.text = onlineLeaderboard.LastError;
                return;
            }
            playerNamePanel.SetActive(false);
            identityAccepted?.Invoke();
            identityAccepted = null;
        }

        private void ToggleLeaderboard()
        {
            bool opening = !leaderboardPanel.activeSelf;
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            powerUpPanel.SetActive(false);
            statisticsPanel.SetActive(false);
            leaderboardPanel.SetActive(opening);
            gameOverPanel.SetActive(!opening && gameOverVisible);
            if (!opening) return;
            leaderboardSearchInput.SetTextWithoutNotify(string.Empty);
            leaderboardStatus.text = "CHARGEMENT DU CLASSEMENT...";
            RenderLeaderboardRows(onlineLeaderboard.CachedEntries);
            RefreshLeaderboard();
        }

        private void SearchLeaderboard(string query)
        {
            IReadOnlyList<OrbitLeaderboardEntry> entries = onlineLeaderboard.Filter(query);
            leaderboardStatus.text = entries.Count > 0 ? entries.Count + " PILOTE" + (entries.Count > 1 ? "S" : string.Empty) : "AUCUN PILOTE TROUVÉ DANS LE TOP 100";
            RenderLeaderboardRows(entries);
        }

        private void SelectLeaderboardBoard(RunMode mode)
        {
            if (leaderboardLoading || onlineLeaderboard == null) return;
            onlineLeaderboard.SelectBoard(mode);
            leaderboardEndlessTab.color = mode == RunMode.Endless ? new Color(1f,.75f,.24f) : new Color(.65f,.82f,.9f);
            leaderboardSprintTab.color = mode == RunMode.Sprint ? new Color(1f,.75f,.24f) : new Color(.65f,.82f,.9f);
            leaderboardEndlessTabImage.color = mode == RunMode.Endless ? new Color(.08f,.4f,.5f) : new Color(.035f,.16f,.24f);
            leaderboardSprintTabImage.color = mode == RunMode.Sprint ? new Color(.08f,.4f,.5f) : new Color(.035f,.16f,.24f);
            leaderboardModeHint.text = mode == RunMode.Endless ? "MEILLEURE DISTANCE · TOP 100" : "MEILLEURE DISTANCE EN 90 S · TOP 100";
            leaderboardSearchInput.SetTextWithoutNotify(string.Empty);
            RenderLeaderboardRows(onlineLeaderboard.CachedEntries);
            RefreshLeaderboard();
        }

        private void RenderLeaderboardRows(IReadOnlyList<OrbitLeaderboardEntry> entries)
        {
            Canvas.ForceUpdateCanvases();
            int visibleCount = Mathf.Min(entries.Count, leaderboardRows.Length);
            float rowHeight = leaderboardScroll.viewport.rect.height / 10f;
            if (leaderboardContent != null)
            {
                // Une fenêtre affiche environ 10 lignes; au-delà, le contenu devient
                // naturellement scrollable jusqu'à la 100e place.
                leaderboardScroll.StopMovement();
                leaderboardContent.sizeDelta = new Vector2(0f, visibleCount * rowHeight);
                leaderboardContent.anchoredPosition = new Vector2(0f, 0f);
            }
            for (int i = 0; i < leaderboardRows.Length; i++)
            {
                Text row = leaderboardRows[i];
                bool visible = i < entries.Count;
                row.gameObject.SetActive(visible);
                if (leaderboardContent != null && visible)
                {
                    row.rectTransform.anchorMin = new Vector2(0.03f, 1f);
                    row.rectTransform.anchorMax = new Vector2(0.97f, 1f);
                    row.rectTransform.pivot = new Vector2(0.5f, 1f);
                    row.rectTransform.sizeDelta = new Vector2(0f, rowHeight - 4f);
                    row.rectTransform.anchoredPosition = new Vector2(0f, -i * rowHeight - 2f);
                }
                if (!visible) continue;
                OrbitLeaderboardEntry entry = entries[i];
                row.text = OnlineLeaderboard.FormatRow(entry, onlineLeaderboard.ActiveMode);
                row.color = entry.IsLocalPlayer ? new Color(1f, 0.75f, 0.24f) : new Color(0.72f, 0.92f, 1f);
            }
        }

        private GameObject CreatePlayerNamePanel(Transform safe)
        {
            GameObject panel = new GameObject("Player Name Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false); SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.005f, 0.015f, 0.045f, 0.96f);
            Image card = CreateImage(panel.transform, "Pilot Identity Card", new Color(0.025f, 0.075f, 0.14f, 0.99f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.09f, 0.31f), new Vector2(0.91f, 0.7f), Vector2.zero, Vector2.zero);
            Text eyebrow = CreateText(card.transform, "Eyebrow", "PREMIER DÉCOLLAGE", 21, TextAnchor.MiddleCenter, FontStyle.Bold);
            eyebrow.color = new Color(1f, 0.72f, 0.24f); SetRect(eyebrow.rectTransform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.91f), Vector2.zero, Vector2.zero);
            Text title = CreateText(card.transform, "Title", "CHOISIS TON PSEUDO", 42, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.color = new Color(0.76f, 0.98f, 1f); SetRect(title.rectTransform, new Vector2(0.06f, 0.58f), new Vector2(0.94f, 0.8f), Vector2.zero, Vector2.zero);
            playerNameInput = CreateInputField(card.transform, "Player Name", "TON PSEUDO", 16);
            SetRect(playerNameInput.GetComponent<RectTransform>(), new Vector2(0.1f, 0.37f), new Vector2(0.9f, 0.55f), Vector2.zero, Vector2.zero);
            playerNameStatus = CreateText(card.transform, "Status", string.Empty, 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            playerNameStatus.color = new Color(0.52f, 0.76f, 0.9f); playerNameStatus.resizeTextForBestFit = true; playerNameStatus.resizeTextMinSize = 11;
            SetRect(playerNameStatus.rectTransform, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.37f), Vector2.zero, Vector2.zero);
            GameObject accept = CreateButton(card.transform, "Confirm Name", "ENTRER EN ORBITE", new Color(0.08f, 0.52f, 0.64f), AcceptPlayerName);
            SetRect(accept.GetComponent<RectTransform>(), new Vector2(0.2f, 0.07f), new Vector2(0.8f, 0.22f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private GameObject CreateLeaderboardPanel(Transform safe)
        {
            GameObject panel = new GameObject("Leaderboard Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); panel.GetComponent<Image>().color = new Color(0.005f, 0.015f, 0.045f, 0.94f);
            Image card = CreateImage(panel.transform, "Leaderboard Card", new Color(0.025f, 0.075f, 0.14f, 0.99f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.055f, 0.08f), new Vector2(0.945f, 0.92f), Vector2.zero, Vector2.zero);
            Text title = CreateText(card.transform, "Title", "CLASSEMENT MONDIAL", 40, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.color = new Color(0.76f, 0.98f, 1f); SetRect(title.rectTransform, new Vector2(0.05f, 0.91f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);
            GameObject endlessTab=CreateButton(card.transform,"Endless Board","INFINI",new Color(.035f,.16f,.24f),()=>SelectLeaderboardBoard(RunMode.Endless));
            GameObject sprintTab=CreateButton(card.transform,"Sprint Board","SPRINT 90 S",new Color(.035f,.16f,.24f),()=>SelectLeaderboardBoard(RunMode.Sprint));
            SetRect(endlessTab.GetComponent<RectTransform>(),new Vector2(.1f,.84f),new Vector2(.49f,.9f),Vector2.zero,Vector2.zero);
            SetRect(sprintTab.GetComponent<RectTransform>(),new Vector2(.51f,.84f),new Vector2(.9f,.9f),Vector2.zero,Vector2.zero);
            leaderboardEndlessTab=endlessTab.transform.Find("Label").GetComponent<Text>();leaderboardSprintTab=sprintTab.transform.Find("Label").GetComponent<Text>();
            leaderboardEndlessTabImage=endlessTab.GetComponent<Image>(); leaderboardSprintTabImage=sprintTab.GetComponent<Image>();
            leaderboardEndlessTab.color=new Color(1f,.75f,.24f);
            leaderboardEndlessTabImage.color=new Color(.08f,.4f,.5f);
            leaderboardModeHint=CreateText(card.transform,"Board Context","MEILLEURE DISTANCE · TOP 100",15,TextAnchor.MiddleCenter,FontStyle.Bold);
            leaderboardModeHint.color=new Color(.48f,.77f,.9f); SetRect(leaderboardModeHint.rectTransform,new Vector2(.08f,.805f),new Vector2(.92f,.842f),Vector2.zero,Vector2.zero);
            leaderboardSearchInput = CreateInputField(card.transform, "Search", "RECHERCHER DANS LE TOP 100", 24);
            SetRect(leaderboardSearchInput.GetComponent<RectTransform>(), new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.8f), Vector2.zero, Vector2.zero);
            leaderboardSearchInput.onValueChanged.AddListener(SearchLeaderboard);
            leaderboardStatus = CreateText(card.transform, "Status", string.Empty, 17, TextAnchor.MiddleCenter, FontStyle.Bold);
            leaderboardStatus.color = new Color(1f, 0.72f, 0.24f); SetRect(leaderboardStatus.rectTransform, new Vector2(0.06f, 0.665f), new Vector2(0.94f, 0.715f), Vector2.zero, Vector2.zero);
            GameObject viewportObject = new GameObject("Leaderboard Scroll Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewportObject.transform.SetParent(card.transform, false);
            Image viewportImage = viewportObject.GetComponent<Image>(); viewportImage.color = new Color(0f, 0f, 0f, 0.001f); viewportImage.raycastTarget = true;
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            SetRect(viewport, new Vector2(0.06f, 0.17f), new Vector2(0.94f, 0.66f), Vector2.zero, Vector2.zero);
            GameObject contentObject = new GameObject("Leaderboard Scroll Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            leaderboardContent = content;
            content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f); content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0f, 620f);
            leaderboardScroll = viewportObject.AddComponent<ScrollRect>();
            leaderboardScroll.viewport = viewport; leaderboardScroll.content = content;
            leaderboardScroll.horizontal = false; leaderboardScroll.vertical = true; leaderboardScroll.movementType = ScrollRect.MovementType.Clamped;
            leaderboardScroll.scrollSensitivity = 48f; leaderboardScroll.inertia = true; leaderboardScroll.decelerationRate = 0.12f;
            for (int i = 0; i < leaderboardRows.Length; i++)
            {
                float top = 1f - i * 0.1f;
                Text row = CreateText(content, "Rank " + (i + 1), string.Empty, 23, TextAnchor.MiddleLeft, FontStyle.Bold);
                row.resizeTextForBestFit = true; row.resizeTextMinSize = 12; row.resizeTextMaxSize = 23;
                row.horizontalOverflow = HorizontalWrapMode.Wrap; row.verticalOverflow = VerticalWrapMode.Truncate;
                SetRect(row.rectTransform, new Vector2(0.03f, top - 0.085f), new Vector2(0.97f, top), Vector2.zero, Vector2.zero);
                leaderboardRows[i] = row;
            }
            GameObject refresh = CreateButton(card.transform, "Refresh", "ACTUALISER", new Color(0.08f, 0.36f, 0.48f), ToggleLeaderboard);
            refresh.GetComponent<Button>().onClick.RemoveAllListeners(); refresh.GetComponent<Button>().onClick.AddListener(RefreshLeaderboard);
            SetRect(refresh.GetComponent<RectTransform>(), new Vector2(0.08f, 0.065f), new Vector2(0.44f, 0.135f), Vector2.zero, Vector2.zero);
            GameObject close = CreateButton(card.transform, "Close", "FERMER", new Color(0.06f, 0.2f, 0.3f), ToggleLeaderboard);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.56f, 0.065f), new Vector2(0.92f, 0.135f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private async void RefreshLeaderboard()
        {
            if (leaderboardLoading) return;
            leaderboardLoading = true;
            try
            {
            leaderboardStatus.text = "ACTUALISATION...";
            IReadOnlyList<OrbitLeaderboardEntry> entries = await onlineLeaderboard.RefreshAsync(leaderboardSearchInput.text);
            if (this == null) return;
            leaderboardStatus.text = !string.IsNullOrEmpty(onlineLeaderboard.LastError)
                ? (entries.Count > 0 ? "HORS LIGNE · DERNIERS RÉSULTATS CONSERVÉS" : onlineLeaderboard.LastError)
                : "À JOUR · " + (onlineLeaderboard.LastRefreshUtc?.ToLocalTime().ToString("HH:mm") ?? "—");
            RenderLeaderboardRows(onlineLeaderboard.Filter(leaderboardSearchInput.text));
            }
            finally { leaderboardLoading = false; }
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

        private GameObject CreateMissionsPanel(Transform safe)
        {
            var panel = new GameObject("Missions Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false); SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.008f, 0.02f, 0.055f, 0.94f);
            Image card = CreateImage(panel.transform, "Challenges Card", new Color(0.025f, 0.075f, 0.14f, 0.99f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.06f, 0.14f), new Vector2(0.94f, 0.86f), Vector2.zero, Vector2.zero);
            Text title = CreateText(card.transform, "Title", "DÉFIS ACTIFS", 39, TextAnchor.MiddleLeft, FontStyle.Bold); title.color = new Color(1f, 0.72f, 0.24f);
            SetRect(title.rectTransform, new Vector2(0.07f, 0.87f), new Vector2(0.65f, 0.97f), Vector2.zero, Vector2.zero);
            missionCurrencyText = CreateText(card.transform, "Balance", string.Empty, 24, TextAnchor.MiddleRight, FontStyle.Bold); missionCurrencyText.color = new Color(0.3f, 0.94f, 1f);
            SetRect(missionCurrencyText.rectTransform, new Vector2(0.58f, 0.87f), new Vector2(0.93f, 0.97f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < 3; i++)
            {
                int slot = i; float top = 0.84f - i * 0.225f;
                Image row = CreateImage(card.transform, "Challenge " + (i + 1), new Color(0.035f, 0.12f, 0.2f, 0.98f)); ApplyRounded(row);
                SetRect(row.rectTransform, new Vector2(0.055f, top - 0.19f), new Vector2(0.945f, top), Vector2.zero, Vector2.zero);
                challengeLabels[i] = CreateText(row.transform, "Objective", string.Empty, 23, TextAnchor.MiddleLeft, FontStyle.Bold);
                challengeLabels[i].resizeTextForBestFit = true; challengeLabels[i].resizeTextMinSize = 14; challengeLabels[i].resizeTextMaxSize = 23;
                SetRect(challengeLabels[i].rectTransform, new Vector2(0.05f, 0.49f), new Vector2(0.73f, 0.93f), Vector2.zero, Vector2.zero);
                challengeProgressTexts[i] = CreateText(row.transform, "Progress", string.Empty, 18, TextAnchor.MiddleLeft, FontStyle.Bold); challengeProgressTexts[i].color = new Color(0.55f, 0.8f, 0.94f);
                SetRect(challengeProgressTexts[i].rectTransform, new Vector2(0.05f, 0.16f), new Vector2(0.59f, 0.47f), Vector2.zero, Vector2.zero);
                Image track = CreateImage(row.transform, "Track", new Color(0.01f, 0.04f, 0.08f, 0.9f)); ApplyRounded(track);
                SetRect(track.rectTransform, new Vector2(0.05f, 0.08f), new Vector2(0.58f, 0.15f), Vector2.zero, Vector2.zero);
                challengeFills[i] = CreateImage(track.transform, "Fill", new Color(0.2f, 0.9f, 1f)); ApplyRounded(challengeFills[i]);
                SetRect(challengeFills[i].rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
                GameObject claim = CreateButton(row.transform, "Claim", string.Empty, new Color(0.08f, 0.38f, 0.48f), () => ClaimChallenge(slot));
                SetRect(claim.GetComponent<RectTransform>(), new Vector2(0.63f, 0.19f), new Vector2(0.95f, 0.78f), Vector2.zero, Vector2.zero);
                challengeButtons[i] = claim.GetComponent<Button>(); challengeButtonLabels[i] = claim.transform.Find("Label").GetComponent<Text>();
                challengeButtonLabels[i].fontSize = 18; challengeButtonLabels[i].resizeTextForBestFit = true; challengeButtonLabels[i].resizeTextMinSize = 11;
            }
            GameObject close = CreateButton(card.transform, "Close", "FERMER", new Color(0.12f, 0.48f, 0.58f), ToggleMissions);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.3f, 0.025f), new Vector2(0.7f, 0.095f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private void RefreshMission() => RefreshMissions();
        private void RefreshMissions()
        {
            missionCurrencyText.text = MetaProgression.Materials + "  MATÉRIAUX";
            for (int i = 0; i < 3; i++)
            {
                ChallengeDefinition c = MetaProgression.Challenge(MetaProgression.ActiveChallengeId(i)); int progress = Mathf.Min(MetaProgression.ChallengeProgress(i), c.Target);
                bool complete = progress >= c.Target; bool claimed = MetaProgression.ChallengeClaimed(i);
                challengeLabels[i].text = c.Label; challengeProgressTexts[i].text = progress + " / " + c.Target + "     +" + c.Reward + " MAT";
                challengeFills[i].rectTransform.anchorMax = new Vector2(progress / (float)c.Target, 1f); challengeFills[i].color = complete ? new Color(1f, 0.72f, 0.24f) : new Color(0.2f, 0.9f, 1f);
                challengeButtons[i].interactable = complete && !claimed; challengeButtonLabels[i].text = claimed ? "TERMINÉ" : complete ? "RÉCUPÉRER" : "EN COURS";
            }
        }
        private void ClaimChallenge(int slot)
        {
            if (!MetaProgression.Claim(slot)) return;
            hudFeedback?.ChallengeRewardClaimed();
            StartCoroutine(PulseRewardBalance());
            RefreshMissions();
        }

        private IEnumerator PulseRewardBalance()
        {
            float elapsed = 0f;
            while (elapsed < 0.42f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 0.42f);
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.24f;
                if (missionCurrencyText != null) missionCurrencyText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            if (missionCurrencyText != null) missionCurrencyText.transform.localScale = Vector3.one;
        }

        private GameObject CreateHangarPanel(Transform safe)
        {
            var panel = new GameObject("Hangar Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); panel.GetComponent<Image>().color = new Color(0.008f, 0.02f, 0.055f, 0.94f);
            Image card = CreateImage(panel.transform, "Hangar Card", new Color(0.025f, 0.075f, 0.14f, 0.99f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.045f, 0.08f), new Vector2(0.955f, 0.92f), Vector2.zero, Vector2.zero);
            Text title = CreateText(card.transform, "Title", "HANGAR", 45, TextAnchor.MiddleLeft, FontStyle.Bold); title.color = new Color(0.76f, 0.98f, 1f);
            SetRect(title.rectTransform, new Vector2(0.06f, 0.9f), new Vector2(0.5f, 0.98f), Vector2.zero, Vector2.zero);
            hangarCurrencyText = CreateText(card.transform, "Balance", string.Empty, 24, TextAnchor.MiddleRight, FontStyle.Bold); hangarCurrencyText.color = new Color(1f, 0.72f, 0.24f);
            SetRect(hangarCurrencyText.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(0.94f, 0.98f), Vector2.zero, Vector2.zero);
            string[] tabs = { "FUSÉES", "FEUX", "PLANÈTES", "FONDS", "MUSIQUE" };
            for (int i = 0; i < tabs.Length; i++) { int tab=i; GameObject b=CreateButton(card.transform,"Tab "+i,tabs[i],new Color(0.045f,0.14f,0.22f),()=>SelectHangarCategory((CosmeticKind)tab)); hangarTabImages[i]=b.GetComponent<Image>(); SetRect(b.GetComponent<RectTransform>(),new Vector2(0.025f+i*0.192f,0.82f),new Vector2(0.207f+i*0.192f,0.89f),Vector2.zero,Vector2.zero); var label=b.transform.Find("Label").GetComponent<Text>(); label.fontSize=17; label.resizeTextForBestFit=true; label.resizeTextMinSize=11; }
            Image previewPlate = CreateImage(card.transform, "Preview Plate", new Color(0.01f, 0.04f, 0.085f, 0.96f)); ApplyRounded(previewPlate);
            SetRect(previewPlate.rectTransform, new Vector2(0.07f, 0.49f), new Vector2(0.93f, 0.8f), Vector2.zero, Vector2.zero);
            previewPlate.gameObject.AddComponent<RectMask2D>();
            hangarBackdrop = CreateImage(previewPlate.transform, "Equipped Background", Color.white * 0.55f);
            hangarBackdrop.raycastTarget = false;
            SetRect(hangarBackdrop.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            hangarPreview = CreateImage(previewPlate.transform, "Preview", Color.white); hangarPreview.preserveAspect=true; hangarPreview.raycastTarget=false;
            SetRect(hangarPreview.rectTransform,new Vector2(0.31f,0.34f),new Vector2(0.69f,0.95f),Vector2.zero,Vector2.zero);
            musicPreviewButton = CreateButton(previewPlate.transform,"Listen","ÉCOUTER 8 S",new Color(0.06f,0.25f,0.32f),()=> { var items=CurrentCosmetics(); if(selectedCosmeticIndex<items.Count) hudFeedback.PreviewMusic(items[selectedCosmeticIndex].VisualIndex); });
            SetRect(musicPreviewButton.GetComponent<RectTransform>(),new Vector2(0.3f,0.37f),new Vector2(0.7f,0.62f),Vector2.zero,Vector2.zero);
            SetRect(musicPreviewButton.GetComponent<RectTransform>(),new Vector2(0.43f,0.55f),new Vector2(0.91f,0.8f),Vector2.zero,Vector2.zero);
            musicDescription = CreateText(previewPlate.transform,"Music Description",string.Empty,19,TextAnchor.MiddleCenter,FontStyle.Bold);
            SetRect(musicDescription.rectTransform,new Vector2(.06f,.32f),new Vector2(.94f,.47f),Vector2.zero,Vector2.zero);
            hangarItemName=CreateText(previewPlate.transform,"Item Name",string.Empty,27,TextAnchor.LowerLeft,FontStyle.Bold); SetRect(hangarItemName.rectTransform,new Vector2(0.04f,0.05f),new Vector2(0.47f,0.3f),Vector2.zero,Vector2.zero);
            hangarItemPrice=CreateText(previewPlate.transform,"Price",string.Empty,22,TextAnchor.LowerRight,FontStyle.Bold); hangarItemPrice.color=new Color(1f,0.72f,0.24f); SetRect(hangarItemPrice.rectTransform,new Vector2(0.53f,0.05f),new Vector2(0.96f,0.3f),Vector2.zero,Vector2.zero);
            for(int i=0;i<4;i++){int slot=i; GameObject b=CreateButton(card.transform,"Cosmetic "+i,string.Empty,new Color(0.04f,0.13f,0.21f),()=>SelectCosmeticCard(slot)); cosmeticCards[i]=b.GetComponent<Button>(); SetRect(b.GetComponent<RectTransform>(),new Vector2(0.05f+i*0.235f,0.32f),new Vector2(0.255f+i*0.235f,0.47f),Vector2.zero,Vector2.zero); cosmeticCardPreviews[i]=CreateImage(b.transform,"Preview",Color.white); cosmeticCardPreviews[i].preserveAspect=true; cosmeticCardPreviews[i].raycastTarget=false; SetRect(cosmeticCardPreviews[i].rectTransform,new Vector2(0.2f,0.37f),new Vector2(0.8f,0.92f),Vector2.zero,Vector2.zero); cosmeticCardLabels[i]=b.transform.Find("Label").GetComponent<Text>(); cosmeticCardLabels[i].fontSize=14; cosmeticCardLabels[i].alignment=TextAnchor.LowerCenter; }
            GameObject prev=CreateButton(card.transform,"Previous","‹",new Color(0.08f,0.3f,0.4f),()=>ChangeHangarPage(-1)); SetRect(prev.GetComponent<RectTransform>(),new Vector2(0.05f,0.22f),new Vector2(0.18f,0.29f),Vector2.zero,Vector2.zero);
            GameObject next=CreateButton(card.transform,"Next","›",new Color(0.08f,0.3f,0.4f),()=>ChangeHangarPage(1)); SetRect(next.GetComponent<RectTransform>(),new Vector2(0.82f,0.22f),new Vector2(0.95f,0.29f),Vector2.zero,Vector2.zero);
            hangarStatus=CreateText(card.transform,"Status",string.Empty,18,TextAnchor.MiddleCenter,FontStyle.Bold); hangarStatus.color=new Color(0.55f,0.8f,0.94f); SetRect(hangarStatus.rectTransform,new Vector2(0.18f,0.22f),new Vector2(0.82f,0.29f),Vector2.zero,Vector2.zero);
            GameObject action=CreateButton(card.transform,"Action",string.Empty,new Color(0.12f,0.48f,0.58f),BuyOrEquipSelected); hangarActionButton=action.GetComponent<Button>(); hangarActionLabel=action.transform.Find("Label").GetComponent<Text>(); SetRect(action.GetComponent<RectTransform>(),new Vector2(0.21f,0.12f),new Vector2(0.79f,0.2f),Vector2.zero,Vector2.zero);
            GameObject close=CreateButton(card.transform,"Close","FERMER",new Color(0.06f,0.2f,0.3f),ToggleHangar); SetRect(close.GetComponent<RectTransform>(),new Vector2(0.31f,0.035f),new Vector2(0.69f,0.1f),Vector2.zero,Vector2.zero);
            return panel;
        }

        private System.Collections.Generic.List<CosmeticDefinition> CurrentCosmetics(){var list=new System.Collections.Generic.List<CosmeticDefinition>();foreach(var item in MetaProgression.Catalog)if(item.Kind==hangarCategory)list.Add(item);return list;}
        private void SelectHangarCategory(CosmeticKind kind){hangarCategory=kind;hangarPage=0;selectedCosmeticIndex=0;RefreshHangar(string.Empty);}
        private void ChangeHangarPage(int delta){var list=CurrentCosmetics();int pages=Mathf.Max(1,Mathf.CeilToInt(list.Count/4f));hangarPage=(hangarPage+delta+pages)%pages;selectedCosmeticIndex=hangarPage*4;RefreshHangar(string.Empty);}
        private void SelectCosmeticCard(int slot){selectedCosmeticIndex=hangarPage*4+slot;RefreshHangar(string.Empty);}
        private void BuyOrEquipSelected(){var list=CurrentCosmetics();if(selectedCosmeticIndex<0||selectedCosmeticIndex>=list.Count)return;var item=list[selectedCosmeticIndex];if(!MetaProgression.BuyOrEquip(item)){RefreshHangar(DailyCourse.IsExclusiveRocket(item.Id)?"EXCLUSIVITÉ DU PARCOURS DU JOUR":"MATÉRIAUX INSUFFISANTS");return;}if(item.Kind==CosmeticKind.Music)hudFeedback.SelectMusic(item.VisualIndex);else CosmeticsChanged?.Invoke();RefreshHangar(item.Name+" ÉQUIPÉ");}
        private void RefreshHangar(string message)
        {
            hudFeedback.StopMusicPreview();
            foreach (var label in cosmeticCardLabels)
            {
                label.fontSize = 18; label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 12; label.resizeTextMaxSize = 18;
                label.alignment = TextAnchor.MiddleCenter;
                SetRect(label.rectTransform,new Vector2(.03f,.035f),new Vector2(.97f,.32f),Vector2.zero,Vector2.zero);
            }
            var list=CurrentCosmetics();if(list.Count==0)return;selectedCosmeticIndex=Mathf.Clamp(selectedCosmeticIndex,0,list.Count-1);var selected=list[selectedCosmeticIndex];hangarCurrencyText.text=MetaProgression.Materials+"  MATÉRIAUX";
            for(int i=0;i<4;i++){int index=hangarPage*4+i;bool visible=index<list.Count;cosmeticCards[i].gameObject.SetActive(visible);if(!visible)continue;var item=list[index];cosmeticCardLabels[i].text=item.Name;cosmeticCardPreviews[i].sprite=CosmeticPreview(item);cosmeticCardPreviews[i].color=CosmeticColor(item);cosmeticCards[i].GetComponent<Image>().color=index==selectedCosmeticIndex?new Color(0.08f,0.4f,0.5f):new Color(0.04f,0.13f,0.21f);}
            for(int i=0;i<hangarTabImages.Length;i++)hangarTabImages[i].color=i==(int)hangarCategory?new Color(0.1f,0.43f,0.54f):new Color(0.045f,0.14f,0.22f);
            musicPreviewButton.SetActive(selected.Kind==CosmeticKind.Music);
            hangarPreview.enabled=true;
            bool music = selected.Kind == CosmeticKind.Music;
            musicDescription.gameObject.SetActive(music);
            musicDescription.text = music ? MusicLibrary.Description(selected.VisualIndex) : string.Empty;
            SetRect(hangarPreview.rectTransform,music?new Vector2(.12f,.52f):new Vector2(.31f,.34f),music?new Vector2(.34f,.87f):new Vector2(.69f,.95f),Vector2.zero,Vector2.zero);
            hangarBackdrop.sprite=RuntimeAssets.GetBackgroundSprite(MetaProgression.Selected(CosmeticKind.Background));
            hangarPreview.sprite=CosmeticPreview(selected);hangarPreview.color=CosmeticColor(selected);hangarItemName.text=selected.Name;bool owned=MetaProgression.Owned(selected);bool equipped=MetaProgression.Selected(selected.Kind)==selected.VisualIndex;bool daily=DailyCourse.IsExclusiveRocket(selected.Id);int days=DailyCourse.RequiredDaysForRocket(selected.Id);hangarItemPrice.text=owned?"ACQUIS":daily?days+" PARCOURS":""+selected.Price+" MAT";hangarActionLabel.text=equipped?"ÉQUIPÉ":owned?"ÉQUIPER":daily?"RÉCOMPENSE QUOTIDIENNE":"ACHETER · "+selected.Price;hangarActionButton.interactable=!equipped&&!daily||owned&&!equipped;hangarStatus.text=string.IsNullOrEmpty(message)?"PAGE "+(hangarPage+1)+" / "+Mathf.Max(1,Mathf.CeilToInt(list.Count/4f)):message;
        }
        private static Sprite CosmeticPreview(CosmeticDefinition item){return item.Kind==CosmeticKind.Rocket?RuntimeAssets.GetRocketSprite(item.VisualIndex):item.Kind==CosmeticKind.PlanetPack?RuntimeAssets.GetPlanetPackSprite(item.VisualIndex,item.VisualIndex*7+1):item.Kind==CosmeticKind.Background?RuntimeAssets.GetBackgroundSprite(item.VisualIndex):item.Kind==CosmeticKind.Music?RuntimeAssets.MusicIcon:RuntimeAssets.GetTrailSprite(item.VisualIndex);}
        private static Color CosmeticColor(CosmeticDefinition item){return item.Kind==CosmeticKind.Trail?GameProgression.TrailColor(item.VisualIndex):Color.white;}

        private void CreateHyperspaceOverlay(Transform safe)
        {
            hyperspaceVeil = CreateImage(safe, "Hyperspace", new Color(0.015f, 0.025f, 0.12f, 0f));
            hyperspaceOverlay = hyperspaceVeil.gameObject;
            SetRect(hyperspaceVeil.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            hyperspaceVeil.raycastTarget = false;
            var tunnel = new GameObject("Quantum Energy Tunnel", typeof(RectTransform), typeof(CanvasRenderer), typeof(QuantumTunnelGraphic));
            tunnel.transform.SetParent(hyperspaceOverlay.transform, false);
            hyperspaceTunnel = tunnel.GetComponent<QuantumTunnelGraphic>();
            hyperspaceTunnel.raycastTarget = false;
            SetRect(hyperspaceTunnel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            hyperspaceOverlay.transform.SetAsLastSibling();
            hyperspaceOverlay.SetActive(false);
        }

        public void BeginHyperspace()
        {
            for (int i = 0; i < powerUpInventoryButtons.Length; i++) powerUpInventoryButtons[i].GetComponent<Button>().interactable = false;
            hyperspaceOverlay.transform.SetAsLastSibling();
            hyperspaceOverlay.SetActive(true);
            hyperspaceTunnel.ResetTunnel();
            UpdateHyperspace(0f);
        }

        public void UpdateHyperspace(float intensity)
        {
            if (hyperspaceOverlay == null) return;
            intensity = Mathf.Clamp01(intensity);
            intensity *= GamePreferences.EnhancedEffects ? 1f : 0.2f;
            hyperspaceVeil.color = new Color(0.012f, 0.025f, 0.13f, intensity * 0.12f);
            hyperspaceTunnel.SetIntensity(intensity);
        }

        public void EndHyperspace()
        {
            if (hyperspaceOverlay != null) hyperspaceOverlay.SetActive(false);
        }

        private GameObject CreatePowerUpPanel(Transform safe)
        {
            GameObject panel = new GameObject("Power Ups Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false); SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.006f, 0.018f, 0.05f, 0.94f);
            Image card = CreateImage(panel.transform, "Power Ups Card", new Color(0.025f, 0.075f, 0.14f, 0.99f)); ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.055f, 0.08f), new Vector2(0.945f, 0.92f), Vector2.zero, Vector2.zero);
            Text title = CreateText(card.transform, "Title", "MODULES BONUS", 45, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.fontSize = 36;
            title.color = new Color(0.72f, 0.98f, 1f); SetRect(title.rectTransform, new Vector2(0.06f, 0.91f), new Vector2(0.94f, 0.98f), Vector2.zero, Vector2.zero);
            powerUpCurrencyText = CreateText(card.transform, "Currency", string.Empty, 22, TextAnchor.MiddleRight, FontStyle.Bold);
            powerUpCurrencyText.color = new Color(1f, 0.72f, 0.24f); SetRect(powerUpCurrencyText.rectTransform, new Vector2(0.06f, 0.87f), new Vector2(0.94f, 0.91f), Vector2.zero, Vector2.zero);
            Text hint = CreateText(card.transform, "Hint", "5 CHARGES PAR BONUS · CONSERVÉES ENTRE LES PARTIES\nAPPUIE SUR UNE ICÔNE EN JEU POUR L'ACTIVER", 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            hint.color = new Color(0.5f, 0.76f, 0.9f); hint.resizeTextForBestFit = true; hint.resizeTextMinSize = 11;
            hint.resizeTextMaxSize = 22;
            hint.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(hint.rectTransform, new Vector2(0.05f, 0.82f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);

            for (int i = 0; i < PowerUpProgression.Catalog.Length; i++)
            {
                int index = i; PowerUpDefinition definition = PowerUpProgression.Catalog[i];
                float top = 0.805f - i * 0.135f; float bottom = top - 0.12f;
                Image row = CreateImage(card.transform, definition.Type + " Row", new Color(0.035f, 0.12f, 0.2f, 0.96f)); ApplyRounded(row);
                SetRect(row.rectTransform, new Vector2(0.045f, bottom), new Vector2(0.955f, top), Vector2.zero, Vector2.zero);
                Image icon = CreateImage(row.transform, "Icon", definition.Color); icon.sprite = RuntimeAssets.GetPowerUpIcon(definition.Type); icon.preserveAspect = true;
                SetRect(icon.rectTransform, new Vector2(0.025f, 0.25f), new Vector2(0.14f, 0.9f), Vector2.zero, Vector2.zero);
                Text name = CreateText(row.transform, "Name", definition.Name, 21, TextAnchor.UpperLeft, FontStyle.Bold);
                name.fontSize = 28;
                name.resizeTextForBestFit = true; name.resizeTextMinSize = 20; name.resizeTextMaxSize = 28;
                name.verticalOverflow = VerticalWrapMode.Truncate;
                name.color = definition.Color; SetRect(name.rectTransform, new Vector2(0.16f, 0.76f), new Vector2(0.68f, 0.96f), Vector2.zero, Vector2.zero);
                powerUpStockTexts[i] = CreateText(row.transform, "Stock", string.Empty, 23, TextAnchor.MiddleCenter, FontStyle.Bold);
                powerUpStockTexts[i].resizeTextForBestFit = false;
                powerUpStockTexts[i].horizontalOverflow = HorizontalWrapMode.Overflow;
                powerUpStockTexts[i].color = definition.Color; SetRect(powerUpStockTexts[i].rectTransform, new Vector2(0.025f, 0.025f), new Vector2(0.14f, 0.24f), Vector2.zero, Vector2.zero);
                Text description = CreateText(row.transform, "Description", definition.Description, 14, TextAnchor.LowerLeft, FontStyle.Normal);
                description.color = new Color(0.67f, 0.84f, 0.94f); description.resizeTextForBestFit = true; description.resizeTextMinSize = 10;
                description.resizeTextMaxSize = 22;
                description.verticalOverflow = VerticalWrapMode.Truncate;
                SetRect(description.rectTransform, new Vector2(0.16f, 0.53f), new Vector2(0.68f, 0.74f), Vector2.zero, Vector2.zero);
                powerUpStats[i] = CreateText(row.transform, "Stats", string.Empty, 13, TextAnchor.MiddleLeft, FontStyle.Bold);
                powerUpStats[i].color = new Color(0.92f, 0.98f, 1f); powerUpStats[i].resizeTextForBestFit = true; powerUpStats[i].resizeTextMinSize = 9;
                powerUpStats[i].fontSize = 17;
                powerUpStats[i].resizeTextMinSize = 14;
                powerUpStats[i].resizeTextMaxSize = 22;
                powerUpStats[i].verticalOverflow = VerticalWrapMode.Truncate;
                SetRect(powerUpStats[i].rectTransform, new Vector2(0.16f, 0.04f), new Vector2(0.68f, 0.5f), Vector2.zero, Vector2.zero);
                powerUpLevelTexts[i] = CreateText(row.transform, "Level", string.Empty, 18, TextAnchor.UpperRight, FontStyle.Bold);
                powerUpLevelTexts[i].color = Color.white; SetRect(powerUpLevelTexts[i].rectTransform, new Vector2(0.72f, 0.76f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);
                for (int pip = 0; pip < 5; pip++)
                {
                    Image levelPip = CreateImage(row.transform, "Level " + (pip + 1), new Color(0.08f, 0.2f, 0.28f, 1f)); ApplyRounded(levelPip);
                    float left = 0.72f + pip * 0.048f;
                    SetRect(levelPip.rectTransform, new Vector2(left, 0.59f), new Vector2(left + 0.038f, 0.69f), Vector2.zero, Vector2.zero);
                    powerUpLevelPips[i, pip] = levelPip;
                }
                GameObject upgrade = CreateButton(row.transform, "Upgrade", string.Empty, new Color(0.08f, 0.42f, 0.54f), () => UpgradePowerUp((PowerUpType)index));
                powerUpUpgradeButtons[i] = upgrade.GetComponent<Button>(); powerUpPriceTexts[i] = upgrade.transform.Find("Label").GetComponent<Text>(); powerUpPriceTexts[i].fontSize = 16;
                SetRect(upgrade.GetComponent<RectTransform>(), new Vector2(0.72f, 0.08f), new Vector2(0.96f, 0.46f), Vector2.zero, Vector2.zero);
            }
            powerUpMenuStatus = CreateText(card.transform, "Status", string.Empty, 17, TextAnchor.MiddleCenter, FontStyle.Bold);
            powerUpMenuStatus.color = new Color(1f, 0.72f, 0.24f); SetRect(powerUpMenuStatus.rectTransform, new Vector2(0.06f, 0.075f), new Vector2(0.94f, 0.13f), Vector2.zero, Vector2.zero);
            GameObject close = CreateButton(card.transform, "Close", "FERMER", new Color(0.06f, 0.22f, 0.32f), TogglePowerUps);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.31f, 0.015f), new Vector2(0.69f, 0.07f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private void UpgradePowerUp(PowerUpType type)
        {
            string message = PowerUpProgression.Upgrade(type) ? PowerUpProgression.Definition(type).Name + " AMÉLIORÉ" :
                PowerUpProgression.Level(type) >= 5 ? "NIVEAU MAXIMUM" : "MATÉRIAUX INSUFFISANTS";
            if (message.Contains("AMÉLIORÉ")) hudFeedback.ChallengeRewardClaimed();
            RefreshPowerUps(message);
        }

        private void RefreshPowerUps(string message)
        {
            powerUpCurrencyText.text = MetaProgression.Materials + " MAT";
            for (int i = 0; i < PowerUpProgression.Catalog.Length; i++)
            {
                PowerUpDefinition definition = PowerUpProgression.Catalog[i]; int level = PowerUpProgression.Level(definition.Type);
                powerUpLevelTexts[i].text = "NIV. " + level + "/5";
                powerUpStockTexts[i].text = PowerUpProgression.StoredCount(definition.Type) + "/" + PowerUpProgression.MaxInventory;
                powerUpStats[i].text = PowerUpProgression.Stats(definition.Type, level) + (level < 5 ? "\nSuivant : " + PowerUpProgression.Stats(definition.Type, level + 1) : "\nNiveau maximum");
                for (int pip = 0; pip < 5; pip++) powerUpLevelPips[i, pip].color = pip < level ? definition.Color : new Color(0.08f, 0.2f, 0.28f, 1f);
                powerUpPriceTexts[i].text = level >= 5 ? "MAX" : "AMÉLIORER\n" + definition.UpgradePrice(level) + " MAT";
                powerUpUpgradeButtons[i].interactable = level < 5 && MetaProgression.Materials >= definition.UpgradePrice(level);
            }
            powerUpMenuStatus.text = message;
        }

        private const string DiscordInviteUrl = "https://discord.gg/jGXyYbYQRX";
        private void OpenDiscordInvite() => Application.OpenURL(DiscordInviteUrl);

        private GameObject CreateCreditsPanel(Transform safe)
        {
            var panel = new GameObject("Credits Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image veil = panel.GetComponent<Image>();
            veil.color = new Color(0.008f, 0.02f, 0.055f, 0.88f);

            Image card = CreateImage(panel.transform, "Credits Card", new Color(0.025f, 0.075f, 0.14f, 0.98f));
            ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.11f, 0.27f), new Vector2(0.89f, 0.72f), Vector2.zero, Vector2.zero);

            Text title = CreateText(card.transform, "Credits Title", "ORBIT BREAKER", 48, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.color = new Color(0.72f, 0.97f, 1f, 1f);
            SetRect(title.rectTransform, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.94f), Vector2.zero, Vector2.zero);

            Text author = CreateText(card.transform, "Author", "UN JEU DE JEANEDOUART © 2026", 28, TextAnchor.MiddleCenter, FontStyle.Bold);
            author.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(author.rectTransform, new Vector2(0.08f, 0.53f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero);

            Text details = CreateText(card.transform, "Credits Details", "CONCEPTION & DÉVELOPPEMENT\nJEANEDOUART\n\nL'INTELLIGENCE ARTIFICIELLE A ÉTÉ UTILISÉE\nCOMME OUTIL D'ASSISTANCE AU DÉVELOPPEMENT.\n\nVERSION " + Application.version, 22, TextAnchor.MiddleCenter, FontStyle.Normal);
            details.color = new Color(0.67f, 0.84f, 0.95f, 0.95f);
            details.lineSpacing = 1.15f;
            details.resizeTextForBestFit = true;
            details.resizeTextMinSize = 16;
            details.resizeTextMaxSize = 22;
            SetRect(details.rectTransform, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.56f), Vector2.zero, Vector2.zero);

            GameObject close = CreateButton(card.transform, "Close Credits", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleCredits);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.25f, 0.055f), new Vector2(0.75f, 0.18f), Vector2.zero, Vector2.zero);

            GameObject discord = CreateIconButton(card.transform, "Discord Button", RuntimeAssets.DiscordIcon, OpenDiscordInvite);
            SetRect(discord.GetComponent<RectTransform>(), new Vector2(0.78f, 0.055f), new Vector2(0.91f, 0.18f), Vector2.zero, Vector2.zero);
            discord.GetComponent<Image>().color = new Color(0.345f, 0.396f, 0.949f, 0.95f);
            return panel;
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

        private static InputField CreateInputField(Transform parent, string name, string placeholder, int characterLimit)
        {
            GameObject instance = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            instance.transform.SetParent(parent, false);
            Image background = instance.GetComponent<Image>();
            background.color = new Color(0.015f, 0.045f, 0.09f, 1f); ApplyRounded(background);
            Text value = CreateText(instance.transform, "Text", string.Empty, 27, TextAnchor.MiddleLeft, FontStyle.Bold);
            value.color = new Color(0.82f, 0.97f, 1f); value.supportRichText = false;
            SetRect(value.rectTransform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), Vector2.zero, Vector2.zero);
            Text hint = CreateText(instance.transform, "Placeholder", placeholder, 22, TextAnchor.MiddleLeft, FontStyle.Italic);
            hint.color = new Color(0.38f, 0.59f, 0.72f, 0.8f);
            SetRect(hint.rectTransform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), Vector2.zero, Vector2.zero);
            InputField input = instance.GetComponent<InputField>();
            input.targetGraphic = background; input.textComponent = value; input.placeholder = hint;
            input.characterLimit = characterLimit; input.lineType = InputField.LineType.SingleLine;
            input.contentType = InputField.ContentType.Standard;
            return input;
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

        private static GameObject CreateRoundTextButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction callback)
        {
            var instance = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            instance.transform.SetParent(parent, false);
            Image background = instance.GetComponent<Image>();
            background.sprite = RuntimeAssets.CircleSprite;
            background.color = new Color(0.025f, 0.18f, 0.26f, 0.82f);
            Outline outline = instance.GetComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.85f, 1f, 0.7f);
            outline.effectDistance = new Vector2(2f, -2f);
            Button button = instance.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(callback);
            Text text = CreateText(instance.transform, "Label", label, 34, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);
            text.color = new Color(0.78f, 0.98f, 1f, 1f);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, 2f));
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

        private static void SetSquareRect(RectTransform rect, Vector2 normalizedPosition, float size)
        {
            rect.anchorMin = normalizedPosition;
            rect.anchorMax = normalizedPosition;
            rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.one * size;
        }
    }
}
