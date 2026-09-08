using System.Collections.Generic;
using System.Collections;
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
        private OnlineLeaderboard onlineLeaderboard;
        private SpaceBackground spaceBackground;
        private int bestScore;
        private int anchorsCaptured;
        private int distanceScore;
        private float bankedHeight;
        private bool runActive;
        private bool tutorialVisible;
        private bool identityReady;
        private float restartAvailableAt;
        private readonly Dictionary<int, int> checkpointScores = new Dictionary<int, int>();
        private readonly Dictionary<int, float> checkpointHeights = new Dictionary<int, float>();
        private int furthestSequence;
        private int runSynchronizations;
        private int runNearMisses;
        private int bestRunSkip;
        private float bestRunMultiplier;
        private int runMaterials;
        private int runSkips;
        private readonly HashSet<int> tacticalReturnOrbits = new HashSet<int>();
        private int skipChain;
        private readonly bool[] challengeCompletionNotified = new bool[3];
        private readonly int[] powerUpInventory = new int[5];
        private int powerUpInventoryCount;
        private bool warpInProgress;
        private int pendingWarpDistance;
        private int bestRunChain;
        private float runElapsed;
        private int dailySeed;
        private DailyCourseDefinition dailyCourse;
        private DailyCourseProgress dailyProgress;
        private bool currentRunOwnsDailyAttempt;
        private const float DeathReplayDuration = 3f;
        private readonly List<DeathReplayFrame> deathReplayFrames = new List<DeathReplayFrame>(200);
        private bool deathReplayInProgress;
        private bool deathReplaySkipRequested;
        private float deathReplaySkippableAt;
        private int activeParisDayKey;
        public RunMode CurrentRunMode { get; private set; }
        public float RunElapsedSeconds => runElapsed;
        public float SprintRemainingSeconds => Mathf.Max(0f, 90f - runElapsed);
        public int LastRunBestChain => bestRunChain;
        public bool LastRunTimedOut { get; private set; }
        public bool LastRunDailyCompleted { get; private set; }
        public int LastDailyReward { get; private set; }
        public string LastDailyUnlock { get; private set; }
        public int DailyCaptures => dailyProgress != null ? dailyProgress.Captures : 0;
        public int DailyTarget => dailyProgress != null ? dailyProgress.Definition.RequiredCaptures : 0;
        public int DailyTier => dailyProgress != null ? dailyProgress.Definition.Tier : 0;
        public bool DailyAttemptedToday => DailyCourse.IsAttempted(DailyCourse.ForDate(System.DateTime.UtcNow).DayKey);
        public bool DailyCompletedToday => DailyCourse.IsClaimed(DailyCourse.ForDate(System.DateTime.UtcNow).DayKey);
        public bool SetRunMode(RunMode mode)
        {
            if (!identityReady || runActive && !tutorialVisible || warpInProgress) return false;
            if (!System.Enum.IsDefined(typeof(RunMode), mode)) return false;
            CurrentRunMode = mode;
            StartRun();
            return true;
        }

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
            onlineLeaderboard = CreateSystem<OnlineLeaderboard>("Online Leaderboard");

            player.Initialize();
            feedback.Initialize();
            hud.Initialize(feedback, onlineLeaderboard);
            hud.CosmeticsChanged += HandleCosmeticsChanged;
            hud.PowerUpRequested += HandlePowerUpRequested;
            player.Captured += HandleCaptured;
            player.MaterialCollected += HandleMaterialCollected;
            player.PowerUpCollected += HandlePowerUpCollected;
            player.Died += HandleDeath;
            player.NearMissed += HandleNearMiss;
            bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        private void Start()
        {
            hud.PreparePlayerIdentity(() => { identityReady = true; StartRun(); });
            _ = onlineLeaderboard.InitializeAsync();
        }

        private void Update()
        {
            if (!identityReady) return;
            MetaProgression.FlushCollectedMaterials();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                MetaProgression.AddMaterials(10000);
                hud.ShowMaterialPickup(player != null ? (Vector2)player.transform.position : Vector2.zero, 10000);
                hud.RefreshMetaPanels();
            }
#endif
            if (deathReplayInProgress)
            {
                if (Time.unscaledTime >= deathReplaySkippableAt && WasGameplayPressedThisFrame()) deathReplaySkipRequested = true;
                return;
            }

            int parisDayKey = FrenchGameClock.ParisDayKey(System.DateTime.UtcNow);
            if (CurrentRunMode == RunMode.Daily && tutorialVisible && activeParisDayKey != parisDayKey)
            {
                StartRun();
                return;
            }
            if (hud.IsPaused)
            {
                MetaProgression.FlushCollectedMaterials(true);
                if (WasGameplayPressedThisFrame()) hud.ResumeGame();
                return;
            }

            float deltaTime = Mathf.Min(Time.deltaTime, 1f / 20f);

            if (runActive && !tutorialVisible && !hud.SettingsOpen)
            {
                runElapsed += Time.deltaTime;
                if (CurrentRunMode == RunMode.Sprint && runElapsed >= 90f && !warpInProgress)
                {
                    LastRunTimedOut = true;
                    HandleDeath(DeathReason.LostInSpace);
                    return;
                }
            }

            if (warpInProgress) return;

            if (runActive)
            {
                bool dailyLaunchAllowed = CurrentRunMode != RunMode.Daily || currentRunOwnsDailyAttempt || !DailyCourse.IsAttempted(dailyCourse.DayKey);
                if (dailyLaunchAllowed && !hud.SettingsOpen && WasGameplayPressedThisFrame() && player.Launch())
                {
                    if (CurrentRunMode == RunMode.Daily && !currentRunOwnsDailyAttempt)
                    {
                        currentRunOwnsDailyAttempt = DailyCourse.TryBeginAttempt(dailyCourse.DayKey);
                        if (!currentRunOwnsDailyAttempt) return;
                    }
                    tutorialVisible = false;
                    hud.HideTutorial();
                    hud.UpdatePowerUpInventory(powerUpInventory, true);
                    feedback.Launch(player.transform.position);
                }

                player.Tick(deltaTime, world.Anchors, world.Hazards, world.FreeDebris, world.Materials, world.PowerUps, cameraRig.CameraY);
                hud.UpdateFlightDisplay(player.transform.position, player.FlightMultiplier, player.FlightDanger01, player.State == PlayerOrbitState.Flying);
                hud.UpdateActivePowerUps(player);
                feedback.UpdateCharge(player.FlightMultiplier, player.State == PlayerOrbitState.Flying);
                bestRunMultiplier = Mathf.Max(bestRunMultiplier, player.FlightMultiplier);
                CheckChallengeCompletions();
                Vector2 anchorPosition = player.CurrentAnchor != null ? player.CurrentAnchor.transform.position : player.transform.position + (Vector3)player.Velocity.normalized * 2f;
                cameraRig.SetTarget(player.transform.position, anchorPosition);
                cameraRig.SetFlightShake(player.FlightDanger01, player.State == PlayerOrbitState.Flying);
                RecordDeathReplayFrame();
                world.RecycleBehind(cameraRig.CameraY, player.LastSequence);
            }
            else if (!hud.SettingsOpen && Time.unscaledTime >= restartAvailableAt && WasGameplayPressedThisFrame())
            {
                StartRun();
            }
        }

        private void OnDestroy()
        {
            MetaProgression.FlushCollectedMaterials(true);
            if (player == null) return;
            player.Captured -= HandleCaptured;
            player.Died -= HandleDeath;
            player.NearMissed -= HandleNearMiss;
            player.MaterialCollected -= HandleMaterialCollected;
            player.PowerUpCollected -= HandlePowerUpCollected;
            hud.CosmeticsChanged -= HandleCosmeticsChanged;
            hud.PowerUpRequested -= HandlePowerUpRequested;
        }

        private void StartRun()
        {
            MetaProgression.FlushCollectedMaterials(true);
            hud.ResumeGame();
            anchorsCaptured = 0;
            distanceScore = 0;
            bankedHeight = GameTuning.StartingHeight;
            runActive = true;
            tutorialVisible = true;
            runSynchronizations = 0;
            runNearMisses = 0;
            bestRunSkip = 0;
            bestRunMultiplier = 1f;
            runMaterials = 0;
            runSkips = 0;
            tacticalReturnOrbits.Clear();
            skipChain = 0;
            bestRunChain = 0;
            runElapsed = 0f;
            LastRunTimedOut = false;
            LastRunDailyCompleted = false;
            LastDailyReward = 0;
            LastDailyUnlock = string.Empty;
            dailySeed = FrenchGameClock.ParisDayKey(System.DateTime.UtcNow);
            dailyCourse = DailyCourse.ForDate(System.DateTime.UtcNow);
            activeParisDayKey = dailyCourse.DayKey;
            currentRunOwnsDailyAttempt = false;
            bestScore = CurrentRunMode == RunMode.Endless ? PlayerPrefs.GetInt(BestScoreKey, 0) : LocalRunStats.Best(CurrentRunMode, dailySeed);
            spaceBackground.SetDistance(0, true);
            powerUpInventoryCount = PowerUpProgression.TotalStored();
            warpInProgress = false;
            pendingWarpDistance = 0;
            for (int i = 0; i < powerUpInventory.Length; i++) powerUpInventory[i] = PowerUpProgression.StoredCount((PowerUpType)i);
            for (int i = 0; i < challengeCompletionNotified.Length; i++)
            {
                ChallengeDefinition challenge = MetaProgression.Challenge(MetaProgression.ActiveChallengeId(i));
                challengeCompletionNotified[i] = MetaProgression.ChallengeProgress(i) >= challenge.Target;
            }
            OrbitAnchor first = world.ResetWorld(CurrentRunMode == RunMode.Daily ? dailySeed : (int?)null);
            dailyProgress = CurrentRunMode == RunMode.Daily ? new DailyCourseProgress(dailyCourse, first.Sequence) : null;
            if (dailyProgress != null) world.SetDifficultyDistance(dailyCourse.DifficultyDistance);
            checkpointScores.Clear();
            checkpointHeights.Clear();
            checkpointScores[first.Sequence] = 0;
            checkpointHeights[first.Sequence] = GameTuning.StartingHeight;
            furthestSequence = first.Sequence;
            player.ResetTo(first);
            player.SetScore(0);
            cameraRig.Snap(first.transform.position);
            deathReplayInProgress = false;
            deathReplaySkipRequested = false;
            deathReplayFrames.Clear();
            RecordDeathReplayFrame();
            hud.ShowPlaying(distanceScore, bestScore, tutorialVisible);
            if (CurrentRunMode == RunMode.Daily)
                hud.ShowDailyAvailability(DailyCourse.IsAttempted(dailyCourse.DayKey), DailyCourse.IsClaimed(dailyCourse.DayKey));
            if (dailyProgress != null) hud.ShowDailyProgress(0, dailyProgress.Definition.RequiredCaptures, dailyProgress.Definition.Tier);
            // Les boutons d'inventaire restent masqués sur l'écran de préparation;
            // ils apparaissent au premier lancement via HideTutorial().
            hud.UpdatePowerUpInventory(powerUpInventory, false);
        }

        private void HandleCaptured(CaptureResult result)
        {
            int previousScore = distanceScore;
            bool revisited = checkpointScores.TryGetValue(result.Anchor.Sequence, out int savedScore);
            // Once per distinct previously captured orbit: bouncing on the same route cannot farm a challenge.
            if (revisited && result.IsBacktrack && !warpInProgress)
                tacticalReturnOrbits.Add(result.Anchor.Sequence);
            bool qualifyingSkip = !revisited && !result.IsBacktrack && !warpInProgress && result.Anchor.Sequence > furthestSequence && result.SkippedAnchors > 0;
            skipChain = GameTuning.NextSkipChain(skipChain, qualifyingSkip);
            bestRunChain = Mathf.Max(bestRunChain, skipChain);
            if (revisited)
            {
                distanceScore = savedScore;
                bankedHeight = checkpointHeights[result.Anchor.Sequence];
            }
            else
            {
                int reward = GameTuning.BankedDistance(bankedHeight, result.Anchor.transform.position.y, result.Multiplier * GameTuning.SkipChainMultiplier(skipChain));
                distanceScore += reward;
                bankedHeight = Mathf.Max(bankedHeight, result.Anchor.transform.position.y);
                checkpointScores[result.Anchor.Sequence] = distanceScore;
                checkpointHeights[result.Anchor.Sequence] = bankedHeight;
                anchorsCaptured++;
                if (CurrentRunMode == RunMode.Endless) PlanetJournal.Record(MetaProgression.Selected(CosmeticKind.PlanetPack), result.Anchor.Sequence);
                if (CurrentRunMode == RunMode.Daily && dailyProgress.RegisterCapture(result.Anchor.Sequence) && dailyProgress.IsComplete)
                {
                    LastRunDailyCompleted = true;
                    if (DailyCourse.TryClaim(dailyProgress, out DailyCourseReward dailyReward))
                    { LastDailyReward = dailyReward.Materials; LastDailyUnlock = dailyReward.UnlockedRocketId; }
                }
            }
            int scoreDelta = distanceScore - previousScore;
            if (pendingWarpDistance > 0)
            {
                distanceScore += pendingWarpDistance;
                scoreDelta += pendingWarpDistance;
                pendingWarpDistance = 0;
                checkpointScores[result.Anchor.Sequence] = distanceScore;
            }
            furthestSequence = Mathf.Max(furthestSequence, result.Anchor.Sequence);
            spaceBackground.SetDistance(distanceScore);
            if (SpaceBackground.SectorForDistance(distanceScore) > SpaceBackground.SectorForDistance(previousScore))
                hud.ShowSector(SpaceBackground.SectorForDistance(distanceScore));
            player.SetScore(distanceScore);
            world.SetDifficultyDistance(CurrentRunMode == RunMode.Daily ? dailyCourse.DifficultyDistance : distanceScore);
            world.EnsureAhead(furthestSequence);
            int rewardedSkips = !revisited && !result.IsBacktrack && !warpInProgress ? result.SkippedAnchors : 0;
            if (rewardedSkips > 0) runSkips++;
            if (result.Synchronized && !revisited && !result.IsBacktrack && !warpInProgress) runSynchronizations++;
            bestRunSkip = Mathf.Max(bestRunSkip, rewardedSkips);
            bestRunMultiplier = Mathf.Max(bestRunMultiplier, result.Multiplier);
            feedback.Capture(player.transform.position, result.Synchronized, rewardedSkips);
            if (result.Synchronization == SynchronizationResult.WrongDirection)
                feedback.SynchronizationMiss(player.transform.position);
            cameraRig.ShakeCapture();
            UpdateBestScore(distanceScore);
            hud.ShowLanding(distanceScore, bestScore, scoreDelta, result.Multiplier, rewardedSkips, result.IsBacktrack, revisited && !result.IsBacktrack, result.Synchronization, skipChain);
            if (dailyProgress != null) hud.ShowDailyProgress(dailyProgress.Captures, dailyProgress.Definition.RequiredCaptures, dailyProgress.Definition.Tier);
            CheckChallengeCompletions();
            if (LastRunDailyCompleted) HandleDeath(DeathReason.LostInSpace);
        }

        private void HandleNearMiss(NearMissResult result)
        {
            runNearMisses++;
            feedback.NearMiss(result.Position, result.Chain);
            hud.ShowNearMiss(result.Chain, player.FlightMultiplier);
            CheckChallengeCompletions();
        }

        private void HandleCosmeticsChanged()
        {
            player.ApplyCosmetics();
            world.RefreshCosmetics();
            spaceBackground.ApplyCosmetics();
            feedback.Capture(player.transform.position, true, 0);
        }

        private void HandleMaterialCollected(int value, Vector2 position)
        {
            runMaterials += value;
            if (CurrentRunMode == RunMode.Endless) MetaProgression.CollectMaterials(value);
            feedback.Material(position, value);
            hud.ShowMaterialPickup(position, value);
            CheckChallengeCompletions();
        }

        private void HandlePowerUpCollected(PowerUpType type, Vector2 position)
        {
            if (CurrentRunMode != RunMode.Endless)
            {
                int slot = (int)type;
                bool added = powerUpInventory[slot] < PowerUpProgression.MaxInventory;
                if (added) powerUpInventory[slot]++;
                hud.UpdatePowerUpInventory(powerUpInventory, true);
                hud.ShowPowerUpPickup(type, powerUpInventory[slot], added);
                feedback.PowerUp(position, type, added);
                return;
            }
            if (!PowerUpProgression.TryStore(type))
            {
                hud.ShowPowerUpPickup(type, PowerUpProgression.MaxInventory, false);
                feedback.PowerUp(position, type, false);
                return;
            }
            powerUpInventory[(int)type] = PowerUpProgression.StoredCount(type);
            powerUpInventoryCount = PowerUpProgression.TotalStored();
            hud.UpdatePowerUpInventory(powerUpInventory, true);
            hud.ShowPowerUpPickup(type, powerUpInventory[(int)type], true);
            feedback.PowerUp(position, type, true);
        }

        private void HandlePowerUpRequested(PowerUpType type)
        {
            int index = (int)type;
            if (!runActive || warpInProgress || hud.IsPaused || hud.SettingsOpen || player.State == PlayerOrbitState.Dead
                || index < 0 || index >= powerUpInventory.Length || powerUpInventory[index] <= 0) return;
            if (player.PowerUpRemaining(type) > 0f) return;
            tutorialVisible = false;
            hud.HideTutorial();
            hud.UpdatePowerUpInventory(powerUpInventory, true);
            int level = PowerUpProgression.Level(type);
            if (type == PowerUpType.Wormhole)
            {
                StartCoroutine(ActivateWormhole(level));
                return;
            }
            if (!ConsumeRunPowerUp(type)) return;
            switch (type)
            {
                case PowerUpType.OrbitMagnet: player.ActivateMagnet(level); break;
                case PowerUpType.Shield: player.ActivateShield(level); break;
                case PowerUpType.IonOverdrive: player.ActivateOverdrive(level); break;
                case PowerUpType.QuantumExtractor: player.ActivateExtractor(level); break;
            }
            hud.UpdatePowerUpInventory(powerUpInventory, true);
            hud.ShowPowerUpActivated(type);
            feedback.PowerUp(player.transform.position, type, true);
        }

        private IEnumerator ActivateWormhole(int level)
        {
            OrbitAnchor target = world.PrepareSafeWarpTarget(player.LastSequence, PowerUpProgression.WormholeOrbitSkip(level));
            if (target == null) yield break;
            if (!ConsumeRunPowerUp(PowerUpType.Wormhole)) yield break;
            hud.UpdatePowerUpInventory(powerUpInventory, true); hud.ShowPowerUpActivated(PowerUpType.Wormhole);
            feedback.PowerUp(player.transform.position, PowerUpType.Wormhole, true);
            warpInProgress = true;
            hud.BeginHyperspace();
            Vector3 originalScale = player.transform.localScale;
            Vector3 startPosition = player.transform.position;
            Vector3 destination = target.transform.position + Vector3.down * target.Radius;
            Vector2 cameraStart = Camera.main.transform.position;
            Vector2 cameraDestination = new Vector2(destination.x * 0.12f, Mathf.Max(0f, destination.y + 2.25f));
            Quaternion startRotation = player.transform.rotation;
            float elapsed = 0f;
            while (elapsed < 0.48f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.48f));
                hud.UpdateHyperspace(t * 0.72f);
                spaceBackground.SetHyperspace(t);
                feedback.UpdateWarpAudio(t * 0.5f);
                player.transform.localScale = originalScale * Mathf.Lerp(1f, 0.72f, t);
                player.transform.rotation = Quaternion.Slerp(startRotation, Quaternion.FromToRotation(Vector3.up, destination - startPosition), t);
                player.SetWarpEngine(t * 0.5f);
                yield return null;
            }
            elapsed = 0f;
            const float travelDuration = 2.1f;
            while (elapsed < travelDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelDuration);
                float progress = t * t * t * (t * (t * 6f - 15f) + 10f);
                float intensity = Mathf.Sin(t * Mathf.PI);
                hud.UpdateHyperspace(intensity);
                spaceBackground.SetHyperspace(intensity);
                feedback.UpdateWarpAudio(intensity);
                player.transform.position = Vector3.Lerp(startPosition, destination, progress);
                player.transform.up = (destination - startPosition).normalized;
                cameraRig.SetCinematicPosition(Vector2.Lerp(cameraStart, cameraDestination, progress));
                player.SetWarpEngine(intensity);
                player.transform.localScale = Vector3.Scale(originalScale, new Vector3(1f - intensity * 0.12f, 1f + intensity * 0.2f, 1f));
                hud.UpdateFlightDisplay(player.transform.position, 1f, 0f, false);
                yield return null;
            }
            pendingWarpDistance = PowerUpProgression.WormholeDistance(level);
            player.WarpTo(target);
            player.SetWarpEngine(0f);
            feedback.UpdateWarpAudio(0f);
            Quaternion arrivalRotation = player.transform.rotation;
            elapsed = 0f;
            while (elapsed < 0.62f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.62f));
                hud.UpdateHyperspace(0f);
                spaceBackground.SetHyperspace(0f);
                player.transform.localScale = originalScale;
                player.transform.rotation = Quaternion.Slerp(arrivalRotation, Quaternion.FromToRotation(Vector3.up, Vector3.right * target.Direction), t);
                yield return null;
            }
            player.transform.localScale = originalScale;
            spaceBackground.SetHyperspace(0f);
            hud.EndHyperspace();
            player.RefreshCaptureProtection();
            warpInProgress = false;
            hud.UpdatePowerUpInventory(powerUpInventory, true);
        }

        private void CheckChallengeCompletions()
        {
            if (CurrentRunMode != RunMode.Endless) return;
            for (int slot = 0; slot < challengeCompletionNotified.Length; slot++)
            {
                if (challengeCompletionNotified[slot] || MetaProgression.ChallengeClaimed(slot)) continue;
                ChallengeDefinition challenge = MetaProgression.Challenge(MetaProgression.ActiveChallengeId(slot));
                int projected = MetaProgression.ProjectedProgress(slot, distanceScore, anchorsCaptured, runSkips, runSynchronizations, runNearMisses, runMaterials, bestRunMultiplier, bestRunSkip, tacticalReturnOrbits.Count);
                if (projected < challenge.Target) continue;
                challengeCompletionNotified[slot] = true;
                hud.ShowChallengeComplete(challenge.Label);
                feedback.ChallengeCompleted();
            }
        }

        private void HandleDeath(DeathReason reason)
        {
            if (!runActive) return;
            MetaProgression.FlushCollectedMaterials(true);
            runActive = false;
            restartAvailableAt = float.PositiveInfinity;
            cameraRig.SetFlightShake(0f, false);
            feedback.UpdateCharge(1f, false);
            if (CurrentRunMode == RunMode.Endless)
            {
                GameProgression.RecordRun(distanceScore, runSynchronizations, runNearMisses);
                MetaProgression.RecordRun(distanceScore, anchorsCaptured, runSkips, runSynchronizations, runNearMisses, runMaterials, bestRunMultiplier, bestRunSkip, tacticalReturnOrbits.Count);
                LocalRunStats.Record(runElapsed, bestRunSkip, bestRunChain, reason);
                PlayerPrefs.Save();
                _ = onlineLeaderboard.SubmitBestScoreAsync(bestScore);
            }
            else
            {
                LocalRunStats.RecordModeBest(CurrentRunMode, dailySeed, bestScore);
                if (CurrentRunMode == RunMode.Sprint) _ = onlineLeaderboard.SubmitSprintScoreAsync(bestScore);
            }
            hud.UpdatePowerUpInventory(powerUpInventory, false);
            if (LastRunDailyCompleted || deathReplayFrames.Count < 2)
            {
                FinishDeath(reason);
                return;
            }
            StartCoroutine(PlayDeathReplay(reason));
        }

        private void RecordDeathReplayFrame()
        {
            if (player == null || cameraRig == null || player.State == PlayerOrbitState.Dead) return;
            float now = Time.unscaledTime;
            deathReplayFrames.Add(player.CaptureReplayFrame(now, cameraRig.CameraPosition));
            float oldest = now - DeathReplayDuration - 0.15f;
            int remove = 0;
            while (remove < deathReplayFrames.Count - 2 && deathReplayFrames[remove].Time < oldest) remove++;
            if (remove > 0) deathReplayFrames.RemoveRange(0, remove);
        }

        private IEnumerator PlayDeathReplay(DeathReason reason)
        {
            deathReplayInProgress = true;
            deathReplaySkipRequested = false;
            deathReplaySkippableAt = Time.unscaledTime + 0.3f;
            hud.ShowDeathReplay();

            float firstTime = deathReplayFrames[0].Time;
            float lastTime = deathReplayFrames[deathReplayFrames.Count - 1].Time;
            float recordedDuration = Mathf.Max(0.05f, lastTime - firstTime);
            float playbackStart = Time.unscaledTime;
            int cursor = 0;
            while (!deathReplaySkipRequested)
            {
                float elapsed = Time.unscaledTime - playbackStart;
                if (elapsed >= recordedDuration) break;
                float sampleTime = firstTime + elapsed;
                while (cursor + 1 < deathReplayFrames.Count && deathReplayFrames[cursor + 1].Time < sampleTime) cursor++;
                DeathReplayFrame a = deathReplayFrames[cursor];
                DeathReplayFrame b = deathReplayFrames[Mathf.Min(cursor + 1, deathReplayFrames.Count - 1)];
                float t = Mathf.InverseLerp(a.Time, b.Time, sampleTime);
                var blended = new DeathReplayFrame(sampleTime,
                    Vector3.Lerp(a.PlayerPosition, b.PlayerPosition, t), Quaternion.Slerp(a.PlayerRotation, b.PlayerRotation, t),
                    Vector3.Lerp(a.PlayerScale, b.PlayerScale, t), Vector3.Lerp(a.CameraPosition, b.CameraPosition, t),
                    t < 0.5f ? a.BodyVisible : b.BodyVisible, t < 0.5f ? a.EngineVisible : b.EngineVisible,
                    t < 0.5f ? a.ShieldVisible : b.ShieldVisible, Mathf.Lerp(a.Fuel, b.Fuel, t));
                player.ApplyReplayFrame(blended);
                cameraRig.ApplyReplayPosition(blended.CameraPosition);
                yield return null;
            }
            deathReplayInProgress = false;
            hud.HideDeathReplay();
            FinishDeath(reason);
        }

        private void FinishDeath(DeathReason reason)
        {
            player.RestoreDeathVisual(reason);
            if (!LastRunDailyCompleted) feedback.Death(player.transform.position, reason);
            if (reason == DeathReason.Breaker && !LastRunDailyCompleted) cameraRig.ShakeExplosion();
            hud.ShowGameOver(distanceScore, bestScore, anchorsCaptured, reason, runSynchronizations, runNearMisses, bestRunSkip, bestRunMultiplier, runMaterials);
            restartAvailableAt = Time.unscaledTime + 0.55f;
        }

        private void UpdateBestScore(int currentScore)
        {
            if (currentScore <= bestScore) return;
            bestScore = currentScore;
            if (CurrentRunMode == RunMode.Endless) PlayerPrefs.SetInt(BestScoreKey, bestScore);
        }

        private bool ConsumeRunPowerUp(PowerUpType type)
        {
            int index = (int)type;
            if (powerUpInventory[index] <= 0) return false;
            if (CurrentRunMode == RunMode.Endless && !PowerUpProgression.TryConsume(type)) return false;
            powerUpInventory[index]--;
            powerUpInventoryCount = 0;
            for (int i = 0; i < powerUpInventory.Length; i++) powerUpInventoryCount += powerUpInventory[i];
            return true;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) MetaProgression.FlushCollectedMaterials(true);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) MetaProgression.FlushCollectedMaterials(true);
        }

        private void OnApplicationQuit() => MetaProgression.FlushCollectedMaterials(true);

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
