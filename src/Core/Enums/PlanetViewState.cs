namespace CraigStars
{
    /// <summary>
    /// The planet view state. Normally this would be part of the client, but I 
    /// want it saved with the player data, so I put it here
    /// </summary>
    public enum PlanetViewState
    {
        // I do enjoy the classics
        Normal,
        SurfaceMinerals,
        MineralConcentration,
        Percent,
        Population,

        /// Show a bunch of gray dots. How boring
        None,
    }
}
