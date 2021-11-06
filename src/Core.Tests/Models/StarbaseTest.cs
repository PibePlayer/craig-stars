using System.Collections.Generic;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class StarbaseTest
    {
        Rules rules = new Rules(0);

        [Test]
        public void TestComputeAggregate()
        {
            // make a starbase with two mass drivers
            var player = new Player();
            var design = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Death Star",
                Hull = Techs.DeathStar,
                HullSetNumber = 0,
                Slots = new List<ShipDesignSlot>()
                {
                    new ShipDesignSlot(Techs.MassDriver7, 1, 1),
                    new ShipDesignSlot(Techs.MassDriver7, 11, 1),
                }
            };

            var starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                }
            };

            starbase.ComputeAggregate(player);

            Assert.AreEqual(7, starbase.Aggregate.BasePacketSpeed);
            Assert.AreEqual(8, starbase.Aggregate.SafePacketSpeed);
        }
    }

}