using CraigStars.Singletons;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetSpriteTest
    {
        [Test]
        public void TestGetPeers()
        {
            var p = new PlanetSprite();
            p.Planet = new Planet()
            {
                Player = PlayersManager.Instance.Me
            };
            Assert.AreEqual(0, p.GetPeers().Count);

            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                Player = PlayersManager.Instance.Me
            };
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            Assert.AreEqual(1, p.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Instance.Me
            };
            p.OrbitingFleets.Add(f2);
            Assert.AreEqual(2, p.GetPeers().Count);
        }
    }
}