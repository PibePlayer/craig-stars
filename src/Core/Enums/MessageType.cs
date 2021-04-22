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
        FleetGeneratedFuel              = 1 << 12,
        FleetScrapped                   = 1 << 13,
        Invalid                        = 1 << 14,
        PlanetColonized                 = 1 << 15,
        GainTechLevel                   = 1 << 16,
        MyPlanetBombed                  = 1 << 17,
        EnemyPlanetBombed               = 1 << 18,
        MyPlanetInvaded                 = 1 << 19,
        EnemyPlanetInvaded              = 1 << 20,
        Battle                          = 1 << 21,
        CargoTransferred                = 1 << 22,
        MinesSwept                      = 1 << 23,
        MinesLaid                       = 1 << 24,
        MineFieldHit                    = 1 << 26,
    }
}
