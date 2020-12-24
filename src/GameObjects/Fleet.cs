using Godot;
using Stateless;
using System;
using System.Collections.Generic;

public class Fleet : MapObject
{
    #region Planet Stats

    public Player Player { get; set; }
    public Cargo Cargo { get; set; }
    public Planet Orbiting { get; set; }
    #endregion

    public override void _Ready()
    {
        base._Ready();
    }

    protected override void OnSelected()
    {
    }

    protected override void OnDeselected()
    {
    }

}
