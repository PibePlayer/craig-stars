using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Places wormholes in the universe
    /// </summary>
    public class WormholeGenerationStep : UniverseGenerationStep
    {
        public WormholeGenerationStep(IProvider<Game> gameProvider) : base(gameProvider, UniverseGenerationState.Wormholes) { }

        public override void Process()
        {
            Game.Wormholes = GenerateWormholes(Game.Rules, Game.Planets);
        }

        public List<Wormhole> GenerateWormholes(Rules rules, List<Planet> planets)
        {
            var wormholes = new List<Wormhole>();
            int numPairs = rules.WormholePairsForSize[Game.Size];

             var (width, height) = rules.GetArea(Game.Size);

            // create a set of locations with planets
            var planetPositions = planets.Select(planet => planet.Position).ToHashSet();
            var wormholePositions = new HashSet<Vector2>();

            for (int i = 0; i < numPairs * 2; i++)
            {
                var wormhole = GenerateWormhole(rules, planetPositions, wormholePositions);
                wormholePositions.Add(wormhole.Position);

                // grab the previous wormhole for our pair
                if ((i % 2) > 0)
                {
                    var companion = wormholes[i - 1];
                    companion.Destination = wormhole;
                    wormhole.Destination = companion;
                }

                wormholes.Add(wormhole);
            }

            return wormholes;
        }

        public Wormhole GenerateWormhole(Rules rules, HashSet<Vector2> planetPositions, HashSet<Vector2> wormholePositions)
        {
            int width, height;
            var area = rules.GetArea(Game.Size);
            width = (int)area.x;
            height = (int)area.y;

            var random = rules.Random;

            var stabilityValues = Enum.GetValues(typeof(WormholeStability));
            var loc = new Vector2(random.Next(width), random.Next(height));

            // make sure this location is ok
            // it must be away from planets and at least 1/4 the universe away
            // from other wormholes
            int count = 0;
            while (
                !IsLocationValid(loc, wormholePositions, (height + width) / 2 / 4) ||
                !IsLocationValid(loc, planetPositions, rules.WormholeMinDistance)
                )
            {
                loc = new Vector2(random.Next(width), random.Next(height));
                count++;
                if (count > 100)
                {
                    // give up
                    break;
                }
            }
            var wormhole = new Wormhole()
            {
                Position = loc,
                Stability = (WormholeStability)random.Next(stabilityValues.Length)
            };

            return wormhole;

        }


    }
}