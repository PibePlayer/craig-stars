using System.Collections.Generic;
using System.Linq;
using System;
using log4net;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Godot;

namespace CraigStars
{
    interface IAggregatable<T> where T : ShipDesignAggregate
    {
        T Aggregate { get; }
    }

    [JsonObject(IsReference = true)]
    public class Fleet : MapObject, SerializableMapObject, ICargoHolder, IAggregatable<FleetAggregate>
    {
        static CSLog log = LogProvider.GetLogger(typeof(Fleet));

        /// <summary>
        /// Fleets are named "Long Range Scout #9"
        /// This is the base name without the id number
        /// </summary>
        /// <value></value>
        public string BaseName { get; set; }

        #region Stats

        public int Age { get; set; }
        public Cargo Cargo { get; set; } = new Cargo();
        [JsonIgnore]
        public int AvailableCapacity { get => Aggregate.CargoCapacity - Cargo.Total; }

        public int Fuel { get; set; }
        [JsonIgnore] public int FuelCapacity { get => Aggregate.FuelCapacity; }
        [JsonIgnore] public int FuelMissing { get => Aggregate.FuelCapacity - Fuel; }
        public int Damage { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet Orbiting { get; set; }
        [JsonIgnore]
        public List<Fleet> OtherFleets { get; set; } = new List<Fleet>();

        public bool Scrapped { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
        public bool RepeatOrders { get; set; }

        [JsonProperty(IsReference = true)]
        public BattlePlan BattlePlan { get; set; }

        [JsonProperty(IsReference = true)]
        public FleetComposition FleetComposition { get; set; } = new FleetComposition();

        // These are all publicly viewable when a fleet is scanned
        public List<ShipToken> Tokens { get; set; } = new List<ShipToken>();
        public Vector2 Heading { get; set; }
        public int WarpSpeed { get; set; }
        public int IdleTurns { get; set; }

        /// <summary>
        /// During movement, we keep track of the starting position of each fleet so we can 
        /// pen scan planets as we travel through
        /// </summary>
        /// <value></value>
        [JsonIgnore] public Vector2? PreviousPosition { get; set; }

        [JsonIgnore]
        public int Mass { get => Aggregate.Mass; set => Aggregate.Mass = value; }

        public FleetAggregate Aggregate { get; set; } = new FleetAggregate();

        #endregion

        #region Serializer callbacks

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Orbiting?.OrbitingFleets.Add(this);
        }

        #endregion

        /// <summary>
        /// Get the primary shipToken, i.e. the one that is most powerful
        /// </summary>
        /// <returns></returns>
        public ShipToken GetPrimaryToken()
        {
            // TODO: write this code
            return Tokens.FirstOrDefault();
        }

        /// <summary>
        /// Attempt to transfer cargo to/from this fleet
        /// This is used to handle both immediate cargo transfers that the player made in the UI, and by the waypoint tasks
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        public bool AttemptTransfer(Cargo transfer, int fuelTransfer = 0)
        {
            var cargoResult = Cargo + transfer;
            var fuelResult = Fuel + fuelTransfer;

            if (cargoResult >= 0 && cargoResult.Total <= Aggregate.CargoCapacity && fuelResult <= Aggregate.FuelCapacity)
            {
                // The transfer doesn't leave us with 0 minerals, or with so many minerals and fuel that we can't hold it
                Cargo = cargoResult;
                Fuel = fuelResult;
                return true;
            }
            return false;
        }
    }
}
