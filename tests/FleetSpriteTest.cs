using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client.Tests
{
    public class FleetSpriteTest : WAT.Test
    {
        public override void Pre()
        {
            PlayersManager.Me = PlayersManager.CreateNewPlayer(0);
        }

        [Test]
        public void TestGetPeers()
        {
            var f = new FleetSprite();
            f.Fleet = new Fleet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            Assert.IsEqual(0, f.GetPeers().Count);

            var p = new PlanetSprite();
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            p.Planet = new Planet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            Assert.IsEqual(1, f.GetPeers().Count);

            var f2 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            p.OrbitingFleets.Add(f2);
            Assert.IsEqual(2, f.GetPeers().Count);

            // this fleet is before, should not count towards peers
            var f3 = new FleetSprite();
            f2.Fleet = new Fleet()
            {
                PlayerNum = PlayersManager.Me.Num
            };
            p.OrbitingFleets.Insert(0, f3);
            Assert.IsEqual(2, f.GetPeers().Count);
        }
    }
}