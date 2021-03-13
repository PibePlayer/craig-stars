using System.Collections.Generic;
using System.Linq;
using System;
using log4net;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Godot;

namespace CraigStars
{
    [JsonObject(IsReference = true)]
    public class Fleet : MapObject, SerializableMapObject, ICargoHolder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Fleet));

        #region Planet Stats

        public Cargo Cargo { get; set; } = new Cargo();

        [JsonIgnore]
        public int Fuel
        {
            get => Cargo.Fuel;
            set => Cargo = Cargo.WithFuel(value);
        }

        public int Damage { get; set; }

        [JsonProperty(IsReference = true)]
        public Planet Orbiting { get; set; }
        public bool Scrapped { get; set; }
        public List<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

        // These are all publicly viewable when a fleet is scanned
        public List<ShipToken> Tokens { get; set; } = new List<ShipToken>();
        public Vector2 Heading { get; set; }
        public int WarpSpeed { get; set; }
        public int Mass { get => Aggregate.Mass; set => Aggregate.Mass = value; }

        [JsonIgnore]
        public FleetAggregate Aggregate { get; } = new FleetAggregate();

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

        public void Move()
        {
            if (Waypoints.Count > 1)
            {
                Waypoint wp0 = Waypoints[0];
                Waypoint wp1 = Waypoints[1];
                float totaldist = wp0.Position.DistanceTo(wp1.Position);
                float dist = wp1.WarpFactor * wp1.WarpFactor;

                // go with the lower
                if (totaldist < dist)
                {
                    dist = totaldist;
                }

                // get the cost for the fleet
                int fuelCost = GetFuelCost(wp1.WarpFactor, dist);
                Fuel -= fuelCost;

                // assuming we move at all, make sure we are no longer orbiting any planets
                if (dist > 0 && Orbiting != null)
                {
                    Orbiting.OrbitingFleets.Remove(this);
                    Orbiting = null;
                }

                if (totaldist == dist)
                {
                    Position = wp1.Position;
                    if (wp1.Target != null && wp1.Target is Planet planet)
                    {
                        Orbiting = planet;
                        planet.OrbitingFleets.Add(this);
                    }

                    // remove the previous waypoint, it's been processed already
                    Waypoints.RemoveAt(0);

                    // we arrived, process the current task (the previous waypoint)
                    ProcessTask();
                    if (Waypoints.Count == 1)
                    {
                        Message.FleetCompletedAssignedOrders(Player, this);
                        WarpSpeed = 0;
                        Heading = Vector2.Zero;
                    }
                    else
                    {
                        wp1 = Waypoints[1];
                        WarpSpeed = wp1.WarpFactor;
                        Heading = (wp1.Position - Position).Normalized();
                    }
                }
                else
                {
                    // move this fleet closer to the next waypoint
                    WarpSpeed = wp1.WarpFactor;
                    Heading = (wp1.Position - Position).Normalized();
                    wp0.Target = null;
                    // sprite.LookAt(wp1.Position);

                    Position += Heading * dist;
                    wp0.Position = Position;
                }
            }
            else
            {
                WarpSpeed = 0;
                Heading = Vector2.Zero;
            }
        }

        /// <summary>
        /// Process the task at the current waypoint
        /// </summary>
        /// <param name="wp"></param>
        public void ProcessTask()
        {
            if (Waypoints.Count > 0)
            {
                var wp = Waypoints[0];
                log.Debug($"Processing waypoint for {Name} at {wp.TargetName} -> {wp.Task}");

                switch (wp.Task)
                {
                    case WaypointTask.Colonize:
                        Colonize(wp);
                        break;
                    case WaypointTask.LayMineField:
                        break;
                    case WaypointTask.MergeWithFleet:
                        break;
                    case WaypointTask.Patrol:
                        break;
                    case WaypointTask.RemoteMining:
                        break;
                    case WaypointTask.Route:
                        break;
                    case WaypointTask.ScrapFleet:
                        Scrap(wp);
                        break;
                    case WaypointTask.TransferFleet:
                        break;
                    case WaypointTask.Transport:
                        Transport(wp);
                        break;

                }
            }

        }


        /// <summary>
        /// Scrap this fleet, adding resources to the waypoint
        /// from the stars wiki:
        /// After battle, 1/3 of the mineral cost of the destroyed ships is left as salvage. If the battle took place in orbit, these minerals are deposited on the planet below.
        /// In deep space, each type of mineral decays 10%, or 10kT per year, whichever is higher. Salvage deposited on planets does not decay.
        /// Scrapping: (from help file)
        /// 
        /// A ship scrapped at a starbase deposits 80% of the original minerals on the planet, or 90% of the minerals and 70% of the resources if the LRT 'Ultimate Recycling' is selected.
        /// A ship scrapped at a planet with no starbase leaves 33% of the original minerals on the planet, or 45% of the minerals if the LRT Ultimate Recycling is selected.
        /// Wih UR the resources recovered is:
        /// (resources the ship costs * resources on the planet)/(resources the ship cost + resources on the planet)
        /// The maximum recoverable resources occurs when the cost of the scrapped ship equals the resources produced at the planet where it is scrapped.
        /// 
        /// A ship scrapped in space leaves no minerals behind.
        /// When a ship design is deleted, all such ships vanish leaving nothing behind. (moral: scrap before you delete!)
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>    
        void Scrap(Waypoint wp)
        {
            // create a new cargo instance out of our fleet cost
            Cargo cargo = Aggregate.Cost;

            if (Player.Race.HasLRT(LRT.UR))
            {
                // we only recover 40% of our minerals on scrapping
                cargo *= .45f;

                // TODO: handle resource gain for an occupied planet
            }
            else
            {
                // we only recover 1/3rd of our minerals on scrapping
                cargo /= 3;
            }

            // add in any cargo the fleet was holding
            cargo += Cargo;

            if (wp.Target is Planet planet)
            {
                planet.Cargo += cargo;
                planet.OrbitingFleets.Remove(this);
                Orbiting = null;
                Scrapped = true;
            }
            else
            {
                // TODO: put some scrap in space...
            }
        }

        /// <summary>
        /// Take this fleet and have it colonize a planet
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>
        void Colonize(Waypoint wp)
        {
            if (wp.Target is Planet planet)
            {
                if (planet.Player != null)
                {
                    Message.ColonizeOwnedPlanet(Player, this);
                }
                else if (!Aggregate.Colonizer)
                {
                    Message.ColonizeWithNoModule(Player, this);
                }
                else if (Cargo.Colonists <= 0)
                {
                    Message.ColonizeWithNoColonists(Player, this);
                }
                else
                {
                    // we own this planet now, yay!
                    planet.Player = Player;
                    planet.ProductionQueue = new ProductionQueue();
                    planet.Population = Cargo.Colonists * 100;
                    Cargo = Cargo.WithColonists(0);
                    Scrap(wp);
                }
            }
            else
            {
                Message.ColonizeNonPlanet(Player, this);
            }
        }

        void Transport(Waypoint wp)
        {
            if (wp.Target is ICargoHolder cargoHolder)
            {
                // how much space do we have available
                var capacity = Aggregate.CargoCapacity - Cargo.Total;

                foreach (CargoType taskType in Enum.GetValues(typeof(CargoType)))
                {
                    var task = wp.TransportTasks[taskType];
                    switch (task.action)
                    {
                        case WaypointTaskTransportAction.LoadAll:
                            // load all available, based on our constraints
                            var availableToLoad = cargoHolder.Cargo[taskType];
                            var transferAmount = Math.Min(availableToLoad, capacity);
                            if (transferAmount > 0)
                            {
                                cargoHolder.AttemptTransfer(Cargo.OfAmount(taskType, -transferAmount));
                                AttemptTransfer(Cargo.OfAmount(taskType, transferAmount));
                            }
                            break;
                    }

                }
            }
        }

        /// <summary>
        /// Fuel usage calculation courtesy of m.a@stars
        /// </summary>
        /// <param name = "warpFactor" > The warp speed 1 to 10</param>
        /// <param name = "mass" > The mass of the fleet</param>
        /// <param name = "dist" > The distance travelled</param>
        /// <param name = "ifeFactor" > The factor for improved fuel efficiency (.85 if you have the LRT)</param>
        /// <param name = "engine" > The engine being used</param>
        /// <return> The amount of mg of fuel used</return>
        internal int GetFuelCost(int warpFactor, int mass, double dist, double ifeFactor, TechEngine engine)
        {
            if (warpFactor == 0)
            {
                return 0;
            }
            // 1 mg of fuel will move 200kT of weight 1 LY at a Fuel Usage Number of 100.
            // Number of engines doesn't matter. Neither number of ships with the same engine.

            double distanceCeiling = Math.Ceiling(dist); // rounding to next integer gives best graph fit
                                                         // window.status = 'Actual distance used is ' + Distan + 'ly';

            // IFE is applied to drive specifications, just as the helpfile hints.
            // Stars! probably does it outside here once per turn per engine to save time.
            double engineEfficiency = Math.Ceiling(ifeFactor * engine.FuelUsage[warpFactor - 1]);

            // 20000 = 200*100
            // Safe bet is Stars! does all this with integer math tricks.
            // Subtracting 2000 in a loop would be a way to also get the rounding.
            // Or even bitshift for the 2 and adjust "decimal point" for the 1000
            double teorFuel = (Math.Floor(mass * engineEfficiency * distanceCeiling / 2000) / 10);
            // using only one decimal introduces another artifact: .0999 gets rounded down to .0

            // The heavier ships will benefit the most from the accuracy
            int intFuel = (int)Math.Ceiling(teorFuel);

            // That's all. Nothing really fancy, much less random. Subtle differences in
            // math lib workings might explain the rarer and smaller discrepancies observed
            return intFuel;
            // Unrelated to this fuel math are some quirks inside the
            // "negative fuel" watchdog when the remainder of the
            // trip is < 1 ly. Aahh, the joys of rounding! ;o)
        }

        /// <summary>
        /// Get the Fuel cost for this fleet to travel a certain distance at a certain speed
        /// </summary>
        /// <param name="warpFactor"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public int GetFuelCost(int warpFactor, double distance)
        {
            // figure out how much fuel we're going to use
            double ifeFactor = Player.Race.HasLRT(LRT.IFE) ? .85 : 1.0;

            int fuelCost = 0;

            // compute each ship stack separately
            foreach (var token in Tokens)
            {
                // figure out this ship stack's mass as well as it's proportion of the cargo
                int mass = token.Design.Aggregate.Mass * token.Quantity;
                int fleetCargo = Cargo.Total;
                int stackCapacity = token.Design.Aggregate.CargoCapacity * token.Quantity;
                int fleetCapacity = Aggregate.CargoCapacity;

                if (fleetCapacity > 0)
                {
                    mass += (int)((float)fleetCargo * ((float)stackCapacity / (float)fleetCapacity));
                }
                fuelCost += GetFuelCost(warpFactor, mass, distance, ifeFactor, token.Design.Aggregate.Engine);
            }

            return fuelCost;
        }

        /// <summary>
        /// Get the default warp factor of this fleet.
        /// i.e. the highest warp you can travel using only 100% normal fuel
        /// </summary>
        /// <returns></returns>
        public int GetDefaultWarpFactor()
        {
            var warpFactor = 5;
            if (Aggregate.Engine != null)
            {
                var lowestFuelUsage = 0;
                for (int i = 1; i < Aggregate.Engine.FuelUsage.Length; i++)
                {
                    // find the lowest fuel usage until we use more than 100%
                    var fuelUsage = Aggregate.Engine.FuelUsage[i];
                    if (fuelUsage > 100)
                    {
                        break;
                    }
                    else
                    {
                        lowestFuelUsage = fuelUsage;
                        warpFactor = i;
                    }
                }
            }
            return warpFactor;
        }

        public void ComputeAggregate()
        {
            Aggregate.Mass = 0;
            Aggregate.Shield = 0;
            Aggregate.CargoCapacity = 0;
            Aggregate.FuelCapacity = 0;
            Aggregate.Colonizer = false;
            Aggregate.Cost = new Cost();
            Aggregate.SpaceDock = 0;
            Aggregate.ScanRange = TechHullComponent.NoScanner;
            Aggregate.ScanRangePen = TechHullComponent.NoScanner;
            Aggregate.Engine = null;
            Aggregate.MineSweep = 0;

            // compute each token's 
            Tokens.ForEach(token =>
            {
                token.Design.ComputeAggregate(Player);

                // TODO: which default engine do we use for multiple fleets?
                Aggregate.Engine = token.Design.Aggregate.Engine;
                // cost
                Cost cost = token.Design.Aggregate.Cost * token.Quantity;
                Aggregate.Cost += cost;

                // mass
                Aggregate.Mass += token.Design.Aggregate.Mass * token.Quantity;

                // armor
                Aggregate.Armor += token.Design.Aggregate.Armor * token.Quantity;

                // shield
                Aggregate.Shield += token.Design.Aggregate.Shield * token.Quantity;

                // cargo
                Aggregate.CargoCapacity += token.Design.Aggregate.CargoCapacity * token.Quantity;

                // fuel
                Aggregate.FuelCapacity += token.Design.Aggregate.FuelCapacity * token.Quantity;

                // minesweep
                Aggregate.MineSweep += token.Design.Aggregate.MineSweep * token.Quantity;

                // colonization
                if (token.Design.Aggregate.Colonizer)
                {
                    Aggregate.Colonizer = true;
                }

                // We should only have one ship stack with spacdock capabilities, but for this logic just go with the max
                Aggregate.SpaceDock = Math.Max(Aggregate.SpaceDock, token.Design.Aggregate.SpaceDock);

                Aggregate.ScanRange = Math.Max(Aggregate.ScanRange, token.Design.Aggregate.ScanRange);
                Aggregate.ScanRangePen = Math.Max(Aggregate.ScanRangePen, token.Design.Aggregate.ScanRangePen);

            });


        }

        /// <summary>
        /// Attempt to transfer cargo to/from this fleet
        /// This is used to handle both immediate cargo transfers that the player made in the UI, and by the waypoint tasks
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        public bool AttemptTransfer(Cargo transfer)
        {
            if (transfer.Fuel != 0)
            {
                // ignore fuel requests to planets
                return false;
            }

            var result = Cargo + transfer;
            if (result >= 0 && result.Total <= Aggregate.CargoCapacity && result.Fuel <= Aggregate.FuelCapacity)
            {
                // The transfer doesn't leave us with 0 minerals, or with so many minerals and fuel that we can't hold it
                Cargo = result;
                return true;
            }
            return false;
        }
    }
}
