using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerOrdersConsumerTest
    {
        Game game;
        GameRunner gameRunner;
        FleetOrderExecutor fleetOrderExecutor;
        FleetSpecService fleetSpecService;
        Player player;
        Planet playerPlanet;
        Fleet playerFleet;

        PlayerIntelDiscoverer playerIntelDiscoverer;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            player = game.Players[0];
            playerPlanet = player.PlanetsByGuid[game.Planets[0].Guid];
            playerFleet = player.Fleets[0];

            fleetOrderExecutor = new FleetOrderExecutor(game, TestUtils.TestContainer.GetInstance<FleetService>(), TestUtils.TestContainer.GetInstance<CargoTransferer>());
            fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
            playerIntelDiscoverer = TestUtils.TestContainer.GetInstance<PlayerIntelDiscoverer>();
        }

        [Test]
        public void TestConsumeOrders()
        {
            var consumer = new PlayerOrdersConsumer(game, fleetOrderExecutor, fleetSpecService);

            // create a new planet for a fleet target
            var newPlanet = new Planet() { Name = "Planet 2" };
            newPlanet.InitEmptyPlanet();
            game.AddMapObject(newPlanet);
            game.UpdateInternalDictionaries();
            playerIntelDiscoverer.Discover(player, newPlanet);
            player.SetupMapObjectMappings();

            var newDesign = ShipDesigns.ArmoredProbe.Clone(player);
            newDesign.Status = ShipDesign.DesignStatus.New;
            player.Designs.Add(newDesign);
            playerPlanet.ProductionQueue.Items.Add(new ProductionQueueItem(QueueItemType.ShipToken, design: newDesign));
            playerPlanet.ContributesOnlyLeftoverToResearch = true;

            // create some fleet orders
            var playerNewPlanet = player.PlanetsByGuid[newPlanet.Guid];
            playerFleet.Waypoints.Add(Waypoint.TargetWaypoint(playerNewPlanet, warpFactor: 9, WaypointTask.ScrapFleet));
            playerFleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 100), warpFactor: 8, WaypointTask.Colonize));

            var orders = player.GetOrders();

            // update the research for the player
            orders.Research.ResearchAmount = 20;
            orders.Research.Researching = TechField.Weapons;
            orders.Research.NextResearchField = NextResearchField.Propulsion;

            consumer.ConsumeOrders(orders);
            game.UpdateInternalDictionaries();

            var gamePlanet = game.PlanetsByGuid[playerPlanet.Guid];
            var gameFleet = game.FleetsByGuid[playerFleet.Guid];
            var gameDesign = game.DesignsByGuid[newDesign.Guid];

            // validate our production queue is working with a new design
            Assert.AreEqual(1, gamePlanet.ProductionQueue.Items.Count);
            Assert.AreEqual(gameDesign, gamePlanet.ProductionQueue.Items[0].Design);
            Assert.AreEqual(true, gamePlanet.ContributesOnlyLeftoverToResearch);

            // validate the fleet has 3 waypoints with game targets
            Assert.AreEqual(3, gameFleet.Waypoints.Count);
            Assert.AreEqual(newPlanet, gameFleet.Waypoints[1].Target);
            Assert.AreEqual(9, gameFleet.Waypoints[1].WarpFactor);
            Assert.AreEqual(WaypointTask.ScrapFleet, gameFleet.Waypoints[1].Task);
            Assert.AreEqual(new Vector2(100, 100), gameFleet.Waypoints[2].Position);
            Assert.AreEqual(8, gameFleet.Waypoints[2].WarpFactor);
            Assert.AreEqual(WaypointTask.Colonize, gameFleet.Waypoints[2].Task);

            // validate the player's research was updated
            Assert.AreEqual(20, player.ResearchAmount);
            Assert.AreEqual(TechField.Weapons, player.Researching);
            Assert.AreEqual(NextResearchField.Propulsion, player.NextResearchField);
        }
    }
}
