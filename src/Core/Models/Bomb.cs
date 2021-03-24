namespace CraigStars
{
    /// <summary>
    /// For doing bomb calculations, each bomb applies as a group
    /// During fleet aggregation, we collect all bombs for a fleet
    /// to do bombing calculations
    /// </summary>
    public class Bomb
    {
        /// <summary>
        /// The number of bombs of this type
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// The percent kill rate, i.e. 3.5% of the population
        /// </summary>
        public float KillRate { get; set; }

        /// <summary>
        /// The min kill rate, i.e. 300 colonists
        /// </summary>
        public int MinKillRate { get; set; }

        /// <summary>
        /// The percent of structures destroyed
        /// </summary>
        public float StructureDestroyRate { get; set; }

        /// <summary>
        /// The percent a planet is unterraformed
        /// </summary>
        public int UnterraformRate { get; set; }
    }
}