using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class GameTuningTests
    {
        [Test]
        public void SkipSeries_RewardsConsecutiveSkipsAndResets()
        {
            int chain = 0;
            for (int i = 1; i <= 20; i++)
            {
                chain = GameTuning.NextSkipChain(chain, true);
                Assert.That(GameTuning.SkipChainMultiplier(chain), Is.EqualTo(1f + Mathf.Min(i - 1, 6) * 0.25f));
            }
            Assert.That(GameTuning.NextSkipChain(chain, false), Is.Zero);
            Assert.That(GameTuning.SkipChainMultiplier(0), Is.EqualTo(1f));
            Assert.That(GameTuning.BankedDistance(0f, 4f, 1f), Is.EqualTo(8));
            Assert.That(GameTuning.BankedDistance(0f, 4f, 2f * GameTuning.SkipChainMultiplier(3)), Is.EqualTo(24));
        }

        [Test]
        public void HazardStages_RespectDistanceThresholdsAndCaps()
        {
            Assert.That(GameTuning.OrbitHazardChance(99), Is.Zero);
            Assert.That(GameTuning.OrbitHazardChance(100), Is.EqualTo(1f / 6f).Within(0.001f));
            Assert.That(GameTuning.OrbitHazardChance(300), Is.EqualTo(0.4f));
            Assert.That(GameTuning.OrbitHazardChance(9000), Is.EqualTo(0.4f));
            Assert.That(GameTuning.SkipHazardChance(299), Is.Zero);
            Assert.That(GameTuning.SkipHazardChance(300), Is.EqualTo(0.12f));
            Assert.That(GameTuning.SkipHazardChance(600), Is.EqualTo(0.45f));
        }

        [Test]
        public void GeneratedWorld_UsesBothSidesAndKeepsEarlyFlightsSafe()
        {
            var root = new GameObject("Lateral Generation Test");
            try
            {
                var world = root.AddComponent<OrbitWorld>();
                world.ResetWorld(); world.SetDifficultyDistance(99); world.EnsureAhead(100);
                int left = 0, right = 0;
                foreach (var anchor in world.Anchors)
                {
                    if (anchor.transform.position.x < -0.8f) left++;
                    if (anchor.transform.position.x > 0.8f) right++;
                    Assert.That(Mathf.Abs(anchor.transform.position.x), Is.LessThanOrEqualTo(1.6f));
                }
                Assert.That(left, Is.GreaterThan(15)); Assert.That(right, Is.GreaterThan(15));
                Assert.That(world.Hazards.Count, Is.Zero); Assert.That(world.FreeDebris.Count, Is.Zero);
                world.SetDifficultyDistance(299); world.EnsureAhead(200);
                Assert.That(world.Hazards.Count, Is.GreaterThan(5)); Assert.That(world.FreeDebris.Count, Is.Zero);
                foreach (var hazard in world.Hazards)
                {
                    var anchor = world.FindAnchor(hazard.Sequence);
                    Assert.That(GameTuning.IsOrbitFullyVisibleForHazard(anchor.transform.position.x, anchor.Radius), Is.True);
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void WorldDifficulty_UsesDistanceAndDoesNotMutateExistingOrbits()
        {
            var root = new GameObject("Difficulty Test World");
            try
            {
                var world = root.AddComponent<OrbitWorld>();
                OrbitAnchor first = world.ResetWorld();
                world.SetDifficultyDistance(450);
                world.EnsureAhead(20);
                Assert.That(first.DifficultyDistance, Is.Zero);
                Assert.That(world.FindAnchor(20).DifficultyDistance, Is.EqualTo(450));
                world.SetDifficultyDistance(300);
                world.EnsureAhead(40);
                Assert.That(world.FindAnchor(40).DifficultyDistance, Is.EqualTo(450));
                world.SetDifficultyDistance(6000);
                world.EnsureAhead(60);
                Assert.That(world.FindAnchor(60).DifficultyDistance, Is.EqualTo(GameTuning.DifficultyCapDistance));
                Assert.That(world.ResetWorld().DifficultyDistance, Is.Zero);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase(-1, 0)]
        [TestCase(499, 0)]
        [TestCase(500, 1)]
        [TestCase(999, 1)]
        [TestCase(1000, 2)]
        [TestCase(3500, 7)]
        public void BackgroundSectors_FollowDisplayedDistance(int distance, int expected)
        {
            Assert.That(SpaceBackground.SectorForDistance(distance), Is.EqualTo(expected));
        }

        [Test]
        public void Difficulty_IsBoundedAndMonotonic()
        {
            float previous = GameTuning.Difficulty01(0);
            Assert.That(previous, Is.InRange(0f, 1f));

            for (int score = 1; score <= 6000; score++)
            {
                float current = GameTuning.Difficulty01(score);
                Assert.That(current, Is.InRange(0f, 1f));
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                previous = current;
            }
        }

        [Test]
        public void Difficulty_ReachesTheNewLongRunMilestonesGradually()
        {
            Assert.That(GameTuning.Difficulty01(0), Is.Zero);
            Assert.That(GameTuning.Difficulty01(300), Is.EqualTo(1f / 3f).Within(0.001f));
            Assert.That(GameTuning.Difficulty01(450), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(GameTuning.Difficulty01(3000), Is.EqualTo(1f));
            Assert.That(GameTuning.Difficulty01(6000), Is.EqualTo(1f));
            Assert.That(GameTuning.AngularSpeed(10000), Is.LessThanOrEqualTo(GameTuning.MaxAngularSpeed));
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(500)]
        public void Speeds_StayWithinDesignedLimits(int score)
        {
            Assert.That(GameTuning.AngularSpeed(score), Is.InRange(GameTuning.BaseAngularSpeed, GameTuning.MaxAngularSpeed));
            Assert.That(GameTuning.LaunchSpeed(score), Is.InRange(GameTuning.BaseLaunchSpeed, GameTuning.MaxLaunchSpeed));
        }

        [Test]
        public void FlightMultiplier_IsBoundedAndMonotonic()
        {
            float previous = GameTuning.FlightMultiplier(0f);
            for (float time = 0.1f; time < 10f; time += 0.1f)
            {
                float current = GameTuning.FlightMultiplier(time);
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                Assert.That(current, Is.InRange(1f, GameTuning.MaxDistanceMultiplier));
                previous = current;
            }
        }

        [Test]
        public void FlightMultiplier_AdvancesByTenths()
        {
            Assert.That(GameTuning.FlightMultiplier(0f), Is.EqualTo(1f));
            Assert.That(GameTuning.FlightMultiplier(GameTuning.MultiplierStepDuration), Is.EqualTo(1.1f).Within(0.001f));
            Assert.That(GameTuning.FlightMultiplier(GameTuning.MultiplierStepDuration * 2f), Is.EqualTo(1.2f).Within(0.001f));
        }

        [Test]
        public void FlightDanger_FillsExactlyAtTimeout()
        {
            Assert.That(GameTuning.FlightDanger01(0f), Is.Zero);
            Assert.That(GameTuning.FlightDanger01(GameTuning.MaxFlightTime * 0.5f), Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(GameTuning.FlightDanger01(GameTuning.MaxFlightTime), Is.EqualTo(1f));
            Assert.That(GameTuning.FlightDanger01(GameTuning.MaxFlightTime * 2f), Is.EqualTo(1f));
        }

        [Test]
        public void Multiplier_IncreasesBankedDistance()
        {
            int normal = GameTuning.BankedDistance(0f, 4f, 1f);
            int risky = GameTuning.BankedDistance(0f, 4f, 3f);
            Assert.That(risky, Is.EqualTo(normal * 3));
        }

        [Test]
        public void BankedDistance_NeverRewardsFalling()
        {
            Assert.That(GameTuning.BankedDistance(10f, 5f, 6f), Is.Zero);
        }

        [Test]
        public void Hazards_AreDisabledDuringOnboarding()
        {
            for (int sequence = 0; sequence < GameTuning.HazardIntroductionSequence; sequence++)
                Assert.That(GameTuning.HasHazard(sequence), Is.False);
        }

        [Test]
        public void HazardPattern_IsDeterministicAfterOnboarding()
        {
            int initial = 0, capped = 0;
            for (int sequence = 4; sequence < 200; sequence++)
            {
                if (GameTuning.HasHazard(sequence, 0)) initial++;
                if (GameTuning.HasHazard(sequence, 3000)) capped++;
                if (GameTuning.HasHazard(sequence, 0)) Assert.That(GameTuning.HasHazard(sequence, 3000), Is.True);
            }
            Assert.That(capped, Is.GreaterThan(initial));
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(40)]
        [TestCase(1500)]
        [TestCase(3000)]
        public void TypicalNextOrbit_HasAComfortableLaunchWindow(int sequence)
        {
            int samples = GameTuning.CountReachableLaunchSamples(
                Vector2.zero, 1.2f, 1,
                new Vector2(1.4f, 3.8f), 1.2f,
                sequence);

            Assert.That(samples, Is.GreaterThanOrEqualTo(GameTuning.MinimumReachableLaunchSamples(sequence)));
        }

        [Test]
        public void OrbitOutsideFlightRange_IsRejected()
        {
            bool reachable = GameTuning.IsAnchorReachable(
                Vector2.zero, 1.1f, 1,
                new Vector2(0f, 40f), 1.1f,
                0);

            Assert.That(reachable, Is.False);
        }

        [Test]
        public void HazardRequiresExtraLandingOptions()
        {
            int minimum = GameTuning.MinimumReachableLaunchSamples(20);
            Assert.That(GameTuning.CanAddHazardToLayout(minimum, 20), Is.False);
            Assert.That(GameTuning.CanAddHazardToLayout(minimum + 3, 20), Is.True);
        }

        [Test]
        public void SkipChallenge_OnlyUsesComfortableOptionalRoutes()
        {
            Assert.That(GameTuning.CanAddSkipChallenge(3, 20), Is.False);
            int comfortable = Mathf.Max(4, GameTuning.MinimumReachableLaunchSamples(20) - 2);
            Assert.That(GameTuning.CanAddSkipChallenge(comfortable, 20), Is.True);
            Assert.That(GameTuning.CanAddSkipChallenge(99, 8), Is.False);
        }

        [Test]
        public void TransferPickupPoint_IsOnAReachableFlight()
        {
            bool found = GameTuning.TryFindTransferPickupPoint(Vector2.zero, 1.2f, 1,
                new Vector2(1.4f, 3.8f), 1.2f, 20, out Vector2 point);
            Assert.That(found, Is.True);
            Assert.That(point.y, Is.GreaterThan(0.5f).And.LessThan(3.4f));
            Assert.That(Mathf.Abs(point.x), Is.LessThan(GameTuning.HorizontalLimit));
        }

        [Test]
        public void SkipChallengePoint_SitsInsideTheActualLongFlight()
        {
            bool found = GameTuning.TryFindSkipChallengePoint(
                Vector2.zero, 1.2f, 1,
                new Vector2(1.8f, 3.6f), 1.2f,
                new Vector2(-0.4f, 7.4f), 1.25f,
                18, out Vector2 point, out float clearance);

            Assert.That(found, Is.True);
            Assert.That(point.y, Is.GreaterThan(1.4f).And.LessThan(6.3f));
            Assert.That(clearance, Is.GreaterThan(-0.4f));
        }

        [Test]
        public void SkipChallengePoint_RejectsARouteCapturedByTheMiddleOrbit()
        {
            bool found = GameTuning.TryFindSkipChallengePoint(
                Vector2.zero, 1.2f, 1,
                new Vector2(0.7f, 3.6f), 1.2f,
                new Vector2(-0.4f, 7.4f), 1.25f,
                18, out _, out _);

            Assert.That(found, Is.False);
        }

        [Test]
        public void Synchronization_RequiresCorrectZoneAndDirection()
        {
            Vector2 radial = Vector2.right;
            float zone = 0f;
            Assert.That(GameTuning.IsSynchronizedCapture(radial, Vector2.up, 1, 10, zone), Is.True);
            Assert.That(GameTuning.IsSynchronizedCapture(radial, Vector2.down, 1, 10, zone), Is.False);
            Assert.That(GameTuning.IsSynchronizedCapture(Vector2.up, Vector2.left, 1, 10, zone), Is.False);
        }

        [TestCase(0, 1.35f, 3.35f)]
        [TestCase(18, -1.6f, 4.0f)]
        [TestCase(45, 0.4f, 4.35f)]
        public void SynchronizationGate_ComesFromAReachableTrajectory(int sequence, float targetX, float targetY)
        {
            Vector2 target = new Vector2(targetX, targetY);
            bool clockwise = GameTuning.TryFindSynchronizationGate(
                Vector2.zero, 1.2f, 1, target, 1.25f, 1, sequence,
                out _, out float clockwiseAlignment);
            bool counterClockwise = GameTuning.TryFindSynchronizationGate(
                Vector2.zero, 1.2f, 1, target, 1.25f, -1, sequence,
                out _, out float counterClockwiseAlignment);

            Assert.That(clockwise || counterClockwise, Is.True,
                "At least one orbit direction must provide a fair synchronization gate.");
            Assert.That(Mathf.Max(clockwiseAlignment, counterClockwiseAlignment),
                Is.GreaterThanOrEqualTo(GameTuning.SynchronizationAlignment(sequence)));
        }

        [Test]
        public void DifficultyCycle_ContainsBreathingRoom()
        {
            Assert.That(GameTuning.IsBreatherOrbit(20), Is.False);
            Assert.That(GameTuning.IsBreatherOrbit(21), Is.True);
            Assert.That(GameTuning.IsBreatherOrbit(23), Is.True);
            Assert.That(GameTuning.IsBreatherOrbit(24), Is.False);
        }

        [Test]
        public void ProceduralMotifs_StayInsideHorizontalStepBudget()
        {
            for (int sequence = 0; sequence < 100; sequence++)
            for (int sample = 0; sample <= 10; sample++)
                Assert.That(Mathf.Abs(GameTuning.PatternHorizontalStep(sequence, sample / 10f)), Is.LessThanOrEqualTo(2.2f));
        }

        [Test]
        public void HazardsAreRejectedWhenAnyPartOfOrbitLeavesPortraitView()
        {
            Assert.That(GameTuning.IsOrbitFullyVisibleForHazard(0.8f, 1.25f), Is.True);
            Assert.That(GameTuning.IsOrbitFullyVisibleForHazard(1.55f, 1.25f), Is.False);
            Assert.That(GameTuning.IsOrbitFullyVisibleForHazard(-1.55f, 1.25f), Is.False);
        }
    }
}
