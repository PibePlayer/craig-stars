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
        private readonly PlayerIntel playerIntel;
        private readonly ShipDesignGenerator designer;

        public PlayerShipDesignsGenerationStep(IProvider<Game> gameProvider, ITechStore techStore, PlayerIntel playerIntel, ShipDesignGenerator designer) : base(gameProvider, UniverseGenerationState.ShipDesigns)
        {
            this.techStore = techStore;
            this.playerIntel = playerIntel;
            this.designer = designer;
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
                design.ComputeAggregate(player);

                // let the player know about their new design
                playerIntel.Discover(player, design, true);
            });
        }

        internal List<ShipDesign> GetStartingShipDesigns(Player player)
        {
            List<ShipDesign> designs = new List<ShipDesign>();

            // every player gets scouts and colony ships
            designs.Add(designer.DesignShip(Techs.Scout, "Long Range Scout", player, player.DefaultHullSet, ShipDesignPurpose.Scout));
            designs.Add(designer.DesignShip(Techs.ColonyShip, "Santa Maria", player, player.DefaultHullSet, ShipDesignPurpose.Colonizer));

            // PRT specific starting designs
            switch (player.Race.PRT)
            {
                case PRT.SD:
                    designs.Add(designer.DesignShip(Techs.MiniMineLayer, "Little Hen", player, player.DefaultHullSet, ShipDesignPurpose.DamageMineLayer));
                    designs.Add(designer.DesignShip(Techs.MiniMineLayer, "Speed Turtle", player, player.DefaultHullSet, ShipDesignPurpose.SpeedMineLayer));
                    break;
                case PRT.IT:
                    designs.Add(designer.DesignShip(Techs.Privateer, "Swashbuckler", player, player.DefaultHullSet, ShipDesignPurpose.ArmedFreighter));
                    designs.Add(designer.DesignShip(Techs.Destroyer, "Stalwart Defender", player, player.DefaultHullSet, ShipDesignPurpose.FighterScout));
                    break;
                case PRT.JoaT:
                    designs.Add(designer.DesignShip(Techs.Scout, "Armed Probe", player, player.DefaultHullSet, ShipDesignPurpose.ArmedScout));
                    designs.Add(designer.DesignShip(Techs.MediumFreighter, "Teamster", player, player.DefaultHullSet, ShipDesignPurpose.Freighter));
                    designs.Add(designer.DesignShip(Techs.MiniMiner, "Cotton Picker", player, player.DefaultHullSet, ShipDesignPurpose.Miner));
                    designs.Add(designer.DesignShip(Techs.Destroyer, "Stalwart Defender", player, player.DefaultHullSet, ShipDesignPurpose.FighterScout));
                    break;
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
            var starbase = new ShipDesign()
            {
                PlayerNum = player.Num,
                Name = "Starbase",
                Purpose = ShipDesignPurpose.Starbase,
                Hull = Techs.SpaceStation,
                HullSetNumber = player.DefaultHullSet,
            };

            FillStarbaseSlots(starbase, player.Race);
            designs.Add(starbase);

            switch (player.Race.PRT)
            {
                case PRT.IT:
                case PRT.PP:
                    var fort = new ShipDesign()
                    {
                        PlayerNum = player.Num,
                        Name = "Accelerator Platform",
                        Purpose = ShipDesignPurpose.Fort,
                        Hull = Techs.OrbitalFort,
                        HullSetNumber = player.DefaultHullSet,

                    };
                    FillStarbaseSlots(fort, player.Race);
                    designs.Add(fort);
                    break;
            }

            return designs;
        }

        /// <summary>
        /// Player starting starbases are all the same, regardless of starting tech level
        /// They get half filled with the starter beam, shield, and armor
        /// </summary>
        /// <param name="starbase"></param>
        internal void FillStarbaseSlots(ShipDesign starbase, Race race)
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
                        if (race.PRT == PRT.IT && !placedStargate)
                        {
                            starbase.Slots.Add(new ShipDesignSlot(stargate, index + 1, 1));
                            placedStargate = true;
                        }
                        else if (race.PRT == PRT.PP && !placedMassDriver)
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