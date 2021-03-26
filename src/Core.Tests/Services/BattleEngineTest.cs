using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class BattleEngineTest
    {
        BattleEngine battleEngine = new BattleEngine();

        [Test]
        public void TestWillTarget()
        {
            BattleToken token = new BattleToken()
            {
                Attributes = BattleTokenAttribute.Armed | BattleTokenAttribute.Starbase
            };

            // Armed starbases are targeted by Any, Starbase, and ArmedShip
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.Any, token));
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.Starbase, token));
            Assert.IsTrue(battleEngine.WillTarget(BattleTargetType.ArmedShip, token));

            // unarmed, none, freighters, and bomber targets won't work on this token
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.None, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.UnarmedShips, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.Freighters, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.BombersFreighters, token));
            Assert.IsFalse(battleEngine.WillTarget(BattleTargetType.FuelTransports, token));
        }
    }

}