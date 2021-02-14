using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;

public class FleetTile : MarginContainer
{
    public FleetSprite ActiveFleet
    {
        get => activeFleet; 
        set
        {
            if (activeFleet != value)
            {
                activeFleet = value;
                OnNewActiveFleet();
            }
        }
    }
    FleetSprite activeFleet;

    public override void _Ready()
    {
        Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        Signals.TurnPassedEvent += OnTurnPassed;
    }

    protected virtual void OnMapObjectActivated(MapObjectSprite mapObject)
    {
        ActiveFleet = mapObject as FleetSprite;
        UpdateControls();
    }

    protected virtual void OnTurnPassed(int year)
    {
        UpdateControls();
    }

    /// <summary>
    /// Called when a new active fleet has been selected
    /// Note, this will be called when setting the ActiveFleet to null
    /// </summary>
    protected virtual void OnNewActiveFleet() { }
    protected virtual void UpdateControls()
    {
        Visible = ActiveFleet != null;
    }

}
