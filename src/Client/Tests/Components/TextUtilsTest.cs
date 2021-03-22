using CraigStars.Utils;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TextUtilsTest
    {
        [Test]
        public void TestGetGravityString()
        {
            Assert.AreEqual("1.00g", TextUtils.GetGravString(50));
        }

        [Test]
        public void TestGetTempString()
        {
            Assert.AreEqual("0°C", TextUtils.GetTempString(50));
        }
        [Test]
        public void TestGetRadString()
        {
            Assert.AreEqual("50mR", TextUtils.GetRadString(50));
        }

    }
}