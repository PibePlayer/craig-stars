using CraigStars.Singletons;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetSpriteTest
    {
        [SetUp]
        public void SetUp()
        {
            PlayersManager.Instance.SetupPlayers();
        }

        [Test]
        public void TestGetPeers()
        {
            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                Player = PlayersManager.Instance.Me
            };
            Assert.AreEqual(0, f.GetPeers().Count);

            var p = new PlanetSprite();
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            p.Planet = new Planet()
            {
                Player = PlayersManager.Instance.Me
            };
            Assert.AreEqual(1, f.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Instance.Me
            };
            p.OrbitingFleets.Add(f2);
            Assert.AreEqual(2, f.GetPeers().Count);

            // this fleet is before, should not count towards peers
            var f3 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Instance.Me
            };
            p.OrbitingFleets.Insert(0, f3);
            Assert.AreEqual(2, f.GetPeers().Count);
        }
    }
}