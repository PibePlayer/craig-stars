using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering minefield
    /// </summary>
    public class MineFieldDiscoverer : Discoverer<MineField>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MineFieldDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<MineField> GetOwnedItemReports(Player player) => player.MineFields;
        protected override List<MineField> GetForeignItemReports(Player player) => player.ForeignMineFields;
        protected override Dictionary<Guid, MineField> GetItemsByGuid(Player player) => player.MineFieldsByGuid;

        protected override MineField CreateEmptyReport(MineField item) => new MineField()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            RaceName = item.Player.Race.Name,
            RacePluralName = item.Player.Race.PluralName,
            Owner = item.Owner,
            ReportAge = 0,
            Radius = item.Radius,
            NumMines = item.NumMines,
            Type = item.Type
        };

        protected override void DiscoverForeign(Player player, MineField item, MineField itemReport, bool penScanned)
        {
            itemReport.Radius = item.Radius;
            itemReport.NumMines = item.NumMines;
            itemReport.Type = item.Type;
            itemReport.ReportAge = 0;
        }

        protected override void DiscoverOwn(Player player, MineField item, MineField itemReport)
        {
            itemReport.Player = player;
            itemReport.Radius = item.Radius;
            itemReport.NumMines = item.NumMines;
            itemReport.Type = item.Type;
            itemReport.ReportAge = 0;
        }
    }
}