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
    public class PlanetProductionStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetProductionStepTest));

        Game game;
        GameRunner gameRunner;
        PlanetProductionStep step;
        PlayerService playerService = TestUtils.TestContainer.GetInstance<PlayerService>();
        PlanetService planetService = TestUtils.TestContainer.GetInstance<PlanetService>();
        FleetSpecService fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new PlanetProductionStep(gameRunner.GameProvider, planetService, playerService, fleetSpecService);
        }

        [Test]
        public void TestProcessItem()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // this should build a mine with nothing leftover
            var item = new ProductionQueueItem(QueueItemType.Mine, 1);
            var result = step.ProcessItem(planet, player, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(0, item.Quantity);

            // this should build one mine, but not complete
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, player, item, new Cost(resources: 5));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(1, item.Quantity);

            // this should build two mines, with some leftover cost for building new stuff
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, player, item, new Cost(10, 20, 30, 40));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(10, 20, 30, 30), result.remainingCost);
            Assert.AreEqual(2, planet.Mines);
            Assert.AreEqual(0, item.Quantity);

            // this should build one mine and partially build 1 more
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2);
            result = step.ProcessItem(planet, player, item, new Cost(resources: 8));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(new Cost(resources: 3), item.Allocated);
            Assert.AreEqual(1, item.Quantity);

            // this should build one auto mine and partially build 1 more
            // but we should still have 2 auto mines in the queue
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.AutoMines, 2);
            result = step.ProcessItem(planet, player, item, new Cost(resources: 8));
            Assert.AreEqual(false, result.completed);
            Assert.AreEqual(Cost.Zero, result.remainingCost);
            Assert.AreEqual(1, result.numCompleted); // completed one item
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(new Cost(resources: 3), item.Allocated);
            Assert.AreEqual(2, item.Quantity); // auto items don't reduce in quantity

        }

        [Test]
        public void TestProcessItemOverBuild()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            var maxMines = planet.Spec.MaxMines;
            var maxPossibleMines = planet.Spec.MaxPossibleMines;

            // we have the max possible mines, so we won't actually build any
            planet.Mines = maxPossibleMines;
            var item = new ProductionQueueItem(QueueItemType.Mine, 1);
            var result = step.ProcessItem(planet, player, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 5), result.remainingCost);
            Assert.AreEqual(maxPossibleMines, planet.Mines);

            // we have the max usable mines, so we won't actually build any auto mines
            planet.Mines = maxMines;
            item = new ProductionQueueItem(QueueItemType.AutoMines, 1);
            result = step.ProcessItem(planet, player, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 5), result.remainingCost);
            Assert.AreEqual(maxMines, planet.Mines);
        }

        [Test]
        public void TestProcessItemContinueBuild()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // we partially built this mine last round, finish it this round
            var item = new ProductionQueueItem(QueueItemType.Mine, 1, allocated: new Cost(resources: 3));
            var result = step.ProcessItem(planet, player, item, new Cost(resources: 5));
            Assert.AreEqual(true, result.completed);
            Assert.AreEqual(new Cost(resources: 3), result.remainingCost); // 3 leftover resources
            Assert.AreEqual(0, item.Quantity);
            Assert.AreEqual(1, planet.Mines);

            // we partially built one mine last round, finish it this and allocate to the
            // next mine
            planet.Mines = 0;
            item = new ProductionQueueItem(QueueItemType.Mine, 2, allocated: new Cost(resources: 3));
            result = step.ProcessItem(planet, player, item, new Cost(resources: 5)); // allocate 5 more resources
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
            step.BuildItem(planet, player, new ProductionQueueItem(QueueItemType.Mine), 1);
            Assert.AreEqual(1, planet.Mines);

            // should build two factories
            step.BuildItem(planet, player, new ProductionQueueItem(QueueItemType.AutoFactories), 2);
            Assert.AreEqual(2, planet.Factories);

            // should build three defenses
            step.BuildItem(planet, player, new ProductionQueueItem(QueueItemType.Defenses), 3);
            Assert.AreEqual(3, planet.Defenses);

            // give this player some total terraforming and terraform a few times
            planet.BaseHab = planet.Hab = new Hab(40, 40, 40);
            player.Race.LRTs.Add(LRT.TT);
            player.TechLevels = new TechLevel(biotechnology: 10);

            // should terrform three times
            step.BuildItem(planet, player, new ProductionQueueItem(QueueItemType.TerraformEnvironment), 3);
            Assert.AreEqual(new Hab(41, 41, 41), planet.Hab);
        }

        [Test]
        public void TestBuildSingleComplete()
        {
            // make a starter homeworld that only contributes leftovers to research
            var player = game.Players[0];
            var planet = game.Planets[0];
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;
            planet.Spec = planetService.ComputePlanetSpec(planet, player);

            // should build one factory
            // and have 4kt less germanium
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.Factory, 1)
            };
            planet.Cargo = new Cargo(germanium: 4, colonists: planet.Cargo.Colonists);

            var leftoverResources = step.Build(planet, player);
            Assert.AreEqual(11, planet.Factories);
            Assert.AreEqual(0, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(new Cargo(colonists: planet.Cargo.Colonists), planet.Cargo);
            Assert.AreEqual(25, leftoverResources); // 25 resources leftover for research
        }

        [Test]
        public void TestBuildIncomplete()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // build a factory and mine without enough germanium
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.Factory, 1),
                new ProductionQueueItem(QueueItemType.Mine, 1)
            };
            planet.Cargo = new Cargo(germanium: 3, colonists: planet.Cargo.Colonists);
            step.Build(planet, player);

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
            var player = game.Players[0];
            // make the planet a standard homeworld with 35 available resources
            var planet = game.Planets[0];
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;
            planet.Spec = planetService.ComputePlanetSpec(planet, player);

            // build a factory and mine without enough germanium, but auto build tasks
            // so when the factory doesn't complete, it should move on and try the mines
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoFactories, 1),
                new ProductionQueueItem(QueueItemType.AutoMines, 1)
            };
            planet.Cargo = new Cargo(germanium: 3, colonists: planet.Cargo.Colonists);
            int leftoverResources = step.Build(planet, player);

            // should use all germanium and allocate it to factory (and 7 resources, which is 3/4 of the 10 required)
            // the mine should be built
            Assert.AreEqual(10, planet.Factories);
            Assert.AreEqual(11, planet.Mines);
            Assert.AreEqual(2, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[1].Quantity);
            Assert.AreEqual(30, leftoverResources);
            Assert.AreEqual(new Cost(), planet.ProductionQueue.Items[0].Allocated);
            Assert.AreEqual(new Cargo(germanium: 3, colonists: planet.Cargo.Colonists), planet.Cargo);
        }

        [Test]
        public void TestBuildAuto2()
        {
            // make the planet a standard homeworld 35 resources
            var player = game.Players[0];
            var planet = game.Planets[0];
            var initialFleetCount = game.Fleets.Count;
            planet.Factories = 10;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;
            var design = ShipDesigns.LongRangeScount.Clone();
            design.PlayerNum = player.Num;
            fleetSpecService.ComputeDesignSpec(player, design);
            player.Designs.Add(design);
            planet.Spec = planetService.ComputePlanetSpec(planet, player);


            // setup an auto queue that tries to build 20 factories, then 1 mine, then 1 ship
            // we should not build any factories, then build 1 mine, then partially build the ship
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoFactories, 20),
                new ProductionQueueItem(QueueItemType.AutoMines, 1),
                new ProductionQueueItem(QueueItemType.ShipToken, 1, design),
            };
            planet.Cargo = new Cargo(germanium: 3, ironium: 18, boranium: 2, colonists: planet.Cargo.Colonists);
            int leftoverResources = step.Build(planet, player);

            // should skip factories because we can't build one
            // it should build 1 mine, then allocate the rest for the scout
            Assert.AreEqual(10, planet.Factories);
            Assert.AreEqual(11, planet.Mines);
            Assert.AreEqual(3, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(20, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[1].Quantity);

            // it should partially build the fleet
            Assert.AreEqual(QueueItemType.ShipToken, planet.ProductionQueue.Items[2].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[2].Quantity);
            Assert.AreEqual(new Cost(ironium: 6, boranium: 0, germanium: 3, resources: 9), planet.ProductionQueue.Items[2].Allocated);

            // what we couldnt spend is leftover
            Assert.AreEqual(new Cargo(ironium: 12, boranium: 2, germanium: 0, colonists: planet.Cargo.Colonists), planet.Cargo);
            Assert.AreEqual(21, leftoverResources);
            Assert.AreEqual(initialFleetCount, game.Fleets.Count); // we shouldn't have any additional fleets
        }

        [Test]
        public void TestBuildAuto3()
        {
            // make the planet a standard homeworld with one extra factory for 36 available resources
            var player = game.Players[0];
            var planet = game.Planets[0];
            planet.Factories = 11;
            planet.Mines = 10;
            planet.ContributesOnlyLeftoverToResearch = true;
            planet.Spec = planetService.ComputePlanetSpec(planet, player);

            // setup an auto queue that tries to build 20 factories, then 20 mines
            // we should build one factory, and one partial factory, then no mines
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoFactories, 20),
                new ProductionQueueItem(QueueItemType.AutoMines, 20),
            };
            planet.Cargo = new Cargo(germanium: 5, colonists: planet.Cargo.Colonists);
            int leftoverResources = step.Build(planet, player);

            // we should build one factory and but be unable to build a second, so we should
            // move on to building mines. We have 26 resources left after spending 10 on a factory, 
            // so we should be able to build 5 mines and have 1 resource leftover for another mine
            // we'll also still have 1 germanium we didn't spend on a factory
            Assert.AreEqual(12, planet.Factories);
            Assert.AreEqual(15, planet.Mines);
            Assert.AreEqual(3, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.Mine, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(new Cost(resources: 1), planet.ProductionQueue.Items[0].Allocated);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(20, planet.ProductionQueue.Items[1].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[2].Type);
            Assert.AreEqual(20, planet.ProductionQueue.Items[2].Quantity);
            Assert.AreEqual(new Cargo(germanium: 1, colonists: planet.Cargo.Colonists), planet.Cargo);
            Assert.AreEqual(0, leftoverResources);
        }

        [Test]
        public void TestBuildAuto4()
        {
            // make this a larger planet so we can build everything
            var player = game.Players[0];
            var planet = game.Planets[0];
            var initialFleetCount = game.Fleets.Count;
            planet.Population = 1000000; // 1 MILLION people
            planet.Factories = 500;
            planet.Mines = 0;
            planet.ContributesOnlyLeftoverToResearch = true;

            // we are going to auto build some stuff and add a couple ship builds at the end
            var design1 = TestUtils.CreateDesign(game, player, ShipDesigns.LongRangeScount.Clone());
            var design2 = TestUtils.CreateDesign(game, player, ShipDesigns.SantaMaria.Clone());

            // add some auto steps and a couple ships. They should all complete
            planet.ProductionQueue.Items = new List<ProductionQueueItem>() {
                new ProductionQueueItem(QueueItemType.AutoMaxTerraform, 1),
                new ProductionQueueItem(QueueItemType.AutoFactories, 1),
                new ProductionQueueItem(QueueItemType.AutoMines, 1),
                new ProductionQueueItem(QueueItemType.ShipToken, 1, design1),
                new ProductionQueueItem(QueueItemType.ShipToken, 1, design2),
            };
            planet.Cargo = new Cargo(1000, 1000, 1000, colonists: planet.Cargo.Colonists);

            gameRunner.ComputeSpecs();
            step.Build(planet, player);

            // 1 mine, 1 factory, and 2 ships should be built
            // no terraforming because we are maxed
            Assert.AreEqual(501, planet.Factories);
            Assert.AreEqual(1, planet.Mines);
            Assert.AreEqual(3, planet.ProductionQueue.Items.Count);
            Assert.AreEqual(QueueItemType.AutoMaxTerraform, planet.ProductionQueue.Items[0].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[0].Quantity);
            Assert.AreEqual(QueueItemType.AutoFactories, planet.ProductionQueue.Items[1].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[1].Quantity);
            Assert.AreEqual(QueueItemType.AutoMines, planet.ProductionQueue.Items[2].Type);
            Assert.AreEqual(1, planet.ProductionQueue.Items[2].Quantity);
            Assert.AreEqual(initialFleetCount + 2, game.Fleets.Count); // we should have built our two fleets
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

        [Test]
        public void TestBuildFleet()
        {
            var planet = game.Planets[0];
            var player = game.Players[0];

            ProductionQueueItem item = new(QueueItemType.ShipToken, 1, game.Designs[1]);

            Fleet builtFleet = null;

            Action<MapObject> onFleetBuilt = (MapObject mo) =>
            {
                builtFleet = (Fleet)mo;
            };

            EventManager.MapObjectCreatedEvent += onFleetBuilt;
            step.BuildFleet(planet, player, item, 1);
            EventManager.MapObjectCreatedEvent -= onFleetBuilt;

            Assert.NotNull(builtFleet);
            Assert.AreEqual(builtFleet.Position, planet.Position);
        }

        [Test]
        public void TestBuildFleetWithComposition()
        {
            var planet = game.Planets[0];
            var player = game.Players[0];
            var fleet = game.Fleets[0];

            // add a new minibomber design to the player's designs
            game.Designs.Add(ShipDesigns.MiniBomber.Clone(player));

            // add a FleetComposition to the existing Fleet so the PlanetProductionStep
            // will add the newly built token to this fleet
            fleet.FleetComposition = new()
            {
                Type = FleetCompositionType.Bomber,
                Tokens = new()
                {
                    new FleetCompositionToken(ShipDesignPurpose.Scout, 1),
                    new FleetCompositionToken(ShipDesignPurpose.Bomber, 1)
                }
            };
            fleetSpecService.ComputeFleetSpec(player, fleet, true);

            // build the design we just added
            ProductionQueueItem item = new(QueueItemType.ShipToken, 1, game.Designs[game.Designs.Count - 1]);

            Fleet builtFleet = null;

            Action<MapObject> onFleetBuilt = (MapObject mo) =>
            {
                builtFleet = (Fleet)mo;
            };

            EventManager.MapObjectCreatedEvent += onFleetBuilt;
            step.BuildFleet(planet, player, item, 1);
            EventManager.MapObjectCreatedEvent -= onFleetBuilt;

            // we shouldn't get a new notification
            Assert.IsNull(builtFleet);

            // our existing fleet should be updated
            Assert.AreEqual(2, fleet.Tokens.Count);
            Assert.AreEqual(game.Designs[game.Designs.Count - 1], fleet.Tokens[1].Design);
            Assert.AreEqual(1, fleet.Tokens[1].Quantity);
        }

        [Test]
        public void TestBuildPacket()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];
            planet.PacketSpeed = 5;

            var target = new Planet()
            {
                Position = new Vector2(100, 100),
                Name = "Target Planet",
            };
            game.AddMapObject(target);

            planet.PacketTarget = target;

            ProductionQueueItem item = new(QueueItemType.MixedMineralPacket, 1);

            MineralPacket builtPacket = null;
            Action<MapObject> onPacketBuilt = (MapObject mo) =>
            {
                builtPacket = (MineralPacket)mo;
            };

            EventManager.MapObjectCreatedEvent += onPacketBuilt;
            step.BuildPacket(planet, player, new Cargo(1, 1, 1), 1);
            EventManager.MapObjectCreatedEvent -= onPacketBuilt;

            Assert.IsNotNull(builtPacket);
            Assert.GreaterOrEqual(5, builtPacket.WarpFactor);

        }

    }
}