using CraigStars.Singletons;
using Godot;

namespace CraigStars.Tests
{
    public class FleetSpriteTest : WAT.Test
    {
        public override void Pre()
        {
            PlayersManager.Instance.CreatePlayersForNewGame();
        }

        [Test]
        public void TestGetPeers()
        {
            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                Player = PlayersManager.Me
            };
            Assert.IsEqual(0, f.GetPeers().Count);

            var p = new PlanetSprite();
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            p.Planet = new Planet()
            {
                Player = PlayersManager.Me
            };
            Assert.IsEqual(1, f.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Me
            };
            p.OrbitingFleets.Add(f2);
            Assert.IsEqual(2, f.GetPeers().Count);

            // this fleet is before, should not count towards peers
            var f3 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                Player = PlayersManager.Me
            };
            p.OrbitingFleets.Insert(0, f3);
            Assert.IsEqual(2, f.GetPeers().Count);
        }
    }
}