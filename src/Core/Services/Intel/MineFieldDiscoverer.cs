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
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldDiscoverer));

        protected override List<MineField> GetOwnedItemReports(Player player) => player.MineFields;
        protected override List<MineField> GetForeignItemReports(Player player) => player.ForeignMineFields;
        protected override Dictionary<Guid, MineField> GetItemsByGuid(Player player) => player.MineFieldsByGuid;

        protected override MineField CreateEmptyReport(MineField item) => new MineField()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            RaceName = item.RaceName,
            RacePluralName = item.RacePluralName,
            PlayerNum = item.PlayerNum,
            ReportAge = 0,
            NumMines = item.NumMines,
            Type = item.Type
        };

        protected override void DiscoverForeign(Player player, MineField item, MineField itemReport, bool penScanned)
        {
            itemReport.NumMines = item.NumMines;
            itemReport.Type = item.Type;
            itemReport.ReportAge = 0;
        }

        protected override void DiscoverOwn(Player player, MineField item, MineField itemReport)
        {
            itemReport.NumMines = item.NumMines;
            itemReport.Type = item.Type;
            itemReport.ReportAge = 0;
        }
    }
}