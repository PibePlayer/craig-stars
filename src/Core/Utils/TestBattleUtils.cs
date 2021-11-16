using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This is a helper to generate battles
    /// </summary>
    public static class TestBattleUtils
    {
        static PlayerTechService playerTechService = new PlayerTechService(new Provider<ITechStore>(StaticTechStore.Instance));
        static IRulesProvider rulesProvider = new Game();
        static PlayerService playerService = new PlayerService(rulesProvider);
        static FleetAggregator fleetAggregator = new FleetAggregator(rulesProvider);
        static FleetService fleetService = new FleetService(fleetAggregator);
        static ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();
        static ShipDesignerTurnProcessor designerTurnProcessor = new ShipDesignerTurnProcessor(
            new ShipDesignGenerator(playerTechService, fleetAggregator),
            playerTechService,
            fleetAggregator,
            new Provider<ITechStore>(StaticTechStore.Instance)
            );

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static Game GetGameWithSimpleBattle(Player player1 = null, Player player2 = null)
        {
            if (player1 == null)
            {
                player1 = new Player()
                {
                    Num = 0,
                    Name = "Bob"
                };
            }
            if (player2 == null)
            {
                player2 = new Player()
                {
                    Num = 1,
                    Name = "Ted"
                };
            }

            player2.Race.Name = "Rabbitoid";
            player2.Race.PluralName = "Rabbitoids";

            var design1 = ShipDesigns.StalwartDefender.Clone();
            design1.PlayerNum = player1.Num;
            player1.Designs.Add(design1);
            player1.Fleets.Add(new Fleet()
            {
                PlayerNum = player1.Num,
                Name = "Attacker",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design1, 1)
                    },
                BattlePlan = new BattlePlan()
                {
                    Tactic = BattleTactic.MaximizeDamageRatio,
                    PrimaryTarget = BattleTargetType.ArmedShips,
                    SecondaryTarget = BattleTargetType.UnarmedShips,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });

            var design2 = ShipDesigns.LongRangeScount.Clone();
            design2.PlayerNum = player2.Num;
            player2.Designs.Add(design2);
            player2.Fleets.Add(new Fleet()
            {
                PlayerNum = player2.Num,
                Name = "Defender",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design2, 1)
                    },
                BattlePlan = new BattlePlan()
                {
                    Tactic = BattleTactic.Disengage,
                    PrimaryTarget = BattleTargetType.None,
                    SecondaryTarget = BattleTargetType.None,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });

            fleetAggregator.ComputePlayerAggregates(player1);
            fleetAggregator.ComputePlayerAggregates(player2);

            var game = new Game()
            {
                Players = new() { player1, player2 },
                Fleets = new() { player1.Fleets[0], player2.Fleets[0] }
            };

            return game;
        }

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static List<Fleet> GetFleetsForAllDesignBattle(PublicGameInfo gameInfo, Player player1 = null, Player player2 = null)
        {
            if (player1 == null)
            {
                player1 = new Player()
                {
                    Num = 0,
                    Name = "Bob"
                };
            }
            if (player2 == null)
            {
                player2 = new Player()
                {
                    Num = 1,
                    Name = "Ted"
                };
            }

            player2.Race.Name = "Rabbitoid";
            player2.Race.PluralName = "Rabbitoids";

            designerTurnProcessor.Process(gameInfo, player1);

            player1.Fleets.Add(new Fleet()
            {
                PlayerNum = player1.Num,
                Name = "Attacker",
                Tokens = player1.Designs.SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
            });

            designerTurnProcessor.Process(gameInfo, player2);

            player2.Fleets.Add(new Fleet()
            {
                PlayerNum = player2.Num,
                Name = "Defender",
                Tokens = player1.Designs.SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
            });


            var fleets = new List<Fleet>()
            {
                player1.Fleets[0],
                player2.Fleets[0]
            };

            fleetAggregator.ComputePlayerAggregates(player1);
            fleetAggregator.ComputePlayerAggregates(player2);

            return fleets;
        }

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static Game GetGameWithBattle(
            Player player1,
            Player player2,
            HashSet<string> player1DesignNames,
            HashSet<string> player2DesignNames
            )
        {
            Game game = new Game()
            {
                Players = new() { player1, player2 }
            };

            player2.Race.Name = "Rabbitoid";
            player2.Race.PluralName = "Rabbitoids";

            designerTurnProcessor.Process(game.GameInfo, player1);

            player1.Fleets.Add(new Fleet()
            {
                PlayerNum = player1.Num,
                Name = "Attacker",
                Tokens = player1.Designs.Where(design => player1DesignNames.Contains(design.Name)).SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
                BattlePlan = player1.BattlePlans[0]
            });

            designerTurnProcessor.Process(game.GameInfo, player2);

            player2.Fleets.Add(new Fleet()
            {
                PlayerNum = player2.Num,
                Name = "Defender",
                Tokens = player2.Designs.Where(design => player2DesignNames.Contains(design.Name)).SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
                BattlePlan = player2.BattlePlans[0]
            });

            game.Fleets = new()
            {
                player1.Fleets[0],
                player2.Fleets[0]
            };

            fleetAggregator.ComputePlayerAggregates(player1);
            fleetAggregator.ComputePlayerAggregates(player2);

            return game;
        }

        /// <summary>
        /// Gets the battle record for player1 of a two player simple fleet on fleet battle
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static BattleRecord GetSimpleBattleRecord(Player player1 = null, Player player2 = null)
        {
            var game = TestBattleUtils.GetGameWithSimpleBattle(player1, player2);
            BattleEngine battleEngine = new BattleEngine(game, fleetService, designDiscoverer);
            var battle = battleEngine.BuildBattle(game.Fleets);
            battleEngine.RunBattle(battle);

            return battle.PlayerRecords.First().Value;
        }

        /// <summary>
        /// Gets the battle record for player1 of a two player with designs by a set of names
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static BattleRecord GetDesignsBattleRecord(PublicGameInfo gameInfo, Player player1, Player player2, HashSet<string> player1DesignNames, HashSet<string> player2DesignNames)
        {
            Game game = GetGameWithBattle(player1, player2, player1DesignNames, player2DesignNames);
            BattleEngine battleEngine = new BattleEngine(game, fleetService, designDiscoverer);
            var battle = battleEngine.BuildBattle(game.Fleets);
            battleEngine.RunBattle(battle);

            return battle.PlayerRecords.First().Value;
        }
    }
}