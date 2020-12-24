using Godot;
using System;

public class Player : Node
{
    public int NetworkId { get; set; }
    public int Num { get; set; }
    public string PlayerName { get; set; }
    public Boolean Ready { get; set; } = false;
    public Boolean AIControlled { get; set; }
    public Color Color { get; set; } = Colors.Black;
    public Race Race = new Race();

    public override void _Ready()
    {

    }

}
