using System;
using System.Collections.Generic;
using CraigStars;
using CraigStars.Singletons;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetBombStepTest
    {
        FleetBombStep step;
        Game game;
        GameRunner gameRunner;

        [SetUp]
        public void SetUp()
        {
            PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
            (game, gameRunner) = TestUtils.GetTwoPlayerGame();
            var fleetOwner = game.Players[1];
            fleetOwner.PlayerRelations[0].Relation = PlayerRelation.Enemy;

            step = new FleetBombStep(gameRunner.GameProvider, planetService);
        }

        [Test]
        public void TestGetColonistsKilled()
        {
            // 10 cherry bombs
            var killRate = 2.5f;
            var quantity = 10;

            // no defense
            var defenseCoverage = 0f;

            // no defense, 10 bombs should kill 25%
            Assert.AreEqual(2500, step.GetColonistsKilled(10000, defenseCoverage, killRate, quantity));

            // 100 nuetron defs, 10 bombs should kill ~100 rounded up
            defenseCoverage = .9792f;
            Assert.AreEqual(52, step.GetColonistsKilled(10000, defenseCoverage, killRate, quantity), .1f);
        }

        [Test]
        public void TestGetUnterraformAmount()
        {
            var baseHab = new Hab(20, 70, 40);
            var hab = new Hab(50, 50, 50);

            // should unterraform 1 gravity because it's 30 points from the base
            Assert.AreEqual(new Hab(-1, 0, 0), step.GetUnterraformAmount(1, baseHab, hab));

            // if we bomb for -30 terraform points, we should equalize between the two highest
            Assert.AreEqual(new Hab(-20, 10, 0), step.GetUnterraformAmount(30, baseHab, hab));

            // if we bomb for -36 terraform points, we should equalize between all three
            Assert.AreEqual(new Hab(-22, 12, -2), step.GetUnterraformAmount(36, baseHab, hab));

            // no unterraform, planet isn't terraformed
            baseHab = new Hab(50, 50, 50);
            hab = new Hab(50, 50, 50);
            Assert.AreEqual(new Hab(0, 0, 0), step.GetUnterraformAmount(10, baseHab, hab));

            // unterraform by one, even though we bombed with 10
            baseHab = new Hab(50, 50, 50);
            hab = new Hab(50, 51, 50);
            Assert.AreEqual(new Hab(0, -1, 0), step.GetUnterraformAmount(10, baseHab, hab));

        }

        [Test]
        public void TestGetColonistsKilledWithBombs()
        {
            // 10 cherry bombs, 5 M-70 bombs
            List<Bomb> bombs = new List<Bomb>() {
                new Bomb() { KillRate = 2.5f, Quantity = 10 },
                new Bomb() { KillRate = 1.2f, Quantity = 5 },
            };

            var defenseCoverage = .9792f;

            Assert.AreEqual(64.48f, step.GetColonistsKilled(10000, defenseCoverage, bombs), .1f);
        }

        [Test]
        public void TestGetColonistsKilledWithSmartBombs()
        {
            // 10 Annihilators + 5 Neutron bombs
            List<Bomb> bombs = new List<Bomb>() {
                new Bomb() { KillRate = 7f, Quantity = 10 },
                new Bomb() { KillRate = 2.2f, Quantity = 5 },
            };

            var defenseCoverage = .8524f;

            Assert.AreEqual(837, step.GetColonistsKilledWithSmartBombs(10000, defenseCoverage, bombs), .2f);
        }

        [Test]
        public void TestGetStructuresDestroyed()
        {
            // 10 cherry bombs, 5 M-70 bombs
            List<Bomb> bombs = new List<Bomb>() {
                new Bomb() { StructureDestroyRate = 1.0f, Quantity = 10 },
                new Bomb() { StructureDestroyRate = .6f, Quantity = 5 },
            };

            var defenseCoverage = .9792f;

            Assert.AreEqual(66f, step.GetStructuresDestroyed(defenseCoverage, bombs), .5f);
        }

        [Test]
        public void TestWillNotBombPlanet()
        {
            var planet = game.Planets[0];
            var planetOwner = game.Players[0];
            var fleetOwner = game.Players[1];

            planet.Population = 10_000;
            planet.Mines = 100;
            planet.Factories = 100;
            planet.Defenses = 10;

            // one mini-bomber
            var design = TestUtils.CreateDesign(game, fleetOwner, ShipDesigns.MiniBomber);
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Mini-Bomber #2",
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                }
            };
            game.AddMapObject(fleet);

            gameRunner.ComputeSpecs(recompute: true);

            fleet.Orbiting = planet;

            step.BombPlanet(planet);

            Assert.AreEqual(10000, planet.Population);
            Assert.AreEqual(100, planet.Mines);
            Assert.AreEqual(100, planet.Factories);
            Assert.AreEqual(10, planet.Defenses);
        }

        [Test]
        public void TestBombPlanet()
        {
            var planet = game.Planets[0];
            var planetOwner = game.Players[0];
            var fleetOwner = game.Players[1];


            planet.Population = 10_000;
            planet.Mines = 100;
            planet.Factories = 100;
            planet.Defenses = 10;
            planet.Starbase = null;

            // one mini-bomber
            var design = TestUtils.CreateDesign(game, fleetOwner, ShipDesigns.MiniBomber);
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Mini-Bomber #2",
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                }
            };
            game.AddMapObject(fleet);

            gameRunner.ComputeSpecs(recompute: true);

            fleet.Orbiting = planet;

            step.BombPlanet(planet);

            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
            Assert.AreEqual(9500, planet.Population);
            Assert.AreEqual(98, planet.Mines);
            Assert.AreEqual(98, planet.Factories);
            Assert.AreEqual(9, planet.Defenses);
        }

        [Test]
        public void TestBombPlanet2()
        {
            var planet = game.Planets[0];
            var planetOwner = game.Players[0];
            var fleetOwner = game.Players[1];


            planet.Population = 10_000;
            planet.Mines = 100;
            planet.Factories = 100;
            planet.Defenses = 10;
            planet.Starbase = null;

            // one mini-bomber
            var design = TestUtils.CreateDesign(game, fleetOwner, ShipDesigns.MiniBomber);
            var fleet1 = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Mini-Bomber #2",
                Tokens = new List<ShipToken>() {
                    new ShipToken(design, 1)
                }
            };
            game.AddMapObject(fleet1);

            // one mini-bomber with smart bombs
            var fleet2 = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Mini-Bomber #2",
                Tokens = new List<ShipToken>() {
                    new ShipToken() {
                        Design = new ShipDesign() {
                            PlayerNum = fleetOwner.Num,
                            Hull = Techs.MiniBomber,
                            Slots = new List<ShipDesignSlot>() {
                                new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                                new ShipDesignSlot(Techs.SmartBomb, 2, 2),
                            },
                        },
                        Quantity = 1
                    }
                }
            };

            game.Designs.Add(fleet2.Tokens[0].Design);
            game.AddMapObject(fleet2);
            gameRunner.ComputeSpecs(recompute: true);

            fleet1.Orbiting = planet;
            fleet2.Orbiting = planet;

            step.BombPlanet(planet);

            // bomb with smart bombs
            Assert.AreEqual(2, planetOwner.Messages.Count);
            Assert.AreEqual(2, fleetOwner.Messages.Count);
            Assert.AreEqual(98, planet.Mines);
            Assert.AreEqual(98, planet.Factories);
            Assert.AreEqual(9, planet.Defenses);
            Assert.AreEqual(9300, planet.Population);
        }


        [Test]
        public void TestRetroBombPlanet()
        {
            var planet = game.Planets[0];
            var planetOwner = game.Players[0];
            var fleetOwner = game.Players[1];

            // create a terraformed planet
            planet.Population = 10_000;
            planet.BaseHab = new Hab(47, 50, 50);
            planet.Hab = new Hab(50, 50, 50);
            planet.TerraformedAmount = new Hab(3, 0, 0);
            planet.Starbase = null;

            // one mini-bomber with retro-bomb
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Name = "Mini-Bomber #1",
                Tokens = new List<ShipToken>() {
                    new ShipToken() {
                        Design = new ShipDesign() {
                            PlayerNum = fleetOwner.Num,
                            Hull = Techs.MiniBomber,
                            Slots = new List<ShipDesignSlot>() {
                                new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                                new ShipDesignSlot(Techs.RetroBomb, 2, 2),
                            },
                        },
                        Quantity = 1
                    }
                },
                Orbiting = planet,
                Position = planet.Position
            };

            game.Designs.Add(fleet.Tokens[0].Design);
            game.AddMapObject(fleet);

            gameRunner.ComputeSpecs(recompute: true);


            step.BombPlanet(planet);

            Assert.AreEqual(new Hab(48, 50, 50), planet.Hab);
            Assert.AreEqual(new Hab(1, 0, 0), planet.TerraformedAmount);
            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
        }

        [Test]
        public void TestOrbitalConstructionModuleBombPlanet()
        {
            var planet = game.Planets[0];
            var planetOwner = game.Players[0];
            var fleetOwner = game.Players[1];

            planet.Population = 10_000;
            planet.Mines = 100;
            planet.Factories = 100;
            planet.Defenses = 10;
            planet.Starbase = null;

            // one colonizer with orbital construction module
            var colonizer = TestUtils.CreateDesign(game, fleetOwner, ShipDesigns.SantaMaria);
            colonizer.Slots[1].HullComponent = Techs.OrbitalConstructionModule;
            var fleet = new Fleet()
            {
                PlayerNum = fleetOwner.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(colonizer, 1)
                },
                Cargo = new Cargo(colonists: 25)
            };
            game.AddMapObject(fleet);

            gameRunner.ComputeSpecs(recompute: true);

            fleet.Orbiting = planet;

            step.BombPlanet(planet);

            Assert.AreEqual(1, planetOwner.Messages.Count);
            Assert.AreEqual(1, fleetOwner.Messages.Count);
            Assert.AreEqual(8200, planet.Population);
            Assert.AreEqual(100, planet.Mines);
            Assert.AreEqual(100, planet.Factories);
            Assert.AreEqual(10, planet.Defenses);
        }
    }
}