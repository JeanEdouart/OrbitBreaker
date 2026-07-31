using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitBreaker
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const string BestScoreKey = "OrbitBreaker.BestScore";

        private OrbitWorld world;
        private OrbitPlayer player;
        private OrbitCameraRig cameraRig;
        private OrbitHud hud;
        private OrbitFeedback feedback;
        private int score;
        private int bestScore;
        private int anchorsCaptured;
        private int combo;
        private bool runActive;
        private bool tutorialVisible;
        private float restartAvailableAt;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            QualitySettings.vSyncCount = 0;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            cameraRig = mainCamera.GetComponent<OrbitCameraRig>();
            if (cameraRig == null) cameraRig = mainCamera.gameObject.AddComponent<OrbitCameraRig>();
            cameraRig.Initialize(mainCamera);

            world = CreateSystem<OrbitWorld>("World");
            player = CreateSystem<OrbitPlayer>("Player");
            hud = CreateSystem<OrbitHud>("HUD");
            feedback = CreateSystem<OrbitFeedback>("Feedback");

            player.Initialize();
            hud.Initialize();
            feedback.Initialize();
            player.Captured += HandleCaptured;
            player.Died += HandleDeath;
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        private void Start()
        {
            StartRun();
        }

        private void Update()
        {
            float deltaTime = Mathf.Min(Time.deltaTime, 1f / 20f);

            if (runActive)
            {
                if (WasPressedThisFrame() && player.Launch())
                {
                    tutorialVisible = false;
                    hud.HideTutorial();
                    feedback.Launch(player.transform.position);
                }

                player.Tick(deltaTime, world.Anchors, world.Hazards, cameraRig.CameraY);
                Vector2 anchorPosition = player.CurrentAnchor != null ? player.CurrentAnchor.transform.position : player.transform.position + (Vector3)player.Velocity.normalized * 2f;
                cameraRig.SetTarget(player.transform.position, anchorPosition);
                world.RecycleBehind(cameraRig.CameraY, player.LastSequence);
            }
            else if (Time.unscaledTime >= restartAvailableAt && WasPressedThisFrame())
            {
                StartRun();
            }
        }

        private void OnDestroy()
        {
            if (player == null) return;
            player.Captured -= HandleCaptured;
            player.Died -= HandleDeath;
        }

        private void StartRun()
        {
            score = 0;
            anchorsCaptured = 0;
            combo = 0;
            runActive = true;
            tutorialVisible = true;
            OrbitAnchor first = world.ResetWorld();
            player.ResetTo(first);
            player.SetScore(0);
            cameraRig.Snap(first.transform.position);
            hud.ShowPlaying(score, bestScore, combo, tutorialVisible);
        }

        private void HandleCaptured(OrbitAnchor anchor, float normalizedAccuracy)
        {
            anchorsCaptured++;
            bool perfect = normalizedAccuracy <= 0.22f;
            combo = perfect ? combo + 1 : 0;
            score += GameTuning.PointsForCapture(normalizedAccuracy, combo);
            player.SetScore(anchorsCaptured);
            world.EnsureAhead(anchor.Sequence);
            feedback.Capture(player.transform.position, perfect);

            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt(BestScoreKey, bestScore);
            }

            hud.ShowPlaying(score, bestScore, combo, false);
        }

        private void HandleDeath()
        {
            if (!runActive) return;
            runActive = false;
            restartAvailableAt = Time.unscaledTime + 0.55f;
            feedback.Death(player.transform.position);
            PlayerPrefs.Save();
            hud.ShowGameOver(score, bestScore, anchorsCaptured);
        }

        private T CreateSystem<T>(string objectName) where T : Component
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(transform, false);
            return instance.AddComponent<T>();
        }

        private static bool WasPressedThisFrame()
        {
            bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool keyboard = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
            return touch || mouse || keyboard;
        }
    }
}
