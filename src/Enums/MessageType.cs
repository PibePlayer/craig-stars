namespace CraigStars
{
    /// <summary>
    /// The type of Message (used for filtering)
    /// </summary>
    public enum MessageType
    {
        Info,
        HomePlanet,
        PlanetDiscovery,
        PlanetProductionQueueEmpty,
        PlanetProductionQueueComplete,
        BuiltMine,
        BuiltFactory,
        BuiltDefense,
        BuiltShip,
        BuiltStarbase,
        FleetOrdersComplete,
        FleetScrapped,
        ColonizeOwnedPlanet,
        ColonizeNonPlanet,
        ColonizeWithNoColonizationModule,
        ColonizeWithNoColonists,
        PlanetColonized,
        GainTechLevel
    }
}
