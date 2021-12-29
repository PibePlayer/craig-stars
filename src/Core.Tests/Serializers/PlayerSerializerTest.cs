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
        static CSLog log = LogProvider.GetLogger(typeof(PlayerSerializerTest));


        [Test]
        public void TestSerialize()
        {
            var player = new Player()
            {
                Name = "Bob",
                Num = 0,
                BattlePlans = new List<BattlePlan>() { new BattlePlan("Default") }
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

            player.Designs.Add(
                new ShipDesign()
                {
                    PlayerNum = player.Num,
                    Name = "Design 1",
                    Hull = Techs.Scout,
                    Slots = new List<ShipDesignSlot>() {
                        new ShipDesignSlot(Techs.QuickJump5, 1, 1)
                    },
                }
            );

            // create an empty planet we own
            var planet1 = new Planet()
            {
                Name = "Planet 1",
                PlayerNum = player.Num,
                Homeworld = true,
            };
            planet1.InitEmptyPlanet();

            // create an unowned and unknown planet
            var planet2 = new Planet()
            {
                Name = "Planet 2",
                PlayerNum = otherPlayer.Num,
            };

            player.Planets.Add(planet1);
            player.ForeignPlanets.Add(planet2);

            // add some fleets
            var fleet1 = new Fleet()
            {
                Name = "Fleet 1",
                Orbiting = planet1,
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(player.Designs[0], 1)
                },
                BattlePlan = player.BattlePlans[0]
            };

            var fleet2 = new Fleet()
            {
                Name = "Fleet 2",
                PlayerNum = otherPlayer.Num,
            };
            player.Fleets.Add(fleet1);
            player.ForeignFleets.Add(fleet2);

            // messages require looking up objects by guid
            player.SetupMapObjectMappings();

            // add a message about our homeworld
            Message.HomePlanet(player, planet1);

            var settings = Serializers.CreatePlayerSettings(StaticTechStore.Instance);
            var json = Serializers.Serialize(player, settings);
            // log.Info(json);

            // populate this player object
            var loadedPlayer = new Player();
            var loadSettings = Serializers.CreatePlayerSettings(StaticTechStore.Instance);
            loadedPlayer = Serializers.DeserializeObject<Player>(json, loadSettings);

            // make sure we have some basic stats
            Assert.AreEqual(player.Name, loadedPlayer.Name);
            Assert.AreEqual(player.Planets.Count, loadedPlayer.Planets.Count);
            Assert.AreEqual(player.Fleets.Count, loadedPlayer.Fleets.Count);

            // make sure our players were re-constituted as fleet owners
            Assert.AreEqual(loadedPlayer.Num, loadedPlayer.Fleets[0].PlayerNum);
            Assert.AreEqual(player.Fleets[0].Tokens.Count, loadedPlayer.Fleets[0].Tokens.Count);
            Assert.AreEqual(otherPlayer.Num, loadedPlayer.ForeignFleets[0].PlayerNum);

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
            Assert.AreEqual(loadedPlayer.Num, loadedPlayer.Designs[0].PlayerNum);

        }

        [Test]
        public void TestSerializeWaypoints()
        {
            var player = new Player()
            {
                Name = "Bob",
                Num = 0,
                BattlePlans = new List<BattlePlan>() { new BattlePlan("Default") }
            };

            player.Designs.Add(
                new ShipDesign()
                {
                    PlayerNum = player.Num,
                    Name = "Design 1",
                    Hull = Techs.Scout,
                    Slots = new List<ShipDesignSlot>() {
                        new ShipDesignSlot(Techs.QuickJump5, 1, 1)
                    },
                }
            );

            // add some fleets
            var fleet1 = new Fleet()
            {
                Name = "Fleet 1",
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(player.Designs[0], 1)
                },
                BattlePlan = player.BattlePlans[0]
            };

            var fleet2 = new Fleet()
            {
                Name = "Fleet 2",
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>() {
                    new ShipToken(player.Designs[0], 1)
                },
                BattlePlan = player.BattlePlans[0]
            };

            fleet1.Waypoints.Add(Waypoint.TargetWaypoint(fleet2));

            player.Fleets.Add(fleet1);
            player.Fleets.Add(fleet2);

            var settings = Serializers.CreatePlayerSettings(StaticTechStore.Instance);
            var json = Serializers.Serialize(player, settings);
            log.Info(json);

            // populate this player object
            var loadedPlayer = new Player();
            var loadSettings = Serializers.CreatePlayerSettings(StaticTechStore.Instance);
            loadedPlayer = Serializers.DeserializeObject<Player>(json, loadSettings);

            // we should be targeting this fleet
            Assert.AreEqual(loadedPlayer.Fleets[0].Waypoints[0].Target, loadedPlayer.Fleets[1]);
        }
    }

}