/// <summary>
/// Each design has a purpose
/// </summary>
public enum ShipDesignPurpose
{
    Scout,
    ArmedScout, // General purpose components will be used for scanners
    Colonizer,
    Bomber,
    Fighter,
    FighterScout, // General purpose components will be used for Scanners
    CapitalShip,
    Freighter, // use extra cargo storage if possible
    FuelFreighter, // use fuel tanks where possible
    ArmedFreighter, // i.e. a Privateer
    Miner,
    Terraformer,
    DamageMineLayer,
    SpeedMineLayer,
    Starbase,
    Fort,
}
