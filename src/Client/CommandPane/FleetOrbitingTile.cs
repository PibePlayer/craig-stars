using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class FleetOrbitingTile : FleetTile
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetOrbitingTile));

        Button gotoButton;
        Button cargoTransferButton;

        public override void _Ready()
        {
            base._Ready();
            gotoButton = (Button)FindNode("GotoButton");
            cargoTransferButton = (Button)FindNode("CargoTransferButton");

            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            cargoTransferButton.Connect("pressed", this, nameof(OnCargoTransferButtonPressed));
        }


        void OnGotoButtonPressed()
        {
            if (CommandedFleet != null && CommandedFleet.Orbiting != null && CommandedFleet.Orbiting.OwnedByMe)
            {
                EventManager.PublishGotoMapObjectEvent(CommandedFleet.Orbiting);
            }
        }

        void OnCargoTransferButtonPressed()
        {
            if (CommandedFleet != null)
            {
                EventManager.PublishCargoTransferDialogRequestedEvent(CommandedFleet.Fleet, CommandedFleet.Fleet.Orbiting);
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                if (CommandedFleet.Fleet.Orbiting != null)
                {
                    UpdateTitle($"Orbiting {CommandedFleet.Fleet.Orbiting.Name}");
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
                    UpdateTitle("In Deep Space");
                    cargoTransferButton.Text = "Jettison";
                    gotoButton.Disabled = true;
                }
            }
        }

    }
}