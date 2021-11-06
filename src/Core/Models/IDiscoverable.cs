using System;

namespace CraigStars
{
    /// <summary>
    /// Represents a discoverable item like a Fleet, Planet, Design, etc
    /// </summary>
    public interface IDiscoverable
    {
        int PlayerNum { get; }
        Guid Guid { get; }
    }
}