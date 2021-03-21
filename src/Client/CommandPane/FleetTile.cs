using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;
using System.Collections.Generic;

public class FleetTile : MarginContainer
{
    public Player Me { get => PlayersManager.Me; }

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

    protected Label titleLabel;

    public override void _Ready()
    {
        titleLabel = GetNode<Label>("VBoxContainer/Title/Name");

        Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        Signals.TurnPassedEvent += OnTurnPassed;
        Signals.FleetDeletedEvent += OnFleetDeleted;
        Signals.FleetsCreatedEvent += OnFleetsCreated;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        Signals.TurnPassedEvent -= OnTurnPassed;
        Signals.FleetDeletedEvent -= OnFleetDeleted;
        Signals.FleetsCreatedEvent -= OnFleetsCreated;
    }

    protected virtual void OnMapObjectActivated(MapObjectSprite mapObject)
    {
        ActiveFleet = mapObject as FleetSprite;
        UpdateControls();
    }

    protected virtual void OnTurnPassed(PublicGameInfo gameInfo)
    {
        UpdateControls();
    }

    protected virtual void OnFleetDeleted(FleetSprite fleet)
    {
        UpdateControls();
    }

    protected virtual void OnFleetsCreated(List<Fleet> fleets)
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
