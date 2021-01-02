using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;

namespace CraigStars
{
    public class Game : Node
    {
        public UniverseSettings UniverseSettings { get; set; } = new UniverseSettings();
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Year { get; set; } = 2400;

        Scanner Scanner { get; set; }

        public override void _Ready()
        {
            Players.AddRange(PlayersManager.Instance.Players);

            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator();
            generator.Generate(this, UniverseSettings, PlayersManager.Instance.Players);

            // add the universe to the viewport
            Scanner = FindNode("Scanner") as Scanner;
            Scanner.AddMapObjects(this);

            Signals.PublishTurnPassedEvent(Year);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("generate_turn"))
            {
                GenerateTurn();
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                GetTree().ChangeScene("res://src/GUI/ShipDesigner/HullSummary.tscn");
            }
        }

        void GenerateTurn()
        {
            TurnGenerator generator = new TurnGenerator();
            generator.GenerateTurn(this, TechStore.Instance);
            Signals.PublishTurnPassedEvent(Year);
        }
    }
}
