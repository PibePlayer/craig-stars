using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering ship designs
    /// </summary>
    public class BattleDiscoverer : Discoverer<BattleRecord>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleDiscoverer));

        protected override List<BattleRecord> GetOwnedItemReports(Player player) => player.BattleIntel.Owned;
        protected override List<BattleRecord> GetForeignItemReports(Player player) => player.BattleIntel.Foriegn;
        protected override Dictionary<Guid, BattleRecord> GetItemsByGuid(Player player) => player.BattleIntel.ItemsByGuid;

        protected override BattleRecord CreateEmptyReport(BattleRecord item)
        {
            return new BattleRecord()
            {
                Guid = item.Guid,
                Owner = item.Owner
            };
        }


        protected override void DiscoverForeign(Player player, BattleRecord item, BattleRecord itemReport, bool penScanned)
        {
            throw new InvalidOperationException("BattleDiscoverer doesn't currently support discovering foreign battles");
        }

        protected override void DiscoverOwn(Player player, BattleRecord item, BattleRecord itemReport)
        {
            itemReport.Guid = item.Guid;
            itemReport.Owner = item.Owner;
            item.Tokens.ForEach(token =>
            {
                // add tokens
            });

            item.ActionsPerRound.ForEach(action =>
            {
                // add actions
            });
        }
    }
}