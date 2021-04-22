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

        PlayerIntel playerIntel = new PlayerIntel();

        PlanetGenerator planetGenerator = new PlanetGenerator();
        WormholeGenerator wormholeGenerator = new WormholeGenerator();
        TechLevelInitializer techLevelInitializer = new TechLevelInitializer();

        public UniverseGenerator(Game game)
        {
            Game = game;
        }

        public void Generate()
        {
            List<Planet> planets = planetGenerator.GeneratePlanets(Game.Rules);
            List<Fleet> fleets = new List<Fleet>();
            List<Planet> ownedPlanets = new List<Planet>();

            // shuffle the planets so we don't end up with the same planet id each time
            Game.Rules.Random.Shuffle(planets);
            for (var i = 0; i < Game.Players.Count; i++)
            {
                var player = Game.Players[i];

                // initialize this player
                player.TechLevels = techLevelInitializer.GetStartingTechLevels(player.Race);
                InitPlans(player);
                InitPlayerPlanetReports(player, planets);
                InitShipDesigns(player);

                player.PlanetaryScanner = player.GetBestPlanetaryScanner();

                var homeworld = planets.Find(p => p.Player == null && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > Game.Rules.Area / Game.Players.Count));
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
            Game.Wormholes = wormholeGenerator.GenerateWormholes(Game.Rules, planets);
            Game.Fleets.AddRange(fleets);

            // accelerate some stuff, if necessary
            GameStartModeModifier gameStartModeModifier = new GameStartModeModifier();
            gameStartModeModifier.AdvanceGame(Game);
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

        List<Fleet> GenerateFleets(Rules rules, Player player, Planet homeworld)
        {
            var fleets = new List<Fleet>();
            switch (player.Race.PRT)
            {
                case PRT.SD:
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.LongRangeScount.Name), player, 1, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.SantaMaria.Name), player, 1, homeworld));
                    fleets.Add(CreateFleet(player.GetLatestDesign(ShipDesignPurpose.DamageMineLayer), player, 1, homeworld));
                    fleets.Add(CreateFleet(player.GetLatestDesign(ShipDesignPurpose.SpeedMineLayer), player, 1, homeworld));
                    break;
                case PRT.JoaT:
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.LongRangeScount.Name), player, 1, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.ArmoredProbe.Name), player, 2, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.SantaMaria.Name), player, 3, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.Teamster.Name), player, 4, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.CottonPicker.Name), player, 5, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.StalwartDefender.Name), player, 6, homeworld));
                    break;
            }

            return fleets;
        }

        Fleet CreateFleet(ShipDesign playerDesign, Player player, int id, Planet planet)
        {
            var design = Game.DesignsByGuid[playerDesign.Guid];
            var fleet = new Fleet()
            {
                Id = id
            };
            fleet.Name = $"{playerDesign.Name} #{fleet.Id}";
            fleet.BattlePlan = player.BattlePlans[0];
            player.Stats.NumFleetsBuilt++;
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
            fleet.Player = player;

            // aggregate all the design data
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;

            return fleet;
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
                BattlePlan = player.BattlePlans[0],
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

        internal void InitPlans(Player player)
        {
            player.BattlePlans.Add(new BattlePlan("Default"));
            player.TransportPlans.Add(new TransportPlan("Default"));
            player.TransportPlans.Add(new TransportPlan("Quick Load")
            {
                Tasks = new WaypointTransportTasks(
                    fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal),
                    ironium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll),
                    boranium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll),
                    germanium: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll)
                    )
            });
            player.TransportPlans.Add(new TransportPlan("Quick Drop")
            {
                Tasks = new WaypointTransportTasks(
                    fuel: new WaypointTransportTask(WaypointTaskTransportAction.LoadOptimal),
                    ironium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll),
                    boranium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll),
                    germanium: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll)
                    )
            });
            player.TransportPlans.Add(new TransportPlan("Load Colonists")
            {
                Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll))
            });
            player.TransportPlans.Add(new TransportPlan("Unload Colonists")
            {
                Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll))
            });
        }

        /// <summary>
        /// Generate ship designs for this player based on their tech level and Race
        /// </summary>
        /// <param name="player">The player to generate ship designs for</param>
        internal void InitShipDesigns(Player player)
        {
            ShipDesignGenerator designer = new ShipDesignGenerator();
            Game.Designs.Add(designer.DesignShip(Techs.Scout, "Long Range Scout", player, player.DefaultHullSet, ShipDesignPurpose.Scout));
            Game.Designs.Add(designer.DesignShip(Techs.ColonyShip, "Santa Maria", player, player.DefaultHullSet, ShipDesignPurpose.Colonizer));
            switch (player.Race.PRT)
            {
                case PRT.SD:
                    Game.Designs.Add(designer.DesignShip(Techs.MiniMineLayer, "Trapper", player, player.DefaultHullSet, ShipDesignPurpose.DamageMineLayer));
                    Game.Designs.Add(designer.DesignShip(Techs.MiniMineLayer, "Keeper", player, player.DefaultHullSet, ShipDesignPurpose.SpeedMineLayer));
                    break;
                case PRT.JoaT:
                    Game.Designs.Add(designer.DesignShip(Techs.Scout, "Armed Probe", player, player.DefaultHullSet, ShipDesignPurpose.ArmedScout));
                    Game.Designs.Add(designer.DesignShip(Techs.MediumFreighter, "Teamster", player, player.DefaultHullSet, ShipDesignPurpose.Freighter));
                    Game.Designs.Add(designer.DesignShip(Techs.MiniMiner, "Cotton Picker", player, player.DefaultHullSet, ShipDesignPurpose.Miner));
                    Game.Designs.Add(designer.DesignShip(Techs.Destroyer, "Stalwart Defender", player, player.DefaultHullSet, ShipDesignPurpose.FighterScout));
                    break;
            }

            // starbases are special, they have a specific design that each player gets
            var starbase = ShipDesigns.Starbase.Copy();
            starbase.HullSetNumber = player.DefaultHullSet;
            starbase.Name = "Starbase";
            starbase.Player = player;
            starbase.Purpose = ShipDesignPurpose.Starbase;
            Game.Designs.Add(starbase);

            Game.Designs.ForEach(design =>
            {
                if (design.Player == player)
                {
                    Game.DesignsByGuid[design.Guid] = design;
                    design.ComputeAggregate(player);
                    playerIntel.Discover(player, design, true);
                }
            });
        }

        /// <summary>
        /// Initialize the player's planet reports and for a new game generation
        /// </summary>
        /// <param name="player"></param>
        /// <param name="planets"></param>
        public void InitPlayerPlanetReports(Player player, List<Planet> planets)
        {
            planets.ForEach(planet =>
            {
                playerIntel.Discover(player, planet);
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