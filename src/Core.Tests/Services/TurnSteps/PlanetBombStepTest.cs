using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlanetBombStepTest
    {
        PlanetBombStep planetBomber = new PlanetBombStep(GameTest.GetSingleUnitGame(), TurnGeneratorState.Bomb);

        [Test]
        public void TestGetColonistsKilled()
        {
            // 10 cherry bombs
            var killRate = 2.5f;
            var quantity = 10;

            // no defense
            var defenseCoverage = 0f;

            // no defense, 10 bombs should kill 25%
            Assert.AreEqual(2500, planetBomber.GetColonistsKilled(10000, defenseCoverage, killRate, quantity));

            // 100 nuetron defs, 10 bombs should kill ~100 rounded up
            defenseCoverage = .9792f;
            Assert.AreEqual(52, planetBomber.GetColonistsKilled(10000, defenseCoverage, killRate, quantity), .1f);
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

            Assert.AreEqual(64.48f, planetBomber.GetColonistsKilled(10000, defenseCoverage, bombs), .1f);
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

            Assert.AreEqual(837, planetBomber.GetColonistsKilledWithSmartBombs(10000, defenseCoverage, bombs), .2f);
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

            Assert.AreEqual(66f, planetBomber.GetStructuresDestroyed(defenseCoverage, bombs), .5f);
        }

        [Test]
        public void TestBombPlanet()
        {
            var planetOwner = new Player()
            {
                Num = 0
            };
            var fleetOwner = new Player()
            {
                Num = 1
            };

            var planet = new Planet()
            {
                Name = "Brin",
                Player = planetOwner,
                Population = 10000,
                Mines = 100,
                Factories = 100,
                Defenses = 10,
            };

            // one mini-bomber
            var fleet = new Fleet()
            {
                Player = fleetOwner,
                Name = "Mini-Bomber #1",
                Tokens = new List<ShipToken>() {
                    new ShipToken() {
                        Design = new ShipDesign() {
                            Player = fleetOwner,
                            Hull = Techs.MiniBomber,
                            Slots = new List<ShipDesignSlot>() {
                                new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                                new ShipDesignSlot(Techs.LadyFingerBomb, 2, 2),
                            },
                        },
                        Quantity = 1
                    }
                }
            };

            fleet.ComputeAggregate();

            fleet.Orbiting = planet;
            planet.OrbitingFleets.Add(fleet);

            planetBomber.BombPlanet(planet);

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
            var planetOwner = new Player()
            {
                Num = 0
            };
            var fleetOwner = new Player()
            {
                Num = 1
            };

            var planet = new Planet()
            {
                Name = "Brin",
                Player = planetOwner,
                Population = 10000,
                Mines = 100,
                Factories = 100,
                Defenses = 10,
            };

            // one mini-bomber
            var fleet1 = new Fleet()
            {
                Player = fleetOwner,
                Name = "Mini-Bomber #1",
                Tokens = new List<ShipToken>() {
                    new ShipToken() {
                        Design = new ShipDesign() {
                            Player = fleetOwner,
                            Hull = Techs.MiniBomber,
                            Slots = new List<ShipDesignSlot>() {
                                new ShipDesignSlot(Techs.QuickJump5, 1, 1),
                                new ShipDesignSlot(Techs.LadyFingerBomb, 2, 2),
                            },
                        },
                        Quantity = 1
                    }
                }
            };

            // one mini-bomber with smart bombs
            var fleet2 = new Fleet()
            {
                Player = fleetOwner,
                Name = "Mini-Bomber #2",
                Tokens = new List<ShipToken>() {
                    new ShipToken() {
                        Design = new ShipDesign() {
                            Player = fleetOwner,
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

            fleet1.ComputeAggregate();
            fleet2.ComputeAggregate();

            fleet1.Orbiting = planet;
            fleet2.Orbiting = planet;
            planet.OrbitingFleets.Add(fleet1);
            planet.OrbitingFleets.Add(fleet2);

            planetBomber.BombPlanet(planet);

            // bomb with smart bombs
            Assert.AreEqual(2, planetOwner.Messages.Count);
            Assert.AreEqual(2, fleetOwner.Messages.Count);
            Assert.AreEqual(98, planet.Mines);
            Assert.AreEqual(98, planet.Factories);
            Assert.AreEqual(9, planet.Defenses);
            Assert.AreEqual(9300, planet.Population);
        }
    }
}