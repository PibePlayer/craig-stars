using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class FleetSpecService
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetSpecService));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public FleetSpecService(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }

        #region ShipDesigns

        /// <summary>
        /// Compute any aggreate values for a ShipDesign. Each design's spec values are built from their
        /// hull, hull components, and player race spec.
        /// </summary>
        /// <param name="player">The player to calculate design values for</param>
        /// <param name="design">The design to compute values for</param>
        /// <param name="recompute">By default, we only compute these once per turn, set this to true to force a recompute</param>
        public void ComputeDesignSpec(Player player, ShipDesign design, bool recompute = false)
        {
            var spec = design.Spec;
            var hull = design.Hull;
            var slots = design.Slots;
            if (spec.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }
            var rules = Rules;
            spec.Mass = hull.Mass;
            spec.Armor = hull.Armor;
            spec.Shield = 0;
            spec.CargoCapacity = 0;
            spec.FuelCapacity = hull.FuelCapacity;
            spec.Colonizer = false;
            spec.Cost = hull.GetPlayerCost(player);
            spec.SpaceDock = 0;
            spec.CargoCapacity += hull.CargoCapacity;
            spec.MineSweep = 0;

            spec.CloakUnits = player.Race.Spec.BuiltInCloakUnits;
            spec.ReduceCloaking = 0;
            spec.Bomber = false;
            spec.Bombs.Clear();
            spec.HasWeapons = false;
            spec.WeaponSlots.Clear();
            spec.Initiative = hull.Initiative;
            spec.PowerRating = 0;
            spec.Movement = 0;
            spec.TorpedoInaccuracyFactor = 1;
            spec.NumEngines = 0;
            spec.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();
            spec.MiningRate = 0;
            spec.ImmuneToOwnDetonation = hull.ImmuneToOwnDetonation;
            
            spec.RepairBonus = hull.RepairBonus;


            var numTachyonDetectors = 0;

            var idealSpeed = 0;

            foreach (ShipDesignSlot slot in slots)
            {
                if (slot.HullComponent != null)
                {
                    if (slot.HullComponent is TechEngine engine)
                    {
                        spec.Engine = engine;
                        idealSpeed = engine.IdealSpeed;
                        spec.NumEngines += slot.Quantity;
                    }
                    if (slot.HullComponent.Category == TechCategory.BeamWeapon && slot.HullComponent.Power > 0 && (slot.HullComponent.Range + hull.RangeBonus) > 0)
                    {
                        // mine sweep is power * (range)^2
                        var gattlingMultiplier = 1;
                        if (slot.HullComponent.Gattling)
                        {
                            // gattlings are 4x more mine-sweepery (all gatlings have range of 2)
                            // lol, 4x, get it?
                            gattlingMultiplier = slot.HullComponent.Range * slot.HullComponent.Range;
                        }
                        spec.MineSweep += slot.Quantity * slot.HullComponent.Power * ((slot.HullComponent.Range + hull.RangeBonus) * slot.HullComponent.Range) * gattlingMultiplier;
                    }
                    Cost cost = slot.HullComponent.GetPlayerCost(player) * slot.Quantity;
                    spec.Cost += cost;
                    spec.Mass += slot.HullComponent.Mass * slot.Quantity;
                    spec.Armor += slot.HullComponent.Armor * slot.Quantity;
                    spec.Shield += slot.HullComponent.Shield * slot.Quantity;
                    spec.CargoCapacity += slot.HullComponent.CargoBonus * slot.Quantity;
                    spec.FuelCapacity += slot.HullComponent.FuelBonus * slot.Quantity;
                    spec.Colonizer = spec.Colonizer || slot.HullComponent.ColonizationModule || slot.HullComponent.OrbitalConstructionModule;
                    spec.Initiative += slot.HullComponent.InitiativeBonus;
                    spec.Movement += slot.HullComponent.MovementBonus * slot.Quantity;
                    spec.MiningRate += slot.HullComponent.MiningRate * slot.Quantity;

                    // Add this mine type to the layers this design has
                    if (slot.HullComponent.MineLayingRate > 0)
                    {
                        if (!spec.MineLayingRateByMineType.ContainsKey(slot.HullComponent.MineFieldType))
                        {
                            spec.MineLayingRateByMineType[slot.HullComponent.MineFieldType] = 0;
                        }
                        spec.MineLayingRateByMineType[slot.HullComponent.MineFieldType] += (int)(slot.HullComponent.MineLayingRate * slot.Quantity * hull.MineLayingFactor);
                    }

                    // i.e. two .3f battle computers is (1 -.3) * (1 - .3) or (.7 * .7) or it decreases innaccuracy by 49%
                    // so a 75% accurate torpedo would be 100 - (100 - 75) * .49 = 100 - 12.25 or 88% accurate
                    // a 75% accurate torpedo with two 30% comps and one 50% comp would be
                    // 100 - (100 - 75) * .7 * .7 * .5 = 94% accurate
                    // if TorpedoInnaccuracyDecrease is 1 (default), it's just 75%
                    spec.TorpedoInaccuracyFactor *= (float)Math.Pow((1 - slot.HullComponent.TorpedoBonus), slot.Quantity);

                    // if this slot has a bomb, this design is a bomber
                    if (slot.HullComponent.HullSlotType == HullSlotType.Bomb)
                    {
                        spec.Bomber = true;
                        var bomb = new Bomb()
                        {
                            Quantity = slot.Quantity,
                            KillRate = slot.HullComponent.KillRate,
                            MinKillRate = slot.HullComponent.MinKillRate,
                            StructureDestroyRate = slot.HullComponent.StructureDestroyRate,
                        };
                        if (slot.HullComponent.Smart)
                        {
                            spec.SmartBombs.Add(bomb);
                        }
                        else
                        {
                            spec.Bombs.Add(bomb);
                        }
                    }

                    if (slot.HullComponent.Power > 0)
                    {
                        spec.HasWeapons = true;
                        spec.PowerRating += slot.HullComponent.Power * slot.Quantity;
                        spec.WeaponSlots.Add(slot);
                    }

                    // cloaking
                    if (slot.HullComponent.CloakUnits > 0)
                    {
                        spec.CloakUnits += slot.HullComponent.CloakUnits * slot.Quantity;
                    }
                    if (slot.HullComponent.ReduceCloaking)
                    {
                        numTachyonDetectors++;
                    }
                }
                // cargo and space doc that are built into the hull
                // the space dock assumes that there is only one slot like that
                // it won't add them up

                TechHullSlot hullSlot = hull.Slots[slot.HullSlotIndex - 1];
                if (hullSlot.Type.HasFlag(HullSlotType.SpaceDock))
                {
                    spec.SpaceDock = hullSlot.Capacity;
                }
            }

            // figure out the cloak as a percentage after we specd our cloak units
            spec.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(spec.CloakUnits);

            if (numTachyonDetectors > 0)
            {
                // 95% ^ (SQRT(#_of_detectors) = Reduction factor for other player's cloaking (Capped at 81% or 17TDs)
                spec.ReduceCloaking = (float)Math.Pow((100 - rules.TachyonCloakReduction) / 100f, Math.Sqrt(numTachyonDetectors));
            }

            if (spec.NumEngines > 0)
            {
                // Movement = IdealEngineSpeed - 2 - Mass / 70 / NumEngines + NumManeuveringJets + 2*NumOverThrusters
                // we added any MovementBonus components above
                // we round up the slightest bit, and we can't go below 2, or above 10
                spec.Movement = Mathf.Clamp((int)Math.Ceiling((double)((idealSpeed - 2) - spec.Mass / 70 / spec.NumEngines + spec.Movement + player.Race.Spec.MovementBonus)), 2, 10);
            }
            else
            {
                spec.Movement = 0;
            }

            // compute the scan ranges
            ComputeDesignScanRanges(player, design);

            spec.Computed = true;
        }

        /// <summary>
        /// Compute the scan ranges for this ship design The formula is: (scanner1**4 + scanner2**4 + ...
        /// + scannerN**4)**(.25)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        public void ComputeDesignScanRanges(Player player, ShipDesign design)
        {
            var spec = design.Spec;
            var hull = design.Hull;
            var slots = design.Slots;

            long scanRange = TechHullComponent.NoScanner;
            long scanRangePen = TechHullComponent.NoScanner;

            // compu thecanner as a built in JoaT scanner if it's build in
            var builtInScannerMultiplier = player.Race.Spec.BuiltInScannerMultiplier;
            if (builtInScannerMultiplier > 0 && hull.BuiltInScanner)
            {
                scanRange = (long)(player.TechLevels.Electronics * builtInScannerMultiplier);
                if (!player.Race.Spec.NoAdvancedScanners)
                {
                    scanRangePen = (long)Math.Pow(scanRange / 2, 4);
                }
                scanRange = (long)Math.Pow(scanRange, 4);
            }

            // spec the scan range from each slot
            foreach (ShipDesignSlot slot in slots)
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
                scanRange = (int)(scanRange * player.Race.Spec.ScanRangeFactor);
            }

            if (scanRangePen != TechHullComponent.NoScanner)
            {
                scanRangePen = (long)(Math.Pow(scanRangePen, .25));
            }

            spec.ScanRange = (int)scanRange;
            spec.ScanRangePen = (int)scanRangePen;

            // if we have no pen scan but we have a regular scan, set the pen scan range to 0
            if (scanRangePen == TechHullComponent.NoScanner)
            {
                if (scanRange != TechHullComponent.NoScanner)
                {
                    spec.ScanRangePen = 0;
                }
                else
                {
                    spec.ScanRangePen = TechHullComponent.NoScanner;
                }
            }
        }

        #endregion

        #region Cost

        /// <summary>
        /// Our cost goes down each time a player gains a level, so it's a separate public
        /// function.
        /// </summary>
        public void ComputeDesignCost(Player player, ShipDesign design)
        {
            design.Spec.Cost = design.Hull.GetPlayerCost(player);
            float componentCostFactor = design.Hull.Starbase ? Rules.StarbaseComponentCostFactor : 1f;
            Cost componentsCost = new();
            foreach (ShipDesignSlot slot in design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    componentsCost += slot.HullComponent.GetPlayerCost(player) * slot.Quantity;
                }
            }

            design.Spec.Cost += componentsCost * componentCostFactor;

            // apply a race's starbase cost factor
            if (design.Hull.Starbase)
            {
                design.Spec.Cost *= player.Race.Spec.StarbaseCostFactor;
            }
        }

        #endregion

        #region Fleets

        /// <summary>
        /// Compute the specs for a fleet and all its tokens. Note, this assumes the design spec is up to date.
        /// </summary>
        public virtual void ComputeFleetSpec(Player player, Fleet fleet, bool recompute = false)
        {
            var spec = fleet.Spec;
            var cargo = fleet.Cargo;
            var tokens = fleet.Tokens;

            if (spec.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }

            spec.Purposes.Clear();
            spec.MassEmpty = 0;
            spec.Mass = cargo.Total;
            spec.Shield = 0;
            spec.CargoCapacity = 0;
            spec.FuelCapacity = 0;
            spec.Colonizer = false;
            spec.Cost = new Cost();
            spec.SpaceDock = 0;
            spec.ScanRange = TechHullComponent.NoScanner;
            spec.ScanRangePen = TechHullComponent.NoScanner;
            spec.RepairBonus = 0;
            spec.Engine = null;
            spec.MineSweep = 0;
            spec.MiningRate = 0;

            // Some races cloak cargo for free, otherwise
            // cloaking cargo comes at a penalty
            bool freeCargoCloaking = player.Race.Spec.FreeCargoCloaking;
            spec.CloakUnits = 0;
            spec.BaseCloakedCargo = 0;
            spec.ReduceCloaking = 0;

            spec.Bomber = false;
            spec.Bombs.Clear();

            spec.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();

            spec.HasWeapons = false;

            spec.TotalShips = 0;

            // compute each token's 
            tokens.ForEach(token =>
            {
                // update our total ship count
                spec.TotalShips += token.Quantity;

                spec.Purposes.Add(token.Design.Purpose);

                // TODO: which default engine do we use for multiple fleets?
                spec.Engine = token.Design.Spec.Engine;
                // cost
                Cost cost = token.Design.Spec.Cost * token.Quantity;
                spec.Cost += cost;

                // mass
                spec.Mass += token.Design.Spec.Mass * token.Quantity;
                spec.MassEmpty += token.Design.Spec.Mass * token.Quantity;

                // armor
                spec.Armor += token.Design.Spec.Armor * token.Quantity;

                // shield
                spec.Shield += token.Design.Spec.Shield * token.Quantity;

                // cargo
                spec.CargoCapacity += token.Design.Spec.CargoCapacity * token.Quantity;

                // fuel
                spec.FuelCapacity += token.Design.Spec.FuelCapacity * token.Quantity;

                // minesweep
                spec.MineSweep += token.Design.Spec.MineSweep * token.Quantity;

                // remote mining
                spec.MiningRate += token.Design.Spec.MiningRate * token.Quantity;

                // colonization
                if (token.Design.Spec.Colonizer)
                {
                    spec.Colonizer = true;
                }

                // spec all mine layers in the fleet
                if (token.Design.Spec.CanLayMines)
                {
                    foreach (var entry in token.Design.Spec.MineLayingRateByMineType)
                    {
                        if (!spec.MineLayingRateByMineType.ContainsKey(entry.Key))
                        {
                            spec.MineLayingRateByMineType[entry.Key] = 0;
                        }
                        spec.MineLayingRateByMineType[entry.Key] += token.Design.Spec.MineLayingRateByMineType[entry.Key] * token.Quantity;
                    }
                }

                // We should only have one ship stack with spacdock capabilities, but for this logic just go with the max
                spec.SpaceDock = Math.Max(spec.SpaceDock, token.Design.Spec.SpaceDock);

                // sadly, the fleet only gets the best repair bonus from one design
                spec.RepairBonus = Math.Max(spec.RepairBonus, token.Design.Spec.RepairBonus);

                spec.ScanRange = Math.Max(spec.ScanRange, token.Design.Spec.ScanRange);
                spec.ScanRangePen = Math.Max(spec.ScanRangePen, token.Design.Spec.ScanRangePen);

                // add bombs
                spec.Bomber = token.Design.Spec.Bomber ? true : spec.Bomber;
                spec.Bombs.AddRange(token.Design.Spec.Bombs);
                spec.SmartBombs.AddRange(token.Design.Spec.SmartBombs);

                // check if any tokens have weapons
                // we process weapon slots per stack, so we don't need to spec all
                // weapons in a fleet
                spec.HasWeapons = token.Design.Spec.HasWeapons ? true : spec.HasWeapons;

                if (token.Design.Spec.CloakUnits > 0)
                {
                    // calculate the cloak units for this token based on the design's cloak units (i.e. 70 cloak units / kT for a stealh cloak)
                    spec.CloakUnits += token.Design.Spec.CloakUnits;
                }
                else
                {
                    // if this ship doesn't have cloaking, it counts as cargo (except for races with free cargo cloaking)
                    if (!freeCargoCloaking)
                    {
                        spec.BaseCloakedCargo += token.Design.Spec.Mass * token.Quantity;
                    }
                }

                // choose the best tachyon detector ship
                spec.ReduceCloaking = Math.Max(spec.ReduceCloaking, token.Design.Spec.ReduceCloaking);
            });

            // compute the cloaking based on the cloak units and cargo
            ComputeFleetCloaking(player, fleet);

            // compute things about the FleetComposition
            ComputeFleetComposition(fleet);

            spec.Computed = true;
        }



        /// <summary>
        /// Compute the fleet's spec cloaking
        /// </summary>
        public void ComputeFleetCloaking(Player player, Fleet fleet)
        {
            var spec = fleet.Spec;
            int cloakUnits = spec.CloakUnits;

            // starbases have no mass or cargo, but fleet cloaking is adjusted for it
            if (fleet.Spec.Mass > 0)
            {
                var cargo = fleet.Cargo;
                var tokens = fleet.Tokens;

                // figure out how much cargo we are cloaking
                var cloakedCargo = spec.BaseCloakedCargo + (player.Race.Spec.FreeCargoCloaking ? 0 : cargo.Total);
                cloakUnits = (int)Math.Round(spec.CloakUnits * (float)spec.MassEmpty / (spec.MassEmpty + cloakedCargo));
            }
            spec.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(cloakUnits);
        }

        /// <summary>
        /// See how close we are to filling the composition of a fleet
        /// </summary>
        /// <param name="fleet"></param>
        public void ComputeFleetComposition(Fleet fleet)
        {
            var spec = fleet.Spec;
            var cargo = fleet.Cargo;
            var tokens = fleet.Tokens;
            var fleetComposition = fleet.FleetComposition;

            // default is we are complete if we have no fleet composition
            spec.FleetCompositionComplete = true;
            spec.FleetCompositionTokensRequired = new List<FleetCompositionToken>();

            if (fleetComposition != null && fleetComposition.Type != FleetCompositionType.None)
            {
                var fleetCompositionByPurpose = fleetComposition.GetQuantityByPurpose();
                // check if we have the tokens we need
                foreach (var token in tokens)
                {
                    if (fleetCompositionByPurpose.TryGetValue(token.Design.Purpose, out var fleetCompositionToken))
                    {
                        int quantityRequired = token.Quantity - fleetCompositionToken.Quantity;
                        if (quantityRequired > 0)
                        {
                            // we still need some of this ShipDesignPurpose
                            fleetCompositionByPurpose[token.Design.Purpose].Quantity = quantityRequired;
                        }
                        else
                        {
                            fleetCompositionByPurpose.Remove(token.Design.Purpose);
                        }
                    }
                }

                // add all the remaining FleetCompositionTokens we need
                spec.FleetCompositionTokensRequired.AddRange(fleetCompositionByPurpose.Values);
                spec.FleetCompositionComplete = spec.FleetCompositionTokensRequired.Count == 0;
            }
        }

        #endregion

        #region Starbases

        /// <summary>
        /// Starbases are like fleets but with a bit more spec data
        /// </summary>
        /// <param name="player"></param>
        /// <param name="starbase"></param>
        /// <param name="recompute"></param>
        public void ComputeStarbaseSpec(Player player, Starbase starbase, bool recompute = false)
        {
            var spec = starbase.Spec;
            var design = starbase.Design;

            if (spec.Computed && !recompute)
            {
                return;
            }

            // compute fleet specs as normal
            ComputeFleetSpec(player, starbase, recompute);

            spec.Stargate = null;
            spec.BasePacketSpeed = 0;
            spec.SafePacketSpeed = 0;
            int numAdditionalMassDrivers = 0;

            foreach (var slot in design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    // find the first massdriver and stargate
                    if (slot.HullComponent.PacketSpeed > 0)
                    {
                        // if we already have a massdriver at this speed, add an additional mass driver to up
                        // our speed
                        if (spec.BasePacketSpeed == slot.HullComponent.PacketSpeed)
                        {
                            numAdditionalMassDrivers++;
                        }
                        spec.BasePacketSpeed = Math.Max(spec.BasePacketSpeed, slot.HullComponent.PacketSpeed);
                    }
                    if (spec.Stargate == null && slot.HullComponent.SafeHullMass > 0)
                    {
                        spec.Stargate = slot.HullComponent;
                    }
                }
            }

            spec.SafePacketSpeed = spec.BasePacketSpeed + numAdditionalMassDrivers;

            // update cloaking for built in fleet cloaking
            if (player.Race.Spec.StarbaseBuiltInCloakUnits > 0)
            {
                spec.CloakUnits += player.Race.Spec.StarbaseBuiltInCloakUnits;
                ComputeFleetCloaking(player, starbase);
            }

        }

        #endregion

        #region Player Specs

        /// <summary>
        /// Compute a player's design, fleet, and starbase specs for the player data
        /// </summary>
        public void ComputePlayerFleetSpecs(Player player, bool recompute = false)
        {
            player.Designs.ForEach(design => ComputeDesignSpec(player, design, recompute));
            player.Fleets.ForEach(fleet => ComputeFleetSpec(player, fleet, recompute));
            foreach (var planet in player.Planets.Where(p => p.HasStarbase))
            {
                ComputeStarbaseSpec(player, planet.Starbase, recompute);
            }

            ComputeDesignsInUse(player);
        }

        /// <summary>
        /// Players need to know which designs are in use and which are not. This determines that and stores it in the design
        /// </summary>
        /// <param name="player"></param>
        void ComputeDesignsInUse(Player player)
        {
            // sum up all designs in use
            // get fleet designs
            var designsInUse = player.Fleets.SelectMany(f => f.Tokens).Select(token => token.Design).ToLookup(design => design).ToDictionary(lookup => lookup.Key, lookup => lookup.Count());
            // add in starbase designs
            var starbaseDesignsInUse = player.Planets.Where(planet => planet.HasStarbase).Select(planet => planet.Starbase.Design).ToLookup(design => design).ToDictionary(lookup => lookup.Key, lookup => lookup.Count());
            foreach (var starbaseDesign in starbaseDesignsInUse)
            {
                designsInUse.Add(starbaseDesign.Key, starbaseDesign.Value);
            }

            player.Designs.ForEach(design =>
            {
                if (designsInUse.TryGetValue(design, out int numInUse))
                {
                    design.NumInUse = numInUse;
                }
            });
        }

        #endregion
    }
}