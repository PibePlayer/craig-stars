using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class FleetSummaryContainer : FleetSummary
    {
        TextureRect icon;
        Label fleetRaceLabel;

        public override void _Ready()
        {
            base._Ready();
            icon = (TextureRect)FindNode("Icon");
            fleetRaceLabel = (Label)FindNode("FleetRaceLabel");
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