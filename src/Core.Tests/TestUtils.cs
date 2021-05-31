using System.Collections.Generic;
using Godot;

namespace CraigStars.Tests
{
    /// <summary>
    /// A utility class for unit testing
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Test helper method to return a simple game
        /// </summary>
        /// <returns>A game with one planet, one player, one fleet</returns>
        internal static Game GetSingleUnitGame()
        {
            PlayerIntel playerIntel = new();
            Game game = new()
            {
                SaveToDisk = false,
                TechStore = StaticTechStore.Instance
            };
            var player = new Player()
            {
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            game.Players.Add(player);

            // create an empty planet and make the player aware of it
            var planet = new Planet()
            {
                Name = "Planet 1",
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                MineralConcentration = new Mineral(100, 100, 100),
            };
            game.Planets.Add(planet);
            playerIntel.Discover(player, planet);

            // take ownership of this planet
            planet.ProductionQueue = new ProductionQueue();
            planet.Scanner = true;
            planet.Player = player;
            planet.Population = 25000;

            var design = ShipDesigns.LongRangeScount.Clone();
            design.Player = player;
            game.Designs.Add(design);

            var fleet = new Fleet()
            {
                Player = player,
                Name = "Fleet 1",
                Tokens = new List<ShipToken>(new ShipToken[] {
                    new ShipToken()
                    {
                        Design = design,
                        Quantity = 1
                    }
                }),
                BattlePlan = player.BattlePlans[0],
                Orbiting = planet,
                Waypoints = new List<Waypoint>() {
                    Waypoint.TargetWaypoint(planet)
                }
            };
            game.Fleets.Add(fleet);
            planet.OrbitingFleets.Add(fleet);

            player.SetupMapObjectMappings();
            game.UpdateDictionaries();
            game.UpdatePlayers();

            return game;
        }

        /// <summary>
        /// Test helper method to return a game with 2 players, 2 planets, and 2 fleets
        /// </summary>
        /// <returns>A game with 2 players, 2 planets, and 2 fleets</returns>
        internal static Game GetTwoPlayerGame()
        {
            PlayerIntel playerIntel = new();
            Game game = new()
            {
                SaveToDisk = false,
                TechStore = StaticTechStore.Instance
            };
            var player1 = new Player()
            {
                Name = "Player 1",
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            var player2 = new Player()
            {
                Name = "Player 2",
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            game.Players.Add(player1);
            game.Players.Add(player2);

            // create empty planets and have the players discover them
            var planet1 = new Planet()
            {
                Name = "Planet 1",
                Position = new Vector2(0, 0),
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                MineralConcentration = new Mineral(100, 100, 100),
            };
            var planet2 = new Planet()
            {
                Name = "Planet 2",
                Position = new Vector2(200, 200),
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                MineralConcentration = new Mineral(100, 100, 100),
            };
            game.Planets.Add(planet1);
            game.Planets.Add(planet2);
            game.Planets.ForEach(planet =>
            {
                playerIntel.Discover(player1, planet);
                playerIntel.Discover(player2, planet);
            });

            // take ownership of this planet
            planet1.ProductionQueue = new ProductionQueue();
            planet1.Scanner = true;
            planet1.Player = player1;
            planet1.Population = 25000;

            planet2.ProductionQueue = new ProductionQueue();
            planet2.Scanner = true;
            planet2.Player = player2;
            planet2.Population = 25000;

            var design1 = ShipDesigns.LongRangeScount.Clone(player1);
            var design2 = ShipDesigns.StalwartDefender.Clone(player2);
            game.Designs.Add(design1);
            game.Designs.Add(design2);

            var fleet1 = new Fleet()
            {
                Player = player1,
                Name = "Fleet 1",
                Position = planet1.Position,
                Tokens = new List<ShipToken>(new ShipToken[] {
                    new ShipToken()
                    {
                        Design = design1,
                        Quantity = 1
                    }
                }),
                BattlePlan = player1.BattlePlans[0],
                Orbiting = planet1,
                Waypoints = new List<Waypoint>() {
                    Waypoint.TargetWaypoint(planet1)
                }
            };
            var fleet2 = new Fleet()
            {
                Player = player2,
                Name = "Fleet 2",
                Position = planet2.Position,
                Tokens = new List<ShipToken>(new ShipToken[] {
                    new ShipToken()
                    {
                        Design = design2,
                        Quantity = 1
                    }
                }),
                BattlePlan = player2.BattlePlans[0],
                Orbiting = planet2,
                Waypoints = new List<Waypoint>() {
                    Waypoint.TargetWaypoint(planet2)
                }
            };
            game.Fleets.Add(fleet1);
            game.Fleets.Add(fleet2);
            planet1.OrbitingFleets.Add(fleet1);
            planet2.OrbitingFleets.Add(fleet2);

            player1.SetupMapObjectMappings();
            player2.SetupMapObjectMappings();
            game.UpdateDictionaries();
            game.UpdatePlayers();

            return game;
        }
    }
}
