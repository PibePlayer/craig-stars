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
        Info                            = 1ul << 0,
        HomePlanet                      = 1ul << 2,
        PlanetDiscovery                 = 1ul << 3,
        PlanetProductionQueueEmpty      = 1ul << 4,
        PlanetProductionQueueComplete   = 1ul << 5,
        BuiltMine                       = 1ul << 6,
        BuiltFactory                    = 1ul << 7,
        BuiltDefense                    = 1ul << 8,
        BuiltShip                       = 1ul << 9,
        BuiltStarbase                   = 1ul << 10,
        BuiltMineralPacket              = 1ul << 11,
        BuiltTerraform                  = 1ul << 12,
        FleetOrdersComplete             = 1ul << 13,
        FleetOutOfFuel                  = 1ul << 14,
        FleetGeneratedFuel              = 1ul << 15,
        FleetScrapped                   = 1ul << 16,
        Invalid                         = 1ul << 17,
        PlanetColonized                 = 1ul << 18,
        GainTechLevel                   = 1ul << 19,
        MyPlanetBombed                  = 1ul << 20,
        EnemyPlanetBombed               = 1ul << 21,
        MyPlanetInvaded                 = 1ul << 22,
        EnemyPlanetInvaded              = 1ul << 23,
        Battle                          = 1ul << 24,
        CargoTransferred                = 1ul << 26,
        MinesSwept                      = 1ul << 27,
        MinesLaid                       = 1ul << 28,
        MineFieldHit                    = 1ul << 29,
        FleetDumpedCargo                = 1ul << 30,
        FleetStargateDamaged            = 1ul << 31,
        MineralPacketCaught             = 1ul << 32,
        MineralPacketDamage             = 1ul << 33,
        MineralPacketLanded             = 1ul << 34,
        Victor                          = 1ul << 35,
        FleetReproduce                  = 1ul << 36,
        
    }
}
