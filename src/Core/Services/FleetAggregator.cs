using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class FleetAggregator
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetAggregator));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public FleetAggregator(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }

        #region ShipDesigns

        /// <summary>
        /// Compute any aggreate values for a ShipDesign. Each design's aggregate values are built from their
        /// hull, hull components, and player race spec.
        /// </summary>
        /// <param name="player">The player to calculate design values for</param>
        /// <param name="design">The design to compute values for</param>
        /// <param name="recompute">By default, we only compute these once per turn, set this to true to force a recompute</param>
        public void ComputeDesignAggregate(Player player, ShipDesign design, bool recompute = false)
        {
            var aggregate = design.Aggregate;
            var hull = design.Hull;
            var slots = design.Slots;
            if (aggregate.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }
            var rules = Rules;
            aggregate.Mass = hull.Mass;
            aggregate.Armor = hull.Armor;
            aggregate.Shield = 0;
            aggregate.CargoCapacity = 0;
            aggregate.FuelCapacity = hull.FuelCapacity;
            aggregate.Colonizer = false;
            aggregate.Cost = hull.GetPlayerCost(player);
            aggregate.SpaceDock = 0;
            aggregate.CargoCapacity += hull.CargoCapacity;
            aggregate.MineSweep = 0;

            aggregate.CloakUnits = player.Race.Spec.BuiltInCloakUnits;
            aggregate.ReduceCloaking = 0;
            aggregate.Bomber = false;
            aggregate.Bombs.Clear();
            aggregate.HasWeapons = false;
            aggregate.WeaponSlots.Clear();
            aggregate.Initiative = hull.Initiative;
            aggregate.PowerRating = 0;
            aggregate.Movement = 0;
            aggregate.TorpedoInaccuracyFactor = 1;
            aggregate.NumEngines = 0;
            aggregate.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();
            aggregate.MiningRate = 0;

            var numTachyonDetectors = 0;

            var idealSpeed = 0;

            foreach (ShipDesignSlot slot in slots)
            {
                if (slot.HullComponent != null)
                {
                    if (slot.HullComponent is TechEngine engine)
                    {
                        aggregate.Engine = engine;
                        idealSpeed = engine.IdealSpeed;
                        aggregate.NumEngines += slot.Quantity;
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
                        aggregate.MineSweep += slot.Quantity * slot.HullComponent.Power * ((slot.HullComponent.Range + hull.RangeBonus) * slot.HullComponent.Range) * gattlingMultiplier;
                    }
                    Cost cost = slot.HullComponent.GetPlayerCost(player) * slot.Quantity;
                    aggregate.Cost += cost;
                    aggregate.Mass += slot.HullComponent.Mass * slot.Quantity;
                    aggregate.Armor += slot.HullComponent.Armor * slot.Quantity;
                    aggregate.Shield += slot.HullComponent.Shield * slot.Quantity;
                    aggregate.CargoCapacity += slot.HullComponent.CargoBonus * slot.Quantity;
                    aggregate.FuelCapacity += slot.HullComponent.FuelBonus * slot.Quantity;
                    aggregate.Colonizer = aggregate.Colonizer || slot.HullComponent.ColonizationModule || slot.HullComponent.OrbitalConstructionModule;
                    aggregate.Initiative += slot.HullComponent.InitiativeBonus;
                    aggregate.Movement += slot.HullComponent.MovementBonus * slot.Quantity;
                    aggregate.MiningRate += slot.HullComponent.MiningRate * slot.Quantity;

                    // Add this mine type to the layers this design has
                    if (slot.HullComponent.MineLayingRate > 0)
                    {
                        if (!aggregate.MineLayingRateByMineType.ContainsKey(slot.HullComponent.MineFieldType))
                        {
                            aggregate.MineLayingRateByMineType[slot.HullComponent.MineFieldType] = 0;
                        }
                        aggregate.MineLayingRateByMineType[slot.HullComponent.MineFieldType] += slot.HullComponent.MineLayingRate * slot.Quantity;
                    }

                    // i.e. two .3f battle computers is (1 -.3) * (1 - .3) or (.7 * .7) or it decreases innaccuracy by 49%
                    // so a 75% accurate torpedo would be 100 - (100 - 75) * .49 = 100 - 12.25 or 88% accurate
                    // a 75% accurate torpedo with two 30% comps and one 50% comp would be
                    // 100 - (100 - 75) * .7 * .7 * .5 = 94% accurate
                    // if TorpedoInnaccuracyDecrease is 1 (default), it's just 75%
                    aggregate.TorpedoInaccuracyFactor *= (float)Math.Pow((1 - slot.HullComponent.TorpedoBonus), slot.Quantity);

                    // if this slot has a bomb, this design is a bomber
                    if (slot.HullComponent.HullSlotType == HullSlotType.Bomb)
                    {
                        aggregate.Bomber = true;
                        var bomb = new Bomb()
                        {
                            Quantity = slot.Quantity,
                            KillRate = slot.HullComponent.KillRate,
                            MinKillRate = slot.HullComponent.MinKillRate,
                            StructureDestroyRate = slot.HullComponent.StructureDestroyRate,
                        };
                        if (slot.HullComponent.Smart)
                        {
                            aggregate.SmartBombs.Add(bomb);
                        }
                        else
                        {
                            aggregate.Bombs.Add(bomb);
                        }
                    }

                    if (slot.HullComponent.Power > 0)
                    {
                        aggregate.HasWeapons = true;
                        aggregate.PowerRating += slot.HullComponent.Power * slot.Quantity;
                        aggregate.WeaponSlots.Add(slot);
                    }

                    // cloaking
                    if (slot.HullComponent.CloakUnits > 0)
                    {
                        aggregate.CloakUnits += slot.HullComponent.CloakUnits * slot.Quantity;
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
                    aggregate.SpaceDock = hullSlot.Capacity;
                }
            }

            // figure out the cloak as a percentage after we aggregated our cloak units
            aggregate.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(aggregate.CloakUnits);

            if (numTachyonDetectors > 0)
            {
                // 95% ^ (SQRT(#_of_detectors) = Reduction factor for other player's cloaking (Capped at 81% or 17TDs)
                aggregate.ReduceCloaking = (float)Math.Pow((100 - rules.TachyonCloakReduction) / 100f, Math.Sqrt(numTachyonDetectors));
            }

            if (aggregate.NumEngines > 0)
            {
                // Movement = IdealEngineSpeed - 2 - Mass / 70 / NumEngines + NumManeuveringJets + 2*NumOverThrusters
                // we added any MovementBonus components above
                // we round up the slightest bit, and we can't go below 2, or above 10
                aggregate.Movement = Mathf.Clamp((int)Math.Ceiling((double)((idealSpeed - 2) - aggregate.Mass / 70 / aggregate.NumEngines + aggregate.Movement + player.Race.Spec.MovementBonus)), 2, 10);
            }
            else
            {
                aggregate.Movement = 0;
            }

            // compute the scan ranges
            ComputeDesignScanRanges(player, design);

            aggregate.Computed = true;
        }

        /// <summary>
        /// Compute the scan ranges for this ship design The formula is: (scanner1**4 + scanner2**4 + ...
        /// + scannerN**4)**(.25)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        public void ComputeDesignScanRanges(Player player, ShipDesign design)
        {
            var aggregate = design.Aggregate;
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

            // aggregate the scan range from each slot
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
            }

            if (scanRangePen != TechHullComponent.NoScanner)
            {
                scanRangePen = (long)(Math.Pow(scanRangePen, .25));
            }

            aggregate.ScanRange = (int)scanRange;
            aggregate.ScanRangePen = (int)scanRangePen;

            // if we have no pen scan but we have a regular scan, set the pen scan range to 0
            if (scanRangePen == TechHullComponent.NoScanner)
            {
                if (scanRange != TechHullComponent.NoScanner)
                {
                    aggregate.ScanRangePen = 0;
                }
                else
                {
                    aggregate.ScanRangePen = TechHullComponent.NoScanner;
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
            design.Aggregate.Cost = design.Hull.GetPlayerCost(player);
            foreach (ShipDesignSlot slot in design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    Cost cost = slot.HullComponent.GetPlayerCost(player) * slot.Quantity;
                    design.Aggregate.Cost += cost;
                }
            }
        }

        #endregion

        #region Fleets

        /// <summary>
        /// Compute the aggregates for a fleet and all its tokens. Note, this assumes the design aggregate is up to date.
        /// </summary>
        public virtual void ComputeAggregate(Player player, Fleet fleet, bool recompute = false)
        {
            var aggregate = fleet.Aggregate;
            var cargo = fleet.Cargo;
            var tokens = fleet.Tokens;

            if (aggregate.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }

            aggregate.Purposes.Clear();
            aggregate.MassEmpty = 0;
            aggregate.Mass = cargo.Total;
            aggregate.Shield = 0;
            aggregate.CargoCapacity = 0;
            aggregate.FuelCapacity = 0;
            aggregate.Colonizer = false;
            aggregate.Cost = new Cost();
            aggregate.SpaceDock = 0;
            aggregate.ScanRange = TechHullComponent.NoScanner;
            aggregate.ScanRangePen = TechHullComponent.NoScanner;
            aggregate.Engine = null;
            aggregate.MineSweep = 0;
            aggregate.MiningRate = 0;

            // Some races cloak cargo for free, otherwise
            // cloaking cargo comes at a penalty
            bool freeCargoCloaking = player.Race.Spec.FreeCargoCloaking;
            aggregate.CloakUnits = 0;
            aggregate.BaseCloakedCargo = 0;
            aggregate.ReduceCloaking = 0;

            aggregate.Bomber = false;
            aggregate.Bombs.Clear();

            aggregate.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();

            aggregate.HasWeapons = false;

            aggregate.TotalShips = 0;

            // compute each token's 
            tokens.ForEach(token =>
            {
                // update our total ship count
                aggregate.TotalShips += token.Quantity;

                aggregate.Purposes.Add(token.Design.Purpose);

                // TODO: which default engine do we use for multiple fleets?
                aggregate.Engine = token.Design.Aggregate.Engine;
                // cost
                Cost cost = token.Design.Aggregate.Cost * token.Quantity;
                aggregate.Cost += cost;

                // mass
                aggregate.Mass += token.Design.Aggregate.Mass * token.Quantity;
                aggregate.MassEmpty += token.Design.Aggregate.Mass * token.Quantity;

                // armor
                aggregate.Armor += token.Design.Aggregate.Armor * token.Quantity;

                // shield
                aggregate.Shield += token.Design.Aggregate.Shield * token.Quantity;

                // cargo
                aggregate.CargoCapacity += token.Design.Aggregate.CargoCapacity * token.Quantity;

                // fuel
                aggregate.FuelCapacity += token.Design.Aggregate.FuelCapacity * token.Quantity;

                // minesweep
                aggregate.MineSweep += token.Design.Aggregate.MineSweep * token.Quantity;

                // remote mining
                aggregate.MiningRate += token.Design.Aggregate.MiningRate * token.Quantity;

                // colonization
                if (token.Design.Aggregate.Colonizer)
                {
                    aggregate.Colonizer = true;
                }

                // aggregate all mine layers in the fleet
                if (token.Design.Aggregate.CanLayMines)
                {
                    foreach (var entry in token.Design.Aggregate.MineLayingRateByMineType)
                    {
                        if (!aggregate.MineLayingRateByMineType.ContainsKey(entry.Key))
                        {
                            aggregate.MineLayingRateByMineType[entry.Key] = 0;
                        }
                        aggregate.MineLayingRateByMineType[entry.Key] += token.Design.Aggregate.MineLayingRateByMineType[entry.Key] * token.Quantity;
                    }
                }

                // We should only have one ship stack with spacdock capabilities, but for this logic just go with the max
                aggregate.SpaceDock = Math.Max(aggregate.SpaceDock, token.Design.Aggregate.SpaceDock);

                aggregate.ScanRange = Math.Max(aggregate.ScanRange, token.Design.Aggregate.ScanRange);
                aggregate.ScanRangePen = Math.Max(aggregate.ScanRangePen, token.Design.Aggregate.ScanRangePen);

                // add bombs
                aggregate.Bomber = token.Design.Aggregate.Bomber ? true : aggregate.Bomber;
                aggregate.Bombs.AddRange(token.Design.Aggregate.Bombs);
                aggregate.SmartBombs.AddRange(token.Design.Aggregate.SmartBombs);

                // check if any tokens have weapons
                // we process weapon slots per stack, so we don't need to aggregate all
                // weapons in a fleet
                aggregate.HasWeapons = token.Design.Aggregate.HasWeapons ? true : aggregate.HasWeapons;

                if (token.Design.Aggregate.CloakUnits > 0)
                {
                    // calculate the cloak units for this token based on the design's cloak units (i.e. 70 cloak units / kT for a stealh cloak)
                    aggregate.CloakUnits += token.Design.Aggregate.CloakUnits;
                }
                else
                {
                    // if this ship doesn't have cloaking, it counts as cargo (except for races with free cargo cloaking)
                    if (!freeCargoCloaking)
                    {
                        aggregate.BaseCloakedCargo += token.Design.Aggregate.Mass * token.Quantity;
                    }
                }

                // choose the best tachyon detector ship
                aggregate.ReduceCloaking = Math.Max(aggregate.ReduceCloaking, token.Design.Aggregate.ReduceCloaking);
            });

            // compute the cloaking based on the cloak units and cargo
            ComputeFleetCloaking(player, fleet);

            // compute things about the FleetComposition
            ComputeFleetComposition(fleet);

            aggregate.Computed = true;
        }



        /// <summary>
        /// Compute the fleet's aggregate cloaking
        /// </summary>
        public void ComputeFleetCloaking(Player player, Fleet fleet)
        {
            var aggregate = fleet.Aggregate;
            var cargo = fleet.Cargo;
            var tokens = fleet.Tokens;

            // figure out how much cargo we are cloaking
            // TODO: use playerService when computeAggregate is migrated to its own service
            var cloakedCargo = aggregate.BaseCloakedCargo + (player.Race.Spec.FreeCargoCloaking ? 0 : cargo.Total);
            int cloakUnitsWithCargo = (int)Math.Round(aggregate.CloakUnits * (float)aggregate.MassEmpty / (aggregate.MassEmpty + cloakedCargo));
            aggregate.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(cloakUnitsWithCargo);
        }

        /// <summary>
        /// See how close we are to filling the composition of a fleet
        /// </summary>
        /// <param name="fleet"></param>
        public void ComputeFleetComposition(Fleet fleet)
        {
            var aggregate = fleet.Aggregate;
            var cargo = fleet.Cargo;
            var tokens = fleet.Tokens;
            var fleetComposition = fleet.FleetComposition;

            // default is we are complete if we have no fleet composition
            aggregate.FleetCompositionComplete = true;
            aggregate.FleetCompositionTokensRequired = new List<FleetCompositionToken>();

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
                aggregate.FleetCompositionTokensRequired.AddRange(fleetCompositionByPurpose.Values);
                aggregate.FleetCompositionComplete = aggregate.FleetCompositionTokensRequired.Count == 0;
            }
        }

        #endregion

        #region Starbases

        /// <summary>
        /// Starbases are like fleets but with a bit more aggregate data
        /// </summary>
        /// <param name="player"></param>
        /// <param name="starbase"></param>
        /// <param name="recompute"></param>
        public void ComputeStarbaseAggregate(Player player, Starbase starbase, bool recompute = false)
        {
            var Aggregate = starbase.Aggregate;
            var Design = starbase.Design;

            if (Aggregate.Computed && !recompute)
            {
                return;
            }

            // compute fleet aggregates as normal
            ComputeAggregate(player, starbase, recompute);

            Aggregate.Stargate = null;
            Aggregate.BasePacketSpeed = 0;
            Aggregate.SafePacketSpeed = 0;
            int numAdditionalMassDrivers = 0;

            foreach (var slot in Design.Slots)
            {
                if (slot.HullComponent != null)
                {
                    // find the first massdriver and stargate
                    if (slot.HullComponent.PacketSpeed > 0)
                    {
                        // if we already have a massdriver at this speed, add an additional mass driver to up
                        // our speed
                        if (Aggregate.BasePacketSpeed == slot.HullComponent.PacketSpeed)
                        {
                            numAdditionalMassDrivers++;
                        }
                        Aggregate.BasePacketSpeed = Math.Max(Aggregate.BasePacketSpeed, slot.HullComponent.PacketSpeed);
                    }
                    if (Aggregate.Stargate == null && slot.HullComponent.SafeHullMass > 0)
                    {
                        Aggregate.Stargate = slot.HullComponent;
                    }
                }
            }

            Aggregate.SafePacketSpeed = Aggregate.BasePacketSpeed + numAdditionalMassDrivers;
        }

        #endregion

        #region Player Aggregates

        /// <summary>
        /// Compute a player's design, fleet, and starbase aggregates for the player data
        /// </summary>
        public void ComputePlayerAggregates(Player player, bool recompute = false)
        {
            player.Designs.ForEach(design => ComputeDesignAggregate(player, design, recompute));
            player.Fleets.ForEach(fleet => ComputeAggregate(player, fleet, recompute));
            foreach (var planet in player.Planets.Where(p => p.HasStarbase))
            {
                ComputeStarbaseAggregate(player, planet.Starbase, recompute);
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