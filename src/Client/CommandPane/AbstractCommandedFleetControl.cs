using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;
using System.Collections.Generic;

public abstract class AbstractCommandedFleetControl : Control
{
    public Player Me { get => PlayersManager.Me; }

    public FleetSprite CommandedFleet
    {
        get => commandedFleet;
        set
        {
            if (commandedFleet != value)
            {
                commandedFleet = value;
                OnNewCommandedFleet();
            }
        }
    }
    FleetSprite commandedFleet;

    public override void _Ready()
    {
        Signals.MapObjectCommandedEvent += OnMapObjectActivated;
        Signals.TurnPassedEvent += OnTurnPassed;
        Signals.FleetDeletedEvent += OnFleetDeleted;
        Signals.FleetsCreatedEvent += OnFleetsCreated;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Signals.MapObjectCommandedEvent -= OnMapObjectActivated;
        Signals.TurnPassedEvent -= OnTurnPassed;
        Signals.FleetDeletedEvent -= OnFleetDeleted;
        Signals.FleetsCreatedEvent -= OnFleetsCreated;
    }

    protected virtual void OnMapObjectActivated(MapObjectSprite mapObject)
    {
        CommandedFleet = mapObject as FleetSprite;
        UpdateControls();
    }

    protected virtual void OnTurnPassed(PublicGameInfo gameInfo, Player player)
    {
        CommandedFleet = null;
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
    /// Note, this will be called when setting the CommandedFleet to null
    /// </summary>
    protected abstract void OnNewCommandedFleet();
    protected abstract void UpdateControls();

}
