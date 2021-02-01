using System;

namespace CraigStars
{
    /// <summary>
    /// A mineral packet flying through space
    /// </summary>
    public class MineralPacket : MapObject, ICargoHolder
    {
        public Waypoint Target { get; set; } = new Waypoint();
        public Cargo Cargo { get; set; }

        public bool AttemptTransfer(Cargo transfer)
        {
            throw new NotImplementedException();
        }
    }
}
