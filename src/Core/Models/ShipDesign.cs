using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class ShipDesign : Discoverable
    {
        static CSLog log = LogProvider.GetLogger(typeof(ShipDesign));
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

        [JsonIgnore]
        public bool InUse { get => NumInUse > 0; }
        public int NumInUse { get; set; }
        public int NumBuilt { get; set; }

        /// <summary>
        /// An aggregate of all components of a ship design
        /// </summary>
        /// <returns></returns>
        [JsonIgnore]
        public ShipDesignAggregate Aggregate { get; } = new ShipDesignAggregate();

        // public aggregate values
        public int Shields { get => Aggregate.Shield; set => Aggregate.Shield = value; }
        public int Armor { get => Aggregate.Armor; set => Aggregate.Armor = value; }

        public ShipDesign()
        {
            EventManager.PlayerResearchLevelIncreasedEvent += OnPlayerResearchLevelIncreased;
        }

        ~ShipDesign()
        {
            EventManager.PlayerResearchLevelIncreasedEvent -= OnPlayerResearchLevelIncreased;
        }

        public override string ToString()
        {
            return $"{Player?.Name} {Name}";
        }

        /// <summary>
        /// Create a clone of this ship design
        /// </summary>
        /// <returns></returns>
        public ShipDesign Clone(Player player = null)
        {
            var design = Copy();
            design.Owner = Owner;
            design.Name = Name;
            design.Guid = Guid;
            design.Player = player != null ? player : Player;
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
                Version = Version,
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
            Aggregate.Cost = Hull.GetPlayerCost(player);
            Aggregate.SpaceDock = 0;
            Aggregate.CargoCapacity += Hull.CargoCapacity;
            Aggregate.MineSweep = 0;
            Aggregate.CloakUnits = Player.BuiltInCloaking;
            Aggregate.ReduceCloaking = 0;
            Aggregate.Bomber = false;
            Aggregate.Bombs.Clear();
            Aggregate.HasWeapons = false;
            Aggregate.WeaponSlots.Clear();
            Aggregate.Initiative = Hull.Initiative;
            Aggregate.PowerRating = 0;
            Aggregate.Movement = 0;
            Aggregate.TorpedoInaccuracyFactor = 1;
            Aggregate.NumEngines = 0;
            Aggregate.MineLayingRateByMineType = new Dictionary<MineFieldType, int>();
            Aggregate.MiningRate = 0;

            var numTachyonDetectors = 0;

            var idealSpeed = 0;

            foreach (ShipDesignSlot slot in Slots)
            {
                if (slot.HullComponent != null)
                {
                    if (slot.HullComponent is TechEngine engine)
                    {
                        Aggregate.Engine = engine;
                        idealSpeed = engine.IdealSpeed;
                        Aggregate.NumEngines += slot.Quantity;
                    }
                    if (slot.HullComponent.Category == TechCategory.BeamWeapon && slot.HullComponent.Power > 0 && (slot.HullComponent.Range + Hull.RangeBonus) > 0)
                    {
                        // mine sweep is power * (range)^2
                        var gattlingMultiplier = 1;
                        if (slot.HullComponent.Gattling)
                        {
                            // gattlings are 4x more mine-sweepery (all gatlings have range of 2)
                            // lol, 4x, get it?
                            gattlingMultiplier = slot.HullComponent.Range * slot.HullComponent.Range;
                        }
                        Aggregate.MineSweep += slot.Quantity * slot.HullComponent.Power * ((slot.HullComponent.Range + Hull.RangeBonus) * slot.HullComponent.Range) * gattlingMultiplier;
                    }
                    Cost cost = slot.HullComponent.GetPlayerCost(player) * slot.Quantity;
                    Aggregate.Cost += cost;
                    Aggregate.Mass += slot.HullComponent.Mass * slot.Quantity;
                    Aggregate.Armor += slot.HullComponent.Armor * slot.Quantity;
                    Aggregate.Shield += slot.HullComponent.Shield * slot.Quantity;
                    Aggregate.CargoCapacity += slot.HullComponent.CargoBonus * slot.Quantity;
                    Aggregate.FuelCapacity += slot.HullComponent.FuelBonus * slot.Quantity;
                    Aggregate.Colonizer = Aggregate.Colonizer || slot.HullComponent.ColonizationModule || slot.HullComponent.OrbitalConstructionModule;
                    Aggregate.Initiative += slot.HullComponent.InitiativeBonus;
                    Aggregate.Movement += slot.HullComponent.MovementBonus * slot.Quantity;
                    Aggregate.MiningRate += slot.HullComponent.MiningRate * slot.Quantity;

                    // Add this mine type to the layers this design has
                    if (slot.HullComponent.MineLayingRate > 0)
                    {
                        if (!Aggregate.MineLayingRateByMineType.ContainsKey(slot.HullComponent.MineFieldType))
                        {
                            Aggregate.MineLayingRateByMineType[slot.HullComponent.MineFieldType] = 0;
                        }
                        Aggregate.MineLayingRateByMineType[slot.HullComponent.MineFieldType] += slot.HullComponent.MineLayingRate * slot.Quantity;
                    }

                    // i.e. two .3f battle computers is (1 -.3) * (1 - .3) or (.7 * .7) or it decreases innaccuracy by 49%
                    // so a 75% accurate torpedo would be 100 - (100 - 75) * .49 = 100 - 12.25 or 88% accurate
                    // a 75% accurate torpedo with two 30% comps and one 50% comp would be
                    // 100 - (100 - 75) * .7 * .7 * .5 = 94% accurate
                    // if TorpedoInnaccuracyDecrease is 1 (default), it's just 75%
                    Aggregate.TorpedoInaccuracyFactor *= (float)Math.Pow((1 - slot.HullComponent.TorpedoBonus), slot.Quantity);

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
                        Aggregate.PowerRating += slot.HullComponent.Power * slot.Quantity;
                        Aggregate.WeaponSlots.Add(slot);
                    }

                    // cloaking
                    if (slot.HullComponent.CloakUnits > 0)
                    {
                        Aggregate.CloakUnits += slot.HullComponent.CloakUnits * slot.Quantity;
                    }
                    if (slot.HullComponent.ReduceCloaking)
                    {
                        numTachyonDetectors++;
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

            // figure out the cloak as a percentage after we aggregated our cloak units
            Aggregate.CloakPercent = CloakUtils.GetCloakPercentForCloakUnits(Aggregate.CloakUnits);

            if (numTachyonDetectors > 0)
            {
                // 95% ^ (SQRT(#_of_detectors) = Reduction factor for other player's cloaking (Capped at 81% or 17TDs)
                Aggregate.ReduceCloaking = (float)Math.Pow((100 - rules.TachyonCloakReduction) / 100f, Math.Sqrt(numTachyonDetectors));
            }

            if (Aggregate.NumEngines > 0)
            {
                // Movement = IdealEngineSpeed - 2 - Mass / 70 / NumEngines + NumManeuveringJets + 2*NumOverThrusters
                // we added any MovementBonus components above
                // we round up the slightest bit, and we can't go below 2, or above 10
                Aggregate.Movement = Mathf.Clamp((int)Math.Ceiling((double)((idealSpeed - 2) - Aggregate.Mass / 70 / Aggregate.NumEngines + Aggregate.Movement)), 2, 10);
            }
            else
            {
                Aggregate.Movement = 0;
            }

            // compute the scan ranges
            ComputeScanRanges(player);

            Aggregate.Computed = true;
        }

        /// <summary>
        /// Recompute the cost every time a player gains a level.
        /// </summary>
        void ComputeCost(Player player)
        {
            Aggregate.Cost = Hull.GetPlayerCost(player);
            foreach (ShipDesignSlot slot in Slots)
            {
                if (slot.HullComponent != null)
                {
                    Cost cost = slot.HullComponent.GetPlayerCost(Player) * slot.Quantity;
                    Aggregate.Cost += cost;
                }
            }
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

            if (Player.Race.PRT == PRT.JoaT && Hull != null && Hull.BuiltInScannerForJoaT && field == TechField.Electronics)
            {
                // update our scanner aggregate
                ComputeScanRanges(player);
            }

            ComputeCost(player);
        }

        #endregion

    }
}