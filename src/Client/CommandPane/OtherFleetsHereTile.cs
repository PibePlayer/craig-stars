using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class OtherFleetsHereTile : FleetTile
    {
        static CSLog log = LogProvider.GetLogger(typeof(OtherFleetsHereTile));

        OptionButton otherFleetsOptionButton;
        Button gotoButton;
        Button mergeButton;
        Button cargoTransferButton;

        FleetSprite selectedFleet;
        List<FleetSprite> otherFleets;
        bool canStealCargo;
        CargoTransferer cargoTransferer = new();

        public override void _Ready()
        {
            base._Ready();
            otherFleetsOptionButton = (OptionButton)FindNode("OtherFleetsOptionButton");
            gotoButton = (Button)FindNode("GotoButton");
            mergeButton = (Button)FindNode("MergeButton");
            cargoTransferButton = (Button)FindNode("CargoTransferButton");

            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));
            mergeButton.Connect("pressed", this, nameof(OnMergeButtonPressed));
            cargoTransferButton.Connect("pressed", this, nameof(OnCargoTransferButtonPressed));
            otherFleetsOptionButton.Connect("item_selected", this, nameof(OnOtherFleetsOptionItemSelected));

        }

        protected override void OnNewCommandedFleet()
        {
            base.OnNewCommandedFleet();
            if (CommandedFleet != null)
            {
                canStealCargo = cargoTransferer.GetCanStealFleetCargo(CommandedFleet.Fleet, Me.MapObjectsByLocation);
            }
        }

        void OnGotoButtonPressed()
        {
            if (CommandedFleet != null && selectedFleet != null)
            {
                EventManager.PublishGotoMapObjectEvent(selectedFleet);
            }
        }

        void OnMergeButtonPressed()
        {

        }

        void OnCargoTransferButtonPressed()
        {
            if (CommandedFleet != null)
            {
                EventManager.PublishCargoTransferDialogRequestedEvent(CommandedFleet.Fleet, selectedFleet.Fleet);
            }
        }

        void OnOtherFleetsOptionItemSelected(int index)
        {
            if (otherFleets.Count > index)
            {
                selectedFleet = otherFleets[index];

                cargoTransferButton.Disabled = !(selectedFleet.OwnedByMe || canStealCargo);
                mergeButton.Disabled = !selectedFleet.OwnedByMe;
                gotoButton.Disabled = !selectedFleet.OwnedByMe;

            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                otherFleetsOptionButton.Clear();
                otherFleets = new List<FleetSprite>(CommandedFleet.OtherFleets);

                if (otherFleets?.Count > 0)
                {
                    foreach (var fleet in otherFleets)
                    {
                        otherFleetsOptionButton.AddItem(fleet.Fleet.Name);
                    }
                    // select the first fleet
                    OnOtherFleetsOptionItemSelected(0);
                }
                else
                {
                    selectedFleet = null;
                    cargoTransferButton.Disabled = true;
                    mergeButton.Disabled = true;
                    gotoButton.Disabled = true;
                }
            }
        }

    }
}