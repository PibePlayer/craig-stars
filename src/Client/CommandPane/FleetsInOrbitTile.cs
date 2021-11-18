using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class FleetsInOrbitTile : PlanetTile
    {
        OptionButton fleetsInOrbitOptionButton;
        CargoBar fuelBar;
        CargoBar cargoBar;
        Button gotoButton;

        FleetSprite SelectedFleet;
        List<FleetSprite> OribitingFleets;

        public override void _Ready()
        {
            base._Ready();

            fleetsInOrbitOptionButton = (OptionButton)FindNode("FleetsInOrbitOptionButton");
            gotoButton = (Button)FindNode("GotoButton");

            fuelBar = (CargoBar)FindNode("FuelBar");
            cargoBar = (CargoBar)FindNode("CargoBar");

            fleetsInOrbitOptionButton.Connect("item_selected", this, nameof(OnFleetsInOrbitOptionItemSelected));
            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));

            cargoBar.ValueUpdatedEvent += OnCargoBarPressed;

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
            }
        }

        void OnCargoBarPressed(int newValue)
        {
            if (SelectedFleet?.Fleet != null && SelectedFleet.Fleet.Spec.CargoCapacity > 0 && SelectedFleet?.Fleet?.Orbiting != null)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                EventManager.PublishCargoTransferDialogRequestedEvent(SelectedFleet.Fleet, SelectedFleet.Fleet.Orbiting);
            }
        }

        void OnFleetsInOrbitOptionItemSelected(int index)
        {
            if (OribitingFleets.Count > index)
            {
                SelectedFleet = OribitingFleets[index];
                UpdateCargoBars();
            }
        }

        void OnGotoButtonPressed()
        {
            if (SelectedFleet != null)
            {
                EventManager.PublishGotoMapObjectEvent(SelectedFleet);
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedPlanet != null)
            {
                fleetsInOrbitOptionButton.Clear();
                OribitingFleets = CommandedPlanet.OrbitingFleets.Where(f => f.OwnedByMe).ToList();
                if (OribitingFleets.Count > 0)
                {
                    SelectedFleet = OribitingFleets[0];
                    cargoBar.Visible = true;
                    fuelBar.Visible = true;
                    gotoButton.Disabled = false;

                    foreach (FleetSprite fleet in OribitingFleets)
                    {
                        fleetsInOrbitOptionButton.AddItem(fleet.Fleet.Name);
                    }

                    UpdateCargoBars();
                }
                else
                {
                    gotoButton.Disabled = true;
                    SelectedFleet = null;
                    OribitingFleets.Clear();
                    cargoBar.Visible = false;
                    fuelBar.Visible = false;
                }
            }

        }

        void UpdateCargoBars()
        {
            cargoBar.Cargo = SelectedFleet.Fleet.Cargo;
            cargoBar.Capacity = SelectedFleet.Fleet.Spec.CargoCapacity;
            fuelBar.Fuel = SelectedFleet.Fleet.Fuel;
            fuelBar.Capacity = SelectedFleet.Fleet.Spec.FuelCapacity;
        }
    }
}
