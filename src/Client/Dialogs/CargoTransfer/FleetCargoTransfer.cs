using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{

    public class FleetCargoTransfer : VBoxContainer, ICargoTransferControl
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetCargoTransfer));
        public event CargoTransferRequested CargoTransferRequestedEvent;

        public ICargoHolder CargoHolder
        {
            get => Fleet;
            set
            {
                Fleet = value as Fleet;
                UpdateControls();
            }
        }

        public Fleet Fleet { get; set; } = new Fleet()
        {
            Name = "UI Test Fleet",
            Cargo = new Cargo(1, 2, 3, 4),
        };

        public Cargo Cargo
        {
            get => Fleet.Cargo;
            set
            {
                Fleet.Cargo = value;
                UpdateControls();
            }
        }

        public int Fuel
        {
            get => Fleet.Fuel;
            set
            {
                Fleet.Fuel = value;
                UpdateControls();
            }
        }

        public bool ShowFuel { get; set; } = true;
        public bool ShowColonists { get; set; } = true;

        Label nameLabel;
        Label fuelLabel;
        Label colonistsLabel;
        Label noColonistsSpacerLabel;
        Label noColonistsSpacerLabel2;
        Label noFuelSpacerLabel;
        Label noFuelSpacerLabel2;

        CargoBar fuelBar;
        CargoBar cargoBar;
        CargoBar ironiumBar;
        CargoBar boraniumBar;
        CargoBar germaniumBar;
        CargoBar colonistsBar;


        public override void _Ready()
        {
            // for testing, update the spec
            Fleet.Spec.CargoCapacity = Fleet.Cargo.Total * 2;
            Fleet.Spec.FuelCapacity = Fleet.Fuel;
            nameLabel = GetNode<Label>("NameLabel");
            fuelLabel = GetNode<Label>("GridContainer/FuelLabel");
            colonistsLabel = GetNode<Label>("GridContainer/ColonistsLabel");
            fuelBar = GetNode<CargoBar>("GridContainer/FuelBar");
            cargoBar = GetNode<CargoBar>("GridContainer/CargoBar");
            ironiumBar = GetNode<CargoBar>("GridContainer/IroniumBar");
            boraniumBar = GetNode<CargoBar>("GridContainer/BoraniumBar");
            germaniumBar = GetNode<CargoBar>("GridContainer/GermaniumBar");
            colonistsBar = GetNode<CargoBar>("GridContainer/ColonistsBar");

            noColonistsSpacerLabel = GetNode<Label>("GridContainer/NoColonistsSpacerLabel");
            noColonistsSpacerLabel2 = GetNode<Label>("GridContainer/NoColonistsSpacerLabel2");
            noFuelSpacerLabel = GetNode<Label>("GridContainer/NoFuelSpacerLabel");
            noFuelSpacerLabel2 = GetNode<Label>("GridContainer/NoFuelSpacerLabel2");

            fuelBar.ValueUpdatedEvent += OnFuelBarValueUpdated;
            ironiumBar.ValueUpdatedEvent += OnIroniumBarValueUpdated;
            boraniumBar.ValueUpdatedEvent += OnBoraniumBarValueUpdated;
            germaniumBar.ValueUpdatedEvent += OnGermaniumBarValueUpdated;
            colonistsBar.ValueUpdatedEvent += OnColonistsBarValueUpdated;

            UpdateControls();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                fuelBar.ValueUpdatedEvent -= OnFuelBarValueUpdated;
                ironiumBar.ValueUpdatedEvent -= OnIroniumBarValueUpdated;
                boraniumBar.ValueUpdatedEvent -= OnBoraniumBarValueUpdated;
                germaniumBar.ValueUpdatedEvent -= OnGermaniumBarValueUpdated;
                colonistsBar.ValueUpdatedEvent -= OnColonistsBarValueUpdated;
            }
        }

        /// <summary>
        /// If the user updates the ironium bar, request a new transfer of ironium
        /// The interceptor of this event will determine if the request is allowed and update
        /// the fleet cargo, and then call UpdateControls on us to update our controls
        /// </summary>
        /// <param name="newValue"></param>
        void OnIroniumBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Spec.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithIronium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Ironium + available));

            // log.Debug($"Fleet {Fleet.Name} requested a new cargo transfer: {newCargo}");

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnBoraniumBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Spec.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithBoranium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Boranium + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnGermaniumBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Spec.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithGermanium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Germanium + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnColonistsBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Spec.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithColonists(Mathf.Clamp(newValue, 0, Fleet.Cargo.Colonists + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnFuelBarValueUpdated(int newValue)
        {
            int newFuel = Fleet.Fuel;

            // how much room we have in the hold
            var available = Fleet.Spec.FuelCapacity - Fleet.Fuel;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newFuel = Mathf.Clamp(newValue, 0, Fleet.Fuel + available);

            CargoTransferRequestedEvent?.Invoke(new Cargo(), newFuel);
        }

        public void UpdateControls()
        {
            nameLabel.Text = Fleet.Name;
            cargoBar.Cargo = Fleet.Cargo;
            colonistsBar.Visible = colonistsLabel.Visible = ShowColonists;
            noColonistsSpacerLabel.Visible = noColonistsSpacerLabel2.Visible = !ShowColonists;
            fuelLabel.Visible = fuelBar.Visible = ShowColonists;
            noFuelSpacerLabel.Visible = noFuelSpacerLabel2.Visible = !ShowFuel;

            ironiumBar.Cargo = new Cargo(ironium: Fleet.Cargo.Ironium);
            boraniumBar.Cargo = new Cargo(boranium: Fleet.Cargo.Boranium);
            germaniumBar.Cargo = new Cargo(germanium: Fleet.Cargo.Germanium);
            colonistsBar.Cargo = new Cargo(colonists: Fleet.Cargo.Colonists);
            fuelBar.Fuel = Fleet.Fuel;

            // if we are transferring from another fleet, we don't know their cargo capacity spec
            ironiumBar.Capacity = Fleet.Spec.CargoCapacity;
            boraniumBar.Capacity = Fleet.Spec.CargoCapacity;
            germaniumBar.Capacity = Fleet.Spec.CargoCapacity;
            colonistsBar.Capacity = Fleet.Spec.CargoCapacity;

            cargoBar.Capacity = Fleet.Spec.CargoCapacity;
            fuelBar.Capacity = Fleet.Spec.FuelCapacity;

        }

    }
}