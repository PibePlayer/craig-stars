using System.Collections.Generic;
using Godot;
using SimpleInjector;

namespace CraigStars.Tests
{
    /// <summary>
    /// A utility class for unit testing
    /// </summary>
    public static class TestUtils
    {
        public static SimpleInjector.Container TestContainer { get; set; } = new();

        /// <summary>
        /// Most of the services are just ways to logically separate functionality 
        /// so we don't have multi-thousand line Planet, Game, Player, and Fleet
        /// classes. We provide a SimpleInjector to wire up these dependencies
        /// so it's easier to get an instance of things like the PlayerIntel, which has a big tree
        /// when doing a test.
        /// 
        /// I know they *should* be mocked and you should test single bits only, but none
        /// of this code talks to an external system, it's just game logic so I don't see
        /// harm in it running during tests
        /// </summary>
        static TestUtils()
        {
            TestContainer.RegisterInstance<IProvider<ITechStore>>(new Provider<ITechStore>(StaticTechStore.Instance));
            TestContainer.RegisterInstance<ITechStore>(StaticTechStore.Instance);
            TestContainer.Register<IRulesProvider, TestRulesProvider>(Lifestyle.Singleton);

            TestContainer.Register<RaceService>(Lifestyle.Singleton);
            TestContainer.Register<CargoTransferer>(Lifestyle.Singleton);
            TestContainer.Register<PlanetService>(Lifestyle.Singleton);
            TestContainer.Register<PlayerService>(Lifestyle.Singleton);
            TestContainer.Register<FleetService>(Lifestyle.Singleton);
            TestContainer.Register<FleetSpecService>(Lifestyle.Singleton);
            TestContainer.Register<FleetScrapperService>(Lifestyle.Singleton);
            TestContainer.Register<Researcher>(Lifestyle.Singleton);
            TestContainer.Register<PlayerTechService>(Lifestyle.Singleton);
            TestContainer.Register<ShipDesignGenerator>(Lifestyle.Singleton);
            TestContainer.Register<InvasionProcessor>(Lifestyle.Singleton);
            TestContainer.Register<MineFieldDamager>(Lifestyle.Singleton);
            TestContainer.Register<MineFieldDecayer>(Lifestyle.Singleton);
            TestContainer.Register<ProductionQueueEstimator>(Lifestyle.Singleton);

            // register player intel and all discoverers
            TestContainer.Register<PlayerIntelDiscoverer>(Lifestyle.Singleton);

            TestContainer.Register<PlayerInfoDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<PlanetDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<FleetDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<ShipDesignDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<MineFieldDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<MineralPacketDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<SalvageDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<WormholeDiscoverer>(Lifestyle.Singleton);
            TestContainer.Register<MysteryTraderDiscoverer>(Lifestyle.Singleton);

            TestContainer.Verify();

        }

        /// <summary>
        /// Test helper method to return a simple game
        /// </summary>
        /// <returns>A game with one planet, one player, one fleet</returns>
        internal static (Game, GameRunner) GetSingleUnitGame()
        {
            GameRunner gameRunner = GameRunnerContainer.CreateGameRunner(new Game(), StaticTechStore.Instance);
            var game = gameRunner.Game;
            var player = new Player()
            {
                BattlePlans = new()
                {
                    new BattlePlan("Default")
                },
                PlayerRelations = new() { new PlayerRelationship(PlayerRelation.Friend) }
            };
            player.PlayerRelations.Add(new PlayerRelationship(PlayerRelation.Friend));
            player.SetupPlanMappings();

            game.Players.Add(player);

            // create an empty planet and make the player aware of it
            var starbase = CreateDesign(game, player, ShipDesigns.Starbase.Clone(player));
            var planet = new Planet()
            {
                Name = "Brin",
                Cargo = new Cargo(),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
            };
            game.AddMapObject(planet);

            // we have to discover this planet first as empty, then a second item as owned
            // because that's how the universe generation works. We do some logic on
            // switching planets from foreign to owned that could probably be improved
            // but we need this until then
            var playerIntelDiscoverer = TestContainer.GetInstance<PlayerIntelDiscoverer>();
            playerIntelDiscoverer.Discover(player, planet);

            // take ownership of this planet
            planet.ProductionQueue = new ProductionQueue();
            planet.Scanner = true;
            planet.PlayerNum = player.Num;
            planet.Population = 25000;
            planet.Starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Tokens = new List<ShipToken>()
                    {
                        new ShipToken(starbase, 1),
                    },
                BattlePlan = player.BattlePlans[0],
            };

            playerIntelDiscoverer.Discover(player, starbase);
            playerIntelDiscoverer.Discover(player, planet);


            var design = ShipDesigns.LongRangeScount.Clone(player);
            game.Designs.Add(design);

            var fleet = new Fleet()
            {
                PlayerNum = player.Num,
                Name = "Long Range Scout #1",
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
            game.AddMapObject(fleet);

            // setup mappings for the planet guids
            player.SetupMapObjectMappings();
            playerIntelDiscoverer.Discover(player, design);
            playerIntelDiscoverer.Discover(player, fleet);

            player.SetupMapObjectMappings();
            game.UpdateInternalDictionaries();
            gameRunner.ComputeSpecs();
            gameRunner.AfterTurnGeneration();

            // fill er' up
            fleet.Fuel = fleet.FuelCapacity;

            return (game, gameRunner);
        }

        /// <summary>
        /// Test helper method to return a game with 2 players, 2 planets, and 2 fleets
        /// </summary>
        /// <returns>A game with 2 players, 2 planets, and 2 fleets</returns>
        internal static (Game, GameRunner) GetTwoPlayerGame()
        {
            GameRunner gameRunner = GameRunnerContainer.CreateGameRunner(new Game(), StaticTechStore.Instance);
            var game = gameRunner.Game;
            var player1 = new Player()
            {
                Name = "Player 1",
                Num = 0,
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            var player2 = new Player()
            {
                Name = "Player 2",
                Num = 1,
                BattlePlans = new List<BattlePlan>() {
                    new BattlePlan("Default")
                }
            };
            player1.PlayerRelations.AddRange(
                new List<PlayerRelationship>()
                {
                    new PlayerRelationship(PlayerRelation.Friend),
                    new PlayerRelationship(PlayerRelation.Neutral),
                }
            );
            player2.PlayerRelations.AddRange(
                new List<PlayerRelationship>()
                {
                    new PlayerRelationship(PlayerRelation.Neutral),
                    new PlayerRelationship(PlayerRelation.Friend),
                }
            );
            game.Players.Add(player1);
            game.Players.Add(player2);

            var starbase1 = CreateDesign(game, player1, ShipDesigns.Starbase.Clone(player1));
            var starbase2 = CreateDesign(game, player2, ShipDesigns.Starbase.Clone(player2));

            var playerIntelDiscoverer = TestContainer.GetInstance<PlayerIntelDiscoverer>();
            playerIntelDiscoverer.Discover(player1, starbase1);
            playerIntelDiscoverer.Discover(player2, starbase2);

            // create empty planets and have the players discover them
            var planet1 = new Planet()
            {
                Name = "Brin",
                Position = new Vector2(0, 0),
                Cargo = new Cargo(colonists: 250),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
                Starbase = new Starbase()
                {
                    PlayerNum = player1.Num,
                    Tokens = new List<ShipToken>()
                    {
                        new ShipToken(starbase1, 1),
                    },
                    BattlePlan = player1.BattlePlans[0],
                }
            };
            planet1.Starbase.Orbiting = planet1;
            var planet2 = new Planet()
            {
                Name = "Bob",
                Position = new Vector2(200, 200),
                Cargo = new Cargo(colonists: 250),
                MineYears = new Mineral(),
                BaseHab = new Hab(50, 50, 50),
                Hab = new Hab(50, 50, 50),
                TerraformedAmount = new Hab(),
                MineralConcentration = new Mineral(100, 100, 100),
                Starbase = new Starbase()
                {
                    PlayerNum = player2.Num,
                    Tokens = new List<ShipToken>()
                    {
                        new ShipToken(starbase2, 1),
                    },
                    BattlePlan = player2.BattlePlans[0],
                }
            };
            planet2.Starbase.Orbiting = planet2;
            game.AddMapObject(planet1);
            game.AddMapObject(planet2);

            game.Planets.ForEach(planet =>
            {
                playerIntelDiscoverer.Discover(player1, planet);
                playerIntelDiscoverer.Discover(player2, planet);
            });

            // take ownership of this planet
            planet1.ProductionQueue = new ProductionQueue();
            planet1.Scanner = true;
            planet1.PlayerNum = player1.Num;
            planet1.Population = 25000;

            planet2.ProductionQueue = new ProductionQueue();
            planet2.Scanner = true;
            planet2.PlayerNum = player2.Num;
            planet2.Population = 25000;

            var design1 = ShipDesigns.LongRangeScount.Clone(player1);
            var design2 = ShipDesigns.StalwartDefender.Clone(player2);
            game.Designs.Add(design1);
            game.Designs.Add(design2);

            playerIntelDiscoverer.Discover(player1, design1);
            playerIntelDiscoverer.Discover(player2, design2);

            var fleet1 = new Fleet()
            {
                PlayerNum = player1.Num,
                Name = "Long Range Scout #1",
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
                PlayerNum = player2.Num,
                Name = "Peepers #1",
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
            game.AddMapObject(fleet1);
            game.AddMapObject(fleet2);

            player1.SetupMapObjectMappings();
            player2.SetupMapObjectMappings();
            gameRunner.ComputeSpecs();
            gameRunner.AfterTurnGeneration();

            // fill er' up
            fleet1.Fuel = fleet1.FuelCapacity;
            fleet2.Fuel = fleet2.FuelCapacity;

            return (game, gameRunner);
        }

        /// <summary>
        /// Helper method to get a long range scout fleet
        /// </summary>
        /// <returns></returns>
        internal static Fleet GetLongRangeScout(Player player)
        {
            return GetFleet(player, ShipDesigns.LongRangeScount);
        }

        /// <summary>
        /// Helper method to get a long range scout fleet
        /// </summary>
        /// <returns></returns>
        internal static Fleet GetStalwartDefender(Player player)
        {
            return GetFleet(player, ShipDesigns.StalwartDefender);
        }


        internal static Fleet GetFleet(Player player, ShipDesign sourceDesign)
        {
            var design = sourceDesign.Clone(player);

            var fleet = new Fleet()
            {
                PlayerNum = design.PlayerNum,
                Tokens = new List<ShipToken>() {
                  new ShipToken(design, 1)
                },
                BattlePlan = player.BattlePlans[0]
            };

            var fleetSpecService = TestUtils.TestContainer.GetInstance<FleetSpecService>();
            fleetSpecService.ComputeDesignSpec(player, design);
            fleetSpecService.ComputeFleetSpec(player, fleet);
            fleet.Fuel = fleet.FuelCapacity;
            return fleet;
        }

        /// <summary>
        /// Helper to create a design and add it to the game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        /// <param name="design"></param>
        /// <returns></returns>
        internal static ShipDesign CreateDesign(Game game, Player player, ShipDesign design)
        {
            var gameDesign = design.Clone(player);
            game.Designs.Add(gameDesign);
            player.Designs.Add(gameDesign);

            return gameDesign;
        }
    }
}
