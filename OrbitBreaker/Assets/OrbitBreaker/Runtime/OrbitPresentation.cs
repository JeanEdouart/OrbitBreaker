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

        public void Initialize(Camera camera)
        {
            targetCamera = camera;
            for (int i = 0; i < nebulaTiles.Length; i++)
            {
                var tile = new GameObject("Nebula Tile " + (i + 1));
                tile.transform.SetParent(transform, false);
                nebulaTiles[i] = tile.AddComponent<SpriteRenderer>();
                nebulaTiles[i].sprite = RuntimeAssets.GetBackgroundSprite(MetaProgression.Selected(CosmeticKind.Background));
                nebulaTiles[i].color = new Color(0.72f, 0.78f, 0.92f, 0.72f);
                nebulaTiles[i].sortingOrder = -100;
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
            RefreshPositions();
        }

        private void RefreshPositions()
        {
            if (targetCamera == null) return;
            float cameraY = targetCamera.transform.position.y;
            float cameraX = targetCamera.transform.position.x;
            float biomeProgress = Mathf.Repeat(Mathf.Max(0f, cameraY) / 34f, 1f);
            int biome = Mathf.FloorToInt(Mathf.Max(0f, cameraY) / 34f) % 3;
            Color biomeA = biome == 0 ? new Color(0.72f, 0.78f, 0.92f, 0.72f)
                : biome == 1 ? new Color(0.72f, 0.52f, 1f, 0.72f) : new Color(0.42f, 0.9f, 0.88f, 0.72f);
            Color biomeB = biome == 0 ? new Color(0.72f, 0.52f, 1f, 0.72f)
                : biome == 1 ? new Color(0.42f, 0.9f, 0.88f, 0.72f) : new Color(0.72f, 0.78f, 0.92f, 0.72f);
            Color biomeTint = Color.Lerp(biomeA, biomeB, biomeProgress);
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
                float relativeY = Mathf.Repeat(starSeedY[i] - drift + StarSpan * 0.5f, StarSpan) - StarSpan * 0.5f;
                stars[i].position = new Vector3(cameraX + starSeedX[i], cameraY + relativeY, 1f);
            }
        }
    }

    public sealed class OrbitCameraRig : MonoBehaviour
    {
        private Camera targetCamera;
        private Vector3 velocity;
        private Vector3 basePosition;
        private float targetY;
        private float targetX;
        private float impactShakeRemaining;
        private float impactShakeDuration;
        private float impactShakeStrength;
        private float flightShakeStrength;

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
            basePosition = new Vector3(targetX, targetY, -10f);
            targetCamera.transform.position = basePosition;
            velocity = Vector3.zero;
            impactShakeRemaining = 0f;
            flightShakeStrength = 0f;
        }

        public void SetTarget(Vector2 playerPosition, Vector2 anchorPosition)
        {
            if (GamePreferences.FixedCamera)
            {
                // Stable mode keeps the playfield readable: vertical tracking remains
                // essential in an endless game, while lateral movement and shake are removed.
                float stableY = Mathf.Max(0f, playerPosition.y + 2.15f);
                targetY = Mathf.MoveTowards(targetY, stableY, 4.25f * Time.deltaTime);
                targetX = Mathf.MoveTowards(targetX, 0f, 2.5f * Time.deltaTime);
                return;
            }
            float desiredY = Mathf.Max(0f, Mathf.Max(playerPosition.y, anchorPosition.y) + 2.15f);
            targetY = desiredY >= targetY ? desiredY : Mathf.MoveTowards(targetY, desiredY, 5.5f * Time.deltaTime);
            targetX = Mathf.Lerp(playerPosition.x, anchorPosition.x, 0.65f) * 0.12f;
        }

        public void ShakeCapture()
        {
            if (!GamePreferences.FixedCamera && GamePreferences.CaptureShake) TriggerImpactShake(0.13f, 0.065f);
        }

        public void ShakeExplosion()
        {
            if (!GamePreferences.FixedCamera && GamePreferences.ExplosionShake) TriggerImpactShake(0.38f, 0.19f);
        }

        public void SetFlightShake(float danger01, bool flying)
        {
            float desired = !GamePreferences.FixedCamera && GamePreferences.FlightShake && flying
                ? Mathf.InverseLerp(0.42f, 1f, danger01) * 0.045f
                : 0f;
            flightShakeStrength = Mathf.MoveTowards(flightShakeStrength, desired, Time.unscaledDeltaTime * 0.12f);
        }

        private void TriggerImpactShake(float duration, float strength)
        {
            impactShakeDuration = duration;
            impactShakeRemaining = duration;
            impactShakeStrength = strength;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            Vector3 destination = new Vector3(targetX, targetY, -10f);
            float smoothTime = GamePreferences.FixedCamera ? 0.42f : 0.28f;
            basePosition = Vector3.SmoothDamp(basePosition, destination, ref velocity, smoothTime, 18f, Time.unscaledDeltaTime);
            float impact = 0f;
            if (impactShakeRemaining > 0f)
            {
                impactShakeRemaining = Mathf.Max(0f, impactShakeRemaining - Time.unscaledDeltaTime);
                impact = impactShakeStrength * (impactShakeRemaining / Mathf.Max(0.01f, impactShakeDuration));
            }

            float strength = GamePreferences.FixedCamera ? 0f : impact + flightShakeStrength;
            float time = Time.unscaledTime;
            Vector3 shakeOffset = new Vector3(
                (Mathf.PerlinNoise(time * 31f, 2.7f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(7.1f, time * 37f) - 0.5f) * 2f,
                0f) * strength;
            targetCamera.transform.position = basePosition + shakeOffset;
        }
    }

    public sealed class ToggleSwitchVisual : MonoBehaviour
    {
        private static readonly Color OffColor = new Color(0.06f, 0.16f, 0.24f, 1f);
        private static readonly Color OnColor = new Color(0.08f, 0.62f, 0.72f, 1f);
        private Image track;
        private RectTransform knob;
        private float position;
        private float target;

        public void Initialize(Image trackImage, RectTransform knobTransform, bool isOn)
        {
            track = trackImage;
            knob = knobTransform;
            position = target = isOn ? 1f : 0f;
            Apply();
        }

        public void SetValue(bool isOn)
        {
            target = isOn ? 1f : 0f;
        }

        private void Update()
        {
            if (Mathf.Approximately(position, target)) return;
            position = Mathf.MoveTowards(position, target, Time.unscaledDeltaTime * 7.5f);
            Apply();
        }

        private void Apply()
        {
            if (track == null || knob == null) return;
            float center = Mathf.Lerp(0.27f, 0.73f, position);
            knob.anchorMin = new Vector2(center - 0.19f, 0.12f);
            knob.anchorMax = new Vector2(center + 0.19f, 0.88f);
            knob.offsetMin = Vector2.zero;
            knob.offsetMax = Vector2.zero;
            track.color = Color.Lerp(OffColor, OnColor, position);
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
        private GameObject settingsPanel;
        private GameObject settingsButton;
        private GameObject infoButton;
        private GameObject styleButton;
        private GameObject missionsButton;
        private GameObject leaderboardButton;
        private GameObject leaderboardPanel;
        private GameObject playerNamePanel;
        private InputField playerNameInput;
        private Text playerNameStatus;
        private InputField leaderboardSearchInput;
        private Text leaderboardStatus;
        private readonly Text[] leaderboardRows = new Text[10];
        private OnlineLeaderboard onlineLeaderboard;
        private Action identityAccepted;
        private GameObject missionsPanel;
        private GameObject hangarPanel;
        private readonly Text[] challengeLabels = new Text[3];
        private readonly Text[] challengeProgressTexts = new Text[3];
        private readonly Image[] challengeFills = new Image[3];
        private readonly Button[] challengeButtons = new Button[3];
        private readonly Text[] challengeButtonLabels = new Text[3];
        private Text missionCurrencyText;
        private Text missionProgressText;
        private Image missionProgressFill;
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
        private readonly Image[] hangarTabImages = new Image[4];
        private CosmeticKind hangarCategory;
        private int hangarPage;
        private int selectedCosmeticIndex;
        private Text[] styleRowLabels;
        private Image[] styleRowImages;
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
            || (leaderboardPanel != null && leaderboardPanel.activeSelf)
            || (playerNamePanel != null && playerNamePanel.activeSelf);
        public bool IsPaused => pausePanel != null && pausePanel.activeSelf;
        public event Action CosmeticsChanged;
        public event Action<int> StyleSelected;

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
            ApplyRounded(materialToastImage);
            materialToastImage.raycastTarget = false;
            SetRect(materialToastImage.rectTransform, new Vector2(0.32f, 0.805f), new Vector2(0.68f, 0.855f), Vector2.zero, Vector2.zero);
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
            Text tipsText = CreateText(tipsPanel.transform, "Tips", "VISE LA PORTE COLORÉE DANS SON SENS POUR UN BONUS SYNCHRO\nELLE EST OPTIONNELLE : TOUTE L'ORBITE PEUT TE CAPTURER\nSKIP PLUSIEURS ORBITES POUR MULTIPLIER TA DISTANCE", 17, TextAnchor.MiddleLeft, FontStyle.Normal);
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

            settingsButton = CreateIconButton(safe, "Settings Button", RuntimeAssets.SettingsIcon, ToggleSettings);
            SetSquareRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.5f), 112f);

            infoButton = CreateRoundTextButton(safe, "Credits Button", "i", ToggleCredits);
            SetSquareRect(infoButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.5f), 76f);

            styleButton = CreateIconButton(safe, "Hangar Button", RuntimeAssets.RocketSprite, ToggleHangar);
            SetSquareRect(styleButton.GetComponent<RectTransform>(), new Vector2(0.91f, 0.405f), 92f);

            missionsButton = CreateIconButton(safe, "Missions Button", RuntimeAssets.TrophyIcon, ToggleMissions);
            SetSquareRect(missionsButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.405f), 92f);

            leaderboardButton = CreateIconButton(safe, "Leaderboard Button", RuntimeAssets.LeaderboardIcon, ToggleLeaderboard);
            SetSquareRect(leaderboardButton.GetComponent<RectTransform>(), new Vector2(0.07f, 0.31f), 92f);

            pauseButton = CreateIconButton(safe, "Pause Button", RuntimeAssets.PauseIcon, PauseGame);
            SetSquareRect(pauseButton.GetComponent<RectTransform>(), new Vector2(0.095f, 0.92f), 104f);
            pauseButton.SetActive(false);

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
            leaderboardPanel = CreateLeaderboardPanel(safe);
            leaderboardPanel.SetActive(false);
            playerNamePanel = CreatePlayerNamePanel(safe);
            playerNamePanel.SetActive(false);
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
            UpdateProgress(distance, best);
            stuntText.text = string.Empty;
            nearMissText.text = string.Empty;
            titleText.gameObject.SetActive(tutorial);
            hintGroup.gameObject.SetActive(tutorial);
            tutorialTips.SetActive(tutorial);
            settingsButton.SetActive(tutorial);
            infoButton.SetActive(tutorial);
            styleButton.SetActive(tutorial);
            missionsButton.SetActive(tutorial);
            leaderboardButton.SetActive(tutorial);
            pauseButton.SetActive(!tutorial);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            gameOverVisible = false;
            if (materialToast != null) materialToast.SetActive(false);
            if (challengeToast != null) challengeToast.SetActive(false);
            if (nearMissText != null) nearMissText.text = string.Empty;
        }

        public void UpdateProgress(int distance, int best)
        {
            scoreText.text = distance + " m";
            bestText.text = "RECORD " + best + " m";
            comboText.text = string.Empty;
        }

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
                    local.y = Mathf.Clamp(local.y + 105f, bounds.yMin + 100f, bounds.yMax - 235f);
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

        public void ShowLanding(int distance, int best, int gainedDistance, float multiplier, int skippedAnchors, bool backtrack, bool revisited, SynchronizationResult synchronization)
        {
            UpdateProgress(distance, best);
            multiplierBadge.SetActive(false);
            settingsButton.SetActive(false);
            pauseButton.SetActive(true);
            string label = backtrack ? "RETOUR ORBITAL" : revisited ? "CHECKPOINT" : synchronization == SynchronizationResult.Success
                ? "SYNCHRO +0.4x" + (skippedAnchors > 0 ? "  SKIP x" + (skippedAnchors + 1) : string.Empty)
                : synchronization == SynchronizationResult.WrongDirection ? "ZONE ATTEINTE  MAUVAIS SENS"
                : skippedAnchors > 0 ? "SKIP x" + (skippedAnchors + 1) : multiplier >= 2f ? "LONG VOL" : string.Empty;
            string delta = gainedDistance > 0 ? "+" + gainedDistance : gainedDistance < 0 ? gainedDistance.ToString() : string.Empty;
            stuntText.text = string.IsNullOrEmpty(label) ? string.Empty : label + (string.IsNullOrEmpty(delta) ? string.Empty : "  " + delta + " m");
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
            titleText.gameObject.SetActive(false);
            hintGroup.gameObject.SetActive(false);
            tutorialTips.SetActive(false);
            settingsButton.SetActive(false);
            infoButton.SetActive(false);
            styleButton.SetActive(false);
            missionsButton.SetActive(false);
            leaderboardButton.SetActive(false);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            pauseButton.SetActive(true);
        }

        public void ShowGameOver(int distance, int best, int anchors, DeathReason reason, int synchronizations, int nearMisses, int bestSkip, float bestMultiplier)
        {
            if (materialToast != null) materialToast.SetActive(false);
            if (challengeToast != null) challengeToast.SetActive(false);
            if (nearMissText != null) nearMissText.text = string.Empty;
            scoreText.text = distance + " m";
            multiplierBadge.SetActive(false);
            bestText.text = "RECORD " + best + " m";
            gameOverDistance.text = "DISTANCE     " + distance + " m";
            gameOverOrbits.text = "ORBITES      " + anchors;
            gameOverRecord.text = "RECORD       " + best + " m";
            gameOverSummary.text = "SYNCHRO " + synchronizations + "   FRÔLEMENTS " + nearMisses + "   SKIP " + bestSkip + "   MAX x" + bestMultiplier.ToString("0.0") + "   STYLES " + GameProgression.UnlockedStyleCount + "/4";
            gameOverTitle.text = reason == DeathReason.Breaker ? "VOTRE VAISSEAU\nA EXPLOSÉ" : "VOUS VOUS ÊTES PERDU\nDANS L'ESPACE";
            gameOverTitle.color = reason == DeathReason.Breaker ? new Color(1f, 0.28f, 0.38f) : new Color(0.45f, 0.82f, 1f);
            gameOverPanel.SetActive(true);
            settingsButton.SetActive(true);
            infoButton.SetActive(true);
            styleButton.SetActive(true);
            missionsButton.SetActive(true);
            leaderboardButton.SetActive(true);
            pauseButton.SetActive(false);
            gameOverVisible = true;
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

        private void ToggleSettings()
        {
            bool opening = !settingsPanel.activeSelf;
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
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
            missionsPanel.SetActive(opening);
            if (opening) RefreshMission();
            gameOverPanel.SetActive(!opening && gameOverVisible);
        }

        private void ToggleHangar()
        {
            bool opening = !hangarPanel.activeSelf;
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            hangarPanel.SetActive(opening);
            if (opening) RefreshHangar(string.Empty);
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

        private async void ToggleLeaderboard()
        {
            bool opening = !leaderboardPanel.activeSelf;
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            missionsPanel.SetActive(false);
            hangarPanel.SetActive(false);
            leaderboardPanel.SetActive(opening);
            gameOverPanel.SetActive(!opening && gameOverVisible);
            if (!opening) return;
            leaderboardSearchInput.SetTextWithoutNotify(string.Empty);
            leaderboardStatus.text = "CHARGEMENT DU CLASSEMENT...";
            RenderLeaderboardRows(Array.Empty<OrbitLeaderboardEntry>());
            IReadOnlyList<OrbitLeaderboardEntry> entries = await onlineLeaderboard.RefreshAsync();
            leaderboardStatus.text = entries.Count > 0
                ? "TOP 100 · " + onlineLeaderboard.PlayerName.ToUpperInvariant()
                : onlineLeaderboard.LastError;
            RenderLeaderboardRows(entries);
        }

        private void SearchLeaderboard(string query)
        {
            IReadOnlyList<OrbitLeaderboardEntry> entries = onlineLeaderboard.Filter(query);
            leaderboardStatus.text = entries.Count > 0 ? entries.Count + " PILOTE" + (entries.Count > 1 ? "S" : string.Empty) : "AUCUN PILOTE TROUVÉ DANS LE TOP 100";
            RenderLeaderboardRows(entries);
        }

        private void RenderLeaderboardRows(IReadOnlyList<OrbitLeaderboardEntry> entries)
        {
            for (int i = 0; i < leaderboardRows.Length; i++)
            {
                Text row = leaderboardRows[i];
                bool visible = i < entries.Count;
                row.gameObject.SetActive(visible);
                if (!visible) continue;
                OrbitLeaderboardEntry entry = entries[i];
                row.text = entry.Rank.ToString().PadLeft(3) + "    " + entry.PlayerName.ToUpperInvariant() + "    " + entry.Score + " m";
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
            title.color = new Color(0.76f, 0.98f, 1f); SetRect(title.rectTransform, new Vector2(0.05f, 0.875f), new Vector2(0.95f, 0.97f), Vector2.zero, Vector2.zero);
            leaderboardSearchInput = CreateInputField(card.transform, "Search", "RECHERCHER DANS LE TOP 100", 24);
            SetRect(leaderboardSearchInput.GetComponent<RectTransform>(), new Vector2(0.08f, 0.77f), new Vector2(0.92f, 0.855f), Vector2.zero, Vector2.zero);
            leaderboardSearchInput.onValueChanged.AddListener(SearchLeaderboard);
            leaderboardStatus = CreateText(card.transform, "Status", string.Empty, 17, TextAnchor.MiddleCenter, FontStyle.Bold);
            leaderboardStatus.color = new Color(1f, 0.72f, 0.24f); SetRect(leaderboardStatus.rectTransform, new Vector2(0.06f, 0.705f), new Vector2(0.94f, 0.765f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < leaderboardRows.Length; i++)
            {
                float top = 0.69f - i * 0.057f;
                Text row = CreateText(card.transform, "Rank " + (i + 1), string.Empty, 23, TextAnchor.MiddleLeft, FontStyle.Bold);
                row.resizeTextForBestFit = true; row.resizeTextMinSize = 14; row.horizontalOverflow = HorizontalWrapMode.Wrap;
                SetRect(row.rectTransform, new Vector2(0.09f, top - 0.05f), new Vector2(0.91f, top), Vector2.zero, Vector2.zero);
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
            leaderboardStatus.text = "ACTUALISATION...";
            IReadOnlyList<OrbitLeaderboardEntry> entries = await onlineLeaderboard.RefreshAsync(leaderboardSearchInput.text);
            leaderboardStatus.text = entries.Count > 0 ? "CLASSEMENT À JOUR" : onlineLeaderboard.LastError;
            RenderLeaderboardRows(entries);
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

        private GameObject CreateMissionsPanelLegacy(Transform safe)
        {
            var panel = new GameObject("Missions Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.008f, 0.02f, 0.055f, 0.9f);

            Image card = CreateImage(panel.transform, "Mission Card", new Color(0.025f, 0.075f, 0.14f, 0.99f));
            ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.09f, 0.27f), new Vector2(0.91f, 0.73f), Vector2.zero, Vector2.zero);

            Text eyebrow = CreateText(card.transform, "Mission Eyebrow", "MISSION DU JOUR", 22, TextAnchor.MiddleCenter, FontStyle.Bold);
            eyebrow.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(eyebrow.rectTransform, new Vector2(0.08f, 0.8f), new Vector2(0.92f, 0.94f), Vector2.zero, Vector2.zero);

            missionProgressText = CreateText(card.transform, "Mission Progress", string.Empty, 36, TextAnchor.MiddleCenter, FontStyle.Bold);
            missionProgressText.color = new Color(0.76f, 0.98f, 1f, 1f);
            missionProgressText.resizeTextForBestFit = true;
            missionProgressText.resizeTextMinSize = 22;
            missionProgressText.resizeTextMaxSize = 36;
            SetRect(missionProgressText.rectTransform, new Vector2(0.07f, 0.47f), new Vector2(0.93f, 0.8f), Vector2.zero, Vector2.zero);

            Image track = CreateImage(card.transform, "Mission Progress Track", new Color(0.04f, 0.14f, 0.23f, 1f));
            ApplyRounded(track);
            SetRect(track.rectTransform, new Vector2(0.12f, 0.37f), new Vector2(0.88f, 0.43f), Vector2.zero, Vector2.zero);
            missionProgressFill = CreateImage(track.transform, "Mission Progress Fill", new Color(0.24f, 0.9f, 1f, 1f));
            ApplyRounded(missionProgressFill);
            SetRect(missionProgressFill.rectTransform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero);

            Text note = CreateText(card.transform, "Mission Note", "LA PROGRESSION EST CONSERVÉE ENTRE TES PARTIES", 18, TextAnchor.MiddleCenter, FontStyle.Bold);
            note.color = new Color(0.55f, 0.75f, 0.88f, 0.9f);
            SetRect(note.rectTransform, new Vector2(0.07f, 0.22f), new Vector2(0.93f, 0.34f), Vector2.zero, Vector2.zero);

            GameObject close = CreateButton(card.transform, "Close Missions", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleMissions);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.25f, 0.055f), new Vector2(0.75f, 0.18f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private void RefreshMissionLegacy()
        {
            int progress = Mathf.Min(GameProgression.MissionProgress, GameProgression.MissionTarget);
            string objective = GameProgression.MissionType == DailyMissionType.Distance
                ? "PARCOURIR " + GameProgression.MissionTarget + " m"
                : GameProgression.MissionType == DailyMissionType.Synchronizations
                    ? "RÉUSSIR " + GameProgression.MissionTarget + " SYNCHRONISATIONS"
                    : "RÉUSSIR " + GameProgression.MissionTarget + " FRÔLEMENTS";
            missionProgressText.text = objective + "\n" + progress + " / " + GameProgression.MissionTarget;
            missionProgressFill.rectTransform.anchorMax = new Vector2(progress / (float)GameProgression.MissionTarget, 1f);
            missionProgressFill.color = progress >= GameProgression.MissionTarget
                ? new Color(1f, 0.72f, 0.24f, 1f)
                : new Color(0.24f, 0.9f, 1f, 1f);
        }

        private GameObject CreateHangarPanelLegacy(Transform safe)
        {
            var panel = new GameObject("Hangar Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(safe, false);
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            panel.GetComponent<Image>().color = new Color(0.008f, 0.02f, 0.055f, 0.9f);

            Image card = CreateImage(panel.transform, "Hangar Card", new Color(0.025f, 0.075f, 0.14f, 0.99f));
            ApplyRounded(card);
            SetRect(card.rectTransform, new Vector2(0.075f, 0.14f), new Vector2(0.925f, 0.86f), Vector2.zero, Vector2.zero);

            Text title = CreateText(card.transform, "Hangar Title", "HANGAR", 48, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.color = new Color(0.76f, 0.98f, 1f, 1f);
            SetRect(title.rectTransform, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero);

            hangarStatus = CreateText(card.transform, "Hangar Status", string.Empty, 19, TextAnchor.MiddleCenter, FontStyle.Bold);
            hangarStatus.color = new Color(1f, 0.72f, 0.24f, 1f);
            hangarStatus.resizeTextForBestFit = true;
            hangarStatus.resizeTextMinSize = 14;
            hangarStatus.resizeTextMaxSize = 19;
            SetRect(hangarStatus.rectTransform, new Vector2(0.06f, 0.73f), new Vector2(0.94f, 0.84f), Vector2.zero, Vector2.zero);

            styleRowLabels = new Text[4];
            styleRowImages = new Image[4];
            for (int i = 0; i < 4; i++)
            {
                int style = i;
                float top = 0.7f - i * 0.14f;
                GameObject row = CreateButton(card.transform, "Style " + GameProgression.StyleName(i), string.Empty, new Color(0.045f, 0.14f, 0.22f, 1f), () => SelectStyleFromHangarLegacy(style));
                SetRect(row.GetComponent<RectTransform>(), new Vector2(0.09f, top - 0.11f), new Vector2(0.91f, top), Vector2.zero, Vector2.zero);
                styleRowImages[i] = row.GetComponent<Image>();

                Image swatch = CreateImage(row.transform, "Color", GameProgression.TrailColor(i));
                swatch.sprite = RuntimeAssets.CircleSprite;
                swatch.preserveAspect = true;
                swatch.raycastTarget = false;
                SetRect(swatch.rectTransform, new Vector2(0.04f, 0.22f), new Vector2(0.14f, 0.78f), Vector2.zero, Vector2.zero);
                styleRowLabels[i] = row.transform.Find("Label").GetComponent<Text>();
                styleRowLabels[i].alignment = TextAnchor.MiddleLeft;
                SetRect(styleRowLabels[i].rectTransform, new Vector2(0.18f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
            }

            GameObject close = CreateButton(card.transform, "Close Hangar", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleHangar);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.25f, 0.045f), new Vector2(0.75f, 0.12f), Vector2.zero, Vector2.zero);
            return panel;
        }

        private void SelectStyleFromHangarLegacy(int style)
        {
            if (!GameProgression.SelectStyle(style))
            {
                int remaining = Mathf.Max(0, GameProgression.UnlockDistanceForStyle(style) - GameProgression.LifetimeDistance);
                RefreshHangar("ENCORE " + remaining + " m POUR DÉBLOQUER " + GameProgression.StyleName(style));
                return;
            }
            StyleSelected?.Invoke(style);
            RefreshHangar(GameProgression.StyleName(style) + " ÉQUIPÉ");
        }

        private void RefreshHangarLegacy(string message)
        {
            hangarStatus.text = string.IsNullOrEmpty(message)
                ? "DISTANCE CUMULÉE  " + GameProgression.LifetimeDistance + " m"
                : message;
            for (int i = 0; i < styleRowLabels.Length; i++)
            {
                bool unlocked = i < GameProgression.UnlockedStyleCount;
                bool selected = i == GameProgression.SelectedStyle;
                string state = selected ? "ÉQUIPÉ" : unlocked ? "CHOISIR" : "À " + GameProgression.UnlockDistanceForStyle(i) + " m";
                styleRowLabels[i].text = GameProgression.StyleName(i) + "                         " + state;
                styleRowLabels[i].color = unlocked ? new Color(0.78f, 0.96f, 1f, 1f) : new Color(0.42f, 0.56f, 0.68f, 1f);
                styleRowImages[i].color = selected
                    ? new Color(0.08f, 0.39f, 0.5f, 1f)
                    : new Color(0.045f, 0.14f, 0.22f, unlocked ? 1f : 0.72f);
            }
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
            string[] tabs = { "FUSÉES", "FEUX", "PLANÈTES", "FONDS" };
            for (int i = 0; i < 4; i++) { int tab=i; GameObject b=CreateButton(card.transform,"Tab "+i,tabs[i],new Color(0.045f,0.14f,0.22f),()=>SelectHangarCategory((CosmeticKind)tab)); hangarTabImages[i]=b.GetComponent<Image>(); SetRect(b.GetComponent<RectTransform>(),new Vector2(0.04f+i*0.235f,0.82f),new Vector2(0.255f+i*0.235f,0.89f),Vector2.zero,Vector2.zero); b.transform.Find("Label").GetComponent<Text>().fontSize=17; }
            Image previewPlate = CreateImage(card.transform, "Preview Plate", new Color(0.01f, 0.04f, 0.085f, 0.96f)); ApplyRounded(previewPlate);
            SetRect(previewPlate.rectTransform, new Vector2(0.07f, 0.49f), new Vector2(0.93f, 0.8f), Vector2.zero, Vector2.zero);
            hangarPreview = CreateImage(previewPlate.transform, "Preview", Color.white); hangarPreview.preserveAspect=true; hangarPreview.raycastTarget=false;
            SetRect(hangarPreview.rectTransform,new Vector2(0.31f,0.12f),new Vector2(0.69f,0.88f),Vector2.zero,Vector2.zero);
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
        private void BuyOrEquipSelected(){var list=CurrentCosmetics();if(selectedCosmeticIndex<0||selectedCosmeticIndex>=list.Count)return;var item=list[selectedCosmeticIndex];if(!MetaProgression.BuyOrEquip(item)){RefreshHangar("MATÉRIAUX INSUFFISANTS");return;}CosmeticsChanged?.Invoke();RefreshHangar(item.Name+" ÉQUIPÉ");}
        private void RefreshHangar(string message)
        {
            var list=CurrentCosmetics();if(list.Count==0)return;selectedCosmeticIndex=Mathf.Clamp(selectedCosmeticIndex,0,list.Count-1);var selected=list[selectedCosmeticIndex];hangarCurrencyText.text=MetaProgression.Materials+"  MATÉRIAUX";
            for(int i=0;i<4;i++){int index=hangarPage*4+i;bool visible=index<list.Count;cosmeticCards[i].gameObject.SetActive(visible);if(!visible)continue;var item=list[index];cosmeticCardLabels[i].text=item.Name;cosmeticCardPreviews[i].sprite=CosmeticPreview(item);cosmeticCardPreviews[i].color=CosmeticColor(item);cosmeticCards[i].GetComponent<Image>().color=index==selectedCosmeticIndex?new Color(0.08f,0.4f,0.5f):new Color(0.04f,0.13f,0.21f);}
            for(int i=0;i<4;i++)hangarTabImages[i].color=i==(int)hangarCategory?new Color(0.1f,0.43f,0.54f):new Color(0.045f,0.14f,0.22f);
            hangarPreview.sprite=CosmeticPreview(selected);hangarPreview.color=CosmeticColor(selected);hangarItemName.text=selected.Name;bool owned=MetaProgression.Owned(selected);bool equipped=MetaProgression.Selected(selected.Kind)==selected.VisualIndex;hangarItemPrice.text=owned?"ACQUIS":selected.Price+" MAT";hangarActionLabel.text=equipped?"ÉQUIPÉ":owned?"ÉQUIPER":"ACHETER · "+selected.Price;hangarActionButton.interactable=!equipped;hangarStatus.text=string.IsNullOrEmpty(message)?"PAGE "+(hangarPage+1)+" / "+Mathf.Max(1,Mathf.CeilToInt(list.Count/4f)):message;
        }
        private static Sprite CosmeticPreview(CosmeticDefinition item){return item.Kind==CosmeticKind.Rocket?RuntimeAssets.GetRocketSprite(item.VisualIndex):item.Kind==CosmeticKind.PlanetPack?RuntimeAssets.GetPlanetPackSprite(item.VisualIndex,item.VisualIndex*7+1):item.Kind==CosmeticKind.Background?RuntimeAssets.GetBackgroundSprite(item.VisualIndex):RuntimeAssets.CircleSprite;}
        private static Color CosmeticColor(CosmeticDefinition item){return item.Kind==CosmeticKind.Trail?GameProgression.TrailColor(item.VisualIndex):Color.white;}

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

            Text details = CreateText(card.transform, "Credits Details", "CONCEPTION & DÉVELOPPEMENT\nJEANEDOUART\n\nL'INTELLIGENCE ARTIFICIELLE A ÉTÉ UTILISÉE\nCOMME OUTIL D'ASSISTANCE AU DÉVELOPPEMENT.", 22, TextAnchor.MiddleCenter, FontStyle.Normal);
            details.color = new Color(0.67f, 0.84f, 0.95f, 0.95f);
            details.lineSpacing = 1.15f;
            details.resizeTextForBestFit = true;
            details.resizeTextMinSize = 16;
            details.resizeTextMaxSize = 22;
            SetRect(details.rectTransform, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.56f), Vector2.zero, Vector2.zero);

            GameObject close = CreateButton(card.transform, "Close Credits", "FERMER", new Color(0.12f, 0.48f, 0.58f, 0.95f), ToggleCredits);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.25f, 0.055f), new Vector2(0.75f, 0.18f), Vector2.zero, Vector2.zero);
            return panel;
        }

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

            settingsGameplayPage = CreateSettingsPage(panel.transform, "Gameplay Page");
            Text gameplayNote = CreateText(settingsGameplayPage.transform, "Visual Options Note", "MASQUER UNE AIDE NE CHANGE PAS LE GAMEPLAY NI LES COLLISIONS", 16, TextAnchor.MiddleCenter, FontStyle.Bold);
            gameplayNote.color = new Color(1f, 0.72f, 0.18f, 0.95f);
            gameplayNote.resizeTextForBestFit = true;
            gameplayNote.resizeTextMinSize = 11;
            gameplayNote.resizeTextMaxSize = 16;
            SetRect(gameplayNote.rectTransform, new Vector2(0.04f, 0.89f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
            CreateToggleRow(settingsGameplayPage.transform, "GUIDES DE ROTATION", 0.71f, GamePreferences.RotationGuides, GamePreferences.SetRotationGuides);
            CreateToggleRow(settingsGameplayPage.transform, "ANNEAUX D'ORBITE", 0.535f, GamePreferences.OrbitRings, GamePreferences.SetOrbitRings);
            CreateToggleRow(settingsGameplayPage.transform, "JAUGES DE VOL", 0.36f, GamePreferences.FlightGauges, GamePreferences.SetFlightGauges);
            CreateToggleRow(settingsGameplayPage.transform, "BOUCLIER D'IMMUNITÉ", 0.185f, GamePreferences.Shield, GamePreferences.SetShield);
            CreateToggleRow(settingsGameplayPage.transform, "VIBRATIONS", 0.01f, GamePreferences.Haptics, GamePreferences.SetHaptics);

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
        private AudioClip synchronizationMissClip;
        private AudioClip deathClip;
        private AudioClip skipClip;
        private AudioClip nearMissClip;
        private AudioClip materialClip;
        private AudioClip challengeCompleteClip;
        private AudioClip challengeRewardClip;

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
            synchronizationMissClip = RuntimeAssets.CreateTone("Synchronization Miss", 245f, 0.12f, 0.22f);
            deathClip = RuntimeAssets.CreateTone("Break", 115f, 0.32f, 0.44f);
            skipClip = RuntimeAssets.CreateSkipStinger();
            nearMissClip = RuntimeAssets.CreateTone("Near Miss", 1120f, 0.11f, 0.3f);
            materialClip = RuntimeAssets.CreateTone("Material", 1320f, 0.13f, 0.32f);
            challengeCompleteClip = RuntimeAssets.CreateTone("Challenge Complete", 940f, 0.2f, 0.3f);
            challengeRewardClip = RuntimeAssets.CreateSkipStinger();
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
