using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering salvage
    /// </summary>
    public class SalvageDiscoverer : Discoverer<Salvage>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SalvageDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<Salvage> GetOwnedItemReports(Player player) => player.SalvageIntel.Owned;
        protected override List<Salvage> GetForeignItemReports(Player player) => player.Salvage;
        protected override Dictionary<Guid, Salvage> GetItemsByGuid(Player player) => player.SalvageByGuid;

        protected override Salvage CreateEmptyReport(Salvage item) => new Salvage()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            Cargo = item.Cargo
        };

        protected override void DiscoverForeign(Player player, Salvage item, Salvage itemReport, bool penScanned)
        {
            itemReport.Cargo = item.Cargo;
        }

        protected override void DiscoverOwn(Player player, Salvage item, Salvage itemReport)
        {
            throw new NotSupportedException("Salvage can't be owned by anyone.");
        }
    }
}