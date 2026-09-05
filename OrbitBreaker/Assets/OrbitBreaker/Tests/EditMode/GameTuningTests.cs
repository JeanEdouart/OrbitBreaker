using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class GameTuningTests
    {
        [Test]
        public void Difficulty_IsBoundedAndMonotonic()
        {
            float previous = GameTuning.Difficulty01(0);
            Assert.That(previous, Is.InRange(0f, 1f));

            for (int score = 1; score <= 200; score++)
            {
                float current = GameTuning.Difficulty01(score);
                Assert.That(current, Is.InRange(0f, 1f));
                Assert.That(current, Is.GreaterThanOrEqualTo(previous));
                previous = current;
            }
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
            Assert.That(GameTuning.HasHazard(7), Is.True);
            Assert.That(GameTuning.HasHazard(8), Is.False);
            Assert.That(GameTuning.HasHazard(10), Is.True);
        }

        [TestCase(0)]
        [TestCase(12)]
        [TestCase(40)]
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
            Assert.That(GameTuning.CanAddSkipChallenge(5, 20), Is.True);
            Assert.That(GameTuning.CanAddSkipChallenge(99, 8), Is.False);
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
    }
}
