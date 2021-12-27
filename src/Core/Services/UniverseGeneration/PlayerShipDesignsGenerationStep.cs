using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Create starting fleet and starbase designs
    /// </summary>
    public class PlayerShipDesignsGenerationStep : UniverseGenerationStep
    {
        private readonly ITechStore techStore;
        private readonly PlayerIntelDiscoverer playerIntelDiscoverer;
        private readonly ShipDesignGenerator designer;
        private readonly FleetSpecService fleetSpecService;

        public PlayerShipDesignsGenerationStep(IProvider<Game> gameProvider, ITechStore techStore, PlayerIntelDiscoverer playerIntel, ShipDesignGenerator designer, FleetSpecService fleetSpecService) : base(gameProvider, UniverseGenerationState.ShipDesigns)
        {
            this.techStore = techStore;
            this.playerIntelDiscoverer = playerIntel;
            this.designer = designer;
            this.fleetSpecService = fleetSpecService;
        }


        public override void Process()
        {
            Game.Players.ForEach(player => Game.Designs.AddRange(GetStartingShipDesigns(player)));

            // each player should discover their designs
            Game.Designs.ForEach(design =>
            {
                var player = Game.Players[design.PlayerNum];
                // setup the game design by guid dictionary
                Game.DesignsByGuid[design.Guid] = design;

                // compute aggeregates about the design
                fleetSpecService.ComputeDesignSpec(player, design);

                // let the player know about their new design
                playerIntelDiscoverer.Discover(player, design, true);
            });
        }

        internal List<ShipDesign> GetStartingShipDesigns(Player player)
        {
            List<ShipDesign> designs = new List<ShipDesign>();

            foreach (StartingFleet startingFleet in player.Race.Spec.StartingFleets)
            {
                var hull = techStore.GetTechByName<TechHull>(startingFleet.HullName);
                designs.Add(designer.DesignShip(hull, startingFleet.Name, player, player.DefaultHullSet, startingFleet.Purpose));
            }

            // starbases are special, they have a specific design that each player gets
            designs.AddRange(GetStarbaseDesigns(player));

            return designs;
        }

        /// <summary>
        /// Get starbase and orbital fort starting designs for this plyaer
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        internal List<ShipDesign> GetStarbaseDesigns(Player player)
        {
            List<ShipDesign> designs = new List<ShipDesign>();

            if (player.Race.Spec.LivesOnStarbases)
            {
                // create a starter colony for AR races
                var starterColony = new ShipDesign()
                {
                    PlayerNum = player.Num,
                    Name = "Starter Colony",
                    Purpose = ShipDesignPurpose.StarterColony,
                    Hull = Techs.OrbitalFort,
                    HullSetNumber = player.DefaultHullSet,
                    CanDelete = false,
                };
                designs.Add(starterColony);
            }

            var starbase = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Starbase",
                Purpose = ShipDesignPurpose.Starbase,
                Hull = Techs.SpaceStation,
                HullSetNumber = player.DefaultHullSet,
            };

            var startingPlanets = player.Race.Spec.StartingPlanets;

            FillStarbaseSlots(starbase, player.Race, startingPlanets[0]);
            designs.Add(starbase);

            // add an orbital fort for players that start with extra planets
            if (startingPlanets.Count > 1)
            {
                var fort = new ShipDesign()
                {
                    PlayerNum = player.Num,
                    Name = "Accelerator Platform",
                    Purpose = ShipDesignPurpose.Fort,
                    Hull = Techs.OrbitalFort,
                    HullSetNumber = player.DefaultHullSet,

                };
                // TODO: Do we want to support a PRT that includes more than 2 planets but only some of them with
                // stargates?
                FillStarbaseSlots(fort, player.Race, startingPlanets[1]);
                designs.Add(fort);
            }

            return designs;
        }

        /// <summary>
        /// Player starting starbases are all the same, regardless of starting tech level
        /// They get half filled with the starter beam, shield, and armor
        /// </summary>
        /// <param name="starbase"></param>
        internal void FillStarbaseSlots(ShipDesign starbase, Race race, StartingPlanet startingPlanet)
        {
            var beamWeapon = techStore.GetTechsByCategory(TechCategory.BeamWeapon).First() as TechHullComponent;
            var shield = techStore.GetTechsByCategory(TechCategory.Shield).First() as TechHullComponent;
            var massDriver = techStore.GetTechsByCategory(TechCategory.Orbital)
                .Cast<TechHullComponent>()
                .Where(tech => tech.PacketSpeed > 0)
                .ToArray()[0] as TechHullComponent;

            var stargate = techStore.GetTechsByCategory(TechCategory.Orbital)
                .Cast<TechHullComponent>()
                .Where(tech => tech.SafeRange > 0)
                .ToArray()[0] as TechHullComponent;

            if (beamWeapon == null)
            {
                throw new InvalidOperationException("No beam weapons found in tech store.");
            }
            else if (shield == null)
            {
                throw new InvalidOperationException("No shields found in tech store.");
            }
            else if (massDriver == null)
            {
                throw new InvalidOperationException("No mass drivers found in tech store.");
            }
            else if (stargate == null)
            {
                throw new InvalidOperationException("No stargates found in tech store.");
            }

            bool placedMassDriver = false;
            bool placedStargate = false;
            starbase.Hull.Slots.Each((slot, index) =>
            {
                switch (slot.Type)
                {
                    case HullSlotType.Weapon:
                        starbase.Slots.Add(new ShipDesignSlot(beamWeapon, index + 1, (int)Math.Round(slot.Capacity / 2.0)));
                        break;
                    case HullSlotType.Shield:
                        starbase.Slots.Add(new ShipDesignSlot(shield, index + 1, (int)Math.Round(slot.Capacity / 2.0)));
                        break;
                    case HullSlotType.Orbital:
                    case HullSlotType.OrbitalElectrical:
                        if (startingPlanet.HasStargate && !placedStargate)
                        {
                            starbase.Slots.Add(new ShipDesignSlot(stargate, index + 1, 1));
                            placedStargate = true;
                        }
                        else if (startingPlanet.HasMassDriver && !placedMassDriver)
                        {
                            starbase.Slots.Add(new ShipDesignSlot(massDriver, index + 1, 1));
                            placedMassDriver = true;
                        }
                        break;
                }
            });
        }
    }
}