using CraigStars.Utils;
using Godot;

namespace CraigStars.Tests
{
    public class TextUtilsTest : WAT.Test
    {
        [Test]
        public void TestGetGravityString()
        {
            Assert.IsEqual("1.00g", TextUtils.GetGravString(50));
        }

        [Test]
        public void TestGetTempString()
        {
            Assert.IsEqual("0°C", TextUtils.GetTempString(50));
        }
        [Test]
        public void TestGetRadString()
        {
            Assert.IsEqual("50mR", TextUtils.GetRadString(50));
        }

    }
}