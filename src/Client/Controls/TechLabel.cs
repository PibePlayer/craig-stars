using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class TechLabel : Label
    {
        public Tech Tech { get; set; }

        public TechLabel()
        {
            MouseFilter = MouseFilterEnum.Pass;
            MouseDefaultCursorShape = (CursorShape)Input.CursorShape.Help;
        }

        public override void _Ready()
        {
            Connect("gui_input", this, nameof(OnGUIInput));
        }

        void OnGUIInput(InputEvent @event)
        {
            if (Tech != null && @event.IsActionPressed("viewport_select"))
            {
                GetTree().SetInputAsHandled();

                TechSummaryPopup.Tech = Tech;
                TechSummaryPopup.ShowAtMouse();
            }
            else if (@event.IsActionReleased("viewport_select"))
            {
                TechSummaryPopup.Instance.Hide();
            }
        }


    }
}