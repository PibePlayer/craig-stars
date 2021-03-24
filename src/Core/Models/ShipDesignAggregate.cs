using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// The aggregate of all the components and hull on a ship design
    /// </summary>
    public class ShipDesignAggregate
    {
        public TechEngine Engine { get; set; }
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
        public int MineSweep { get; set; }
        public float CloakPercent { get; set; }

        /// <summary>
        /// Does this design have any bombs
        /// </summary>
        public bool Bomber { get; set; }
        public List<Bomb> Bombs { get; set; } = new List<Bomb>();
        public List<Bomb> SmartBombs { get; set; } = new List<Bomb>();

        /// <summary>
        /// Does this fleet have scanning capabilities? 
        /// </summary>
        /// <value></value>
        public bool Scanner { get => ScanRange != TechHullComponent.NoScanner; }

        /// <summary>
        /// True if this ship design is in active use
        /// </summary>
        /// <value></value>
        public bool InUse { get; set; }
    }
}