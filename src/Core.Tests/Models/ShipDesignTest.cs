using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class ShipDesignTest
    {
        Rules rules = new Rules();

        [Test]
        public void TestIsValid()
        {
            var design = new ShipDesign { Hull = Techs.Scout };
            Assert.IsFalse(design.IsValid());

            design.Slots.Add(new ShipDesignSlot(Techs.LongHump6, 1, 1));
            Assert.IsTrue(design.IsValid());
        }

        [Test]
        public void TestIsValidManyEngines()
        {
            // this design requires two engines, make sure it is only valid with two
            var design = new ShipDesign { Hull = Techs.LargeFreighter };
            Assert.IsFalse(design.IsValid());

            design.Slots.Add(new ShipDesignSlot(Techs.LongHump6, 1, 1));
            Assert.IsFalse(design.IsValid());

            design.Slots[0].Quantity = 2;
            Assert.IsTrue(design.IsValid());
        }

    }

}