using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class ShipDesign
    {
        public string Name { get; set; }
        public TechHull Hull { get; set; } = new TechHull();
        public int HullSetNumber { get; set; }
        public List<ShipDesignSlot> Slots { get; set; } = new List<ShipDesignSlot>();

        /// <summary>
        /// An aggregate of all components of a ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesignAggregate Aggregate { get; } = new ShipDesignAggregate();

        public void ComputeAggregate(Player player)
        {
            Aggregate.Mass = Hull.Mass;
            Aggregate.Armor = Hull.Armor;
            Aggregate.Shield = 0;
            Aggregate.CargoCapacity = 0;
            Aggregate.FuelCapacity = Hull.FuelCapacity;
            Aggregate.Colonizer = false;
            Aggregate.Cost = Hull.Cost;
            Aggregate.SpaceDock = 0;

            foreach (ShipDesignSlot slot in Slots)
            {
                if (slot.HullComponent != null)
                {
                    if (slot.HullComponent is TechEngine engine)
                    {
                        Aggregate.Engine = engine;
                    }
                    Cost cost = slot.HullComponent.Cost * slot.Quantity;
                    Aggregate.Cost += cost;
                    Aggregate.Mass += slot.HullComponent.Mass * slot.Quantity;
                    Aggregate.Armor += slot.HullComponent.Armor * slot.Quantity;
                    Aggregate.Shield += slot.HullComponent.Shield * slot.Quantity;
                    Aggregate.CargoCapacity += slot.HullComponent.CargoBonus * slot.Quantity;
                    Aggregate.FuelCapacity += slot.HullComponent.FuelBonus * slot.Quantity;
                    Aggregate.Colonizer = slot.HullComponent.ColonizationModule;
                }
                // cargo and space doc that are built into the hull
                // the space dock assumes that there is only one slot like that
                // it won't add them up

                TechHullSlot hullSlot = Hull.Slots[slot.HullSlotIndex];
                if (hullSlot.Types.Contains(HullSlotType.SpaceDock))
                {
                    Aggregate.SpaceDock = hullSlot.Capacity;
                }
                if (hullSlot.Types.Contains(HullSlotType.Cargo))
                {
                    Aggregate.CargoCapacity = Aggregate.CargoCapacity + hullSlot.Capacity;
                }
            }
            // compute the scan ranges
            ComputeScanRanges(player);
        }

        /**
         * Compute the scan ranges for this ship design The formula is: (scanner1**4 + scanner2**4 + ...
         * + scannerN**4)**(.25)
         */
        void ComputeScanRanges(Player player)
        {
            long scanRange = TechHullComponent.NoScanner;
            long scanRangePen = TechHullComponent.NoScanner;

            // compu thecanner as a built in JoaT scanner if it's build in
            if (player.Race.PRT == PRT.JoaT && Hull.BuiltInScannerForJoaT)
            {
                scanRange = (long)(player.TechLevels.Electronics * UniverseSettings.BuiltInScannerJoaTMultiplier);
                if (!player.Race.HasLRT(LRT.NAS))
                {
                    scanRangePen = (long)Math.Pow(scanRange / 2, 4);
                }
                scanRange = (long)Math.Pow(scanRange, 4);
            }

            // aggregate the scan range from each slot
            foreach (ShipDesignSlot slot in Slots)
            {
                if (slot.HullComponent != null)
                {
                    // bat scanners have 0 range
                    if (slot.HullComponent.ScanRange != TechHullComponent.NoScanner)
                    {
                        scanRange += (long)(Math.Pow(slot.HullComponent.ScanRange, 4) * slot.Quantity);
                    }

                    if (slot.HullComponent.ScanRangePen != TechHullComponent.NoScanner)
                    {
                        scanRangePen += (long)((Math.Pow(slot.HullComponent.ScanRangePen, 4)) * slot.Quantity);
                    }
                }
            }

            // now quad root it
            if (scanRange != TechHullComponent.NoScanner)
            {
                scanRange = (long)(Math.Pow(scanRange, .25));
            }

            if (scanRangePen != TechHullComponent.NoScanner)
            {
                scanRangePen = (long)(Math.Pow(scanRangePen, .25));
            }

            Aggregate.ScanRange = (int)scanRange;
            Aggregate.ScanRangePen = (int)scanRangePen;

            // if we have no pen scan but we have a regular scan, set the pen scan range to 0
            if (scanRange != TechHullComponent.NoScanner)
            {
                Aggregate.ScanRangePen = 0;
            }
            else
            {
                Aggregate.ScanRangePen = TechHullComponent.NoScanner;
            }
        }

    }
}