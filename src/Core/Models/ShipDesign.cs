using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class ShipDesign : Discoverable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShipDesign));
        public string Name { get; set; }
        public int Version { get; set; } = 1;
        public Guid Guid { get; set; } = Guid.NewGuid();

        public PublicPlayerInfo Owner
        {
            get
            {
                if (Player != null)
                {
                    owner = Player;
                }
                return owner;
            }
            set
            {
                owner = value;
            }
        }
        PublicPlayerInfo owner;

        [JsonProperty(IsReference = true)]
        public Player Player { get; set; }

        public ShipDesignPurpose Purpose { get; set; }
        public TechHull Hull { get; set; } = new TechHull();
        public int HullSetNumber { get; set; }
        public List<ShipDesignSlot> Slots { get; set; } = new List<ShipDesignSlot>();

        /// <summary>
        /// An aggregate of all components of a ship design
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public ShipDesignAggregate Aggregate { get; } = new ShipDesignAggregate();

        public ShipDesign()
        {
            EventManager.PlayerResearchLevelIncreasedEvent += OnPlayerResearchLevelIncreased;
        }

        ~ShipDesign()
        {
            EventManager.PlayerResearchLevelIncreasedEvent -= OnPlayerResearchLevelIncreased;
        }

        /// <summary>
        /// Create a clone of this ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesign Clone()
        {
            var design = Copy();
            design.Owner = Owner;
            design.Name = Name;
            design.Guid = Guid;
            return design;
        }

        /// <summary>
        /// Create a copy of this ship design with no name
        /// </summary>
        /// <returns></returns>
        public ShipDesign Copy()
        {
            var clone = new ShipDesign()
            {
                Player = Player,
                Hull = Hull,
                HullSetNumber = HullSetNumber,
                Purpose = Purpose,
                Slots = Slots.Select(s => new ShipDesignSlot(s.HullComponent, s.HullSlotIndex, s.Quantity)).ToList()
            };
            return clone;
        }

        /// <summary>
        /// Return true if this is a valid design.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var requiredHullSlotByIndex = new Dictionary<int, TechHullSlot>();
            for (int i = 0; i < Hull.Slots.Count; i++)
            {
                if (Hull.Slots[i].Required)
                {
                    requiredHullSlotByIndex[i + 1] = Hull.Slots[i];
                }
            }
            int filledRequiredSlots = 0;
            foreach (var slot in Slots)
            {
                if (requiredHullSlotByIndex.TryGetValue(slot.HullSlotIndex, out var hullSlot))
                {
                    if (slot.Quantity == hullSlot.Capacity)
                    {
                        filledRequiredSlots++;
                    }
                }
            }

            // return true if we have filled all required slots
            return filledRequiredSlots == requiredHullSlotByIndex.Count;
        }

        public void ComputeAggregate(Player player, bool recompute = false)
        {
            if (Aggregate.Computed && !recompute)
            {
                // don't recompute unless explicitly requested
                return;
            }
            var rules = player.Rules;
            Aggregate.Mass = Hull.Mass;
            Aggregate.Armor = Hull.Armor;
            Aggregate.Shield = 0;
            Aggregate.CargoCapacity = 0;
            Aggregate.FuelCapacity = Hull.FuelCapacity;
            Aggregate.Colonizer = false;
            Aggregate.Cost = Hull.Cost;
            Aggregate.SpaceDock = 0;
            Aggregate.CargoCapacity += Hull.CargoCapacity;
            Aggregate.MineSweep = 0;
            Aggregate.CloakPercent = 0f; // TODO: compute cloaking..
            Aggregate.Bomber = false;
            Aggregate.Bombs.Clear();
            Aggregate.HasWeapons = false;
            Aggregate.Movement = 0;

            var idealSpeed = 0;
            var numEngines = 0;

            foreach (ShipDesignSlot slot in Slots)
            {
                if (slot.HullComponent != null)
                {
                    if (slot.HullComponent is TechEngine engine)
                    {
                        Aggregate.Engine = engine;
                        idealSpeed = engine.IdealSpeed;
                        numEngines += slot.Quantity;
                    }
                    if (slot.HullComponent.Category == TechCategory.BeamWeapon && slot.HullComponent.Power > 0 && slot.HullComponent.Range > 0)
                    {
                        // mine sweep is power * (range)^2
                        var gattlingMultiplier = 1;
                        if (slot.HullComponent.Gattling)
                        {
                            // gattlings are 4x more mine-sweepery
                            // lol, 4x, get it?
                            gattlingMultiplier = 4;
                        }
                        Aggregate.MineSweep += slot.Quantity * slot.HullComponent.Power * (slot.HullComponent.Range * slot.HullComponent.Range) * gattlingMultiplier;
                    }
                    Cost cost = slot.HullComponent.Cost * slot.Quantity;
                    Aggregate.Cost += cost;
                    Aggregate.Mass += slot.HullComponent.Mass * slot.Quantity;
                    Aggregate.Armor += slot.HullComponent.Armor * slot.Quantity;
                    Aggregate.Shield += slot.HullComponent.Shield * slot.Quantity;
                    Aggregate.CargoCapacity += slot.HullComponent.CargoBonus * slot.Quantity;
                    Aggregate.FuelCapacity += slot.HullComponent.FuelBonus * slot.Quantity;
                    Aggregate.Colonizer = slot.HullComponent.ColonizationModule || slot.HullComponent.OrbitalConstructionModule;
                    Aggregate.Movement += slot.HullComponent.MovementBonus * slot.Quantity;

                    // if this slot has a bomb, this design is a bomber
                    if (slot.HullComponent.HullSlotType == HullSlotType.Bomb)
                    {
                        Aggregate.Bomber = true;
                        var bomb = new Bomb()
                        {
                            Quantity = slot.Quantity,
                            KillRate = slot.HullComponent.KillRate,
                            MinKillRate = slot.HullComponent.MinKillRate,
                            StructureDestroyRate = slot.HullComponent.StructureDestroyRate,
                        };
                        if (slot.HullComponent.Smart)
                        {
                            Aggregate.SmartBombs.Add(bomb);
                        }
                        else
                        {
                            Aggregate.Bombs.Add(bomb);
                        }
                    }

                    if (slot.HullComponent.Power > 0)
                    {
                        Aggregate.HasWeapons = true;
                        Aggregate.WeaponSlots.Add(slot);
                    }
                }
                // cargo and space doc that are built into the hull
                // the space dock assumes that there is only one slot like that
                // it won't add them up

                TechHullSlot hullSlot = Hull.Slots[slot.HullSlotIndex - 1];
                if (hullSlot.Type.HasFlag(HullSlotType.SpaceDock))
                {
                    Aggregate.SpaceDock = hullSlot.Capacity;
                }
            }

            if (numEngines > 0)
            {
                // Movement = IdealEngineSpeed - 2 - Mass / 70 / NumEngines + NumManeuveringJets + 2*NumOverThrusters
                // we added any MovementBonus components above
                // we round up the slightest bit, and we can't go below 2, or above 10
                Aggregate.Movement = Mathf.Clamp((int)Math.Ceiling((double)((idealSpeed - 2) - Aggregate.Mass / 70 / numEngines + Aggregate.Movement)), 2, 10);
            }
            else
            {
                Aggregate.Movement = 0;
            }

            // this ship design is in use if any fleets use it
            if (Player != null)
            {
                if (Hull.Starbase)
                {
                    // look for starbases in use
                    Aggregate.InUse = Player.Planets.Any(planet => planet.Starbase?.Tokens[0].Design.Guid == this.Guid);
                }
                else
                {
                    // look for fleets in use
                    Aggregate.InUse = Player.Fleets.Any(fleet => fleet.Tokens.Any(token =>
                    {
                        return token.Design.Guid == this.Guid;
                    }));
                }
            }

            // compute the scan ranges
            ComputeScanRanges(player);

            Aggregate.Computed = true;
        }

        /// <summary>
        /// Compute the scan ranges for this ship design The formula is: (scanner1**4 + scanner2**4 + ...
        /// + scannerN**4)**(.25)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rules"></param>
        void ComputeScanRanges(Player player)
        {
            long scanRange = TechHullComponent.NoScanner;
            long scanRangePen = TechHullComponent.NoScanner;

            // compu thecanner as a built in JoaT scanner if it's build in
            if (player.Race.PRT == PRT.JoaT && Hull.BuiltInScannerForJoaT)
            {
                scanRange = (long)(player.TechLevels.Electronics * player.Rules.BuiltInScannerJoaTMultiplier);
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
            if (scanRangePen == TechHullComponent.NoScanner)
            {
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

        #region Event

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="field"></param>
        /// <param name="level"></param>
        void OnPlayerResearchLevelIncreased(Player player, TechField field, int level)
        {
            if (player != Player) return;

            if (player.Race != null && player.Race.PRT == PRT.JoaT && Hull != null && Hull.BuiltInScannerForJoaT)
            {
                // update our scanner aggregate
                ComputeScanRanges(player);
            }
        }

        #endregion

    }
}