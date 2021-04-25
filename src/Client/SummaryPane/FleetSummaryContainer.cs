using Godot;
using System;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class FleetSummaryContainer : MapObjectSummary<FleetSprite>
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetSummaryContainer));

        TextureRect icon;
        Label fleetRaceLabel;

        public override void _Ready()
        {
            base._Ready();
            icon = (TextureRect)FindNode("Icon");
            fleetRaceLabel = (Label)FindNode("FleetRaceLabel");

            icon.Connect("mouse_entered", this, nameof(OnIconMouseEntered));
            icon.Connect("mouse_exited", this, nameof(OnIconMouseExited));
        }

        void OnIconMouseEntered()
        {
            var design = MapObject.Fleet.GetPrimaryToken().Design;
            HullSummaryPopup.Instance.Hull = design.Hull;
            HullSummaryPopup.Instance.ShipDesign = design;

            // position the summary view on the corner
            var position = icon.RectGlobalPosition;
            position.x += icon.RectSize.x + 2;
            position.y -= HullSummaryPopup.Instance.RectSize.y + 2;
            HullSummaryPopup.Instance.SetGlobalPosition(position);
            HullSummaryPopup.Instance.Show();
        }

        void OnIconMouseExited()
        {
            HullSummaryPopup.Instance.Hide();
        }


        protected override void UpdateControls()
        {

            if (MapObject != null)
            {
                var race = Me.Race;
                var fleet = MapObject.Fleet;

                icon.Texture = TextureLoader.Instance.FindTexture(fleet.GetPrimaryToken().Design);
                fleetRaceLabel.Text = $"{fleet.Owner.RacePluralName}";
            }
        }
    }
}