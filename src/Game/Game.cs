using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;

namespace CraigStars
{
    public class Game : Node
    {
        public UniverseSettings UniverseSettings { get; set; } = new UniverseSettings();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Year { get; set; }

        Scanner Scanner { get; set; }

        public override void _Ready()
        {
            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator();
            generator.Generate(this, UniverseSettings, PlayersManager.Instance.Players);

            // add the universe to the viewport
            Scanner = FindNode("Scanner") as Scanner;
            Scanner.AddMapObjects(this);
        }


        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("generate_turn"))
            {
                GenerateTurn();
            }
        }

        void GenerateTurn()
        {
            TurnGenerator generator = new TurnGenerator();
            generator.GenerateTurn(this);
            Signals.PublishTurnPassedEvent(Year);
        }
    }
}
