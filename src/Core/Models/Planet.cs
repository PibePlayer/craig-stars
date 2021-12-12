using System.Collections.Generic;
using System.Linq;
using System;
using Newtonsoft.Json;
using Godot;
using log4net;
using System.ComponentModel;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Planet : MapObject, SerializableMapObject, ICargoHolder
    {
        static CSLog log = LogProvider.GetLogger(typeof(Planet));

        #region Scannable Stats

        public Hab? Hab { get; set; }
        public Hab? BaseHab { get; set; }
        public Hab? TerraformedAmount { get; set; }
        public Mineral MineralConcentration { get; set; }

        [JsonIgnore]
        public int Population
        {
            get => Cargo.Colonists * 100;
            set
            {
                Cargo = Cargo.WithColonists(value / 100);
            }
        }


        [JsonIgnore] public List<Fleet> OrbitingFleets { get; set; } = new List<Fleet>();

        public Starbase Starbase { get; set; }
        public int PacketSpeed { get; set; } = 0;
        [JsonProperty(IsReference = true)]
        public Planet PacketTarget { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet RouteTarget { get; set; }

        [JsonIgnore] public bool HasStarbase { get => Starbase != null; }
        [JsonIgnore] public bool HasMassDriver { get => Starbase != null && Starbase.Spec.HasMassDriver; }
        [JsonIgnore] public bool HasStargate { get => Starbase != null && Starbase.Spec.HasStargate; }

        #endregion

        #region Planet Makeup

        public Mineral MineYears { get; set; }
        public Cargo Cargo { get; set; }

        [JsonIgnore]
        public int AvailableCapacity { get => int.MaxValue; }

        [JsonIgnore]
        public int Fuel
        {
            get => Starbase != null ? MapObject.Infinite : 0;
            set
            {
                // ignore setting fuel on a planet
            }
        }

        [JsonIgnore]
        public int FuelCapacity { get => Starbase != null ? MapObject.Infinite : 0; }

        public ProductionQueue ProductionQueue { get; set; }

        public int Mines { get; set; }
        public int Factories { get; set; }

        public int Defenses { get; set; }
        public bool ContributesOnlyLeftoverToResearch { get; set; }
        public bool Homeworld { get; set; }
        public bool Scanner { get; set; }

        [DefaultValue(Unexplored)]
        public int ReportAge { get; set; } = Unexplored;
        // true if the player has remote mined this planet
        public bool RemoteMined { get; set; } = false;
        public bool Explored { get => ReportAge != Unexplored; }

        /// <summary>
        /// The specs for this planet
        /// </summary>
        /// <returns></returns>
        public PlanetSpec Spec = new();

        #endregion

        /// <summary>
        /// The client has null values for these, but the server needs to start with
        /// an empty planet
        /// </summary>
        public void InitEmptyPlanet()
        {
            Hab = new Hab();
            MineralConcentration = new Mineral();
            Cargo = new Cargo();
            ProductionQueue = new ProductionQueue();
            MineYears = new Mineral();
        }

        /// <summary>
        /// Attempt to transfer cargo to/from this planet
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns>true if we have minerals we can transfer</returns>
        public bool AttemptTransfer(Cargo transfer, int fuel = 0)
        {
            if (fuel > 0 || fuel < 0 && Starbase == null)
            {
                // fleets can't deposit fuel onto a planet, or take fuel from a planet without a starbase
                return false;
            }

            var result = Cargo + transfer;
            if (result >= 0)
            {
                // The transfer doesn't leave us with 0 minerals, so allow it
                Cargo = result;
                return true;
            }
            return false;
        }



    }
}