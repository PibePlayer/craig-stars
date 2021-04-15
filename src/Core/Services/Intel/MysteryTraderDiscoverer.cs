using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering mysterytrader
    /// </summary>
    public class MysteryTraderDiscoverer : Discoverer<MysteryTrader>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MysteryTraderDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<MysteryTrader> GetOwnedItemReports(Player player) => player.MysteryTraderIntel.Owned;
        protected override List<MysteryTrader> GetForeignItemReports(Player player) => player.MysteryTraders;
        protected override Dictionary<Guid, MysteryTrader> GetItemsByGuid(Player player) => player.MysteryTradersByGuid;

        protected override MysteryTrader CreateEmptyReport(MysteryTrader item) => new MysteryTrader()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            RaceName = item.Player.Race.Name,
            RacePluralName = item.Player.Race.PluralName,
            Owner = item.Owner,
            AmountRequested = item.AmountRequested,
            WarpFactor = item.WarpFactor,
            Heading = item.Heading,
        };

        protected override void DiscoverForeign(Player player, MysteryTrader item, MysteryTrader itemReport, bool penScanned)
        {
            itemReport.Position = item.Position;
            itemReport.AmountRequested = item.AmountRequested;
            itemReport.WarpFactor = item.WarpFactor;
            itemReport.Heading = item.Heading;
        }

        protected override void DiscoverOwn(Player player, MysteryTrader item, MysteryTrader itemReport)
        {
            throw new NotSupportedException("MysteryTraders can't be owned by anyone.");
        }
    }
}