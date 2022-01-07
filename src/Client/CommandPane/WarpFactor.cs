using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    [Tool]
    public class WarpFactor : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(WarpFactor));
        public event Action<int> WarpSpeedChangedEvent;

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

        [Export]
        public int MinWarpFactor { get; set; } = 0;

        /// <summary>
        /// Max warp factor with 0 being stopped and 11 being use stargate
        /// </summary>
        /// <value></value>
        [Export]
        public int MaxWarpFactor { get; set; } = 11;

        [Export]
        public int StargateWarpFactor { get; set; } = 11;

        [Export]
        public int WarnSpeed { get; set; } = 10;

        [Export]
        public int DangerSpeed { get; set; } = 12;

        [Export]
        public bool HasStargate { get; set; } = false;

        [Export]
        public Color BaseColorOverride { get; set; } = Colors.Fuchsia;

        [Export]
        public string StoppedText { get; set; } = "Stopped!";

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
                int warpSpeedFromClick = Mathf.Clamp((int)Math.Round(mousePosition.x / (panel.RectSize.x - borderWidth) * MaxWarpFactor), 0, MaxWarpFactor);
                log.Debug($"Mouse clicked {mousePosition} for warp speed {warpSpeedFromClick}");
                WarpSpeed = warpSpeedFromClick;
                WarpSpeedChangedEvent?.Invoke(WarpSpeed);
            }
        }

        public override void _Draw()
        {
            if (panel != null)
            {
                var color = BaseColorOverride != Colors.Fuchsia ? BaseColorOverride : GUIColorsProvider.Colors.WarpColor;
                label.Text = $"Warp {WarpSpeed}";
                if (WarpSpeed >= WarnSpeed && WarpSpeed < DangerSpeed)
                {
                    color = GUIColorsProvider.Colors.WarpWarnColor;
                }
                else if (WarpSpeed >= DangerSpeed)
                {
                    color = GUIColorsProvider.Colors.WarpDangerColor;
                }
                else if (WarpSpeed == 0)
                {
                    label.Text = StoppedText;
                }

                if (WarpSpeed == StargateWarpFactor && HasStargate)
                {
                    label.Text = $"Use Stargate";
                    color = GUIColorsProvider.Colors.StargateColor;
                }

                // get the width of our rectangle
                // it's a percentage of the speed, minus the line widths
                float width = panel.RectSize.x * ((float)WarpSpeed / (float)MaxWarpFactor);
                if (width > 0)
                {

                    DrawRect(new Rect2(
                        new Vector2(panel.RectPosition.x + borderWidth / 2, panel.RectPosition.y + borderHeight / 2),
                        new Vector2(width - borderWidth, panel.RectSize.y - (borderHeight))),
                        color
                    );
                }
            }
            else
            {
                log.Debug("panel is null, can't draw");
            }
        }

    }
}