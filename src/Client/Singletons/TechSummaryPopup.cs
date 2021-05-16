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

        public Tech Tech
        {
            get => techSummary.Tech;
            set => techSummary.Tech = value;
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
    }
}