using Godot;
using System;

namespace CraigStars.Singletons
{
    public class TechSummaryPopup : PopupPanel
    {
        static CSLog log = LogProvider.GetLogger(typeof(TechSummaryPopup));

        private static TechSummaryPopup instance;
        public static TechSummaryPopup Instance
        {
            get
            {
                return instance;
            }
        }

        TechSummaryPopup()
        {
            instance = this;
        }

        public static Tech Tech
        {
            get => Instance.techSummary.Tech;
            set => Instance.techSummary.Tech = value;
        }

        TechSummary techSummary;

        public override void _Ready()
        {
            if (instance != this)
            {
                log.Warn("Godot created our singleton twice");
                instance = this;
            }

            techSummary = GetNode<TechSummary>("TechSummary");
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select") || @event.IsActionPressed("ui_cancel"))
            {
                Hide();
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select") || @event.IsActionPressed("ui_cancel"))
            {
                Hide();
            }
        }

        #region static methods to show dialog

        /// <summary>
        /// Show this summary at the mouse position, but don't let it go above the screen
        /// </summary>
        public static void ShowAtMouse()
        {
            // don't let this y go above the screen
            // if our tech summary is 300 px tall and the mouse is at 200y, move the y down 100 px
            var mousePos = Instance.GetGlobalMousePosition();
            var yPos = mousePos.y - Instance.RectSize.y;
            Instance.RectPosition = new Vector2(mousePos.x, Mathf.Clamp(yPos, 0, Instance.GetViewportRect().Size.y - Instance.RectSize.y));
            Instance.Show();
        }

        #endregion
    }
}