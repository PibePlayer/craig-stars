using Godot;
using System;
using System.Collections.Generic;
using CraigStars;

public class UniverseGenerator
{
    public void Generate(Universe universe, UniverseSettings settings, List<Player> players)
    {
        List<Planet> planets = GeneratePlanets(settings);
        List<Fleet> fleets = new List<Fleet>();
        for (var i = 0; i < players.Count; i++)
        {
            var homeworld = planets[i];
            homeworld.Player = players[i];
            MakeHomeworld(settings, homeworld.Player, homeworld, settings.StartingYear);

            fleets.AddRange(GenerateFleets(settings, homeworld.Player, homeworld));
        }

        planets.ForEach(p => universe.AddChild(p));
        fleets.ForEach(f => universe.AddChild(f));

        universe.Planets.AddRange(planets);
        universe.Width = settings.Area;
        universe.Height = settings.Area;
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
        var random = new Random();

        int minConc = settings.MinHomeworldMineralConcentration;
        int maxConc = settings.MaxStartingMineralConcentration;
        planet.MineralConcentration = new Mineral()
        {
            Ironium = random.Next(maxConc) + minConc,
            Boranium = random.Next(maxConc) + minConc,
            Germanium = random.Next(maxConc) + minConc
        };

        planet.Hab = new Hab(
            race.HabHigh.Grav - race.HabLow.Grav / 2,
            race.HabHigh.Temp - race.HabLow.Temp / 2,
            race.HabHigh.Rad - race.HabLow.Rad / 2
        );

        int minSurf = settings.MinStartingMineralSurface;
        int maxSurf = settings.MaxStartingMineralSurface;

        planet.Cargo.Ironium = random.Next(maxConc) + minConc;
        planet.Cargo.Boranium = random.Next(maxConc) + minConc;
        planet.Cargo.Germanium = random.Next(maxConc) + minConc;
        planet.Cargo.Population = settings.StartingPopulation;

        if (race.LRTs.Contains(LRT.LSP))
        {
            planet.Cargo.Population = (int)(planet.Cargo.Population * settings.LowStartingPopulationFactor);
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

}