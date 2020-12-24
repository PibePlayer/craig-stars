using Godot;
using System;

public class MapObjectSummary : Control
{
    public MapObject MapObject
    {
        get => mapObject; set
        {
            mapObject = value;
            UpdateName();
        }
    }
    MapObject mapObject;

    Label nameLabel;

    public override void _Ready()
    {
        nameLabel = FindNode("Name") as Label;

        Signals.MapObjectSelectedEvent += OnMapObjectSelected;
    }

    public override void _ExitTree()
    {
        Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
    }

    void OnMapObjectSelected(MapObject mapObject)
    {
        MapObject = mapObject;
    }

    void UpdateName()
    {
        if (MapObject != null)
        {
            nameLabel.Text = mapObject.ObjectName;
        }
        else
        {
            nameLabel.Text = "Unknown";
        }
    }
}
