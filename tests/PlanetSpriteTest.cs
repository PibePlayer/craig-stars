using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client.Tests
{
    public class PlanetSpriteTest : WAT.Test
    {
        public override void Pre()
        {
            PlayersManager.Me = PlayersManager.CreateNewPlayer(0);
        }

        [Test]
        public void TestGetPeers()
        {
            var p = new PlanetSprite();
            p.Planet = new Planet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            Assert.IsEqual(0, p.GetPeers().Count);

            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            Assert.IsEqual(1, p.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            p.OrbitingFleets.Add(f2);
            Assert.IsEqual(2, p.GetPeers().Count);
        }
    }
}