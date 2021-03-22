using Godot;
using System;

namespace CraigStars
{

    /// <summary>
    /// Draws a bar from low to high, where low and high are between 0 and 100
    /// </summary>
    [Tool]
    public class Bar : ColorRect
    {
        public Color BarColor { get; set; } = Colors.White;
        public int Low { get; set; } = 15;
        public int High { get; set; } = 85;
        public bool ShowBar { get; set; } = true;
        public bool Draggable { get; set; } = true;

        ColorRect colorRect;
        bool dragging = false;
        Vector2 positionOnDrag;

        public override void _Ready()
        {
            colorRect = GetNode<ColorRect>("ColorRect");
            colorRect.Connect("gui_input", this, nameof(OnColorRectGuiInput));
        }

        public override void _Process(float delta)
        {
            if (dragging)
            {
                var mousePosition = GetLocalMousePosition();
                var xoff = mousePosition.x - positionOnDrag.x;

                // scale this number from 0 to 100
                int xoffScaled = (int)(xoff);
                if (xoffScaled >= 1 || xoffScaled <= -1
                && Low + xoffScaled >= 0 && Low + xoffScaled < High
                && High + xoffScaled <= 100 && High + xoffScaled > Low)
                {
                    Low = Mathf.Clamp(Low + xoffScaled, 0, 100);
                    High = Mathf.Clamp(High + xoffScaled, 0, 100);
                    positionOnDrag = mousePosition;
                }
                Update();
            }
        }

        public override void _Draw()
        {
            colorRect.RectPosition = new Vector2((Low / 100f * RectSize.x), RectPosition.y);
            colorRect.RectSize = new Vector2(RectSize.x * ((High - Low) / 100f), RectSize.y);
            colorRect.Visible = ShowBar;
            colorRect.Color = BarColor;
        }

        void OnColorRectGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == (int)Godot.ButtonList.Left)
            {
                if (mouseButton.Pressed)
                {
                    positionOnDrag = GetLocalMousePosition();
                    colorRect.MouseDefaultCursorShape = CursorShape.Drag;
                    dragging = true;
                }
                else
                {
                    colorRect.MouseDefaultCursorShape = CursorShape.PointingHand;
                    dragging = false;
                }
            }
        }

    }
}