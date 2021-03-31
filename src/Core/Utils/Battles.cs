using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This is a helper to generate battles
    /// </summary>
    public static class Battles
    {
        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static List<Fleet> GetFleetsForSimpleBattle(Player player1 = null, Player player2 = null)
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
            design1.Player = player1;
            player1.Designs.Add(design1);
            player1.Fleets.Add(new Fleet()
            {
                Player = player1,
                Name = "Attacker",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design1, 1)
                    },
                BattleOrders = new BattleOrders()
                {
                    Tactic = BattleTactic.MaximizeDamageRatio,
                    PrimaryTarget = BattleTargetType.ArmedShips,
                    SecondaryTarget = BattleTargetType.UnarmedShips,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });

            var design2 = ShipDesigns.LongRangeScount.Clone();
            design2.Player = player2;
            player2.Designs.Add(design2);
            player2.Fleets.Add(new Fleet()
            {
                Player = player2,
                Name = "Defender",
                Tokens = new List<ShipToken>() {
                        new ShipToken(design2, 1)
                    },
                BattleOrders = new BattleOrders()
                {
                    Tactic = BattleTactic.Disengage,
                    PrimaryTarget = BattleTargetType.None,
                    SecondaryTarget = BattleTargetType.None,
                    AttackWho = BattleAttackWho.Enemies,
                }
            });


            var fleets = new List<Fleet>()
            {
                player1.Fleets[0],
                player2.Fleets[0]
            };

            player1.ComputeAggregates();
            player2.ComputeAggregates();

            return fleets;
        }

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static List<Fleet> GetFleetsForAllDesignBattle(Player player1 = null, Player player2 = null)
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

            ShipDesignerTurnProcessor designerTurnProcessor = new ShipDesignerTurnProcessor();
            designerTurnProcessor.Process(2400, player1);

            player1.Fleets.Add(new Fleet()
            {
                Player = player1,
                Name = "Attacker",
                Tokens = player1.Designs.SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
            });

            designerTurnProcessor = new ShipDesignerTurnProcessor();
            designerTurnProcessor.Process(2400, player2);

            player2.Fleets.Add(new Fleet()
            {
                Player = player2,
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

            player1.ComputeAggregates();
            player2.ComputeAggregates();

            return fleets;
        }

        /// <summary>
        /// Build a simple battle between two players, one with a Stalwart Defender, one with a Long Range Scout
        /// </summary>
        /// <returns></returns>
        public static List<Fleet> GetFleetsForDesignsBattle(
            Player player1,
            Player player2,
            HashSet<string> player1DesignNames,
            HashSet<string> player2DesignNames
            )
        {

            player2.Race.Name = "Rabbitoid";
            player2.Race.PluralName = "Rabbitoids";

            ShipDesignerTurnProcessor designerTurnProcessor = new ShipDesignerTurnProcessor();
            designerTurnProcessor.Process(2400, player1);

            player1.Fleets.Add(new Fleet()
            {
                Player = player1,
                Name = "Attacker",
                Tokens = player1.Designs.Where(design => player1DesignNames.Contains(design.Name)).SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
            });

            designerTurnProcessor = new ShipDesignerTurnProcessor();
            designerTurnProcessor.Process(2400, player2);

            player2.Fleets.Add(new Fleet()
            {
                Player = player2,
                Name = "Defender",
                Tokens = player2.Designs.Where(design => player2DesignNames.Contains(design.Name)).SelectMany(design => new List<ShipToken>() {
                        new ShipToken(design, 1)
                    }).ToList(),
            });


            var fleets = new List<Fleet>()
            {
                player1.Fleets[0],
                player2.Fleets[0]
            };

            player1.ComputeAggregates();
            player2.ComputeAggregates();

            return fleets;
        }

        /// <summary>
        /// Gets the battle record for player1 of a two player simple fleet on fleet battle
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static BattleRecord GetSimpleBattleRecord(Player player1 = null, Player player2 = null)
        {
            BattleEngine battleEngine = new BattleEngine(new Rules());
            var battle = battleEngine.BuildBattle(GetFleetsForSimpleBattle());
            battleEngine.RunBattle(battle);

            return battle.PlayerRecords.First().Value;
        }

        /// <summary>
        /// Gets the battle record for player1 of a two player with designs by a set of names
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static BattleRecord GetDesignsBattleRecord(Player player1, Player player2, HashSet<string> player1DesignNames, HashSet<string> player2DesignNames)
        {
            BattleEngine battleEngine = new BattleEngine(new Rules());
            var battle = battleEngine.BuildBattle(GetFleetsForDesignsBattle(player1, player2, player1DesignNames, player2DesignNames));
            battleEngine.RunBattle(battle);

            return battle.PlayerRecords.First().Value;
        }
    }
}