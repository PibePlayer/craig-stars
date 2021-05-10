#if TOOLS

using Godot;

[Tool]
public class CraigStarsComponentsPlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var texture = GD.Load<Texture>("res://addons/CraigStarsComponents/icon.svg");
        AddCustomType("ProductionQueueItemsTable", "MarginContainer", GD.Load<Script>("res://addons/CraigStarsComponents/src/ProductionQueueItemsTable.cs"), texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("ProductionQueueItemsTable");
    }
}

#endif