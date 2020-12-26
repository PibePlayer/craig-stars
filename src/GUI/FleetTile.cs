using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;

public class FleetTile : MarginContainer
{
    public Fleet ActiveFleet { get; set; }

    public override void _Ready()
    {
        Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        Signals.TurnPassedEvent += OnTurnPassed;
    }

    private void OnMapObjectActivated(MapObject mapObject)
    {
        ActiveFleet = mapObject as Fleet;
        UpdateControls();
    }

    protected virtual void OnTurnPassed(int year)
    {
        UpdateControls();
    }

    protected virtual void UpdateControls()
    {
        Visible = ActiveFleet != null;
    }
}
