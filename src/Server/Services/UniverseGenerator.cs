using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

public class UniverseGenerator
{
    Server Server { get; }

    public UniverseGenerator(Server server)
    {
        Server = server;
    }

    public void Generate()
    {
        List<Planet> planets = GeneratePlanets(Server.Settings);
        List<Fleet> fleets = new List<Fleet>();
        List<Planet> ownedPlanets = new List<Planet>();

        // shuffle the planets so we don't end up with the same planet id each time
        Server.Settings.Random.Shuffle(planets);
        for (var i = 0; i < Server.Players.Count; i++)
        {
            var player = Server.Players[i];

            // initialize this player
            InitTechLevels(player);
            InitPlayerReports(player, planets);
            InitShipDesigns(player);
            player.PlanetaryScanner = player.GetBestPlanetaryScanner(TechStore.Instance);

            var homeworld = planets.Find(p => p.Player == null && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > Server.Settings.Area / 4));
            player.Homeworld = homeworld;
            MakeHomeworld(Server.Settings, player, homeworld, Server.Settings.StartingYear);
            ownedPlanets.Add(homeworld);

            fleets.AddRange(GenerateFleets(Server.Settings, player, homeworld));
            Message.Info(player, "Welcome to the universe, go forth and conquer!");
        }

        // add extra planets for this player
        Server.Players.ForEach(player =>
            {
                for (var extraPlanetNum = 0; extraPlanetNum < Server.Settings.StartWithExtraPlanets; extraPlanetNum++)
                {
                    var planet = planets.FirstOrDefault(p => p.Player == null && p.Position.DistanceTo(player.Homeworld.Position) < 100);
                    if (planet != null)
                    {
                        MakeExtraWorld(Server.Settings, player, planet);
                        ownedPlanets.Add(planet);
                    }
                }

            });

        Server.Planets.AddRange(planets);
        Server.Fleets.AddRange(fleets);
        Server.Width = Server.Settings.Area;
        Server.Height = Server.Settings.Area;
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
    /// <param name="settings"></param>
    /// <returns></returns>
    List<Planet> GeneratePlanets(UniverseSettings settings)
    {
        var planets = new List<Planet>();
        int width, height;
        width = height = settings.Area;

        var numPlanets = settings.NumPlanets;
        var ng = new NameGenerator();
        var names = ng.RandomNames;

        Random random = new Random();
        var planetLocs = new Dictionary<Vector2, bool>();
        for (int i = 0; i < numPlanets; i++)
        {
            var loc = new Vector2(random.Next(width), random.Next(height));

            // make sure this location is ok
            while (!IsValidLocation(loc, planetLocs, settings.PlanetMinDistance))
            {
                loc = new Vector2(random.Next(width), random.Next(height));
            }

            // add a new planet
            planetLocs[loc] = true;
            Planet planet = new Planet();
            RandomizePlanet(settings, planet);
            planet.Id = i + 1;
            planet.Name = names[i];
            planet.Position = loc;
            // planet.Randomize();
            planets.Add(planet);
        }

        return planets;
    }

    List<Fleet> GenerateFleets(UniverseSettings settings, Player player, Planet homeworld)
    {
        var fleets = new List<Fleet>(new Fleet[] {
            CreateFleet(ShipDesigns.LongRangeScount, $"Long Range Scout #1", player, homeworld),
            CreateFleet(ShipDesigns.SantaMaria, $"Santa Maria # 1", player, homeworld)
        });

        return fleets;
    }

    Fleet CreateFleet(ShipDesign shipDesign, String name, Player player, Planet planet)
    {
        var fleet = new Fleet();
        fleet.Tokens.Add(
            new ShipToken()
            {
                Design = shipDesign,
                Quantity = 1
            }
        );
        fleet.Position = planet.Position;
        fleet.Orbiting = planet;
        fleet.Waypoints.Add(new Waypoint(fleet.Orbiting));
        planet.OrbitingFleets.Add(fleet);
        fleet.Name = name;
        fleet.Player = player;
        fleet.Id = player.Fleets.Count + 1;

        // aggregate all the design data
        fleet.ComputeAggregate(Server.Settings);
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

    void MakeHomeworld(UniverseSettings settings, Player player, Planet planet, int year)
    {

        var race = player.Race;
        var random = settings.Random;

        // own this planet
        planet.Player = player;
        planet.ReportAge = 0;

        // copy the universe mineral concentrations and surface minerals
        planet.MineralConcentration = settings.HomeWorldMineralConcentration;
        planet.Cargo = settings.HomeWorldSurfaceMinerals;

        planet.Hab = new Hab(
            race.HabCenter.grav,
            race.HabCenter.temp,
            race.HabCenter.rad
        );

        planet.Population = settings.StartingPopulation;

        if (race.LRTs.Contains(LRT.LSP))
        {
            planet.Population = (int)(planet.Population * settings.LowStartingPopulationFactor);
        }

        // homeworlds start with mines and factories
        planet.Mines = settings.StartingMines;
        planet.Factories = settings.StartingFactories;
        planet.Defenses = settings.StartingDefenses;

        // homeworlds have a scanner
        planet.Homeworld = true;
        planet.ContributesOnlyLeftoverToResearch = true;
        planet.Scanner = true;

        // the homeworld gets a starbase
        planet.Starbase = new Starbase()
        {
            Player = player,
            Position = planet.Position,
            Orbiting = planet,
            Waypoints = new List<Waypoint>
            {
                new Waypoint(planet)
            },
            Tokens = new List<ShipToken>
            {
                new ShipToken() {
                    Design = ShipDesigns.Starbase,
                    Quantity = 1,
                }
            }
        };

        Message.HomePlanet(player, planet);
    }

    void MakeExtraWorld(UniverseSettings settings, Player player, Planet planet)
    {
        var random = settings.Random;
        var race = player.Race;

        // own this planet
        planet.Player = player;
        planet.ReportAge = 0;

        // copy the universe mineral concentrations and surface minerals
        planet.MineralConcentration = settings.HomeWorldMineralConcentration;
        planet.Cargo = settings.ExtraWorldSurfaceMinerals;

        planet.Hab = new Hab(
            race.HabCenter.grav + (race.HabWidth.grav - random.Next(race.HabWidth.grav - 1)) / 2,
            race.HabCenter.temp + (race.HabWidth.temp - random.Next(race.HabWidth.temp - 1)) / 2,
            race.HabCenter.rad + (race.HabWidth.rad - random.Next(race.HabWidth.rad - 1)) / 2
        );

        planet.Population = settings.StartingPopulationExtraPlanet;

        if (race.LRTs.Contains(LRT.LSP))
        {
            planet.Population = (int)(planet.Population * settings.LowStartingPopulationFactor);
        }

        // extra worlds start with mines and factories
        planet.Mines = settings.StartingMines;
        planet.Factories = settings.StartingFactories;

        planet.ContributesOnlyLeftoverToResearch = true;
    }

    void RandomizePlanet(UniverseSettings settings, Planet planet)
    {
        var random = settings.Random;
        planet.InitEmptyPlanet();

        int minConc = settings.MinMineralConcentration;
        int maxConc = settings.MaxStartingMineralConcentration;
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
        ShipDesigner designer = new ShipDesigner();
        player.Designs.Add(designer.DesignShip(Techs.Scout, "Long Range Scout", player, Server.TechStore));
        player.Designs.Add(designer.DesignShip(Techs.ColonyShip, "Santa Maria", player, Server.TechStore));
        player.Designs.Add(ShipDesigns.Starbase);

        player.Designs.ForEach(design => design.ComputeAggregate(player, Server.Settings));
    }

    /// <summary>
    /// Initialize the player's planet reports and for a new game generation
    /// </summary>
    /// <param name="player"></param>
    /// <param name="planets"></param>
    internal void InitPlayerReports(Player player, List<Planet> planets)
    {
        planets.ForEach(planet =>
        {
            var planetReport = new Planet()
            {
                Position = planet.Position,
                Guid = planet.Guid,
                Id = planet.Id,
                Name = planet.Name,
                ReportAge = Planet.Unexplored,
            };

            player.Planets.Add(planetReport);
            // build each players dictionary of planets by id
            player.SetupMapObjectMappings();
        });
    }

}