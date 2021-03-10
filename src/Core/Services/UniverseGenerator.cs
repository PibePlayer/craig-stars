using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

namespace CraigStars
{
    public class UniverseGenerator
    {
        Game Game { get; }

        public UniverseGenerator(Game game)
        {
            Game = game;
        }

        public void Generate()
        {
            List<Planet> planets = GeneratePlanets(Game.Rules);
            List<Fleet> fleets = new List<Fleet>();
            List<Planet> ownedPlanets = new List<Planet>();

            // shuffle the planets so we don't end up with the same planet id each time
            Game.Rules.Random.Shuffle(planets);
            for (var i = 0; i < Game.Players.Count; i++)
            {
                var player = Game.Players[i];

                // initialize this player
                InitTechLevels(player);
                InitPlayerReports(player, planets);
                InitShipDesigns(player);
                player.PlanetaryScanner = player.GetBestPlanetaryScanner();

                var homeworld = planets.Find(p => p.Player == null && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > Game.Rules.Area / 4));
                player.Homeworld = homeworld;
                MakeHomeworld(Game.Rules, player, homeworld, Game.Rules.StartingYear);
                ownedPlanets.Add(homeworld);

                fleets.AddRange(GenerateFleets(Game.Rules, player, homeworld));
                Message.Info(player, "Welcome to the universe, go forth and conquer!");
            }

            // add extra planets for this player
            Game.Players.ForEach(player =>
                {
                    for (var extraPlanetNum = 0; extraPlanetNum < Game.Rules.StartWithExtraPlanets; extraPlanetNum++)
                    {
                        var planet = planets.FirstOrDefault(p => p.Player == null && p.Position.DistanceTo(player.Homeworld.Position) < 100);
                        if (planet != null)
                        {
                            MakeExtraWorld(Game.Rules, player, planet);
                            ownedPlanets.Add(planet);
                        }
                    }

                });

            Game.Planets.AddRange(planets);
            Game.Fleets.AddRange(fleets);
        }

        /// <summary>
        /// Get the shortest distance from planet p to other planets
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ownedPlanets"></param>
        /// <returns></returns>
        int ShortestDistanceToPlanets(Planet p, List<Planet> otherPlanets)
        {
            return (int)otherPlanets.Min(otherPlanet => p.Position.DistanceTo(otherPlanet.Position));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        List<Planet> GeneratePlanets(Rules rules)
        {
            var planets = new List<Planet>();
            int width, height;
            width = height = rules.Area;

            var numPlanets = rules.NumPlanets;
            var ng = new NameGenerator();
            var names = ng.RandomNames;

            Random random = new Random();
            var planetLocs = new Dictionary<Vector2, bool>();
            for (int i = 0; i < numPlanets; i++)
            {
                var loc = new Vector2(random.Next(width), random.Next(height));

                // make sure this location is ok
                while (!IsValidLocation(loc, planetLocs, rules.PlanetMinDistance))
                {
                    loc = new Vector2(random.Next(width), random.Next(height));
                }

                // add a new planet
                planetLocs[loc] = true;
                Planet planet = new Planet();
                RandomizePlanet(rules, planet);
                planet.Id = i + 1;
                planet.Name = names[i];
                planet.Position = loc;
                // planet.Randomize();
                planets.Add(planet);
            }

            return planets;
        }

        List<Fleet> GenerateFleets(Rules rules, Player player, Planet homeworld)
        {
            var fleets = new List<Fleet>(new Fleet[] {
            CreateFleet(player.GetDesign(ShipDesigns.LongRangeScount.Name), $"Long Range Scout #1", player, homeworld),
            CreateFleet(player.GetDesign(ShipDesigns.SantaMaria.Name), $"Santa Maria # 1", player, homeworld)
        });

            return fleets;
        }

        Fleet CreateFleet(ShipDesign playerDesign, String name, Player player, Planet planet)
        {
            var design = Game.DesignsByGuid[playerDesign.Guid];
            var fleet = new Fleet();
            fleet.Tokens.Add(
                new ShipToken()
                {
                    Design = design,
                    Quantity = 1
                }
            );
            fleet.Position = planet.Position;
            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(fleet.Orbiting));
            planet.OrbitingFleets.Add(fleet);
            fleet.Name = name;
            fleet.Player = player;
            fleet.Id = player.Fleets.Count + 1;

            // aggregate all the design data
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;

            return fleet;
        }

        /// <summary>
        /// Return true if the location is not already in (or close to another planet) planet_locs
        /// </summary>
        /// <param name="loc">The location to check</param>
        /// <param name="planetLocs">The locations of every planet so far</param>
        /// <param name="offset">The offset to check for</param>
        /// <returns>True if this location (or near it) is not already in use</returns>
        bool IsValidLocation(Vector2 loc, Dictionary<Vector2, bool> planetLocs, int offset)
        {
            float x = loc.x;
            float y = loc.y;
            if (planetLocs.ContainsKey(loc))
            {
                return false;
            }

            for (int yOffset = 0; yOffset < offset; yOffset++)
            {
                for (int xOffset = 0; xOffset < offset; xOffset++)
                {
                    if (planetLocs.ContainsKey(new Vector2(x + xOffset, y + yOffset)))
                    {
                        return false;
                    }
                    if (planetLocs.ContainsKey(new Vector2(x - xOffset, y + yOffset)))
                    {
                        return false;
                    }
                    if (planetLocs.ContainsKey(new Vector2(x - xOffset, y - yOffset)))
                    {
                        return false;
                    }
                    if (planetLocs.ContainsKey(new Vector2(x + xOffset, y - yOffset)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void MakeHomeworld(Rules rules, Player player, Planet planet, int year)
        {

            var race = player.Race;
            var random = rules.Random;

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
            var starbaseDesign = Game.DesignsByGuid[player.GetDesign("Starbase").Guid];

            planet.Starbase = new Starbase()
            {
                Player = player,
                Name = starbaseDesign.Name,
                Position = planet.Position,
                Orbiting = planet,
                Waypoints = new List<Waypoint>
            {
                Waypoint.TargetWaypoint(planet)
            },
                Tokens = new List<ShipToken>
            {
                new ShipToken() {
                    Design = starbaseDesign,
                    Quantity = 1,
                }
            }
            };
            planet.Starbase.ComputeAggregate();

            Message.HomePlanet(player, planet);
        }

        void MakeExtraWorld(Rules rules, Player player, Planet planet)
        {
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
        }

        void RandomizePlanet(Rules rules, Planet planet)
        {
            var random = rules.Random;
            planet.InitEmptyPlanet();

            int minConc = rules.MinMineralConcentration;
            int maxConc = rules.MaxStartingMineralConcentration;
            planet.MineralConcentration = new Mineral(
                random.Next(maxConc) + minConc,
                random.Next(maxConc) + minConc,
                random.Next(maxConc) + minConc
            );

            // generate hab range of this planet
            int grav = random.Next(100);
            if (grav > 1)
            {
                // this is a "normal" planet, so put it in the 10 to 89 range
                grav = random.Next(89) + 10;
            }
            else
            {
                grav = (int)(11 - (float)(random.Next(100)) / 100.0 * 10.0);
            }

            int temp = random.Next(100);
            if (temp > 1)
            {
                // this is a "normal" planet, so put it in the 10 to 89 range
                temp = random.Next(89) + 10;
            }
            else
            {
                temp = (int)(11 - (float)(random.Next(100)) / 100.0 * 10.0);
            }

            int rad = random.Next(98) + 1;

            planet.Hab = new Hab(
                grav,
                temp,
                rad
            );

        }

        void InitTechLevels(Player player)
        {
            var race = player.Race;
            switch (race.PRT)
            {
                case PRT.HE:
                    break;
                case PRT.SS:
                    player.TechLevels.Electronics = 5;
                    break;
                case PRT.WM:
                    player.TechLevels.Weapons = 6;
                    player.TechLevels.Energy = 1;
                    player.TechLevels.Propulsion = 1;
                    break;
                case PRT.CA:
                    player.TechLevels.Energy = 1;
                    player.TechLevels.Weapons = 1;
                    player.TechLevels.Propulsion = 1;
                    player.TechLevels.Construction = 2;
                    player.TechLevels.Biotechnology = 6;
                    break;
                case PRT.IS:
                    break;
                case PRT.SD:
                    player.TechLevels.Propulsion = 2;
                    player.TechLevels.Biotechnology = 2;
                    break;
                case PRT.PP:
                    player.TechLevels.Energy = 4;
                    break;
                case PRT.IT:
                    player.TechLevels.Propulsion = 5;
                    player.TechLevels.Construction = 5;
                    break;
                case PRT.AR:
                    player.TechLevels.Energy = 1;
                    break;
                case PRT.JoaT:
                    player.TechLevels.Energy = 3;
                    player.TechLevels.Weapons = 3;
                    player.TechLevels.Propulsion = 3;
                    player.TechLevels.Construction = 3;
                    player.TechLevels.Electronics = 3;
                    player.TechLevels.Biotechnology = 3;
                    break;
            }

            // if a race has Techs costing exra start high, set the start level to 3
            // for any TechField that is set to research costs extra
            if (race.TechsStartHigh)
            {
                // Jack of All Trades start at 4
                var costsExtraLevel = race.PRT == PRT.JoaT ? 4 : 3;
                foreach (TechField field in Enum.GetValues(typeof(TechField)))
                {
                    var level = player.TechLevels[field];
                    if (race.ResearchCost[field] == ResearchCostLevel.Extra && level < costsExtraLevel)
                    {
                        player.TechLevels[field] = costsExtraLevel;
                    }
                }
            }

            if (race.HasLRT(LRT.IFE) || race.HasLRT(LRT.CE))
            {
                // Improved Fuel Efficiency and Cheap Engines increases propulsion by 1
                player.TechLevels.Propulsion++;
            }
        }

        /// <summary>
        /// Generate ship designs for this player based on their tech level and Race
        /// </summary>
        /// <param name="player">The player to generate ship designs for</param>
        internal void InitShipDesigns(Player player)
        {
            ShipDesignGenerator designer = new ShipDesignGenerator();
            Game.Designs.Add(designer.DesignShip(Techs.Scout, "Long Range Scout", player));
            Game.Designs.Add(designer.DesignShip(Techs.ColonyShip, "Santa Maria", player));
            var starbase = ShipDesigns.Starbase.Copy();
            starbase.Name = "Starbase";
            starbase.Player = player;
            Game.Designs.Add(starbase);

            Game.Designs.ForEach(design =>
            {
                if (design.Player == player)
                {
                    Game.DesignsByGuid[design.Guid] = design;
                    design.ComputeAggregate(player);
                    player.UpdateDesignReport(design, true);
                }
            });
        }

        /// <summary>
        /// Initialize the player's planet reports and for a new game generation
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planets"></param>
        public void InitPlayerReports(Player player, List<Planet> planets)
        {
            // TODO: re-use Player.UpdatePlayerPlanet()/Player.UpdatePlanetReport
            planets.ForEach(planet =>
            {
                // add an empty report for this planet
                var planetReport = new Planet()
                {
                    Position = planet.Position,
                    Guid = planet.Guid,
                    Id = planet.Id,
                    Name = planet.Name,
                    ReportAge = Planet.Unexplored,
                };

                player.PlanetsByGuid[planetReport.Guid] = planetReport;

                if (planet.OwnedBy(player))
                {
                    planetReport.Player = player;
                    player.Planets.Add(planetReport);
                    player.UpdatePlayerPlanet(planet);
                }
                else
                {
                    player.ForeignPlanets.Add(planetReport);
                }
            });
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