using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;

namespace CraigStars.UniverseGeneration
{
    public class UniverseGenerator
    {
        public event Action<UniverseGenerationState> UniverseGeneratorAdvancedEvent;

        Game Game { get; }
        IList<UniverseGenerationStep> steps;

        public void PublishUniverseGeneratorAdvancedEvent(UniverseGenerationState state) => UniverseGeneratorAdvancedEvent?.Invoke(state);

        public UniverseGenerator(Game game, IList<UniverseGenerationStep> steps)
        {
            Game = game;
            this.steps = steps;
        }

        public void Generate()
        {
            PublishUniverseGeneratorAdvancedEvent(UniverseGenerationState.Starting);
            foreach (var step in steps)
            {
                PublishUniverseGeneratorAdvancedEvent(step.State);
                step.Process();
            }

            Game.Players.ForEach(player =>
            {
                // setup initial player relationships
                player.PlayerRelations = Game.Players.Select(p => new PlayerRelationship(p.Num, PlayerRelation.Enemy)).ToList();

                // setup player's object mappings
                player.SetupMapObjectMappings();
            });

        }

    }
}