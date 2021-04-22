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
        Label warpFactorLabel;

        public override void _Ready()
        {
            base._Ready();
            shipCountLabel = GetNode<Label>("ShipCountLabel");
            massLabel = GetNode<Label>("MassLabel");
            warpFactorLabel = GetNode<Label>("WarpFactorLabel");
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
                    massLabel.Text = $"Fleet Mass: {fleet.Mass}kT";
                    warpFactorLabel.Text = $"Warp Speed: {fleet.WarpSpeed}";
                }
                else
                {
                    Visible = false;
                }
            }
        }
    }
}