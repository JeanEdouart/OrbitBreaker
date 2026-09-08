using NUnit.Framework;
using UnityEngine;

namespace OrbitBreaker.Tests
{
    public sealed class ResurrectionTests
    {
        [Test]
        public void CyberpunkUsesStandaloneGeneratedTextures()
        {
            var assets = typeof(OrbitPlayer).Assembly.GetType("OrbitBreaker.RuntimeAssets");
            Sprite rocket = (Sprite)assets.GetMethod("GetRocketSprite").Invoke(null, new object[] {31});
            Sprite background = (Sprite)assets.GetMethod("GetBackgroundSprite").Invoke(null, new object[] {16});
            Assert.That(rocket.texture, Is.SameAs(Resources.Load<Texture2D>("Art/cyberpunk-rocket")));
            Assert.That(background.texture, Is.SameAs(Resources.Load<Texture2D>("Art/cyberpunk-background")));
            for (int i = 0; i < 5; i++)
                Assert.That(((Sprite)assets.GetMethod("GetPlanetPackSprite").Invoke(null, new object[] {16, i})).texture, Is.SameAs(Resources.Load<Texture2D>("Art/cyberpunk-planet-" + i)));
        }

        [Test]
        public void ResurrectionReturnsToOrbitWithoutCaptureReward()
        {
            var shipObject = new GameObject("Resurrection Test Ship");
            var orbitObject = new GameObject("Resurrection Test Orbit");
            try
            {
                var ship = shipObject.AddComponent<OrbitPlayer>();
                ship.Initialize();
                var orbit = orbitObject.AddComponent<OrbitAnchor>();
                orbit.Initialize(0, Vector2.zero, 2f, 1);
                bool rewarded = false;
                ship.Captured += _ => rewarded = true;
                ship.BeginResurrection();
                ship.PoseResurrection(Vector2.down * 2f, Quaternion.identity, 1f);
                ship.CompleteResurrection(orbit);
                Assert.That(ship.State, Is.EqualTo(PlayerOrbitState.Orbiting));
                Assert.That(ship.CurrentAnchor, Is.SameAs(orbit));
                Assert.That(rewarded, Is.False);
                Assert.That(ship.transform.localScale, Is.EqualTo(Vector3.one));
            }
            finally
            {
                Object.DestroyImmediate(shipObject);
                Object.DestroyImmediate(orbitObject);
            }
        }
    }
}
