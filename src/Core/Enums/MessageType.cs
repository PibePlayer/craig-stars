using System;

namespace CraigStars
{
    /// <summary>
    /// The type of Message (used for filtering)
    /// </summary>
    [Flags]
    public enum MessageType : ulong
    {
        None                            = 0,
        Info                            = 1 << 0,
        HomePlanet                      = 1 << 2,
        PlanetDiscovery                 = 1 << 3,
        PlanetProductionQueueEmpty      = 1 << 4,
        PlanetProductionQueueComplete   = 1 << 5,
        BuiltMine                       = 1 << 6,
        BuiltFactory                    = 1 << 7,
        BuiltDefense                    = 1 << 8,
        BuiltShip                       = 1 << 9,
        BuiltStarbase                   = 1 << 10,
        FleetOrdersComplete             = 1 << 11,
        FleetOutOfFuel                  = 1 << 12,
        FleetScrapped                   = 1 << 13,
        ColonizeOwnedPlanet             = 1 << 14,
        ColonizeNonPlanet               = 1 << 15,
        ColonizeWithNoColonizationModule= 1 << 16,
        ColonizeWithNoColonists         = 1 << 17,
        PlanetColonized                 = 1 << 18,
        GainTechLevel                   = 1 << 19,
        MyPlanetBombed                  = 1 << 20,
        EnemyPlanetBombed               = 1 << 21,
    }
}
