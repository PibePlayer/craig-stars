#if TOOLS

using Godot;

[Tool]
public class CSTablePlugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var texture = GD.Load<Texture>("res://addons/CSTable/icon.svg");
        AddCustomType("CSTable", "MarginContainer", GD.Load<Script>("res://addons/CSTable/CSTable.cs"), texture);
        AddCustomType("CSLabelCell", "MarginContainer", GD.Load<Script>("res://addons/CSTable/src/Table/CSLabelCell.cs"), texture);
    }

    public override void _ExitTree()
    {
        RemoveCustomType("CSTable");
        RemoveCustomType("CSLabelCell");
    }
}

#endif