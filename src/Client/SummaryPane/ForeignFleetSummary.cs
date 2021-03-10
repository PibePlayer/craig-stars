using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class ForeignFleetSummary : FleetSummary
    {
        Label shipCountLabel;
        Label massLabel;

        public override void _Ready()
        {
            base._Ready();
            shipCountLabel = (Label)FindNode("ShipCountLabel");
            massLabel = (Label)FindNode("MassLabel");
        }

        protected override void UpdateControls()
        {
            if (Fleet != null)
            {
                var race = Me.Race;
                var fleet = Fleet.Fleet;

                if (!fleet.OwnedBy(Me))
                {
                    Visible = true;
                    shipCountLabel.Text = $"Ship Count: {fleet.Tokens.Count}";
                    massLabel.Text = $"Fleet Mass: {fleet.Aggregate.Mass}";
                }
                else
                {
                    Visible = false;
                }
            }
        }
    }
}