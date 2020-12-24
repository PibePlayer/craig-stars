using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class Game : Node
{
    public UniverseSettings UniverseSettings { get; set; } = new UniverseSettings();
    public Universe Universe { get; private set; }
    public int Year { get; set; }

    Viewport Viewport { get; set; }

    public override void _Ready()
    {
        // generate a new univers
        UniverseGenerator generator = new UniverseGenerator();
        Universe = new Universe();
        generator.Generate(Universe, UniverseSettings, PlayersManager.Instance.Players);

        // add the universe to the viewport
        Viewport = FindNode("Viewport") as Viewport;
        Viewport.AddUniverse(Universe);

        CallDeferred(nameof(FocusHomeworld));
    }


    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("generate_turn"))
        {
            GenerateTurn();
        }
    }

    /// <summary>
    /// Focus on the current player's homeworld
    /// </summary>
    void FocusHomeworld()
    {
        var homeworld = Universe.Planets.Where(p => p.Homeworld && p.Player == PlayersManager.Instance.Me).First();
        if (homeworld != null)
        {
            homeworld.Activate();
            Signals.PublishMapObjectSelectedEvent(homeworld);
        }
    }

    void GenerateTurn()
    {
        TurnGenerator generator = new TurnGenerator();
        generator.GenerateTurn(this);
        Signals.PublishTurnPassedEvent(Year);
    }
}
