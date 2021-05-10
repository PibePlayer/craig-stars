#if TOOLS

using Godot;

[Tool]
public class CSTablePlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var script = GD.Load<Script>("res://addons/CSTable/CSTable.cs");
        var texture = GD.Load<Texture>("res://addons/CSTable/icon.svg");
        AddCustomType("CSTable", "MarginContainer", script, texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("CSTable");
    }
}

#endif