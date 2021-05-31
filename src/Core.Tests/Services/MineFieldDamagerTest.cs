using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Threading.Tasks;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class MineFieldDamagerTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldDamagerTest));

        MineFieldDamager mineFieldDamager = new MineFieldDamager();

        [Test]
        public void TestTakeMineFieldDamage()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player1 = game.Players[0];

            var player2 = new Player()
            {
                Num = 1,
            };
            game.Players.Add(player2);

            game.MineFields.Add(new MineField()
            {
                Player = player2,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = ShipDesigns.StalwartDefender.Clone();
            design.Player = player1;
            var fleet = new Fleet()
            {
                Player = game.Players[0],
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 10)
                },
            };
            fleet.ComputeAggregate();

            FleetMoveStep step = new FleetMoveStep(game);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleetRS = 600,
                DamagePerEngineRS = 125,
                MinDamagePerFleet = 500,
                DamagePerEngine = 100,
            };

            // should do 100 damage per engine (only 1 engine per ship)
            // 1000 damage will destroy 2 ships (350 armor each), and leave 300 damage for the rest
            mineFieldDamager.TakeMineFieldDamage(fleet, game.MineFields[0], stats);
            Assert.AreEqual(8, fleet.Tokens[0].Quantity);
            Assert.AreEqual(8, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(300, fleet.Tokens[0].Damage);

            // reset this token damage
            fleet.Tokens[0].Quantity = 10;
            fleet.Tokens[0].QuantityDamaged = 0;
            fleet.Tokens[0].Damage = 0;

            // give it a ramscoop
            fleet.Tokens[0].Design.Slots[0].HullComponent = Techs.RadiatingHydroRamScoop;
            fleet.Tokens[0].Design.ComputeAggregate(fleet.Player, true);
            fleet.ComputeAggregate(true);
            // should do 125 damage per engine (only 1 engine per ship)
            // 1250 damage will destroy 3 ships (350 armor each), and leave 200 damage for the rest
            mineFieldDamager.TakeMineFieldDamage(fleet, game.MineFields[0], stats);
            Assert.AreEqual(7, fleet.Tokens[0].Quantity);
            Assert.AreEqual(7, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(200, fleet.Tokens[0].Damage);
        }

        [Test]
        public void TestHitMineFieldMinDamage()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player1 = game.Players[0];

            var player2 = new Player()
            {
                Num = 1,
            };
            game.Players.Add(player2);

            game.MineFields.Add(new MineField()
            {
                Player = player2,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = ShipDesigns.StalwartDefender.Clone();
            design.Player = player1;
            var fleet = new Fleet()
            {
                Player = game.Players[0],
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
            };
            fleet.ComputeAggregate();

            FleetMoveStep step = new FleetMoveStep(game);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleet = 200,
                DamagePerEngine = 100,
            };

            // should do 200 damage per engine per ship (only 1 engine per ship)
            mineFieldDamager.TakeMineFieldDamage(fleet, game.MineFields[0], stats);
            Assert.AreEqual(1, fleet.Tokens[0].Quantity);
            Assert.AreEqual(1, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(200, fleet.Tokens[0].Damage);
        }

        [Test]
        public void TestHitMineFieldMinDamageDiverseFleet()
        {
            var game = TestUtils.GetSingleUnitGame();
            var player1 = game.Players[0];

            var player2 = new Player()
            {
                Num = 1,
            };
            game.Players.Add(player2);

            game.MineFields.Add(new MineField()
            {
                Player = player2,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with a scout and a large freighter (with 2 engines)
            var scout = ShipDesigns.LongRangeScount.Clone();
            scout.Player = player1;
            var largeFrieghter = new ShipDesign()
            {
                Hull = Techs.LargeFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 2),
                    new ShipDesignSlot(Techs.Crobmnium, 3, 2)
                }
            };
            largeFrieghter.Player = player1;

            var fleet = new Fleet()
            {
                Player = game.Players[0],
                Tokens = new List<ShipToken>() {
                    new ShipToken(scout, 1),
                    new ShipToken(largeFrieghter, 1)
                },
            };
            fleet.ComputeAggregate();

            FleetMoveStep step = new FleetMoveStep(game);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleet = 200,
                DamagePerEngine = 100,
            };

            // should do 200 damage to the scout, destroying it, and 100 damage to the large
            // freighter for the extra damager for an extra engine
            mineFieldDamager.TakeMineFieldDamage(fleet, game.MineFields[0], stats);
            Assert.AreEqual(1, fleet.Tokens.Count);
            Assert.AreEqual(Techs.LargeFreighter, fleet.Tokens[0].Design.Hull);
            Assert.AreEqual(1, fleet.Tokens[0].Quantity);
            Assert.AreEqual(1, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(100, fleet.Tokens[0].Damage);
        }
    }
}