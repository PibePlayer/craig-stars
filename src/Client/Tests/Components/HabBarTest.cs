using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class HabBarTest
    {
        [Test]
        public void TestGetGravityString()
        {
            Assert.AreEqual("1.00g", HabBar.GetGravString(50));
        }

        [Test]
        public void TestGetTempString()
        {
            Assert.AreEqual("0°C", HabBar.GetTempString(50));
        }
        [Test]
        public void TestGetRadString()
        {
            Assert.AreEqual("50mR", HabBar.GetRadString(50));
        }

    }
}