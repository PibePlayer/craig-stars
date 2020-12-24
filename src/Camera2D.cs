using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    [Export]
    public Vector2 ZoomConstant { get; set; } = new Vector2(.05f, .05f);

    [Export]
    public float ScrollConstant { get; set; } = 1.5f;

    private bool pressed = false;

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("zoom_in"))
        {
            if (Zoom > ZoomConstant)
            {
                Zoom -= ZoomConstant;
            }
        }
        else if (Input.IsActionPressed("zoom_out"))
        {
            if (Zoom <= new Vector2(1, 1) - ZoomConstant)
            {
                Zoom += ZoomConstant;
            }
        }

        if (@event is InputEventMouseMotion eventMouseMotion && pressed)
        {
            Position += eventMouseMotion.Relative * Zoom * -1;
        }

        if (@event is InputEventMouseButton eventMouseButton && (eventMouseButton.ButtonIndex == 3 || eventMouseButton.ButtonIndex == 2))
        {
            pressed = eventMouseButton.Pressed;
        }
    }

    public override void _Process(float delta)
    {
        var scrollAmount = ScrollConstant * Zoom.x;

        if (Input.IsActionPressed("up"))
        {
            Position = new Vector2(Position.x, Position.y - scrollAmount);
        }
        if (Input.IsActionPressed("down"))
        {
            Position = new Vector2(Position.x, Position.y + scrollAmount);
        }
        if (Input.IsActionPressed("left"))
        {
            Position = new Vector2(Position.x - scrollAmount, Position.y);
        }
        if (Input.IsActionPressed("right"))
        {
            Position = new Vector2(Position.x + scrollAmount, Position.y);
        }
    }
}
