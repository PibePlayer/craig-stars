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
        static CSLog log = LogProvider.GetLogger(typeof(ShipDesignerTurnProcessor));

        private readonly ShipDesignGenerator shipDesignGenerator;
        private readonly PlayerTechService playerTechService;
        private readonly FleetAggregator fleetAggregator;
        private readonly IProvider<ITechStore> techStoreProvider;

        ITechStore techStore { get => techStoreProvider.Item; }

        public ShipDesignerTurnProcessor(ShipDesignGenerator shipDesignGenerator, PlayerTechService playerTechService, FleetAggregator fleetAggregator, IProvider<ITechStore> techStoreProvider) : base("Ship Designer")
        {
            this.shipDesignGenerator = shipDesignGenerator;
            this.playerTechService = playerTechService;
            this.fleetAggregator = fleetAggregator;
            this.techStoreProvider = techStoreProvider;
        }

        /// <summary>
        /// a new turn! build some ships
        /// </summary>
        public override void Process(PublicGameInfo gameInfo, Player player)
        {
            var hulls = techStore.Hulls.Where(tech => playerTechService.HasTech(player, tech)).ToList();
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
                        if (!existingDesignForHull.InUse)
                        {
                            // this design is not in use, update the old one
                            for (int i = 0; i < existingDesignForHull.Slots.Count; i++)
                            {
                                existingDesignForHull.Slots[i].HullComponent = updatedDesign.Slots[i].HullComponent;
                                existingDesignForHull.Slots[i].Quantity = updatedDesign.Slots[i].Quantity;
                            }
                            log.Debug($"{player} has upgraded {existingDesignForHull.Name} with new components.");
                        }
                        else
                        {
                            newDesigns.Add(updatedDesign);
                            updatedDesign.Version = newDesignVersion++;

                            log.Debug($"{player} has upgraded {existingDesignForHull.Name} v{existingDesignForHull.Version} to {updatedDesign.Name} v{updatedDesign.Version} with new components.");
                        }
                    }
                }
                else
                {
                    // we don't have a design for this hull yet, create a new one!
                    var newDesign = shipDesignGenerator.DesignShip(hull, hull.Name, player, player.DefaultHullSet, EnumUtils.GetPurposeForTechHullType(hull.Type));
                    log.Debug($"{player} has created new design {newDesign.Name} v{newDesign.Version}.");
                    newDesigns.Add(newDesign);
                }
            }

            player.Designs.AddRange(newDesigns);
            newDesigns.ForEach(design => { player.DesignsByGuid[design.Guid] = design; fleetAggregator.ComputeDesignAggregate(player, design); });
            deletedDesigns.ForEach(design => player.DeletedDesigns.Add(design));

        }

        /// <summary>
        /// Design a colonizer
        /// </summary>
        /// <param name="player"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal ShipDesign DesignColonizer(Player player, string name = null)
        {
            var colonizer = playerTechService.GetBestColonizationModule(player);
            var hull = techStore.Hulls
                .Where(hull => playerTechService.HasTech(player, hull) && hull.CanUse(colonizer))
                .OrderByDescending(hull => hull.CargoCapacity)
                .First();
            var latestVersionDesignByHull = player.Designs
                .Where(design => design.Hull == hull)
                .OrderByDescending(design => design.Version)
                .FirstOrDefault();

            if (name == null)
            {
                name = $"{hull.Name} Colonizer";
            }

            var design = shipDesignGenerator.DesignShip(hull, name, player, player.DefaultHullSet, ShipDesignPurpose.Colonizer);

            if (latestVersionDesignByHull != null)
            {
                if (AreEquivalent(latestVersionDesignByHull, design))
                {
                    design = latestVersionDesignByHull;
                }
                else
                {
                    design.Version = latestVersionDesignByHull.Version + 1;
                    log.Debug($"{player} has updated design {design.Name} v{design.Version}.");
                    player.Designs.Add(design);
                }
            }
            else
            {
                log.Debug($"{player} has created a new design {design.Name} v{design.Version}.");
                player.Designs.Add(design);
            }

            player.DesignsByGuid[design.Guid] = design;
            fleetAggregator.ComputeDesignAggregate(player, design);

            return design;
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