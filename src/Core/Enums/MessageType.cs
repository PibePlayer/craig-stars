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
        BuiltMineralPacket              = 1 << 11,
        FleetOrdersComplete             = 1 << 12,
        FleetOutOfFuel                  = 1 << 13,
        FleetGeneratedFuel              = 1 << 14,
        FleetScrapped                   = 1 << 15,
        Invalid                         = 1 << 16,
        PlanetColonized                 = 1 << 17,
        GainTechLevel                   = 1 << 18,
        MyPlanetBombed                  = 1 << 19,
        EnemyPlanetBombed               = 1 << 20,
        MyPlanetInvaded                 = 1 << 21,
        EnemyPlanetInvaded              = 1 << 22,
        Battle                          = 1 << 23,
        CargoTransferred                = 1 << 24,
        MinesSwept                      = 1 << 26,
        MinesLaid                       = 1 << 27,
        MineFieldHit                    = 1 << 28,
        FleetDumpedCargo                = 1 << 29,
        FleetStargateDamaged            = 1 << 30,
        
    }
}
