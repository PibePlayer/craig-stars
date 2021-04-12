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
            if (CommandedFleet != null && CommandedFleet.Orbiting != null && CommandedFleet.Orbiting.OwnedByMe)
            {
                Signals.PublishGotoMapObjectEvent(CommandedFleet.Orbiting);
            }
        }

        void OnCargoTransferButtonPressed()
        {
            if (CommandedFleet != null)
            {
                Signals.PublishCargoTransferRequestedEvent(CommandedFleet.Fleet, CommandedFleet.Fleet.Orbiting);
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                if (CommandedFleet.Fleet.Orbiting != null)
                {
                    titleLabel.Text = $"Orbiting {CommandedFleet.Fleet.Orbiting.Name}";
                    if (CommandedFleet.Orbiting.OwnedByMe)
                    {
                        gotoButton.Disabled = false;
                        cargoTransferButton.Text = "Cargo";
                    }
                    else
                    {
                        gotoButton.Disabled = true;
                        cargoTransferButton.Text = "Jettison";
                    }
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