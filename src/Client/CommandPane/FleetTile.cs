using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;
using System.Collections.Generic;

public class FleetTile : AbstractCommandedFleetControl
{

    protected Label titleLabel;

    public override void _Ready()
    {
        base._Ready();
        titleLabel = GetNode<Label>("VBoxContainer/Title/Name");

    }

    /// <summary>
    /// Called when a new active fleet has been selected
    /// Note, this will be called when setting the CommandedFleet to null
    /// </summary>
    protected override void OnNewCommandedFleet() { }
    protected override void UpdateControls()
    {
        Visible = CommandedFleet != null;
    }

}
