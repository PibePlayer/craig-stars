using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class MaterialTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Test1()
        {
            var m = new Mineral(1, 2, 3);
            Assert.AreEqual(1, m.Ironium);
            Assert.AreEqual(2, m.Boranium);
            Assert.AreEqual(3, m.Germanium);
        }

    }
}