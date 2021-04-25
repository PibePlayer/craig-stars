using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering MineralPacket
    /// </summary>
    public class MineralPacketDiscoverer : Discoverer<MineralPacket>
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineralPacketDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<MineralPacket> GetOwnedItemReports(Player player) => player.MineralPackets;
        protected override List<MineralPacket> GetForeignItemReports(Player player) => player.ForeignMineralPackets;
        protected override Dictionary<Guid, MineralPacket> GetItemsByGuid(Player player) => player.MineralPacketsByGuid;

        protected override MineralPacket CreateEmptyReport(MineralPacket item) => new MineralPacket()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            RaceName = item.Player.Race.Name,
            RacePluralName = item.Player.Race.PluralName,
            Owner = item.Owner,
            WarpFactor = item.WarpFactor,
            Heading = item.Heading,
        };

        protected override void DiscoverForeign(Player player, MineralPacket item, MineralPacket itemReport, bool penScanned)
        {
            itemReport.WarpFactor = item.WarpFactor;
            itemReport.Heading = item.Heading;
            itemReport.Cargo = item.Cargo;
        }

        protected override void DiscoverOwn(Player player, MineralPacket item, MineralPacket itemReport)
        {
            itemReport.Player = player;
            itemReport.WarpFactor = item.WarpFactor;
            itemReport.Heading = item.Heading;
            itemReport.Cargo = item.Cargo;
            // copy target
            if (player.MapObjectsByGuid.TryGetValue(item.Target.Guid, out var playerPlanet))
            {
                itemReport.Target = playerPlanet as Planet;
            }
            else
            {
                log.Error($"Game Packet has target {item.Target} but no matching planet in player.MapObjectsByGuid.");
            }
        }
    }
}