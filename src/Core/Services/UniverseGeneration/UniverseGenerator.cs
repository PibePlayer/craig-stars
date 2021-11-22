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
        Game Game { get; }

        IList<UniverseGenerationStep> steps;

        public UniverseGenerator(Game game, IList<UniverseGenerationStep> steps)
        {
            Game = game;
            this.steps = steps;
        }

        public void Generate()
        {
            foreach (var step in steps)
            {
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