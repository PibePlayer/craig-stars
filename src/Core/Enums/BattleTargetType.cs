namespace CraigStars
{
    /// <summary>
    /// Different orders ships can have during battle
    /// 
    /// From the help:
    /// 
    /// None/Disengage -- Don't look for a target, just attempt to disengage.
    /// Any -- Target any opponent's fleet you encounter.
    /// Starbase -- Target the opponent's starbase whether it is armed or unarmed.
    /// Armed Ships -- Target any ship or starbase carrying weapons (does not include bombers). Preference is given to the strongest tokens this token is likely to be able to hurt.
    /// Bombers/Freighters -- Target Bombers and Freighters only.
    /// Unarmed Ships -- Target any ship not carrying weapons or bombs.
    /// Fuel Transports -- Target Fuel Transports only.
    /// Freighters -- Target Unarmed Freighters only.
    /// </summary>
    public enum BattleTargetType
    {
        None,
        Any,
        Starbase,
        ArmedShips,
        BombersFreighters,
        UnarmedShips,
        FuelTransports,
        Freighters,
    }
}
