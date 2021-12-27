using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Utils;
using Godot;

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
                // setup initial player relationships and info
                player.PlayerRelations = Game.Players.Select(p => new PlayerRelationship(p.Num == player.Num ? PlayerRelation.Friend : PlayerRelation.Enemy)).ToList();
                player.PlayerInfoIntel = Game.Players.Select(p => new PlayerInfo(p.Num, p.Name)).ToList();
                
                // know thyself
                player.PlayerInfoIntel[player.Num].Seen = true;
                player.PlayerInfoIntel[player.Num].RaceName = player.Race.Name;
                player.PlayerInfoIntel[player.Num].RacePluralName = player.Race.PluralName;

                // setup player's object mappings
                player.SetupMapObjectMappings();
            });

        }

    }
}