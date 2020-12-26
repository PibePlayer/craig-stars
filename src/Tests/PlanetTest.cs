using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetTest
    {
        [Test]
        public void TestGetPeers()
        {
            var p = new Planet();
            Assert.AreEqual(0, p.GetPeers().Count);

            var f = new Fleet();
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            Assert.AreEqual(1, p.GetPeers().Count);

            var f2 = new Fleet();
            p.OrbitingFleets.Add(f2);
            Assert.AreEqual(2, p.GetPeers().Count);
        }
    }
}