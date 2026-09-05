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
    }
}
