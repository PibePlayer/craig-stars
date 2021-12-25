using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;
using CraigStars.Utils;

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
            planet.BaseHab = planet.Hab;
            planet.TerraformedAmount = new Hab();
        }

    }
}