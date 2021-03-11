using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// This generic base class is used for various "Discoverers" used for player's discovering fleets, design, planets, etc
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Discoverer<T> where T : Discoverable
    {
        /// <summary>
        /// Discover a new item and add it to player intel
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="itemReport"></param>
        /// <param name="penScanned"></param>
        public void Discover(Player player, T item, bool penScanned = false)
        {
            var report = GetOrCreateReport(player, item);
            if (item.Player == player)
            {
                DiscoverOwn(player, item, report);
            }
            else
            {
                DiscoverForeign(player, item, report, penScanned);
            }
        }

        # region abstract methods

        /// <summary>
        /// Each player stores a list of items they own. We will add newly created reports to this list
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected abstract List<T> GetOwnedItemReports(Player player);

        /// <summary>
        /// Each player stores a list of foriegn items reports the player has about other races
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected abstract List<T> GetForeignItemReports(Player player);

        /// <summary>
        /// Each player stores items by guid. This function should return that lookup for this item type
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected abstract Dictionary<Guid, T> GetItemsByGuid(Player player);


        /// <summary>
        /// Create an empty report from an existing item.
        /// </summary>
        /// <param name="item">The game item</param>
        /// <returns></returns>
        protected abstract T CreateEmptyReport(T item);

        /// <summary>
        /// Override this to handle discovering a player's own item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="itemReport"></param>
        protected abstract void DiscoverOwn(Player player, T item, T itemReport);

        /// <summary>
        /// Override this to handle discovering an unknowned item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="itemReport"></param>
        protected abstract void DiscoverForeign(Player player, T item, T itemReport, bool penScanned);

        #endregion

        /// <summary>
        /// Called when we create a new report
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemReport"></param>
        protected virtual void OnNewReportCreated(Player player, T item, T itemReport) { }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected T GetOrCreateReport(Player player, T item)
        {
            T report;
            var itemsByGuid = GetItemsByGuid(player);
            if (!itemsByGuid.TryGetValue(item.Guid, out report))
            {
                report = CreateEmptyReport(item);
                itemsByGuid[item.Guid] = report;

                if (item.Player == player)
                {
                    GetOwnedItemReports(player).Add(report);
                }
                else
                {
                    GetForeignItemReports(player).Add(report);
                }

                OnNewReportCreated(player, item, report);
            }

            return report;
        }
    }
}