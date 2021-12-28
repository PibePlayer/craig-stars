using System;
using System.Collections.Generic;
using CraigStars.Utils;
using Godot;
using NUnit.Framework;
using static CraigStars.Utils.Utils;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TextUtilsTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(TextUtilsTest));

        [Test]
        public void TestGetGravString()
        {
            Assert.AreEqual("1.00g", TextUtils.GetGravString(50));
            Assert.AreEqual("8.00g", TextUtils.GetGravString(100));
            Assert.AreEqual("0.12g", TextUtils.GetGravString(0));
        }

    }
}