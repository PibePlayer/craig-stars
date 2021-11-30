using System;

namespace CraigStars
{
    /// <summary>
    /// The type of Message (used for filtering)
    /// </summary>
    [Flags]
    public enum MessageType
    {
        None = 0,
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
        BuiltMineralPacket,
        BuiltTerraform,
        FleetOrdersComplete,
        FleetEngineFailure,
        FleetOutOfFuel,
        FleetGeneratedFuel,
        FleetScrapped,
        Invalid,
        PlanetColonized,
        GainTechLevel,
        MyPlanetBombed,
        MyPlanetRetroBombed,
        EnemyPlanetBombed,
        EnemyPlanetRetroBombed,
        MyPlanetInvaded,
        EnemyPlanetInvaded,
        Battle,
        CargoTransferred,
        MinesSwept,
        MinesLaid,
        MineFieldHit,
        FleetDumpedCargo,
        FleetStargateDamaged,
        MineralPacketCaught,
        MineralPacketDamage,
        MineralPacketLanded,
        Victor,
        FleetReproduce,
        RandomMineralDeposit,
        Permaform,
        Instaform,
        PacketTerraform,
        PacketPermaform,

    }
}
