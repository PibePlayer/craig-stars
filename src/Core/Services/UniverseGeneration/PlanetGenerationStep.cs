using System;
using System.Collections.Generic;
using System.Linq;
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
            GeneratePlanets(Game.Rules).ForEach(planet => Game.AddMapObject(planet));
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

            // From @SuicideJunkie's tests and @edmundmk's previous research, grav and temp are weighted slightly towards
            // the center, rad is completely random
            // @edmundmk: 
            // "I'm certain gravity and temperature probability is constant between 10 and 90 inclusive, and falls off towards 0 and 100.  
            // It never generates 0 or 100 so I have to change my random formula to (1 to 90)+(0 to 9)
            // damn you all for sucking me into stars! again lol"
            int grav = random.Next(0, 91) + random.Next(0, 10);
            int temp = random.Next(0, 91) + random.Next(0, 10);
            int rad = random.Next(1, 100);

            planet.Hab = new Hab(
                grav,
                temp,
                rad
            );
            planet.BaseHab = planet.Hab;
            planet.TerraformedAmount = new Hab();


            int minConc = rules.MinStartingMineralConcentration;
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

            // also from @SuicideJunkie about a bonus to germ for high rad
            // Only the exact example given in the help file it seems... "extreme values" is exactly rads being above 85, giving a small bonus to germanium.
            int germRadBonus = planet.Hab.Value.rad >= rules.HighRadGermaniumBonusThreshold ? rules.HighRadGermaniumBonus : 0;
            Mineral conc = new Mineral(
                random.Next(minConc, maxConc + 1),
                random.Next(minConc, maxConc + 1),
                random.Next(minConc, maxConc + 1)
            );

            planet.MineralConcentration = new Mineral(
                conc[0] > 30 ? 30 + random.Next(0, 45) + random.Next(0, 45) : conc[0],
                conc[1] > 30 ? 30 + random.Next(0, 45) + random.Next(0, 45) : conc[1],
                conc[2] > 30 ? 30 + random.Next(0, 45) + random.Next(germRadBonus, 45) : conc[2]
            );

        }

    }
}