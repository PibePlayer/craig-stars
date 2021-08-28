using Godot;
using System;
using CraigStars.Singletons;
using CraigStars;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class FleetTile : AbstractCommandedFleetControl
    {
        protected FleetService fleetService = new();
        Label titleLabel;

        public override void _Ready()
        {
            base._Ready();
            titleLabel = (Label)FindNode("TitleLabel");
        }

        protected void UpdateTitle(string title)
        {
            titleLabel.Text = title;
        }

        /// <summary>
        /// Called when a new active fleet has been selected
        /// Note, this will be called when setting the CommandedFleet to null
        /// </summary>
        protected override void OnNewCommandedFleet() { }

        protected override void UpdateControls()
        {
            Visible = CommandedFleet != null;
        }

    }
}