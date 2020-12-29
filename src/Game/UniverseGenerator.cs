using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;

public class UniverseGenerator
{
    public void Generate(Game game, UniverseSettings settings, List<Player> players)
    {
        List<Planet> planets = GeneratePlanets(settings);
        List<Fleet> fleets = new List<Fleet>();
        List<Planet> ownedPlanets = new List<Planet>();
        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var homeworld = planets.Find(p => p.Player == null && (ownedPlanets.Count == 0 || ShortestDistanceToPlanets(p, ownedPlanets) > settings.Area / 4));
            player.Homeworld = homeworld;
            MakeHomeworld(settings, player, homeworld, settings.StartingYear);
            ownedPlanets.Add(homeworld);

            fleets.AddRange(GenerateFleets(settings, player, homeworld));
        }

        // add extra planets for this player
        players.ForEach(player =>
        {
            for (var extraPlanetNum = 0; extraPlanetNum < settings.StartWithExtraPlanets; extraPlanetNum++)
            {
                var planet = planets.FirstOrDefault(p => p.Player == null && p.Position.DistanceTo(player.Homeworld.Position) < 100);
                if (planet != null)
                {
                    MakeExtraWorld(settings, player, planet);
                    ownedPlanets.Add(planet);
                }
            }

        });

        planets.ForEach(p => game.AddChild(p));
        fleets.ForEach(f => game.AddChild(f));

        game.Planets.AddRange(planets);
        game.Width = settings.Area;
        game.Height = settings.Area;
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

    List<Planet> GeneratePlanets(UniverseSettings settings)
    {
        var planets = new List<Planet>();
        PackedScene planetScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Planet.tscn");
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
            Planet planet = planetScene.Instance() as Planet;
            RandomizePlanet(settings, planet);
            planet.ObjectName = names[i];
            planet.Position = loc;
            // planet.Randomize();
            planets.Add(planet);
        }

        return planets;
    }

    List<Fleet> GenerateFleets(UniverseSettings settings, Player player, Planet homeworld)
    {
        var fleets = new List<Fleet>();
        PackedScene fleetScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/Fleet.tscn");

        var fleet = fleetScene.Instance() as Fleet;
        fleet.Player = player;
        fleet.Position = homeworld.Position;
        fleet.Orbiting = homeworld;
        homeworld.OrbitingFleets.Add(fleet);
        fleet.ObjectName = $"{player.PlayerName} Fleet #1";
        fleets.Add(fleet);

        return fleets;
    }

    /**
    * Return true if the location is not already in (or close to another planet) planet_locs
    * 
    * @param loc The location to check
    * @param planetLocs The locations of every planet so far
    * @param offset The offset to check for
    * @return True if this location (or near it) is not already in use
    */
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
        (
            planet.MineralConcentration.Ironium,
            planet.MineralConcentration.Boranium,
            planet.MineralConcentration.Germanium
        ) = settings.HomeWorldMineralConcentration;

        (
            planet.Cargo.Ironium,
            planet.Cargo.Boranium,
            planet.Cargo.Germanium
        ) = settings.HomeWorldSurfaceMinerals;

        planet.Hab = new Hab(
            race.HabCenter.Grav,
            race.HabCenter.Temp,
            race.HabCenter.Rad
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
        planet.ContributesToResearch = true;
        planet.Scanner = true;
    }

    void MakeExtraWorld(UniverseSettings settings, Player player, Planet planet)
    {
        var random = settings.Random;
        var race = player.Race;

        // own this planet
        planet.Player = player;
        planet.ReportAge = 0;

        // copy the universe mineral concentrations and surface minerals
        (
            planet.MineralConcentration.Ironium,
            planet.MineralConcentration.Boranium,
            planet.MineralConcentration.Germanium
        ) = settings.HomeWorldMineralConcentration;

        (
            planet.Cargo.Ironium,
            planet.Cargo.Boranium,
            planet.Cargo.Germanium
        ) = settings.ExtraWorldSurfaceMinerals;

        planet.Hab = new Hab(
            race.HabCenter.Grav + (race.HabWidth.Grav - random.Next(race.HabWidth.Grav - 1)) / 2,
            race.HabCenter.Temp + (race.HabWidth.Temp - random.Next(race.HabWidth.Temp - 1)) / 2,
            race.HabCenter.Rad + (race.HabWidth.Rad - random.Next(race.HabWidth.Rad - 1)) / 2
        );

        planet.Population = settings.StartingPopulationExtraPlanet;

        if (race.LRTs.Contains(LRT.LSP))
        {
            planet.Population = (int)(planet.Population * settings.LowStartingPopulationFactor);
        }

        // extra worlds start with mines and factories
        planet.Mines = settings.StartingMines;
        planet.Factories = settings.StartingFactories;

        planet.ContributesToResearch = true;
    }

    void RandomizePlanet(UniverseSettings settings, Planet planet)
    {
        var random = settings.Random;

        int minConc = settings.MinMineralConcentration;
        int maxConc = settings.MaxStartingMineralConcentration;
        planet.MineralConcentration = new Mineral()
        {
            Ironium = random.Next(maxConc) + minConc,
            Boranium = random.Next(maxConc) + minConc,
            Germanium = random.Next(maxConc) + minConc
        };

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

}