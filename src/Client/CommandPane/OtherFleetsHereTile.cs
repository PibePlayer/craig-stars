using Godot;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using log4net;
using CraigStars.Utils;
using System.Linq;

namespace CraigStars
{
    public class OtherFleetsHereTile : FleetTile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OtherFleetsHereTile));

        OptionButton otherFleetsOptionButton;
        Button gotoButton;
        Button mergeButton;
        Button cargoTransferButton;

        FleetSprite selectedFleet;
        List<FleetSprite> otherFleets;

        public override void _Ready()
        {
            base._Ready();
            otherFleetsOptionButton = GetNode<OptionButton>("VBoxContainer/OtherFleetsOptionButton");
            gotoButton = GetNode<Button>("VBoxContainer/HBoxContainer/GotoButton");
            mergeButton = GetNode<Button>("VBoxContainer/HBoxContainer/MergeButton");
            cargoTransferButton = GetNode<Button>("VBoxContainer/HBoxContainer/CargoTransferButton");

            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            mergeButton.Connect("pressed", this, nameof(OnMergeButtonPressed));
            cargoTransferButton.Connect("pressed", this, nameof(OnCargoTransferButtonPressed));
            otherFleetsOptionButton.Connect("item_selected", this, nameof(OnOtherFleetsOptionItemSelected));

        }

        void OnGotoButtonPressed()
        {
            if (CommandedFleet != null && CommandedFleet.Orbiting != null)
            {
                Signals.PublishGotoMapObjectEvent(CommandedFleet.Orbiting);
            }
        }

        void OnMergeButtonPressed()
        {

        }

        void OnCargoTransferButtonPressed()
        {
            if (CommandedFleet != null)
            {
                Signals.PublishCargoTransferRequestedEvent(CommandedFleet.Fleet, CommandedFleet.Fleet.Orbiting);
            }
        }

        void OnOtherFleetsOptionItemSelected(int index)
        {
            if (otherFleets.Count > index)
            {
                selectedFleet = otherFleets[index];
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                otherFleetsOptionButton.Clear();
                otherFleets = CommandedFleet.OtherFleets?.Where(f => f.OwnedByMe).ToList();
                if (otherFleets?.Count > 0)
                {
                    cargoTransferButton.Disabled = false;
                    mergeButton.Disabled = false;
                    gotoButton.Disabled = false;

                    foreach (var fleet in otherFleets)
                    {
                        otherFleetsOptionButton.AddItem(fleet.Fleet.Name);
                    }
                }
                else
                {
                    cargoTransferButton.Disabled = true;
                    mergeButton.Disabled = true;
                    gotoButton.Disabled = true;
                }
            }
        }

    }
}