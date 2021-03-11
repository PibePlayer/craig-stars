using System;

namespace CraigStars
{
    /// <summary>
    /// Represents a discoverable item like a Fleet, Planet, Design, etc
    /// </summary>
    public interface Discoverable
    {
        Player Player { get; }
        PublicPlayerInfo Owner { get; }
        Guid Guid { get; }
    }
}