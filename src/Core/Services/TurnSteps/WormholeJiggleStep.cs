using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.UniverseGeneration;
using CraigStars.Utils;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Jiggle, Degrade and Jump wormholes
    /// 
    /// https://starsautohost.org/sahforum2/index.php?t=msg&th=2775&rid=0#msg_24279
    /// </summary>
    public class WormholeJiggleStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(WormholeJiggleStep));

        WormholeGenerationStep wormholeGenerator;
        HashSet<Vector2> planetPositions;
        HashSet<Vector2> wormholePositions;
        List<Wormhole> newWormholes;

        public WormholeJiggleStep(Game game) : base(game, TurnGenerationState.WormholeJiggle) { }

        public override void Process()
        {
            wormholeGenerator = new WormholeGenerationStep(Game);

            planetPositions = Game.Planets.Select(planet => planet.Position).ToHashSet();
            wormholePositions = Game.Wormholes.Select(wh => wh.Position).ToHashSet();

            newWormholes = new List<Wormhole>();
            foreach (var wormhole in Game.Wormholes)
            {
                Jiggle(wormhole);
                Degrade(wormhole);
                Jump(wormhole);
            }

            // don't add while we are iterating over the collection
            newWormholes.ForEach(wormhole => EventManager.PublishMapObjectCreatedEvent(wormhole));
        }

        internal void Jiggle(Wormhole wormhole)
        {
            var stats = Game.Rules.WormholeStatsByStability[wormhole.Stability];

            // don't infinite jiggle
            int jiggleCount = 0;
            do
            {
                var originalPosition = wormhole.Position;
                wormhole.Position = new Vector2(
                    wormhole.Position.x + Game.Rules.Random.Next(-stats.jiggleDistance / 2, stats.jiggleDistance / 2),
                    wormhole.Position.y + Game.Rules.Random.Next(-stats.jiggleDistance / 2, stats.jiggleDistance / 2)
                );
                log.Debug($"{Game.Year} Wormhole {TextUtils.GetPositionString(originalPosition)} jiggled to {wormhole.Position}");
                jiggleCount++;
            } while (Game.MapObjectsByLocation.ContainsKey(wormhole.Position) && jiggleCount < 100);

        }

        internal void Degrade(Wormhole wormhole)
        {
            var stats = Game.Rules.WormholeStatsByStability[wormhole.Stability];

            wormhole.YearsAtStability++;
            if (wormhole.YearsAtStability > stats.yearsToDegrade)
            {
                if (wormhole.Stability != WormholeStability.ExtremelyVolatile)
                {
                    // go to the next stability
                    wormhole.Stability = (WormholeStability)(wormhole.Stability + 1);
                    wormhole.YearsAtStability = 0;
                    log.Debug($"{Game.Year} Wormhole {TextUtils.GetPositionString(wormhole.Position)} degraded to {wormhole.Stability}");
                }
            }

        }

        internal void Jump(Wormhole wormhole)
        {
            var stats = Game.Rules.WormholeStatsByStability[wormhole.Stability];
            if (stats.chanceToJump > Game.Rules.Random.NextDouble())
            {
                // this wormhole jumped. We actually delete the previous one and create a new one. This way scanner history is reset
                var newWormhole = wormholeGenerator.GenerateWormhole(Game.Rules, planetPositions, wormholePositions);
                newWormhole.Stability = WormholeStability.RockSolid;
                newWormhole.YearsAtStability = 0;

                log.Debug($"{Game.Year} Wormhole {TextUtils.GetPositionString(wormhole.Position)} jumped to {TextUtils.GetPositionString(newWormhole.Position)}");

                var companion = wormhole.Destination;
                companion.Destination = newWormhole;
                newWormhole.Destination = companion;
                EventManager.PublishMapObjectDeletedEvent(wormhole);
                newWormholes.Add(newWormhole);
            }


        }
    }
}