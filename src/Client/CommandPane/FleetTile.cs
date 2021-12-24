using System;
using System.Collections.Generic;
using CraigStars;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class FleetTile : AbstractCommandedFleetControl
    {
        [Inject] protected FleetService fleetService;
        Label titleLabel;

        public override void _Ready()
        {
            this.ResolveDependencies();

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