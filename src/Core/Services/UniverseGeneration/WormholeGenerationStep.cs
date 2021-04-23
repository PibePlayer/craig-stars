using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Places wormholes in the universe
    /// </summary>
    public class WormholeGenerationStep : UniverseGenerationStep
    {
        public WormholeGenerationStep(Game game) : base(game, UniverseGenerationState.Wormholes) { }

        public override void Process()
        {
            Game.Wormholes = GenerateWormholes(Game.Rules, Game.Planets);
        }

        public List<Wormhole> GenerateWormholes(Rules rules, List<Planet> planets)
        {
            var wormholes = new List<Wormhole>();
            int numPairs = rules.WormholePairsForSize[rules.Size];

            int width, height;
            width = height = rules.Area;

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
            width = height = rules.Area;

            var random = rules.Random;

            var stabilityValues = Enum.GetValues(typeof(WormholeStability));
            var loc = new Vector2(random.Next(width), random.Next(height));

            // make sure this location is ok
            // it must be away from planets and at least 1/4 the universe away
            // from other wormholes
            while (
                !IsLocationValid(loc, wormholePositions, width / 4) ||
                !IsLocationValid(loc, planetPositions, rules.WormholeMinDistance)
                )
            {
                loc = new Vector2(random.Next(width), random.Next(height));
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