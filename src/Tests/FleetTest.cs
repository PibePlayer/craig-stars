using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetTest
    {
        [Test]
        public void TestGetPeers()
        {
            var f = new Fleet();
            Assert.AreEqual(0, f.GetPeers().Count);

            var p = new Planet();
            f.Orbiting = p;
            p.OrbitingFleets.Add(f);
            Assert.AreEqual(1, f.GetPeers().Count);

            var f2 = new Fleet();
            p.OrbitingFleets.Add(f2);
            Assert.AreEqual(2, f.GetPeers().Count);

            // this fleet is before, should not count towards peers
            var f3 = new Fleet();
            p.OrbitingFleets.Insert(0, f3);
            Assert.AreEqual(2, f.GetPeers().Count);
        }
    }
}