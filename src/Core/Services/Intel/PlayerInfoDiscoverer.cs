using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering planets
    /// </summary>
    public class PlayerInfoDiscoverer
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerInfoDiscoverer));

        public void Discover(Player player, Player otherPlayer)
        {
            if (otherPlayer.Num >= 0 && otherPlayer.Num < player.PlayerInfoIntel.Count)
            {
                var playerInfo = player.PlayerInfoIntel[otherPlayer.Num];
                if (!playerInfo.Seen)
                {
                    playerInfo.Seen = true;
                    playerInfo.RaceName = otherPlayer.Race.Name;
                    playerInfo.RacePluralName = otherPlayer.Race.PluralName;

                    Message.PlayerDiscovered(player, otherPlayer);
                }
            }
            else
            {
                log.Error($"{player} cannot discover info about {otherPlayer} because {otherPlayer}'s number is out of range.");
            }
        }
    }
}