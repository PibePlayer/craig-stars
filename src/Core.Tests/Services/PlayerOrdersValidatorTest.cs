using System.Collections.Generic;
using Godot;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerOrdersValidatorTest
    {
        Game game;
        GameRunner gameRunner;
        Player player;

        [SetUp]
        public void SetUp()
        {
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            player = game.Players[0];
        }

        [Test]
        public void TestValidate()
        {
            var orders = player.GetOrders();
            var validator = new PlayerOrdersValidator(game);

            var result = validator.Validate(orders);

            Assert.AreEqual(0, result.Errors.Count);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void TestValidatePlayerToken()
        {
            var orders = player.GetOrders();
            var validator = new PlayerOrdersValidator(game);
            var result = new PlayerOrdersValidatorResult();

            validator.ValidatePlayerToken(orders, result);
            Assert.IsTrue(result.IsValid);

            orders.PlayerNum = 10;
            result = new PlayerOrdersValidatorResult();
            validator.ValidatePlayerToken(orders, result);
            Assert.IsFalse(result.IsValid);

            orders.PlayerNum = player.Num;
            orders.Token = "invalid";
            result = new PlayerOrdersValidatorResult();
            validator.ValidatePlayerToken(orders, result);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void TestValidateDesigns()
        {
            // add a new design
            var newDesign = ShipDesigns.ArmoredProbe.Clone(player);
            newDesign.Status = ShipDesign.DesignStatus.New;
            player.Designs.Add(newDesign);
            player.DesignsByGuid[newDesign.Guid] = newDesign;

            // build the new design
            player.Planets[0].ProductionQueue.Items.Add(new ProductionQueueItem(QueueItemType.ShipToken, design: newDesign));

            var orders = player.GetOrders();
            var validator = new PlayerOrdersValidator(game);
            var result = validator.Validate(orders);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void TestValidateFleetOrders()
        {
            // create a new planet to target
            var newPlanet = new Planet() { Name = "New Planet", Position = new Vector2(100, 100) };
            newPlanet.InitEmptyPlanet();
            game.Planets.Add(newPlanet);
            game.UpdateInternalDictionaries();
            var playerIntelDiscoverer = TestUtils.TestContainer.GetInstance<PlayerIntelDiscoverer>();
            playerIntelDiscoverer.Discover(player, newPlanet);

            var fleet = player.Fleets[0];
            fleet.Waypoints.Add(Waypoint.PositionWaypoint(new Vector2(100, 0)));
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(player.ForeignPlanets[0]));

            var orders = player.GetOrders();
            var validator = new PlayerOrdersValidator(game);
            var result = validator.Validate(orders);
            Assert.IsTrue(result.IsValid);
        }
    }
}
