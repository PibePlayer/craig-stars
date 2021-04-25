using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering wormhole
    /// </summary>
    public class WormholeDiscoverer : Discoverer<Wormhole>
    {
        static CSLog log = LogProvider.GetLogger(typeof(WormholeDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<Wormhole> GetOwnedItemReports(Player player) => player.WormholeIntel.Owned;
        protected override List<Wormhole> GetForeignItemReports(Player player) => player.Wormholes;
        protected override Dictionary<Guid, Wormhole> GetItemsByGuid(Player player) => player.WormholesByGuid;

        protected override Wormhole CreateEmptyReport(Wormhole item) => new Wormhole()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            ReportAge = 0,
            Stability = item.Stability,
        };

        protected override void DiscoverForeign(Player player, Wormhole item, Wormhole itemReport, bool penScanned)
        {
            itemReport.ReportAge = 0;
            itemReport.Stability = item.Stability;
            itemReport.Position = item.Position;
            // if we already know about the destination, hook it up
            if (player.WormholesByGuid.TryGetValue(item.Destination.Guid, out var playerDestination))
            {
                itemReport.Destination = playerDestination;
                playerDestination.Destination = itemReport;
            }
        }

        protected override void DiscoverOwn(Player player, Wormhole item, Wormhole itemReport)
        {
            throw new NotSupportedException("Wormholes can't be owned by anyone.");
        }
    }
}