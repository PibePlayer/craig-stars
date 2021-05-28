using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// The aggregate of all the components and hull on a ship design
    /// </summary>
    public class ShipDesignAggregate
    {
        /// <summary>
        /// Have we already computed the aggregate for this design/fleet?
        /// </summary>
        /// <value></value>
        public bool Computed { get; set; }

        public TechEngine Engine { get; set; }
        public int NumEngines { get; set; } = 1;
        public Cost Cost { get; set; } = new Cost();
        public int Mass { get; set; }
        public int Armor { get; set; }
        public int Shield { get; set; }
        public int CargoCapacity { get; set; }
        public int FuelCapacity { get; set; }
        public int ScanRange { get; set; }
        public int ScanRangePen { get; set; }
        public bool Colonizer { get; set; }
        public int SpaceDock { get; set; }
        public int MiningRate { get; set; }

        public int MineSweep { get; set; }
        public int CloakUnits { get; set; }
        public int CloakPercent { get; set; }
        public float ReduceCloaking { get; set; }
        public float TorpedoInaccuracyFactor { get; set; }


        /// <summary>
        /// The number of movement points in battle spread across 4 rounds
        /// </summary>
        /// <value></value>
        public int Initiative { get; set; }
        public int Movement { get; set; }
        public int PowerRating { get; set; }

        /// <summary>
        /// Does this design have any bombs
        /// </summary>
        public bool Bomber { get; set; }
        public List<Bomb> Bombs { get; set; } = new List<Bomb>();
        public List<Bomb> SmartBombs { get; set; } = new List<Bomb>();

        /// <summary>
        /// Does this design have any weapons?
        /// </summary>
        public bool HasWeapons { get; set; }

        /// <summary>
        /// The ship design's weapon slots, in slot order
        /// </summary>
        public List<ShipDesignSlot> WeaponSlots = new List<ShipDesignSlot>();

        /// <summary>
        /// Does this fleet have scanning capabilities? 
        /// </summary>
        /// <value></value>
        public bool Scanner { get => ScanRange != TechHullComponent.NoScanner; }

        /// <summary>
        /// Can this design lay mines?
        /// </summary>
        public bool CanLayMines { get => MineLayingRateByMineType.Count > 0; }

        /// <summary>
        /// The total number of mines this design can lay in a year
        /// </summary>
        public Dictionary<MineFieldType, int> MineLayingRateByMineType { get; set; } = new Dictionary<MineFieldType, int>();

    }
}