using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using log4net;
using NUnit.Framework;

namespace CraigStars.Tests
{

    [TestFixture]
    public class GameSerializerTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameSerializerTest));


        [Test]
        public void TestSerialize()
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
                    Player = player,
                    Name = "Design 1",
                    Hull = Techs.Scout,
                    Slots = new List<ShipDesignSlot>() {
                        new ShipDesignSlot(Techs.QuickJump5, 1, 1)
                    }
                }
            );

            var otherPlayer = new Player()
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
                Player = otherPlayer,
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
                },
                BattlePlan = player.BattlePlans[0]
            };
            planet1.OrbitingFleets.Add(fleet1);

            var fleet2 = new Fleet()
            {
                Name = "Fleet 2",
                Player = otherPlayer,
            };
            player.Fleets.Add(fleet1);
            player.Fleets.Add(fleet2);

            // messages require looking up objects by guid
            player.SetupMapObjectMappings();

            // add a message about our homeworld
            Message.HomePlanet(player, planet1);

            Game game = new Game()
            {
                SaveToDisk = false,
                TechStore = StaticTechStore.Instance,
                Players = new List<Player>() { player, otherPlayer },
                Planets = new List<Planet>() { planet1, planet2 },
                Fleets = new List<Fleet>() { fleet1, fleet2 }
            };

            var json = Serializers.SerializeGame(game, Serializers.CreateGameSettings(game));
            log.Info($"\n{json}");

            Game loaded = new Game() { SaveToDisk = false, TechStore = StaticTechStore.Instance };
            Serializers.PopulateGame(json, loaded, Serializers.CreateGameSettings(loaded));

            Assert.AreEqual(game.Players.Count, loaded.Players.Count);
            Assert.AreEqual(game.Players[0].Name, loaded.Players[0].Name);
            Assert.AreEqual(game.Fleets.Count, loaded.Fleets.Count);
            Assert.AreEqual(loaded.Players[0], loaded.Fleets[0].Player);
        }

        [Test]
        public void TestSerializeAll()
        {
            var player1 = new Player()
            {
                Name = "Bob",
                Num = 0,
            };

            var player2 = new Player()
            {
                Name = "Ted",
                Num = 1,
            };

            // generate a tiny universe
            Game game = new Game() { SaveToDisk = false };
            game.Init(new List<Player>() { player1, player2 }, new Rules(0) { Size = Size.Tiny, Density = Density.Sparse }, StaticTechStore.Instance, new TestGamesManager());
            game.GenerateUniverse();

            var gameSettings = Serializers.CreateGameSettings(game);
            var gameJson = Serializers.SerializeGame(game, gameSettings);
            var playerSettings = Serializers.CreatePlayerSettings(game.Players.Cast<PublicPlayerInfo>().ToList(), game.TechStore);

            var player1Json = Serializers.Serialize(player1, playerSettings);
            var player2Json = Serializers.Serialize(player2, playerSettings);
            log.Info($"Game: \n{gameJson}");
            log.Info($"Player1: \n{player1Json}");
            // log.Info($"Player2: \n{player2Json}");

            // reload the game
            Game loaded = new Game() { SaveToDisk = false, TechStore = StaticTechStore.Instance };
            Serializers.PopulateGame(gameJson, loaded, Serializers.CreateGameSettings(loaded));
            var loadSettings = Serializers.CreatePlayerSettings(loaded.Players.Cast<PublicPlayerInfo>().ToList(), loaded.TechStore);
            Serializers.PopulatePlayer(player1Json, loaded.Players[0], loadSettings);
            Serializers.PopulatePlayer(player2Json, loaded.Players[1], loadSettings);

            Assert.AreEqual(game.Players.Count, loaded.Players.Count);
            Assert.AreEqual(game.Players[0].Name, loaded.Players[0].Name);
            Assert.AreEqual(game.Players[0].Planets.Count, loaded.Players[0].Planets.Count);
            Assert.AreEqual(game.Players[0].Fleets.Count, loaded.Players[0].Fleets.Count);
            Assert.AreEqual(game.Players[0].Designs.Count, loaded.Players[0].Designs.Count);
            Assert.AreEqual(game.Players[0].Name, loaded.Players[0].Name);
            Assert.AreEqual(game.Fleets.Count, loaded.Fleets.Count);
            Assert.AreEqual(game.Planets.Count, loaded.Planets.Count);
            Assert.AreEqual(loaded.Players[0], loaded.Fleets[0].Player);
        }
    }

}