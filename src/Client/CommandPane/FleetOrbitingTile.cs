using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using log4net;
using CraigStars.Utils;

namespace CraigStars
{
    public class FleetOrbitingTile : FleetTile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetOrbitingTile));

        Button gotoButton;
        Button cargoTransferButton;

        public override void _Ready()
        {
            base._Ready();
            gotoButton = GetNode<Button>("VBoxContainer/HBoxContainer/GotoButton");
            cargoTransferButton = GetNode<Button>("VBoxContainer/HBoxContainer/CargoTransferButton");

            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            cargoTransferButton.Connect("pressed", this, nameof(OnCargoTransferButtonPressed));
        }


        void OnGotoButtonPressed()
        {
            if (ActiveFleet != null && ActiveFleet.Orbiting != null && ActiveFleet.Orbiting.OwnedByMe)
            {
                Signals.PublishGotoMapObjectEvent(ActiveFleet.Orbiting);
            }
        }

        void OnCargoTransferButtonPressed()
        {
            if (ActiveFleet != null)
            {
                Signals.PublishCargoTransferRequestedEvent(ActiveFleet.Fleet, ActiveFleet.Fleet.Orbiting);
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActiveFleet != null)
            {
                if (ActiveFleet.Fleet.Orbiting != null && ActiveFleet.Orbiting.OwnedByMe)
                {
                    titleLabel.Text = $"Orbiting {ActiveFleet.Fleet.Orbiting.Name}";
                    gotoButton.Disabled = false;
                    cargoTransferButton.Text = "Cargo";
                }
                else
                {
                    titleLabel.Text = "In Deep Space";
                    cargoTransferButton.Text = "Jettison";
                    gotoButton.Disabled = true;
                }
            }
        }

    }
}