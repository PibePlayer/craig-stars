using System;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A mineral packet flying through space
    /// </summary>
    public class MineralPacket : MapObject, ICargoHolder
    {
        public Waypoint Target { get; set; } = new Waypoint();
        public Cargo Cargo { get; set; }
        
        [JsonIgnore] 
        public int AvailableCapacity { get => int.MaxValue; }
        
        [JsonIgnore] 
        public int Fuel { get => 0; set { } }

        public bool AttemptTransfer(Cargo transfer, int fuel)
        {
            throw new NotImplementedException();
        }
    }
}
