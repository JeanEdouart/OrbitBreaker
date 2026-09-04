using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private SpaceBackground spaceBackground;
        private int bestScore;
        private int anchorsCaptured;
        private int distanceScore;
        private float bankedHeight;
        private bool runActive;
        private bool tutorialVisible;
        private float restartAvailableAt;
        private readonly Dictionary<int, int> checkpointScores = new Dictionary<int, int>();
        private readonly Dictionary<int, float> checkpointHeights = new Dictionary<int, float>();
        private int furthestSequence;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            GamePreferences.ApplyRuntime();
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

            spaceBackground = CreateSystem<SpaceBackground>("Space Background");
            spaceBackground.Initialize(mainCamera);

            world = CreateSystem<OrbitWorld>("World");
            player = CreateSystem<OrbitPlayer>("Player");
            hud = CreateSystem<OrbitHud>("HUD");
            feedback = CreateSystem<OrbitFeedback>("Feedback");

            player.Initialize();
            feedback.Initialize();
            hud.Initialize(feedback);
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
            if (hud.IsPaused)
            {
                if (WasGameplayPressedThisFrame()) hud.ResumeGame();
                return;
            }

            float deltaTime = Mathf.Min(Time.deltaTime, 1f / 20f);

            if (runActive)
            {
                if (!hud.SettingsOpen && WasGameplayPressedThisFrame() && player.Launch())
                {
                    tutorialVisible = false;
                    hud.HideTutorial();
                    feedback.Launch(player.transform.position);
                }

                player.Tick(deltaTime, world.Anchors, world.Hazards, cameraRig.CameraY);
                hud.UpdateFlightDisplay(player.transform.position, player.FlightMultiplier, player.FlightDanger01, player.State == PlayerOrbitState.Flying);
                feedback.UpdateCharge(player.FlightMultiplier, player.State == PlayerOrbitState.Flying);
                Vector2 anchorPosition = player.CurrentAnchor != null ? player.CurrentAnchor.transform.position : player.transform.position + (Vector3)player.Velocity.normalized * 2f;
                cameraRig.SetTarget(player.transform.position, anchorPosition);
                cameraRig.SetFlightShake(player.FlightDanger01, player.State == PlayerOrbitState.Flying);
                world.RecycleBehind(cameraRig.CameraY, player.LastSequence);
            }
            else if (!hud.SettingsOpen && Time.unscaledTime >= restartAvailableAt && WasGameplayPressedThisFrame())
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
            hud.ResumeGame();
            anchorsCaptured = 0;
            distanceScore = 0;
            bankedHeight = GameTuning.StartingHeight;
            runActive = true;
            tutorialVisible = true;
            OrbitAnchor first = world.ResetWorld();
            checkpointScores.Clear();
            checkpointHeights.Clear();
            checkpointScores[first.Sequence] = 0;
            checkpointHeights[first.Sequence] = GameTuning.StartingHeight;
            furthestSequence = first.Sequence;
            player.ResetTo(first);
            player.SetScore(0);
            cameraRig.Snap(first.transform.position);
            hud.ShowPlaying(distanceScore, bestScore, tutorialVisible);
        }

        private void HandleCaptured(CaptureResult result)
        {
            int previousScore = distanceScore;
            bool revisited = checkpointScores.TryGetValue(result.Anchor.Sequence, out int savedScore);
            if (revisited)
            {
                distanceScore = savedScore;
                bankedHeight = checkpointHeights[result.Anchor.Sequence];
            }
            else
            {
                int reward = GameTuning.BankedDistance(bankedHeight, result.Anchor.transform.position.y, result.Multiplier);
                distanceScore += reward;
                bankedHeight = Mathf.Max(bankedHeight, result.Anchor.transform.position.y);
                checkpointScores[result.Anchor.Sequence] = distanceScore;
                checkpointHeights[result.Anchor.Sequence] = bankedHeight;
                anchorsCaptured++;
            }
            int scoreDelta = distanceScore - previousScore;
            furthestSequence = Mathf.Max(furthestSequence, result.Anchor.Sequence);
            player.SetScore(furthestSequence);
            world.EnsureAhead(furthestSequence);
            int rewardedSkips = !revisited && !result.IsBacktrack ? result.SkippedAnchors : 0;
            feedback.Capture(player.transform.position, result.Multiplier >= 2f, rewardedSkips);
            cameraRig.ShakeCapture();
            UpdateBestScore(distanceScore);
            hud.ShowLanding(distanceScore, bestScore, scoreDelta, result.Multiplier, rewardedSkips, result.IsBacktrack, revisited && !result.IsBacktrack);
        }

        private void HandleDeath(DeathReason reason)
        {
            if (!runActive) return;
            runActive = false;
            restartAvailableAt = Time.unscaledTime + 0.55f;
            feedback.Death(player.transform.position, reason);
            if (reason == DeathReason.Breaker) cameraRig.ShakeExplosion();
            cameraRig.SetFlightShake(0f, false);
            feedback.UpdateCharge(1f, false);
            PlayerPrefs.Save();
            hud.ShowGameOver(distanceScore, bestScore, anchorsCaptured, reason);
        }

        private void UpdateBestScore(int currentScore)
        {
            if (currentScore <= bestScore) return;
            bestScore = currentScore;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
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

        private static bool WasGameplayPressedThisFrame()
        {
            if (!WasPressedThisFrame()) return false;
            if (EventSystem.current == null) return true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && EventSystem.current.IsPointerOverGameObject()) return false;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject(touchId)) return false;
            }
            return true;
        }
    }
}
