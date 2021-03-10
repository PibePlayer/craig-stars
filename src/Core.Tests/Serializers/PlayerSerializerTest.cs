using System;
using System.Collections.Generic;
using Godot;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CraigStars.Tests
{

    [TestFixture]
    public class PlayerSerializerTest
    {
        ILog log = LogManager.GetLogger(typeof(PlayerSerializerTest));


        [Test]
        public void TestSerialize()
        {
            var player = new Player()
            {
                Name = "Bob",
                Num = 0,
            };

            player.Designs = new List<ShipDesign>() {
                    new ShipDesign() {
                        Player = player,
                        Name = "Design 1",
                        Hull = Techs.Scout,
                        Slots = new List<ShipDesignSlot>() {
                            new ShipDesignSlot(Techs.QuickJump5, 1, 1)
                        }
                    }
                };

            var otherPlayer = new PublicPlayerInfo()
            {
                Name = "Other Player",
                Num = 1,
                Ready = true,
                AIControlled = true,
                SubmittedTurn = true,
                Color = Colors.Red
            };

            // create an empty planet we own
            var planet1 = new Planet()
            {
                Name = "Planet 1",
                Player = player,
                Homeworld = true,
            };
            planet1.InitEmptyPlanet();
            player.Homeworld = planet1;

            // create an unowned and unknown planet
            var planet2 = new Planet()
            {
                Name = "Planet 2",
                Owner = otherPlayer,
            };

            player.Planets.Add(planet1);
            player.Planets.Add(planet2);

            // add some fleets
            var fleet1 = new Fleet()
            {
                Name = "Fleet 1",
                Orbiting = planet1,
                Player = player,
                Tokens = new List<ShipToken>() {
                    new ShipToken(player.Designs[0], 1)
                }
            };
            planet1.OrbitingFleets.Add(fleet1);

            var fleet2 = new Fleet()
            {
                Name = "Fleet 2",
                Owner = otherPlayer,
            };
            player.Fleets.Add(fleet1);
            player.Fleets.Add(fleet2);

            // messages require looking up objects by guid
            player.SetupMapObjectMappings();

            // add a message about our homeworld
            Message.HomePlanet(player, planet1);

            var settings = Serializers.CreatePlayerSettings(new List<PublicPlayerInfo>() { player, otherPlayer }, StaticTechStore.Instance);
            var json = Serializers.Serialize(player, settings);
            log.Info($"\n{json}");

            // populate this player object
            var loadedPlayer = new Player()
            {
                TechStore = StaticTechStore.Instance,
            };
            var loadSettings = Serializers.CreatePlayerSettings(new List<PublicPlayerInfo>() { loadedPlayer, otherPlayer }, StaticTechStore.Instance);
            Serializers.PopulatePlayer(json, loadedPlayer, loadSettings);

            // make sure we have some basic stats
            Assert.AreEqual(player.Name, loadedPlayer.Name);
            Assert.AreEqual(player.Planets.Count, loadedPlayer.Planets.Count);
            Assert.AreEqual(player.Fleets.Count, loadedPlayer.Fleets.Count);

            // make sure our players were re-constituted as fleet owners
            Assert.AreEqual(loadedPlayer, loadedPlayer.Fleets[0].Player);
            Assert.AreEqual(player.Fleets[0].Tokens.Count, loadedPlayer.Fleets[0].Tokens.Count);
            Assert.AreEqual(otherPlayer, loadedPlayer.Fleets[1].Owner);

            // make sure our planet we loaded is also the one our first fleet is orbiting
            Assert.AreEqual(loadedPlayer.Planets[0], loadedPlayer.Fleets[0].Orbiting);

            // make sure our messages came through
            Assert.AreEqual(player.Messages.Count, loadedPlayer.Messages.Count);
            Assert.AreEqual(player.Messages[0].Text, loadedPlayer.Messages[0].Text);
            Assert.AreEqual(loadedPlayer.Planets[0], loadedPlayer.Messages[0].Target);

            // make sure our designs saved
            Assert.AreEqual(player.Designs.Count, loadedPlayer.Designs.Count);
            Assert.AreEqual(player.Designs.Count, loadedPlayer.Designs.Count);
            Assert.AreEqual(player.Designs[0].Slots.Count, loadedPlayer.Designs[0].Slots.Count);
            Assert.AreEqual(loadedPlayer, loadedPlayer.Designs[0].Player);

        }

    }

}