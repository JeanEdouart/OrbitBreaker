using System.Collections;
using UnityEngine;
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
            targetY = Mathf.Max(targetY, Mathf.Max(playerPosition.y, anchorPosition.y) + 2.15f);
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
        private Text titleText;
        private Text instructionText;
        private Text gameOverTitle;
        private Text gameOverScore;
        private GameObject gameOverPanel;
        private CanvasGroup hintGroup;

        public void Initialize()
        {
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
            safe.anchorMin = Vector2.zero;
            safe.anchorMax = Vector2.one;
            safe.offsetMin = Vector2.zero;
            safe.offsetMax = Vector2.zero;

            scoreText = CreateText(safe, "Score", "0", 82, TextAnchor.UpperCenter, FontStyle.Bold);
            SetRect(scoreText.rectTransform, new Vector2(0.2f, 0.83f), new Vector2(0.8f, 0.98f), Vector2.zero, Vector2.zero);

            bestText = CreateText(safe, "Best", "BEST 0", 30, TextAnchor.UpperRight, FontStyle.Bold);
            bestText.color = new Color(0.45f, 0.72f, 0.9f, 0.8f);
            SetRect(bestText.rectTransform, new Vector2(0.55f, 0.87f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero);

            comboText = CreateText(safe, "Combo", string.Empty, 34, TextAnchor.UpperLeft, FontStyle.Bold);
            comboText.color = new Color(1f, 0.72f, 0.24f, 1f);
            SetRect(comboText.rectTransform, new Vector2(0.06f, 0.87f), new Vector2(0.45f, 0.97f), Vector2.zero, Vector2.zero);

            titleText = CreateText(safe, "Title", "ORBIT\nBREAKER", 78, TextAnchor.MiddleCenter, FontStyle.Bold);
            titleText.color = new Color(0.76f, 0.98f, 1f, 1f);
            titleText.lineSpacing = 0.75f;
            SetRect(titleText.rectTransform, new Vector2(0.12f, 0.53f), new Vector2(0.88f, 0.73f), Vector2.zero, Vector2.zero);

            var hintObject = new GameObject("Launch Hint", typeof(RectTransform), typeof(CanvasGroup));
            hintObject.transform.SetParent(safe, false);
            hintGroup = hintObject.GetComponent<CanvasGroup>();
            RectTransform hintRect = hintObject.GetComponent<RectTransform>();
            SetRect(hintRect, new Vector2(0.1f, 0.05f), new Vector2(0.9f, 0.22f), Vector2.zero, Vector2.zero);
            instructionText = CreateText(hintRect, "Instruction", "TOUCHE POUR TE PROPULSER", 30, TextAnchor.MiddleCenter, FontStyle.Bold);
            instructionText.color = new Color(0.67f, 0.84f, 0.95f, 0.95f);
            SetRect(instructionText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            gameOverPanel = new GameObject("Game Over Panel", typeof(RectTransform), typeof(Image));
            gameOverPanel.transform.SetParent(safe, false);
            RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
            SetRect(panelRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            gameOverPanel.GetComponent<Image>().color = new Color(0.01f, 0.02f, 0.055f, 0.84f);

            gameOverTitle = CreateText(panelRect, "Game Over", "ORBITE PERDUE", 66, TextAnchor.MiddleCenter, FontStyle.Bold);
            gameOverTitle.color = new Color(1f, 0.3f, 0.43f, 1f);
            SetRect(gameOverTitle.rectTransform, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.72f), Vector2.zero, Vector2.zero);

            gameOverScore = CreateText(panelRect, "Result", string.Empty, 38, TextAnchor.MiddleCenter, FontStyle.Normal);
            gameOverScore.color = new Color(0.74f, 0.9f, 1f, 1f);
            SetRect(gameOverScore.rectTransform, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.57f), Vector2.zero, Vector2.zero);

            Text retry = CreateText(panelRect, "Retry", "TOUCHE POUR RECOMMENCER", 30, TextAnchor.MiddleCenter, FontStyle.Bold);
            retry.color = new Color(1f, 0.76f, 0.28f, 1f);
            SetRect(retry.rectTransform, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.3f), Vector2.zero, Vector2.zero);
            gameOverPanel.SetActive(false);
        }

        public void ShowPlaying(int score, int best, int combo, bool tutorial)
        {
            scoreText.text = score.ToString();
            bestText.text = "BEST " + best;
            comboText.text = combo >= 2 ? "COMBO x" + combo : string.Empty;
            titleText.gameObject.SetActive(tutorial);
            hintGroup.gameObject.SetActive(tutorial);
            gameOverPanel.SetActive(false);
        }

        public void HideTutorial()
        {
            titleText.gameObject.SetActive(false);
            hintGroup.gameObject.SetActive(false);
        }

        public void ShowGameOver(int score, int best, int anchors)
        {
            scoreText.text = score.ToString();
            bestText.text = "BEST " + best;
            gameOverScore.text = "SCORE  " + score + "\nORBITS  " + anchors + "\nBEST  " + best;
            gameOverPanel.SetActive(true);
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
        private AudioSource audioSource;
        private AudioClip launchClip;
        private AudioClip captureClip;
        private AudioClip perfectClip;
        private AudioClip deathClip;

        public void Initialize()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.8f;
            launchClip = RuntimeAssets.CreateTone("Launch", 340f, 0.09f, 0.32f);
            captureClip = RuntimeAssets.CreateTone("Capture", 620f, 0.12f, 0.34f);
            perfectClip = RuntimeAssets.CreateTone("Perfect", 880f, 0.16f, 0.34f);
            deathClip = RuntimeAssets.CreateTone("Break", 115f, 0.32f, 0.44f);
        }

        public void Launch(Vector2 position)
        {
            audioSource.PlayOneShot(launchClip);
            StartCoroutine(Pulse(position, new Color(0.25f, 0.9f, 1f, 0.8f), 0.28f, 0.7f));
        }

        public void Capture(Vector2 position, bool perfect)
        {
            audioSource.PlayOneShot(perfect ? perfectClip : captureClip);
            StartCoroutine(Pulse(position, perfect ? new Color(1f, 0.72f, 0.2f, 0.9f) : new Color(0.2f, 1f, 0.75f, 0.85f), 0.4f, perfect ? 1.5f : 1.05f));
        }

        public void Death(Vector2 position)
        {
            audioSource.PlayOneShot(deathClip);
            StartCoroutine(Pulse(position, new Color(1f, 0.15f, 0.35f, 0.92f), 0.58f, 1.8f));
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
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
