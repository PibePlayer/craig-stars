using System;
using System.Collections.Generic;
using System.Reflection;
using CraigStars.Singletons;
using Godot;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CraigStars.Tests
{

    public class MapObjectGuidConverter : JsonConverter<MapObject>
    {
        public Dictionary<Guid, MapObject> MapObjectsByGuid { get; set; } = new Dictionary<Guid, MapObject>();

        public override MapObject ReadJson(JsonReader reader, Type objectType, MapObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Guid guid = new Guid(reader.ReadAsString());
            return MapObjectsByGuid[guid];
        }

        public override void WriteJson(JsonWriter writer, MapObject value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Guid);
        }
    }

    [TestFixture]
    public class PlayerSerializerTest
    {
        ILog log = LogManager.GetLogger(typeof(PlayerSerializerTest));


        [Test]
        public void TestSerializeNewton()
        {
            var player = new Player()
            {
                Name = "Player 1",
                Designs = new List<ShipDesign>() {
                    new ShipDesign() {
                        Name = "Design 1",
                        Hull = Techs.Scout,
                        Slots = new List<ShipDesignSlot>() {
                            new ShipDesignSlot(Techs.QuickJump5, 1, 1)
                        }
                    }
                }
            };

            // create an empty planet we own
            var planet1 = new Planet()
            {
                Name = "Planet 1",
                Player = player,
            };
            planet1.InitEmptyPlanet();

            // create an unowned and unknown planet
            var planet2 = new Planet()
            {
                Name = "Planet 2",
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
            };
            player.Fleets.Add(fleet1);

            // add a message
            Message.Info(player, "Test message");

            var json = Serializers.Serialize(player, new List<PublicPlayerInfo>() { player }, TechStore.Instance);
            log.Debug($"\n{json}");

            // populate this player object
            var loadedPlayer = new Player();
            Serializers.PopulatePlayer(json, loadedPlayer, new List<PublicPlayerInfo>() { loadedPlayer }, TechStore.Instance);

            Assert.AreEqual(player.Name, loadedPlayer.Name);
            Assert.AreEqual(player.Designs.Count, loadedPlayer.Designs.Count);
            Assert.AreEqual(player.Designs[0].Slots.Count, loadedPlayer.Designs[0].Slots.Count);
            Assert.AreEqual(player.Planets.Count, loadedPlayer.Planets.Count);
            Assert.AreEqual(player.Fleets.Count, loadedPlayer.Fleets.Count);
            Assert.AreEqual(player.Fleets[0].Tokens.Count, loadedPlayer.Fleets[0].Tokens.Count);
            Assert.AreEqual(player.Messages.Count, loadedPlayer.Messages.Count);
            Assert.AreEqual(player.Messages[0].Text, loadedPlayer.Messages[0].Text);
            Assert.AreEqual(player.Designs.Count, loadedPlayer.Designs.Count);

        }

    }

}