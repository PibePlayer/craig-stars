using Godot;
using System;
using CraigStars.Client;

namespace CraigStars.Singletons
{
    public class HullSummaryPopup : PopupPanel
    {
        static CSLog log = LogProvider.GetLogger(typeof(HullSummaryPopup));

        private static HullSummaryPopup instance;
        public static HullSummaryPopup Instance
        {
            get
            {
                return instance;
            }
        }

        HullSummaryPopup()
        {
            instance = this;
        }

        public TechHull Hull
        {
            get => hullSummary.Hull;
            set => hullSummary.Hull = value;
        }

        public ShipDesign ShipDesign
        {
            get => hullSummary.ShipDesign;
            set => hullSummary.ShipDesign = value;
        }

        public ShipToken Token
        {
            get => hullSummary.Token;
            set => hullSummary.Token = value;
        }

        HullSummary hullSummary;

        public override void _Ready()
        {
            if (instance != this)
            {
                log.Warn("Godot created our singleton twice");
                instance = this;
            }

            hullSummary = GetNode<HullSummary>("HullSummary");
        }

        public override void _Input(InputEvent @event)
        {
            if ((@event.IsActionPressed("ui_select") || @event.IsActionPressed("ui_cancel")) && IsVisibleInTree())
            {
                Hide();
                CallDeferred(nameof(DecrementDialogRefCount));
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select") || @event.IsActionPressed("ui_cancel") && IsVisibleInTree())
            {
                Hide();
                CallDeferred(nameof(DecrementDialogRefCount));
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
            Instance.hullSummary.UpdateControls();
            Instance.Show();
            DialogManager.DialogRefCount++;
        }

        #endregion

        void DecrementDialogRefCount()
        {
            DialogManager.DialogRefCount--;
        }

    }
}