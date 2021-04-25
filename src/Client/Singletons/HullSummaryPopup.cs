using Godot;
using log4net;
using System;

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
    }
}