using NUnit.Framework;

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
        public void PerfectCapture_IsWorthMoreThanLooseCapture()
        {
            int perfect = GameTuning.PointsForCapture(0.1f, 3);
            int loose = GameTuning.PointsForCapture(0.9f, 0);
            Assert.That(perfect, Is.GreaterThan(loose));
        }

        [Test]
        public void ComboBonus_IsCapped()
        {
            int capped = GameTuning.PointsForCapture(0.5f, 10);
            int excessive = GameTuning.PointsForCapture(0.5f, 999);
            Assert.That(excessive, Is.EqualTo(capped));
        }
    }
}
