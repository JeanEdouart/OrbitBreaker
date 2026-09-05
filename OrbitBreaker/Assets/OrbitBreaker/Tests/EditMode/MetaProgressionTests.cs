using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace OrbitBreaker.Tests
{
    public sealed class MetaProgressionTests
    {
        [Test]
        public void ChallengeCatalog_ContainsOneHundredValidChallenges()
        {
            var labels = new HashSet<string>();
            for (int id = 0; id < 100; id++)
            {
                ChallengeDefinition challenge = MetaProgression.Challenge(id);
                Assert.That(challenge.Id, Is.EqualTo(id));
                Assert.That(challenge.Target, Is.GreaterThan(0));
                Assert.That(challenge.Reward, Is.GreaterThan(0));
                Assert.That(challenge.Label, Is.Not.Empty);
                labels.Add(challenge.Label);
            }
            Assert.That(labels.Count, Is.EqualTo(100));
        }

        [Test]
        public void CosmeticCatalog_HasEveryPromisedCategory()
        {
            Assert.That(MetaProgression.Catalog.Count(item => item.Kind == CosmeticKind.Rocket), Is.EqualTo(10));
            Assert.That(MetaProgression.Catalog.Count(item => item.Kind == CosmeticKind.Trail), Is.GreaterThanOrEqualTo(4));
            Assert.That(MetaProgression.Catalog.Count(item => item.Kind == CosmeticKind.PlanetPack), Is.EqualTo(3));
            Assert.That(MetaProgression.Catalog.Count(item => item.Kind == CosmeticKind.Background), Is.EqualTo(3));
            Assert.That(MetaProgression.Catalog.Where(item => item.Price == 0).Select(item => item.Kind).Distinct().Count(), Is.EqualTo(4));
        }
    }
}
