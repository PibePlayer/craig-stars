using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerInfoTest
    {
        [Test]
        public void TestToString()
        {
            var playerInfo = new PlayerInfo() { Num = 0, Name = "Craig" };
            Assert.AreEqual("Player 1 - Craig", playerInfo.ToString());

            playerInfo.RacePluralName = "Humanoids";
            Assert.AreEqual("Player 1 - Craig Humanoids", playerInfo.ToString());
        }
    }

}