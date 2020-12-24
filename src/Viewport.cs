using Godot;
using System;
using System.Collections.Generic;

public class Viewport : Node2D
{
    public Universe Universe { get; private set; }

    public override void _Ready()
    {
    }

    public void AddUniverse(Universe universe)
    {
        AddChild(universe);
        Universe = universe;
        CallDeferred(nameof(UpdateViewport));
    }

    public void UpdateViewport()
    {
        Universe.Planets.ForEach(p => p.UpdateVisibleSprites());
    }

}
