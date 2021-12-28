using System;
using System.Collections.Generic;
using System.Linq;
using CasualGodComplex;
using CraigStars;
using CraigStars.Utils;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Generate planets in the universe
    /// </summary>
    public class PlanetGenerationStep : UniverseGenerationStep
    {
        public PlanetGenerationStep(IProvider<Game> gameProvider) : base(gameProvider, UniverseGenerationState.Planets) { }

        public override void Process()
        {
            Game.Planets = GeneratePlanets(Game.Rules);
        }

        public List<Planet> GeneratePlanets(Rules rules)
        {
            var planets = new List<Planet>();
            var area = rules.GetArea(Game.Size);
            int width = (int)area.x;
            int height = (int)area.y;

            var numPlanets = rules.GetNumPlanets(Game.Size, Game.Density);
            var ng = new NameGenerator();
            var names = ng.RandomNames;

            Random random = rules.Random;
            var planetLocs = new HashSet<Vector2>();
            for (int i = 0; i < numPlanets; i++)
            {
                var loc = new Vector2(random.Next(width), random.Next(height));

                // make sure this location is ok
                while (!IsLocationValid(loc, planetLocs, rules.PlanetMinDistance))
                {
                    loc = new Vector2(random.Next(width), random.Next(height));
                }

                // add a new planet
                planetLocs.Add(loc);
                Planet planet = new Planet()
                {
                    Id = i + 1,
                    Name = names[i],
                    Position = loc
                };
                RandomizePlanet(rules, planet);
                // planet.Randomize();
                planets.Add(planet);
            }

            // shuffle these so id 1 is not always the first planet in the list
            // later on we will add homeworlds based on first planet, second planet, etc
            rules.Random.Shuffle(planets);

            return planets;

        }

        void RandomizePlanet(Rules rules, Planet planet)
        {
            var random = rules.Random;
            planet.InitEmptyPlanet();

            int minConc = rules.MinMineralConcentration;
            int maxConc = rules.MaxStartingMineralConcentration;

            // from @edmundmk on the Stars! discord, this is
            //  Generate mineral concentration.  There is a 30% chance of a
            //  concentration between 1 and 30.  Higher concentrations have a
            //  distribution centred on 75, minimum 31 and maximum 199.
            //  x = random 1 to 100 inclusive
            //  if x > 30 then
            //     x = 30 + random 0 to 44 inclusive + random 0 to 44 inclusive
            //  end
            //  return x
            Mineral conc = new Mineral(
                random.Next(minConc, maxConc + 1),
                random.Next(minConc, maxConc + 1),
                random.Next(minConc, maxConc + 1)
            );

            planet.MineralConcentration = new Mineral(
                conc[0] > 30 ? 30 + random.Next(0, 45) + random.Next(0, 45) : conc[0],
                conc[1] > 30 ? 30 + random.Next(0, 45) + random.Next(0, 45) : conc[1],
                conc[2] > 30 ? 30 + random.Next(0, 45) + random.Next(0, 45) : conc[2]
            );

            // From @SuicideJunkie's tests, this is a flat random from 0 to 100
            int grav = random.Next(0, 101);
            int temp = random.Next(0, 101);
            int rad = random.Next(0, 101);

            planet.Hab = new Hab(
                grav,
                temp,
                rad
            );
            planet.BaseHab = planet.Hab;
            planet.TerraformedAmount = new Hab();
        }

    }
}