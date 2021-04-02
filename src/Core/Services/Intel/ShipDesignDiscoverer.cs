using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering ship designs
    /// </summary>
    public class ShipDesignDiscoverer : Discoverer<ShipDesign>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShipDesignDiscoverer));

        protected override List<ShipDesign> GetOwnedItemReports(Player player) => player.Designs;
        protected override List<ShipDesign> GetForeignItemReports(Player player) => player.ForeignDesigns;
        protected override Dictionary<Guid, ShipDesign> GetItemsByGuid(Player player) => player.DesignsByGuid;

        protected override ShipDesign CreateEmptyReport(ShipDesign item) => item.Clone();

        protected override void OnNewReportCreated(Player player, ShipDesign item, ShipDesign itemReport)
        {
            if (item.Player != player)
            {
                // by default, we don't know about design slots unless we pen scan
                itemReport.Slots.Clear();
            }
        }

        protected override void DiscoverForeign(Player player, ShipDesign item, ShipDesign itemReport, bool penScanned)
        {
            itemReport.Name = item.Name;
            itemReport.Owner = item.Owner;
            itemReport.Hull = item.Hull;
            itemReport.HullSetNumber = item.HullSetNumber;

            if (penScanned && itemReport.Slots.Count == 0)
            {
                // copy all the slot data so our player knows about it
                // we only add this info, we don't take it away if the fleet goes out of penScan range
                itemReport.Slots.AddRange(item.Slots.Select(slot => new ShipDesignSlot(slot.HullComponent, slot.HullSlotIndex, slot.Quantity)));
                itemReport.Armor = item.Armor;
                itemReport.Shields = item.Shields;
            }
        }

        protected override void DiscoverOwn(Player player, ShipDesign item, ShipDesign itemReport)
        {
            itemReport.Name = item.Name;
            itemReport.Owner = item.Owner;
            itemReport.Hull = item.Hull;
            itemReport.HullSetNumber = item.HullSetNumber;
            itemReport.Purpose = item.Purpose;
        }
    }
}