using System.Collections.Generic;

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
        public PlanetViewState PlanetViewState { get; set; } = PlanetViewState.Normal;

        /// <summary>
        /// True to show planet names
        /// </summary>
        /// <value></value>
        public bool ShowPlanetNames { get; set; }

        /// <summary>
        /// True to show fleet token counts
        /// </summary>
        /// <value></value>
        public bool ShowFleetTokenCounts { get; set; }

        /// <summary>
        /// Toggle to show/hide scanner view
        /// </summary>
        /// <value></value>
        public bool ShowScanners { get; set; } = true;

        /// <summary>
        /// Toggle to show/hide minefields
        /// </summary>
        /// <value></value>
        public bool ShowMineFields { get; set; } = true;

        /// <summary>
        /// Toggle to show only idle fleets
        /// </summary>
        /// <value></value>
        public bool ShowIdleFleetsOnly { get; set; } = false;

        /// <summary>
        /// Percent to scale scanner range by (useful for determining cloaked ship scan range)
        /// </summary>
        /// <value></value>
        public int ScannerPercent { get; set; } = 100;

        /// <summary>
        /// Percent to scale scanner range by (useful for determining cloaked ship scan range)
        /// </summary>
        /// <value></value>
        public int MineralScale { get; set; } = 5000;

        /// <summary>
        /// Default to showing all messages
        /// </summary>
        /// <value></value>
        public HashSet<MessageType> MessageTypeFilter { get; set; } = new();

    }
}