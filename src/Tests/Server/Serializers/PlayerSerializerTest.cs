using System.Text.Json;
using System.Text.Json.Serialization;
using CraigStars.Singletons;
using Godot;
using log4net;
using NUnit.Framework;

namespace CraigStars.Tests
{
    [TestFixture]
    public class PlayerTest
    {
        ILog log = LogManager.GetLogger(typeof(PlayerTest));

        [Test]
        public void TestSerialize()
        {
            var server = new Server();
            var player = new Player()
            {
                Name = "Bob"
            };
            server.Players.Add(player);
            server.TechStore = TechStore.Instance;
            var universeGenerator = new UniverseGenerator(server);
            universeGenerator.Generate();

            // update our player information as if we'd just generated a new turn
            var turnGenerator = new TurnGenerator(server);
            turnGenerator.UpdatePlayerReports();
            turnGenerator.RunTurnProcessors();

            string json = Serializers.SavePlayer(player);
            log.Debug($"\n{json}");

            var loadedPlayer = Serializers.LoadPlayer(json, server.TechStore);
            Assert.AreEqual(player.Name, loadedPlayer.Name);
            Assert.AreEqual(player.Planets.Count, loadedPlayer.Planets.Count);
            Assert.AreEqual(player.Fleets.Count, loadedPlayer.Fleets.Count);
            Assert.AreEqual(player.Messages.Count, loadedPlayer.Messages.Count);
            Assert.AreEqual(player.Designs.Count, loadedPlayer.Designs.Count);
        }

    }

}