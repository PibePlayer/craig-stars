using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Ensure the player has the required FleetCompositions.
    /// TODO: Maybe this should just be part of the UniverseGeneration?
    /// </summary>
    public class FleetCompositionTurnProcessor : TurnProcessor
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetCompositionTurnProcessor));

        public FleetCompositionTurnProcessor() : base("Fleet Composition") { }

        /// <summary>
        /// Ensure the player has fleet compositions we want
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            foreach (FleetCompositionType type in Enum.GetValues(typeof(FleetCompositionType)))
            {
                if (type == FleetCompositionType.None)
                {
                    continue;
                }
                EnsureFleetComposition(player, type);
            }
        }

        /// <summary>
        /// Ensure that the player has all the fleet compositions we support
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        void EnsureFleetComposition(Player player, FleetCompositionType type)
        {
            if (player.FleetCompositionsByType.ContainsKey(type))
            {
                return;
            }

            FleetComposition composition = type switch
            {
                // bomber's have a couple bombers, a fighter to defend, and a fuel tanker
                FleetCompositionType.Bomber => new FleetComposition()
                {
                    Type = FleetCompositionType.Bomber,
                    Tokens = new()
                    {
                        new FleetCompositionToken(ShipDesignPurpose.Bomber, 2),
                        new FleetCompositionToken(ShipDesignPurpose.Fighter, 1),
                        new FleetCompositionToken(ShipDesignPurpose.FuelFreighter, 1),
                    }
                },
                FleetCompositionType.MineralTransport => new FleetComposition()
                {
                    Type = FleetCompositionType.MineralTransport,
                    Tokens = new()
                    {
                        new FleetCompositionToken(ShipDesignPurpose.Freighter, 2),
                        new FleetCompositionToken(ShipDesignPurpose.FuelFreighter, 1),
                    }
                },
                FleetCompositionType.RemoteMiner => new FleetComposition()
                {
                    Type = FleetCompositionType.RemoteMiner,
                    Tokens = new()
                    {
                        new FleetCompositionToken(ShipDesignPurpose.Miner, 2),
                        new FleetCompositionToken(ShipDesignPurpose.FuelFreighter, 1),
                    }
                },
                _ => null,
            };

            if (composition != null)
            {
                player.FleetCompositions.Add(composition);
                player.FleetCompositionsByGuid[composition.Guid] = composition;
                player.FleetCompositionsByType[composition.Type] = composition;
            }

        }
    }
}