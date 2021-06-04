namespace CraigStars
{
    /// <summary>
    /// This stores all the various UI settings for the player that we want saved between games
    /// </summary>
    public class PlayerUISettings
    {
        /// <summary>
        /// What view are we doing for planets, i.e. normal, percent, surface minerals?
        /// </summary>
        /// <value></value>
        public PlanetViewState PlanetViewState { get; set; }

        /// <summary>
        /// True to show planet names
        /// </summary>
        /// <value></value>
        public bool ShowPlanetNames { get; set; }

        public bool ShowScanners { get; set; }
        public int ScannerPercent { get; set; } = 100;

        /// <summary>
        /// Default to showing all messages
        /// </summary>
        /// <value></value>
        public ulong MessageTypeFilter { get; set; } = ulong.MaxValue;

    }
}