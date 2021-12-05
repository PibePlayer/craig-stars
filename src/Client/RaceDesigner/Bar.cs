using System;
using Godot;

namespace CraigStars.Client
{

    /// <summary>
    /// Draws a bar from low to high, where low and high are between 0 and 100
    /// </summary>
    [Tool]
    public class Bar : ColorRect
    {
        public delegate void BarChanged(int low, int high);
        public event BarChanged BarChangedEvent;

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
            SetProcess(false);
        }

        public override void _Process(float delta)
        {
            var mousePosition = GetLocalMousePosition();

            // move the color rect, but make sure it is constrained in the 
            // outer box. The x coord can be between 0 and as far to the 
            // right as the bar can go. If the total width is 100, the bar is 80 wide
            // then we can go between 0 and 20
            var x = Mathf.Clamp(colorRect.RectPosition.x + mousePosition.x - positionOnDrag.x, 0, RectSize.x - colorRect.RectSize.x);


            // reverse the calculations for our Low/High
            // colorRect.RectPosition.x = Low / 100f * RectSize.x
            // so Low = colorRect.RectPosition.x * 100f / RectSize.x;
            // colorRect.RectSize.x = RectSize.x * (High - Low) / 100f
            // High = (colorRect.RectSize.x * 100f) / RectSize.x + Low
            // yay Algebra!
            var width = High - Low;
            var newLow = (int)(x * 100f / RectSize.x);
            if (newLow != Low)
            {
                Low = newLow;
                High = Low + width;
                BarChangedEvent?.Invoke(Low, High);

                positionOnDrag = mousePosition;

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
                    SetProcess(true);
                }
                else
                {
                    colorRect.MouseDefaultCursorShape = CursorShape.PointingHand;
                    SetProcess(false);
                }
            }
        }

    }
}