#if TOOLS

using Godot;

[Tool]
public class CraigStarsComponentsPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var texture = GD.Load<Texture>("res://addons/CraigStarsComponents/icon.svg");
        AddCustomType("AvailablePlanetProductionQueueItems", "ScrollContainer", GD.Load<Script>("res://addons/CraigStarsComponents/src/ProductionQueue/AvailablePlanetProductionQueueItems.cs"), texture);
        AddCustomType("QueuedPlanetProductionQueueItems", "ScrollContainer", GD.Load<Script>("res://addons/CraigStarsComponents/src/ProductionQueue/QueuedPlanetProductionQueueItems.cs"), texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("AvailablePlanetProductionQueueItems");
        RemoveCustomType("QueuedPlanetProductionQueueItems");
    }
}

#endif