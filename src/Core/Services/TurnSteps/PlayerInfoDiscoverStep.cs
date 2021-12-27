using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using static CraigStars.Utils.Utils;


namespace CraigStars
{
    /// <summary>
    /// Update a player's information about other players
    /// </summary>
    public class PlayerInfoDiscoverStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerInfoDiscoverStep));

        private readonly PlayerIntelDiscoverer playerIntelDiscoverer;

        public PlayerInfoDiscoverStep(IProvider<Game> gameProvider, PlayerIntelDiscoverer playerIntelDiscoverer) : base(gameProvider, TurnGenerationState.PlayerScanStep)
        {
            this.playerIntelDiscoverer = playerIntelDiscoverer;
        }

        public override void Process()
        {
            foreach (var player in Game.Players)
            {
                var discoveredPlayerNums = new HashSet<int>(Game.Players.Count);
                foreach (var mapObject in player.AllMapObjects.Where(it => it.Owned))
                {
                    discoveredPlayerNums.Add(mapObject.PlayerNum);
                }

                foreach (var num in discoveredPlayerNums)
                {
                    playerIntelDiscoverer.Discover(player, Game.Players[num]);
                }
            }
        }

    }
}