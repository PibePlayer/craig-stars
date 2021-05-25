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
    public class PlanetProductionStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetProductionStepTest));

        Game game;
        PlanetProductionStep step;

        [SetUp]
        public void SetUp()
        {
            game = GameTest.GetSingleUnitGame();
            step = new PlanetProductionStep(game);
        }

        [Test]
        public void TestProcessItem()
        {
            var planet = game.Planets[0];

            // this should build a mine with nothing leftover
            var item = new ProductionQueueItem(QueueItemType.Mine, 1);
            var result = step.ProcessItem(planet, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(0, item.Quantity);

            // this should build one mine, but not complete
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, item, new Cost(resources: 5));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(1, item.Quantity);

            // this should build two mines, with some leftover cost for building new stuff
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, item, new Cost(10, 20, 30, 40));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(10, 20, 30, 30), result.remainingCost);
            Assert.AreEqual(2, planet.Mines);
            Assert.AreEqual(0, item.Quantity);

            // this should build one mine and partially build 1 more
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, item, new Cost(resources: 8));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(new Cost(resources: 3), item.Allocated);
            Assert.AreEqual(1, item.Quantity);

            // this should build one auto mine and partially build 1 more
            // but we should still have 2 auto mines in the queue
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.AutoMines, 2);
            result = step.ProcessItem(planet, item, new Cost(resources: 8));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(new Cost(resources: 3), item.Allocated);
            Assert.AreEqual(2, item.Quantity); // auto items don't reduce in quantity

        }

        [Test]
        public void TestProcessItemOverBuild()
        {
            var planet = game.Planets[0];

            // we have the max possible mines, so we won't actually build any
            planet.Mines = planet.MaxPossibleMines;
            var item = new ProductionQueueItem(QueueItemType.Mine, 1);
            var result = step.ProcessItem(planet, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 5), result.remainingCost);
            Assert.AreEqual(planet.MaxPossibleMines, planet.Mines);

            // we have the max usable mines, so we won't actually build any auto mines
            planet.Mines = planet.MaxMines;
            item = new ProductionQueueItem(QueueItemType.AutoMines, 1);
            result = step.ProcessItem(planet, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 5), result.remainingCost);
            Assert.AreEqual(planet.MaxMines, planet.Mines);

        }

        [Test]
        public void TestProcessItemContinueBuild()
        {
            var planet = game.Planets[0];

            // we partially built this mine last round, finish it this round
            var item = new ProductionQueueItem(QueueItemType.Mine, 1, allocated: new Cost(resources: 3));
            var result = step.ProcessItem(planet, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 3), result.remainingCost); // 3 leftover resources
            Assert.AreEqual(0, item.Quantity);
            Assert.AreEqual(1, planet.Mines);

            // we partially built one mine last round, finish it this and allocate to the
            // next mine
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2, allocated: new Cost(resources: 3));
            result = step.ProcessItem(planet, item, new Cost(resources: 5)); // allocate 5 more resources
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost); // spend everything
            Assert.AreEqual(new Cost(resources: 3), item.Allocated); // 3 leftover resources allocated to the next item
            Assert.AreEqual(1, item.Quantity); // one left to build
            Assert.AreEqual(1, planet.Mines); // we build ont

        }

        [Test]
        public void TestBuildItem()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // should build one mine
            step.BuildItem(planet, new ProductionQueueItem(QueueItemType.Mine), 1);
            Assert.AreEqual(1, planet.Mines);

            // should build two factories
            step.BuildItem(planet, new ProductionQueueItem(QueueItemType.AutoFactories), 2);
            Assert.AreEqual(2, planet.Factories);

            // should build three defenses
            step.BuildItem(planet, new ProductionQueueItem(QueueItemType.Defenses), 3);
            Assert.AreEqual(3, planet.Defenses);

            // give this player some total terraforming and terraform a few times
            planet.BaseHab = planet.Hab = new Hab(40, 40, 40);
            player.Race.LRTs.Add(LRT.TT);
            player.TechLevels = new TechLevel(biotechnology: 10);

            // should terrform three times
            step.BuildItem(planet, new ProductionQueueItem(QueueItemType.TerraformEnvironment), 3);
            Assert.AreEqual(new Hab(41, 41, 41), planet.Hab);
        }

        [Test]
        public void TestTerraform()
        {
            var player = game.Players[0];
            // allow Grav3 terraform
            player.TechLevels = new TechLevel(propulsion: 1, biotechnology: 1);

            var planet = new Planet()
            {
                Player = player,
                Hab = new Hab(47, 50, 50),
                BaseHab = new Hab(47, 50, 50),
            };

            // should terraform one point
            step.Terraform(planet);
            Assert.AreEqual(new Hab(48, 50, 50), planet.Hab);

            // should terraform backwards
            planet.Hab = new Hab(53, 50, 50);
            step.Terraform(planet);
            Assert.AreEqual(new Hab(52, 50, 50), planet.Hab);
        }

        [Test]
        public void TestBuildSingleComplete()
        {
            // make a starter homeworld that only contributes leftovers to research
            var planet = game.Planets[0];
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;

            // should build one factory
            // and have 4kt less germanium
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.Factory, 1)
            };
            planet.Cargo = new Cargo(germanium: 4, colonists: planet.Cargo.Colonists);

            var leftoverResources = step.Build(planet);
            Assert.AreEqual(11, planet.Factories);
            Assert.AreEqual(0, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(new Cargo(colonists: planet.Cargo.Colonists), planet.Cargo);
            Assert.AreEqual(25, leftoverResources); // 25 resources leftover for research
        }

        [Test]
        public void TestBuildIncomplete()
        {
            var planet = game.Planets[0];

            // build a factory and mine without enough germanium
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.Factory, 1),
                new ProductionQueueItem(QueueItemType.Mine, 1)
            };
            planet.Cargo = new Cargo(germanium: 3, colonists: planet.Cargo.Colonists);
            step.Build(planet);

            // should use all germanium and allocate it to factory (and 7 resources, which is 3/4 of the 10 required)
            // the mine should remain unbuilt
            Assert.AreEqual(0, planet.Factories);
            Assert.AreEqual(2, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.Factory, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(new Cost(germanium: 3, resources: 7), planet.ProductionQueue.Items[0].Allocated);
            Assert.AreEqual(new Cargo(colonists: planet.Cargo.Colonists), planet.Cargo);
        }

        [Test]
        public void TestBuildAuto()
        {
            // make the planet a standard homeworld with 35 available resources
            var planet = game.Planets[0];
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;

            // build a factory and mine without enough germanium, but auto build tasks
            // so when the factory doesn't complete, it should move on and try the mines
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoFactories, 1),
                new ProductionQueueItem(QueueItemType.AutoMines, 1)
            };
            planet.Cargo = new Cargo(germanium: 3, colonists: planet.Cargo.Colonists);
            step.Build(planet);

            // should use all germanium and allocate it to factory (and 7 resources, which is 3/4 of the 10 required)
            // the mine should be built
            Assert.AreEqual(10, planet.Factories);
            Assert.AreEqual(11, planet.Mines);
            Assert.AreEqual(2, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[1].Quantity);
            Assert.AreEqual(new Cost(germanium: 3, resources: 7), planet.ProductionQueue.Items[0].Allocated);
            Assert.AreEqual(new Cargo(colonists: planet.Cargo.Colonists), planet.Cargo);
        }

        [Test]
        public void TestBuildAuto2()
        {
            // make the planet a standard homeworld with 35 available resources
            var player = game.Players[0];
            var planet = game.Planets[0];
            var initialFleetCount = game.Fleets.Count;
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;
            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = player;
            design.ComputeAggregate();
            player.Designs.Add(design);


            // setup an auto queue that tries to build 20 factories, then 20 mines, then 5 ships
            // we should not build any factories, then build 5 mines, then no longer have enough resources to build
            // anything else.
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoFactories, 20),
                new ProductionQueueItem(QueueItemType.AutoMines, 20),
                new ProductionQueueItem(QueueItemType.ShipToken, 5, design),
            };
            planet.Cargo = new Cargo(germanium: 3, colonists: planet.Cargo.Colonists);
            step.Build(planet);

            // should use all germanium and allocate it to factory (and 7 resources, which is 3/4 of the 10 required)
            // 5 mines should be built, no fleets should be built
            Assert.AreEqual(10, planet.Factories);
            Assert.AreEqual(15, planet.Mines);
            Assert.AreEqual(3, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(20, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(20, planet.ProductionQueue.Items[1].Quantity);
            Assert.AreEqual(new Cost(germanium: 3, resources: 7), planet.ProductionQueue.Items[0].Allocated);
            Assert.AreEqual(new Cargo(colonists: planet.Cargo.Colonists), planet.Cargo);
            Assert.AreEqual(initialFleetCount, game.Fleets.Count); // we shouldn't have any additional fleets
        }

        [Test]
        public void TestAllocateToQueue()
        {
            var planet = game.Planets[0];

            // we only have half the ironium we need, but we have 
            // an abundance of everything else. It should allocate 50% of
            // each item.
            var costPerItem = new Cost(10, 20, 30, 40);
            var allocated = new Cost(5, 100, 100, 100);
            var result = step.AllocatePartialBuild(costPerItem, allocated);
            Assert.AreEqual(new Cost(5, 10, 15, 20), result);
        }
    }
}