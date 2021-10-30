using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetCompositionTest
    {
        [Test]
        public void TestGetQuantityByPurpose()
        {
            FleetComposition fleetComposition = new();

            var quantityByPurpose = fleetComposition.GetQuantityByPurpose();
            Assert.AreEqual(0, quantityByPurpose.Count);

            fleetComposition = new()
            {
                Type = FleetCompositionType.Bomber,
                Tokens = new()
                {
                    new FleetCompositionToken(ShipDesignPurpose.Bomber, 2),
                    new FleetCompositionToken(ShipDesignPurpose.FuelFreighter, 1),
                }
            };

            quantityByPurpose = fleetComposition.GetQuantityByPurpose();
            Assert.AreEqual(2, quantityByPurpose.Count);
            Assert.AreEqual(ShipDesignPurpose.Bomber, quantityByPurpose[ShipDesignPurpose.Bomber].Purpose);
            Assert.AreEqual(2, quantityByPurpose[ShipDesignPurpose.Bomber].Quantity);
            Assert.AreEqual(ShipDesignPurpose.FuelFreighter, quantityByPurpose[ShipDesignPurpose.FuelFreighter].Purpose);
            Assert.AreEqual(1, quantityByPurpose[ShipDesignPurpose.FuelFreighter].Quantity);

        }

    }

}