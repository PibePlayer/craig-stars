using System;
using System.Collections.Generic;
using log4net;

using CraigStars.Utils;

namespace CraigStars
{
    /// <summary>
    /// Class to generate designs for players
    /// </summary>
    public class ShipDesignGenerator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShipDesignGenerator));

        public ShipDesign DesignShip(TechHull hull, String name, Player player)
        {
            var design = new ShipDesign()
            {
                Name = name,
                Player = player,
                Hull = hull
            };

            // populate each slot for this design
            hull.Slots.Each((hullSlot, index) =>
            {
                ShipDesignSlot slot = new ShipDesignSlot()
                {
                    HullSlotIndex = index + 1
                };
                switch (hullSlot.Type)
                {
                    case HullSlotType.Engine:
                        slot.HullComponent = player.GetBestEngine();
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.Scanner:
                        slot.HullComponent = player.GetBestScanner();
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.Shield:
                        slot.HullComponent = player.GetBestShield();
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.Armor:
                    case HullSlotType.ShieldArmor:
                        slot.HullComponent = player.GetBestArmor();
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.Weapon:
                        slot.HullComponent = player.GetBestBeamWeapon();
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.General:
                        // TODO: we need to make this more dynamic
                        slot.HullComponent = Techs.FuelTank;
                        slot.Quantity = hullSlot.Capacity;
                        break;
                    case HullSlotType.Mechanical:
                        if (hull.Name == "Colony Ship")
                        {
                            slot.HullComponent = Techs.ColonizationModule;
                            slot.Quantity = 1;
                        }
                        else
                        {
                            slot.HullComponent = Techs.FuelTank;
                            slot.Quantity = hullSlot.Capacity;
                        }
                        break;

                }
                design.Slots.Add(slot);
            });

            return design;
        }
    }
}