

namespace CraigStars
{
    /// <summary>
    /// The different states ships can be repaired
    /// </summary>
    public enum RepairRate
    {
        None,
        Moving,
        Stopped,
        Orbiting,
        OrbitingOwnPlanet,
        Starbase, // the rate starbases repair, not the rate a fleet repairs at a starbase, that's handled by the TechHull
    }

}
