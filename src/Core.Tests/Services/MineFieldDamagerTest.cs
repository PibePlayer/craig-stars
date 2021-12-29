using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class MineFieldDamagerTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldDamagerTest));

        IRulesProvider rulesProvider = new TestRulesProvider();
        PlayerService playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
        MineFieldDamager mineFieldDamager = TestUtils.TestContainer.GetInstance<MineFieldDamager>();
        ShipDesignDiscoverer designDiscoverer = TestUtils.TestContainer.GetInstance<ShipDesignDiscoverer>();
        FleetService fleetService = TestUtils.TestContainer.GetInstance<FleetService>();
        FleetSpecService fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();

        [Test]
        public void TestTakeMineFieldDamage()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            game.AddMapObject(new MineField()
            {
                PlayerNum = player2.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = ShipDesigns.StalwartDefender.Clone();
            design.PlayerNum = player1.Num;
            game.Designs.Add(design);
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 10)
                },
            };
            game.AddMapObject(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleetRS = 600,
                DamagePerEngineRS = 125,
                MinDamagePerFleet = 500,
                DamagePerEngine = 100,
            };

            // should do 100 damage per engine (only 1 engine per ship)
            // 1000 damage will destroy 2 ships (350 armor each), and leave 300 damage for the rest
            mineFieldDamager.TakeMineFieldDamage(fleet, player1, game.MineFields[0], player2, stats);
            Assert.AreEqual(8, fleet.Tokens[0].Quantity);
            Assert.AreEqual(8, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(300, fleet.Tokens[0].Damage);

            // reset this token damage
            fleet.Tokens[0].Quantity = 10;
            fleet.Tokens[0].QuantityDamaged = 0;
            fleet.Tokens[0].Damage = 0;

            // give it a ramscoop
            fleet.Tokens[0].Design.Slots[0].HullComponent = Techs.RadiatingHydroRamScoop;
            gameRunner.ComputeSpecs(recompute: true);

            // should do 125 damage per engine (only 1 engine per ship)
            // 1250 damage will destroy 3 ships (350 armor each), and leave 200 damage for the rest
            mineFieldDamager.TakeMineFieldDamage(fleet, player1, game.MineFields[0], player2, stats);
            Assert.AreEqual(7, fleet.Tokens[0].Quantity);
            Assert.AreEqual(7, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(200, fleet.Tokens[0].Damage);
        }

        [Test]
        public void TestHitMineFieldMinDamage()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            game.AddMapObject(new MineField()
            {
                PlayerNum = player2.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with 10 stalwart defenders (350 armor each)
            var design = ShipDesigns.StalwartDefender.Clone();
            design.PlayerNum = player1.Num;
            game.Designs.Add(design);
            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                },
            };
            game.AddMapObject(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleet = 200,
                DamagePerEngine = 100,
            };

            // should do 200 damage per engine per ship (only 1 engine per ship)
            mineFieldDamager.TakeMineFieldDamage(fleet, player1, game.MineFields[0], player2, stats);
            Assert.AreEqual(1, fleet.Tokens[0].Quantity);
            Assert.AreEqual(1, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(200, fleet.Tokens[0].Damage);
        }

        [Test]
        public void TestHitMineFieldMinDamageDiverseFleet()
        {
            var (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var player1 = game.Players[0];
            var player2 = game.Players[1];

            game.AddMapObject(new MineField()
            {
                PlayerNum = player2.Num,
                Type = MineFieldType.Standard,
                Position = new Vector2(0, 0),
                NumMines = 100,
            });

            // make a new fleet with a scout and a large freighter (with 2 engines)
            var scout = ShipDesigns.LongRangeScount.Clone();
            scout.PlayerNum = player1.Num;
            game.Designs.Add(scout);
            var largeFrieghter = new ShipDesign()
            {
                Hull = Techs.LargeFreighter,
                Slots = new List<ShipDesignSlot>() {
                    new ShipDesignSlot(Techs.QuickJump5, 1, 2),
                    new ShipDesignSlot(Techs.Crobmnium, 3, 2)
                }
            };
            largeFrieghter.PlayerNum = player1.Num;
            game.Designs.Add(largeFrieghter);

            var fleet = new Fleet()
            {
                PlayerNum = player1.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(scout, 1),
                    new ShipToken(largeFrieghter, 1)
                },
            };
            game.AddMapObject(fleet);
            gameRunner.ComputeSpecs(recompute: true);

            FleetMoveStep step = new FleetMoveStep(gameRunner.GameProvider, rulesProvider, mineFieldDamager, designDiscoverer, fleetService, playerService);

            MineFieldStats stats = new MineFieldStats()
            {
                MinDamagePerFleet = 200,
                DamagePerEngine = 100,
            };

            // should do 200 damage to the scout, destroying it, and 100 damage to the large
            // freighter for the extra damager for an extra engine
            mineFieldDamager.TakeMineFieldDamage(fleet, player1, game.MineFields[0], player2, stats);
            Assert.AreEqual(1, fleet.Tokens.Count);
            Assert.AreEqual(Techs.LargeFreighter, fleet.Tokens[0].Design.Hull);
            Assert.AreEqual(1, fleet.Tokens[0].Quantity);
            Assert.AreEqual(1, fleet.Tokens[0].QuantityDamaged);
            Assert.AreEqual(100, fleet.Tokens[0].Damage);
        }
    }
}