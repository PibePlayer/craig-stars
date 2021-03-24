using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Build and deploy bomber fleets
    /// </summary>
    public class ShipDesignerTurnProcessor : TurnProcessor
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShipDesignerTurnProcessor));

        ShipDesignGenerator shipDesignGenerator = new ShipDesignGenerator();

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(int year, Player player)
        {
            var hulls = player.TechStore.Hulls.Where(tech => player.HasTech(tech)).ToList();
            var designsByHull = player.Designs.ToLookup(design => design.Hull).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());
            var latestVersionByHull = player.Designs.ToLookup(design => design.Hull).ToDictionary(lookup => lookup.Key, lookup => lookup.Max(design => design.Version));

            var newDesigns = new List<ShipDesign>();
            var deletedDesigns = new List<ShipDesign>();
            foreach (var hull in hulls)
            {
                if (designsByHull.TryGetValue(hull, out var designs))
                {
                    // create hulls with new versions
                    int newDesignVersion = latestVersionByHull[hull] + 1;
                    // only update the latest design:
                    designs.Sort((d1, d2) => d1.Version.CompareTo(d2.Version));
                    // we have designs for this hull already, check for upgrades
                    var existingDesignForHull = designs[designs.Count - 1];
                    var updatedDesign = shipDesignGenerator.DesignShip(existingDesignForHull.Hull, existingDesignForHull.Name, player, existingDesignForHull.HullSetNumber, existingDesignForHull.Purpose);
                    if (!AreEquivalent(existingDesignForHull, updatedDesign))
                    {
                        if (!existingDesignForHull.Aggregate.InUse)
                        {
                            // this design is not in use, update the old one
                            for (int i = 0; i < existingDesignForHull.Slots.Count; i++)
                            {
                                existingDesignForHull.Slots[i].HullComponent = updatedDesign.Slots[i].HullComponent;
                                existingDesignForHull.Slots[i].Quantity = updatedDesign.Slots[i].Quantity;
                            }
                            log.Info($"{player} has upgraded {existingDesignForHull.Name} with new components.");
                        }
                        else
                        {
                            newDesigns.Add(updatedDesign);
                            updatedDesign.Version = newDesignVersion++;

                            log.Info($"{player} has upgraded {existingDesignForHull.Name} v{existingDesignForHull.Version} to {updatedDesign.Name} v{updatedDesign.Version} with new components.");
                        }

                    }
                }
                else
                {

                    // we don't have a design for this hull yet, create a new one!
                    var newDesign = shipDesignGenerator.DesignShip(hull, hull.Name, player, player.DefaultHullSet, EnumUtils.GetPurposeForTechHullType(hull.Type));
                    log.Info($"{player} has created new design {newDesign.Name} v{newDesign.Version}.");
                    newDesigns.Add(newDesign);
                }
            }

            player.Designs.AddRange(newDesigns);
            newDesigns.ForEach(design => { player.DesignsByGuid[design.Guid] = design; design.ComputeAggregate(player); });
            deletedDesigns.ForEach(design => player.DeletedDesigns.Add(design));

        }

        /// <summary>
        /// Return true if two designs have identical components
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        bool AreEquivalent(ShipDesign d1, ShipDesign d2)
        {
            if (d1.Hull != d2.Hull || d1.Slots.Count != d2.Slots.Count)
            {
                // different hulls or different slot counts, can't be equivalent
                return false;
            }


            for (int i = 0; i < d1.Slots.Count; i++)
            {
                var d1Slot = d1.Slots[i];
                var d2Slot = d2.Slots[i];

                if (d1Slot.HullComponent != d2Slot.HullComponent || d1Slot.Quantity != d2Slot.Quantity)
                {
                    return false;
                }
            }

            // if we made it here, the designs are using the same hull, same number of hull slots, and they
            // have the same components in each slot
            return true;
        }

    }
}