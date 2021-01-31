using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class TechFieldTest
    {

        [Test]
        public void TestLowest()
        {
            TechLevel level = new TechLevel();
            Assert.AreEqual(TechField.Energy, level.Lowest());

            level = new TechLevel(6, 5, 4, 3, 2, 1);
            Assert.AreEqual(TechField.Biotechnology, level.Lowest());
        }

    }

}