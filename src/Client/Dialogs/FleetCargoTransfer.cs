using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
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

        Label nameLabel;
        CargoBar fuelBar;
        CargoBar cargoBar;
        CargoBar ironiumBar;
        CargoBar boraniumBar;
        CargoBar germaniumBar;
        CargoBar colonistsBar;


        public override void _Ready()
        {
            // for testing, update the aggregate
            Fleet.Aggregate.CargoCapacity = Fleet.Cargo.Total * 2;
            Fleet.Aggregate.FuelCapacity = Fleet.Fuel;
            nameLabel = FindNode("NameLabel") as Label;
            fuelBar = FindNode("FuelBar") as CargoBar;
            cargoBar = FindNode("CargoBar") as CargoBar;
            ironiumBar = FindNode("IroniumBar") as CargoBar;
            boraniumBar = FindNode("BoraniumBar") as CargoBar;
            germaniumBar = FindNode("GermaniumBar") as CargoBar;
            colonistsBar = FindNode("ColonistsBar") as CargoBar;

            fuelBar.ValueUpdatedEvent += OnFuelBarValueUpdated;
            ironiumBar.ValueUpdatedEvent += OnIroniumBarValueUpdated;
            boraniumBar.ValueUpdatedEvent += OnBoraniumBarValueUpdated;
            germaniumBar.ValueUpdatedEvent += OnGermaniumBarValueUpdated;
            colonistsBar.ValueUpdatedEvent += OnColonistsBarValueUpdated;

            UpdateControls();
        }

        public override void _ExitTree()
        {
            fuelBar.ValueUpdatedEvent -= OnFuelBarValueUpdated;
            ironiumBar.ValueUpdatedEvent -= OnIroniumBarValueUpdated;
            boraniumBar.ValueUpdatedEvent -= OnBoraniumBarValueUpdated;
            germaniumBar.ValueUpdatedEvent -= OnGermaniumBarValueUpdated;
            colonistsBar.ValueUpdatedEvent -= OnColonistsBarValueUpdated;
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
            var available = Fleet.Aggregate.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithIronium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Ironium + available));

            // log.Debug($"Fleet {Fleet.Name} requested a new cargo transfer: {newCargo}");

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnBoraniumBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Aggregate.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithBoranium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Boranium + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnGermaniumBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Aggregate.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithGermanium(Mathf.Clamp(newValue, 0, Fleet.Cargo.Germanium + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnColonistsBarValueUpdated(int newValue)
        {
            Cargo newCargo = Fleet.Cargo;

            // how much room we have in the hold
            var available = Fleet.Aggregate.CargoCapacity - Fleet.Cargo.Total;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newCargo = newCargo.WithColonists(Mathf.Clamp(newValue, 0, Fleet.Cargo.Colonists + available));

            CargoTransferRequestedEvent?.Invoke(newCargo, 0);
        }

        void OnFuelBarValueUpdated(int newValue)
        {
            int newFuel = Fleet.Fuel;

            // how much room we have in the hold
            var available = Fleet.Aggregate.FuelCapacity - Fleet.Fuel;

            // make sure we only request a new cargo value between 0 and the most amount of mineral we can put in
            newFuel = Mathf.Clamp(newValue, 0, Fleet.Fuel + available);

            CargoTransferRequestedEvent?.Invoke(new Cargo(), newFuel);
        }

        internal void UpdateControls()
        {
            nameLabel.Text = Fleet.Name;
            cargoBar.Cargo = Fleet.Cargo;
            ironiumBar.Cargo = new Cargo(ironium: Fleet.Cargo.Ironium);
            boraniumBar.Cargo = new Cargo(boranium: Fleet.Cargo.Boranium);
            germaniumBar.Cargo = new Cargo(germanium: Fleet.Cargo.Germanium);
            colonistsBar.Cargo = new Cargo(colonists: Fleet.Cargo.Colonists);
            fuelBar.Fuel = Fleet.Fuel;

            cargoBar.Capacity = Fleet.Aggregate.CargoCapacity;
            ironiumBar.Capacity = Fleet.Aggregate.CargoCapacity;
            boraniumBar.Capacity = Fleet.Aggregate.CargoCapacity;
            germaniumBar.Capacity = Fleet.Aggregate.CargoCapacity;
            colonistsBar.Capacity = Fleet.Aggregate.CargoCapacity;
            fuelBar.Capacity = Fleet.Aggregate.FuelCapacity;
        }

        public bool AttemptTransfer(Cargo newCargo, int newFuel)
        {
            var cargoResult = Fleet.Cargo + newCargo;
            var fuelResult = Fleet.Fuel + newFuel;
            if (cargoResult >= 0 && fuelResult >= 0)
            {
                // update the cargo
                Fleet.Cargo = cargoResult;
                Fleet.Fuel = fuelResult;
                UpdateControls();
                return true;
            }
            return false;
        }
    }
}