using System;

namespace CraigStars
{
    /// <summary>
    /// Battle tokens can have different attributes. They can be armed, and a freighter
    /// 
    /// </summary>
    [Flags]
    public enum BattleTokenAttribute
    {
        Unarmed         = 0,
        Armed           = 1 << 0,
        Bomber          = 1 << 1,
        Freighter       = 1 << 2,
        Starbase        = 1 << 3,
        FuelTransport   = 1 << 4,
    }
}
