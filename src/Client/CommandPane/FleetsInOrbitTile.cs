using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
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

            fleetsInOrbitOptionButton = GetNode<OptionButton>("VBoxContainer/FleetsInOrbitOptionButton");
            gotoButton = GetNode<Button>("VBoxContainer/HBoxContainer/GotoButton");

            fuelBar = (CargoBar)FindNode("FuelBar");
            cargoBar = (CargoBar)FindNode("CargoBar");

            fleetsInOrbitOptionButton.Connect("item_selected", this, nameof(OnFleetsInOrbitOptionItemSelected));
            gotoButton.Connect("pressed", this, nameof(OnGotoButtonPressed));

            cargoBar.ValueUpdatedEvent += OnCargoBarPressed;

        }

        public override void _ExitTree()
        {
            cargoBar.ValueUpdatedEvent -= OnCargoBarPressed;
            base._ExitTree();
        }

        void OnCargoBarPressed(int newValue)
        {
            if (SelectedFleet?.Fleet != null && SelectedFleet.Fleet.Aggregate.CargoCapacity > 0 && SelectedFleet?.Fleet?.Orbiting != null)
            {
                // trigger a cargo transfer event between this fleet and the planet it is orbiting
                Signals.PublishCargoTransferRequestedEvent(SelectedFleet.Fleet, SelectedFleet.Fleet.Orbiting);
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
                Signals.PublishGotoMapObjectEvent(SelectedFleet);
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                OribitingFleets = ActivePlanet.OrbitingFleets.Where(f => f.OwnedByMe).ToList();
                if (OribitingFleets.Count > 0)
                {
                    SelectedFleet = OribitingFleets[0];
                    cargoBar.Visible = true;
                    fuelBar.Visible = true;
                    gotoButton.Disabled = false;

                    fleetsInOrbitOptionButton.Clear();
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
            cargoBar.Capacity = SelectedFleet.Fleet.Aggregate.CargoCapacity;
            fuelBar.Fuel = SelectedFleet.Fleet.Fuel;
            fuelBar.Capacity = SelectedFleet.Fleet.Aggregate.FuelCapacity;
        }
    }
}
