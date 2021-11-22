using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;
using log4net;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Pick a homeworld (and possibly a second world) for each player
    /// </summary>
    public class PlayerHomeworldGenerationStep : UniverseGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerHomeworldGenerationStep));

        readonly PlanetService planetService;
        readonly PlanetDiscoverer planetDiscoverer;
        readonly FleetSpecService fleetSpecService;

        List<Planet> ownedPlanets = new List<Planet>();

        public PlayerHomeworldGenerationStep(
            IProvider<Game> gameProvider,
            PlanetService planetService,
            PlanetDiscoverer planetDiscoverer,
            FleetSpecService fleetSpecService) : base(gameProvider, UniverseGenerationState.Homeworlds)
        {
            this.planetService = planetService;
            this.planetDiscoverer = planetDiscoverer;
            this.fleetSpecService = fleetSpecService;
        }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                MakeHomeworld(player);
            });
        }

        internal void MakeHomeworld(Player player)
        {
            var homeworld = FindHomeworld();

            player.Homeworld = homeworld;
            InitHomeworld(player, homeworld);
            planetDiscoverer.Discover(player, homeworld);

            if (Game.Size > Size.Tiny)
            {
                for (int i = 1; i < player.Race.Spec.StartingPlanets.Count; i++)
                {
                    var extraPlanet = player.Race.Spec.StartingPlanets[i];

                    // extra planet! woo!
                    var planet = Game.Planets.FirstOrDefault(
                        p => !p.Owned &&
                        p.Position.DistanceTo(player.Homeworld.Position) <= Game.Rules.MaxExtraWorldDistance &&
                        p.Position.DistanceTo(player.Homeworld.Position) >= Game.Rules.MinExtraWorldDistance);
                    if (planet != null)
                    {
                        InitExtraWorld(player, planet, extraPlanet);
                        planetDiscoverer.Discover(player, planet);
                        ownedPlanets.Add(planet);
                    }
                    else
                    {
                        // TODO: maybe we should place a planet where we want it? like a random angle but always
                        // about the same distance?
                        // or maybe we should place homeworlds first, then randomly place all the other planets?
                        log.Error("Failed to find valid planet in range of homeworld");
                    }
                }
            }

            ownedPlanets.Add(homeworld);
        }

        internal Planet FindHomeworld()
        {
            var area = Game.Rules.GetArea(Game.Size);
            return Game.Planets.Find(p => !p.Owned && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > area / Game.Players.Count));
        }

        void InitHomeworld(Player player, Planet planet)
        {

            var race = player.Race;
            var rules = Game.Rules;
            var random = Game.Rules.Random;
            var startingPlanet = player.Race.Spec.StartingPlanets[0];

            // own this planet
            planet.PlayerNum = player.Num;
            planet.ProductionQueue = new ProductionQueue();
            planet.ReportAge = 0;

            // copy the universe mineral concentrations and surface minerals
            planet.MineralConcentration = HomeWorldMineralConcentration;
            planet.Cargo = HomeWorldSurfaceMinerals;

            planet.Hab = new Hab(
                race.HabCenter.grav,
                race.HabCenter.temp,
                race.HabCenter.rad
            );
            planet.BaseHab = planet.Hab;
            planet.TerraformedAmount = new Hab();

            planet.Population = (int)(startingPlanet.Population * race.Spec.StartingPopulationFactor);

            // homeworlds start with mines and factories
            planet.Mines = rules.StartingMines;
            planet.Factories = rules.StartingFactories;
            planet.Defenses = rules.StartingDefenses;

            // homeworlds have a scanner
            planet.Homeworld = true;
            planet.ContributesOnlyLeftoverToResearch = false;
            planet.Scanner = true;

            // the homeworld gets a starbase
            var design = Game.DesignsByGuid[player.GetLatestDesign(ShipDesignPurpose.Starbase).Guid];
            CreateStarbaseOnPlanet(player, planet, design);

            planetService.ApplyProductionPlan(planet.ProductionQueue.Items, player, player.ProductionPlans[0]);

            Message.HomePlanet(player, planet);
        }

        void InitExtraWorld(Player player, Planet planet, StartingPlanet startingPlanet)
        {
            var rules = Game.Rules;
            var random = rules.Random;
            var race = player.Race;

            // own this planet
            planet.PlayerNum = player.Num;
            planet.ProductionQueue = new ProductionQueue();
            planet.ReportAge = 0;

            // copy the universe mineral concentrations and surface minerals
            planet.MineralConcentration = HomeWorldMineralConcentration;
            planet.Cargo = ExtraWorldSurfaceMinerals;

            var habPenalty = startingPlanet.HabPenaltyFactor;

            planet.Hab = new Hab(
                (int)(race.HabCenter.grav + (race.HabWidth.grav - random.Next(race.HabWidth.grav - 1)) / 2 * habPenalty),
                (int)(race.HabCenter.temp + (race.HabWidth.temp - random.Next(race.HabWidth.temp - 1)) / 2 * habPenalty),
                (int)(race.HabCenter.rad + (race.HabWidth.rad - random.Next(race.HabWidth.rad - 1)) / 2 * habPenalty)
            );
            planet.BaseHab = planet.Hab;
            planet.TerraformedAmount = new Hab();

            planet.Population = (int)(startingPlanet.Population * race.Spec.StartingPopulationFactor);

            // extra worlds start with mines and factories
            planet.Mines = rules.StartingMines;
            planet.Factories = rules.StartingFactories;
            planet.ContributesOnlyLeftoverToResearch = false;
            planet.Scanner = true;

            // this homeworld gets a fort
            var design = Game.DesignsByGuid[player.GetLatestDesign(ShipDesignPurpose.Fort).Guid];
            CreateStarbaseOnPlanet(player, planet, design);

            planetService.ApplyProductionPlan(planet.ProductionQueue.Items, player, player.ProductionPlans[0]);
        }

        /// <summary>
        /// Create a starbase on this planet
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planet"></param>
        /// <param name="design"></param>
        internal void CreateStarbaseOnPlanet(Player player, Planet planet, ShipDesign design)
        {
            planet.Starbase = new Starbase()
            {
                PlayerNum = player.Num,
                Name = design.Name,
                Position = planet.Position,
                Orbiting = planet,
                BattlePlan = player.BattlePlans[0],
                Waypoints = new List<Waypoint>
            {
                Waypoint.TargetWaypoint(planet)
            },
                Tokens = new List<ShipToken>
            {
                new ShipToken() {
                    Design = design,
                    Quantity = 1,
                }
            }
            };
            fleetSpecService.ComputeDesignSpec(player, planet.Starbase.Design);
            fleetSpecService.ComputeFleetSpec(player, planet.Starbase);
            planet.PacketSpeed = planet.Starbase.Spec.SafePacketSpeed;
        }

        /// <summary>
        /// Get the shortest distance from planet p to other planets
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ownedPlanets"></param>
        /// <returns></returns>
        int ShortestDistanceToPlanets(Planet p, List<Planet> otherPlanets)
        {
            return (int)Math.Sqrt(otherPlanets.Min(otherPlanet => p.Position.DistanceSquaredTo(otherPlanet.Position)));
        }

        #region Starting Minerals

        /// <summary>
        /// All homeworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral HomeWorldMineralConcentration
        {
            get
            {
                if (homeWorldMineralConcentration == null)
                {
                    homeWorldMineralConcentration = new Mineral(
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralConcentration) + Game.Rules.MinHomeworldMineralConcentration,
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralConcentration) + Game.Rules.MinHomeworldMineralConcentration,
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralConcentration) + Game.Rules.MinHomeworldMineralConcentration
                    );
                }
                return homeWorldMineralConcentration.Value;
            }
        }
        Mineral? homeWorldMineralConcentration = null;

        /// <summary>
        /// All homeworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral HomeWorldSurfaceMinerals
        {
            get
            {
                if (homeWorldSurfaceMinerals == null)
                {
                    homeWorldSurfaceMinerals = new Mineral(
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface,
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface,
                        Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface
                    );
                }
                return homeWorldSurfaceMinerals.Value;
            }
        }
        Mineral? homeWorldSurfaceMinerals = null;

        /// <summary>
        /// All extraworlds have the same starting minerals and concentrations
        /// </summary>
        /// <value></value>
        public Mineral ExtraWorldSurfaceMinerals
        {
            get
            {
                if (extraWorldSurfaceMinerals == null)
                {
                    extraWorldSurfaceMinerals = new Mineral(
                        (Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface) / 2,
                        (Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface) / 2,
                        (Game.Rules.Random.Next(Game.Rules.MaxStartingMineralSurface) + Game.Rules.MinStartingMineralSurface) / 2
                    );
                }
                return extraWorldSurfaceMinerals.Value;
            }
        }
        Mineral? extraWorldSurfaceMinerals = null;

        #endregion
    }
}