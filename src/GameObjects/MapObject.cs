using Godot;
using Stateless;
using System;
using System.Collections.Generic;

public class MapObject : Area2D
{
    public enum OwnerAlly
    {
        Unknown,
        Known,
        Owned,
        Friend,
        Enemy
    }

    public enum States
    {
        None,
        Selected,
        Active,
    }

    public enum Triggers
    {
        Select, // this node is clicked on
        Activate, // this node is clicked on
        Deselect, // some other node is clicked on
    }

    [Export]
    public States State { get; set; } = States.None;

    [Export]
    public OwnerAlly OwnerAllyState { get; set; } = OwnerAlly.Unknown;

    public String ObjectName { get; set; } = "";

    StateMachine<States, Triggers> selectedMachine;

    public override void _Ready()
    {
        // hook up mouse events to our area
        Connect("input_event", this, nameof(OnInputEvent));

        selectedMachine = new StateMachine<States, Triggers>(() => State, s => State = s);

        // we can transition into the None state from Selected or Active, and we deselect
        selectedMachine.Configure(States.None)
            .OnEntry(() => OnDeselected())
            .Permit(Triggers.Select, States.Selected)
            .Permit(Triggers.Activate, States.Active)
            .Ignore(Triggers.Deselect);

        selectedMachine.Configure(States.Selected)
            .OnEntry(() => OnSelected())
            .Permit(Triggers.Select, States.Active)
            .Permit(Triggers.Activate, States.Active)
            .Permit(Triggers.Deselect, States.None);

        selectedMachine.Configure(States.Active)
            .OnEntry(() => OnSelected())
            .Permit(Triggers.Select, States.Selected)
            .Permit(Triggers.Deselect, States.None)
            .Ignore(Triggers.Activate);

        selectedMachine.OnTransitioned(t => GD.Print($"OnTransitioned: {t.Source} -> {t.Destination} via {t.Trigger}({string.Join(", ", t.Parameters)})"));

        // wire up signals
        Signals.MapObjectSelectedEvent += OnMapObjectSelected;
    }

    public override void _ExitTree()
    {
        Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
    }

    protected virtual void OnMapObjectSelected(MapObject mapObject)
    {
        // if a different map object is selected, deselect us
        if (mapObject != this)
        {
            Deselect();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            selectedMachine.Fire(Triggers.Deselect);
        }
    }

    void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        if (@event.IsActionPressed("viewport_select"))
        {
            GD.Print("Input Event for this object");
            selectedMachine.Fire(Triggers.Select);
            Signals.PublishMapObjectSelectedEvent(this);
        }
    }

    #region Virtuals

    protected virtual void OnSelected() { }
    protected virtual void OnDeselected() { }

    public virtual void Activate()
    {
        selectedMachine.Fire(Triggers.Activate);
    }

    public virtual void Deselect()
    {
        selectedMachine.Fire(Triggers.Deselect);
    }

    #endregion

}