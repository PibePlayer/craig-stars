using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class FleetSummaryContainer : FleetSummary
    {
        static ILog log = LogManager.GetLogger(typeof(FleetSummaryContainer));

        PopupPanel hullSummaryPopup;
        HullSummary hullSummary;
        TextureRect icon;
        Label fleetRaceLabel;

        public override void _Ready()
        {
            base._Ready();
            icon = (TextureRect)FindNode("Icon");
            fleetRaceLabel = (Label)FindNode("FleetRaceLabel");
            hullSummaryPopup = GetNode<PopupPanel>("HullSummaryPopup");
            hullSummary = GetNode<HullSummary>("HullSummaryPopup/HullSummary");

            icon.Connect("mouse_entered", this, nameof(OnIconMouseEntered));
            icon.Connect("mouse_exited", this, nameof(OnIconMouseExited));
        }

        void OnIconMouseEntered()
        {
            var design = Fleet.Fleet.GetPrimaryToken().Design;
            hullSummary.Hull = design.Hull;
            hullSummary.ShipDesign = design;

            // position the summary view on the corner
            var position = icon.RectGlobalPosition;
            position.x += icon.RectSize.x + 2;
            position.y -= hullSummary.RectSize.y + 2;
            hullSummaryPopup.SetGlobalPosition(position);
            hullSummaryPopup.Show();
        }

        void OnIconMouseExited()
        {
            hullSummaryPopup.Hide();
        }


        protected override void UpdateControls()
        {

            if (Fleet != null)
            {
                var race = Me.Race;
                var fleet = Fleet.Fleet;

                icon.Texture = TextureLoader.Instance.FindTexture(fleet.GetPrimaryToken().Design);
                fleetRaceLabel.Text = $"{fleet.Owner.RacePluralName}";
            }
        }
    }
}