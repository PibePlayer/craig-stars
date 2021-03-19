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
        /// Default to showing all messages
        /// </summary>
        /// <value></value>
        public ulong MessageTypeFilter { get; set; } = ulong.MaxValue;

    }
}