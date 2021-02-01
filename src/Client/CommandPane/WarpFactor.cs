using Godot;
using log4net;
using System;

namespace CraigStars
{
    [Tool]
    public class WarpFactor : Control
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WarpFactor));
        public event Action<int> WarpSpeedChangedEvent;

        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        /// <summary>
        /// Max warp factor with 0 being stopped and 11 being use stargate
        /// </summary>
        /// <value></value>
        [Export]
        public int MaxWarpFactor { get; set; } = 11;

        [Export(PropertyHint.Range),]
        public int WarpSpeed
        {
            get => warpSpeed;
            set
            {
                warpSpeed = value;
                Update();
            }
        }
        int warpSpeed = 5;

        Panel panel;
        Label label;
        StyleBoxFlat panelStyleBox;
        int borderWidth;
        int borderHeight;

        bool updatingWarp = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            panel = GetNode<Panel>("Panel");
            label = GetNode<Label>("Label");
            panelStyleBox = panel.GetStylebox("panel") as StyleBoxFlat;
            panel.Connect("gui_input", this, nameof(OnGUIInput));

            borderWidth = panelStyleBox.BorderWidthLeft + panelStyleBox.BorderWidthRight;
            borderHeight = panelStyleBox.BorderWidthTop + panelStyleBox.BorderWidthBottom;
            Update();
        }

        public override void _Input(InputEvent @event)
        {
            // handle releasing the button outside of the control
            if (@event.IsActionReleased("viewport_select"))
            {
                updatingWarp = false;
            }
        }

        void OnGUIInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                updatingWarp = true;
            }
            else if (@event.IsActionReleased("viewport_select"))
            {
                updatingWarp = false;
            }
            if (updatingWarp && @event is InputEventMouse mouse)
            {
                Vector2 mousePosition = mouse.Position;
                mousePosition = mouse.Position;
                int warpSpeedFromClick = (int)(Math.Round(mousePosition.x / (panel.RectSize.x - borderWidth) * MaxWarpFactor));
                log.Debug($"Mouse clicked {mousePosition} for warp speed {warpSpeedFromClick}");
                if (warpSpeedFromClick >= 0 && warpSpeedFromClick <= MaxWarpFactor)
                {
                    WarpSpeed = warpSpeedFromClick;
                    WarpSpeedChangedEvent?.Invoke(WarpSpeed);
                }
            }
        }

        public override void _Draw()
        {
            if (panel != null)
            {
                var color = GUIColors.WarpColor;
                if (WarpSpeed > 0 && WarpSpeed < MaxWarpFactor)
                {
                    label.Text = $"Warp {WarpSpeed}";
                    if (WarpSpeed == 10)
                    {
                        // TODO: Don't hardcode this damage
                        color = GUIColors.WarpDamageColor;
                    }
                }
                else if (WarpSpeed == 0)
                {
                    label.Text = $"Stopped!";
                }
                else if (WarpSpeed == MaxWarpFactor)
                {
                    label.Text = $"Use Stargate";
                    color = GUIColors.StargateColor;
                }
                // get the width of our rectangle
                // it's a percentage of the speed, minus the line widths
                float width = panel.RectSize.x * ((float)WarpSpeed / (float)MaxWarpFactor) - (borderWidth);
                DrawRect(new Rect2(
                    panel.RectPosition,
                    new Vector2(width, panel.RectSize.y - (borderHeight))),
                    color
                );
            }
            else
            {
                log.Debug("panel is null, can't draw");
            }
        }

    }
}