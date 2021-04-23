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
        static ILog log = LogManager.GetLogger(typeof(PlayerHomeworldGenerationStep));

        public PlayerHomeworldGenerationStep(Game game) : base(game, UniverseGenerationState.Homeworlds) { }

        List<Planet> ownedPlanets = new List<Planet>();

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

            if (player.Race.PRT == PRT.IT || player.Race.PRT == PRT.PP && Game.Rules.Size > Size.Tiny)
            {
                player.Homeworld.Population = Game.Rules.StartingPopulationWithExtraPlanet;
                // extra planet! woo!
                var planet = Game.Planets.FirstOrDefault(
                    p => p.Player == null &&
                    p.Position.DistanceTo(player.Homeworld.Position) <= Game.Rules.MaxExtraWorldDistance &&
                    p.Position.DistanceTo(player.Homeworld.Position) >= Game.Rules.MinExtraWorldDistance);
                if (planet != null)
                {
                    InitExtraWorld(player, planet);
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

            ownedPlanets.Add(homeworld);
        }

        internal Planet FindHomeworld()
        {
            return Game.Planets.Find(p => p.Player == null && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > Game.Rules.Area / Game.Players.Count));
        }

        void InitHomeworld(Player player, Planet planet)
        {

            var race = player.Race;
            var rules = Game.Rules;
            var random = Game.Rules.Random;

            // own this planet
            planet.Player = player;
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

            planet.Population = rules.StartingPopulation;

            if (race.LRTs.Contains(LRT.LSP))
            {
                planet.Population = (int)(planet.Population * rules.LowStartingPopulationFactor);
            }

            // homeworlds start with mines and factories
            planet.Mines = rules.StartingMines;
            planet.Factories = rules.StartingFactories;
            planet.Defenses = rules.StartingDefenses;

            // homeworlds have a scanner
            planet.Homeworld = true;
            planet.ContributesOnlyLeftoverToResearch = true;
            planet.Scanner = true;

            // the homeworld gets a starbase
            var design = Game.DesignsByGuid[player.GetLatestDesign(ShipDesignPurpose.Starbase).Guid];
            CreateStarbaseOnPlanet(player, planet, design);

            Message.HomePlanet(player, planet);
        }

        void InitExtraWorld(Player player, Planet planet)
        {
            var rules = Game.Rules;
            var random = rules.Random;
            var race = player.Race;

            // own this planet
            planet.Player = player;
            planet.ProductionQueue = new ProductionQueue();
            planet.ReportAge = 0;

            // copy the universe mineral concentrations and surface minerals
            planet.MineralConcentration = HomeWorldMineralConcentration;
            planet.Cargo = ExtraWorldSurfaceMinerals;

            planet.Hab = new Hab(
                race.HabCenter.grav + (race.HabWidth.grav - random.Next(race.HabWidth.grav - 1)) / 2,
                race.HabCenter.temp + (race.HabWidth.temp - random.Next(race.HabWidth.temp - 1)) / 2,
                race.HabCenter.rad + (race.HabWidth.rad - random.Next(race.HabWidth.rad - 1)) / 2
            );

            planet.Population = rules.StartingPopulationExtraPlanet;

            if (race.LRTs.Contains(LRT.LSP))
            {
                planet.Population = (int)(planet.Population * rules.LowStartingPopulationFactor);
            }

            // extra worlds start with mines and factories
            planet.Mines = rules.StartingMines;
            planet.Factories = rules.StartingFactories;
            planet.ContributesOnlyLeftoverToResearch = true;
            planet.Scanner = true;

            // this homeworld gets a fort
            var design = Game.DesignsByGuid[player.GetLatestDesign(ShipDesignPurpose.Fort).Guid];
            CreateStarbaseOnPlanet(player, planet, design);

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
                Player = player,
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
            planet.Starbase.ComputeAggregate();
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