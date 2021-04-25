using CraigStars.Singletons;
using Godot;

namespace CraigStars.Tests
{
    public class PlanetSpriteTest : WAT.Test
    {
        public override void Pre()
        {
            PlayersManager.Instance.SetupPlayers();
        }

        [Test]
        public void TestGetPeers()
        {
            var p = new PlanetSprite();
            p.Planet = new Planet()
            {
                Player = PlayersManager.Me
            };
            Assert.IsEqual(0, p.GetPeers().Count);

            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                Player = PlayersManager.Me
            };
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            Assert.IsEqual(1, p.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Me
            };
            p.OrbitingFleets.Add(f2);
            Assert.IsEqual(2, p.GetPeers().Count);
        }
    }
}