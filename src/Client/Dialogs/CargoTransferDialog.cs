using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class CargoTransferDialog : GameViewDialog
    {
        static CSLog log = LogProvider.GetLogger(typeof(CargoTransferDialog));

        [Inject] FleetSpecService fleetSpecService;
        [Inject] PlanetService planetService;

        public ICargoHolder Source { get; set; }
        public ICargoHolder Dest { get; set; }
        ICargoTransferControl sourceCargoTransfer;
        ICargoTransferControl destCargoTransfer;

        FleetCargoTransfer sourceFleetCargoTransfer;
        PlanetCargoTransfer sourcePlanetCargoTransfer;
        FleetCargoTransfer destFleetCargoTransfer;
        PlanetCargoTransfer destPlanetCargoTransfer;

        Button ironiumSourceButton;
        Button boraniumSourceButton;
        Button germaniumSourceButton;
        Button colonistsSourceButton;
        Button fuelSourceButton;

        Button ironiumDestButton;
        Button boraniumDestButton;
        Button germaniumDestButton;
        Button colonistsDestButton;
        Button fuelDestButton;

        int quantityModifier = 1;

        /// <summary>
        /// This is the net cargo difference we record when the OK button is pressed
        /// </summary>
        Cargo netCargoDiff = new Cargo();
        int netFuelDiff = 0;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            sourceFleetCargoTransfer = FindNode("SourceFleetCargoTransfer") as FleetCargoTransfer;
            sourcePlanetCargoTransfer = FindNode("SourcePlanetCargoTransfer") as PlanetCargoTransfer;
            destFleetCargoTransfer = FindNode("DestFleetCargoTransfer") as FleetCargoTransfer;
            destPlanetCargoTransfer = FindNode("DestPlanetCargoTransfer") as PlanetCargoTransfer;

            ironiumSourceButton = FindNode("IroniumSourceButton") as Button;
            boraniumSourceButton = FindNode("BoraniumSourceButton") as Button;
            germaniumSourceButton = FindNode("GermaniumSourceButton") as Button;
            colonistsSourceButton = FindNode("ColonistsSourceButton") as Button;
            fuelSourceButton = FindNode("FuelSourceButton") as Button;
            ironiumDestButton = FindNode("IroniumDestButton") as Button;
            boraniumDestButton = FindNode("BoraniumDestButton") as Button;
            germaniumDestButton = FindNode("GermaniumDestButton") as Button;
            colonistsDestButton = FindNode("ColonistsDestButton") as Button;
            fuelDestButton = FindNode("FuelDestButton") as Button;


            if (Source == null)
            {
                log.Warn("No source specified, probably testing the UI");
                Source = sourceFleetCargoTransfer.Fleet;
                Dest = destPlanetCargoTransfer.Planet;
            }

            ironiumSourceButton.Connect("pressed", this, nameof(OnSourceButtonPressed), new Godot.Collections.Array() { CargoType.Ironium });
            boraniumSourceButton.Connect("pressed", this, nameof(OnSourceButtonPressed), new Godot.Collections.Array() { CargoType.Boranium });
            germaniumSourceButton.Connect("pressed", this, nameof(OnSourceButtonPressed), new Godot.Collections.Array() { CargoType.Germanium });
            colonistsSourceButton.Connect("pressed", this, nameof(OnSourceButtonPressed), new Godot.Collections.Array() { CargoType.Colonists });
            fuelSourceButton.Connect("pressed", this, nameof(OnSourceButtonPressed), new Godot.Collections.Array() { CargoType.Fuel });
            ironiumDestButton.Connect("pressed", this, nameof(OnDestButtonPressed), new Godot.Collections.Array() { CargoType.Ironium });
            boraniumDestButton.Connect("pressed", this, nameof(OnDestButtonPressed), new Godot.Collections.Array() { CargoType.Boranium });
            germaniumDestButton.Connect("pressed", this, nameof(OnDestButtonPressed), new Godot.Collections.Array() { CargoType.Germanium });
            colonistsDestButton.Connect("pressed", this, nameof(OnDestButtonPressed), new Godot.Collections.Array() { CargoType.Colonists });
            fuelDestButton.Connect("pressed", this, nameof(OnDestButtonPressed), new Godot.Collections.Array() { CargoType.Fuel });

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
        }

        /// <summary>
        /// Set the quantity modifier for the dialog
        /// if the user holds shift, we multipy by 10, if they press control we multiply by 100
        /// both multiplies by 1000
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            quantityModifier = this.UpdateQuantityModifer(@event, quantityModifier);
        }


        void OnSourceButtonPressed(CargoType type)
        {
            if (type == CargoType.Fuel)
            {
                AttemptSourceTransfer(Cargo.Empty, -quantityModifier);
            }
            else
            {
                AttemptSourceTransfer(Cargo.OfAmount(type, -quantityModifier), 0);
            }
        }

        void OnDestButtonPressed(CargoType type)
        {
            if (type == CargoType.Fuel)
            {
                AttemptDestTransfer(Cargo.Empty, -quantityModifier);
            }
            else
            {
                AttemptDestTransfer(Cargo.OfAmount(type, -quantityModifier), 0);

            }
        }

        /// <summary>
        /// Wire up events
        /// </summary>
        void OnAboutToShow()
        {
            // clear out the cargoDiff
            netCargoDiff = new Cargo();
            netFuelDiff = 0;
            if (Source is Planet)
            {
                sourceCargoTransfer = sourcePlanetCargoTransfer;
            }
            else if (Source is Fleet)
            {
                sourceCargoTransfer = sourceFleetCargoTransfer;
            }

            if (Dest is Planet)
            {
                destCargoTransfer = destPlanetCargoTransfer;
            }
            else if (Dest is Fleet)
            {
                destCargoTransfer = destFleetCargoTransfer;
            }
            else
            {
                // TODO add deep space jettison
            }

            sourceCargoTransfer.CargoHolder = Source;
            destCargoTransfer.CargoHolder = Dest;

            sourceCargoTransfer.CargoTransferRequestedEvent += OnSourceCargoTransferRequested;
            destCargoTransfer.CargoTransferRequestedEvent += OnDestCargoTransferRequested;
        }

        /// <summary>
        /// Disconnect events
        /// </summary>
        void OnPopupHide()
        {
            sourceCargoTransfer.CargoTransferRequestedEvent -= OnSourceCargoTransferRequested;
            destCargoTransfer.CargoTransferRequestedEvent -= OnDestCargoTransferRequested;
        }

        protected override void OnOk()
        {
            if (netCargoDiff != Cargo.Empty || netFuelDiff != 0)
            {
                Fleet source;
                ICargoHolder dest;

                // CargoTransferOrders require the source to be a fleet.
                if (sourceCargoTransfer.CargoHolder is Fleet sourceFleet)
                {
                    source = sourceFleet;
                    dest = destCargoTransfer.CargoHolder;
                }
                else if (destCargoTransfer.CargoHolder is Fleet destFleet)
                {
                    source = destFleet;
                    dest = sourceCargoTransfer.CargoHolder;
                }
                else
                {
                    log.Error($"Either source or Dest must be a Fleet for a CargoTransfer: Source: {sourceCargoTransfer.CargoHolder.Name}, Dest: {destCargoTransfer.CargoHolder.Name}");
                    return;
                }

                var order = new CargoTransferOrder()
                {
                    Source = source,
                    Dest = dest,
                    Transfer = netCargoDiff,
                    FuelTransfer = netFuelDiff
                };

                Me.CargoTransferOrders.Add(order);
                Me.ImmediateFleetOrders.Add(order);

                // update the spec for the source fleet
                fleetSpecService.ComputeFleetSpec(Me, source, recompute: true);

                if (dest is Fleet fleet)
                {
                    // if the dest is also a fleet, update its spec with a new mass
                    fleetSpecService.ComputeFleetSpec(Me, fleet, recompute: true);
                }
                else if (dest is Planet planet)
                {
                    planet.Spec = planetService.ComputePlanetSpec(planet, base.Me);
                }
                log.Info($"{Me.Name} made immediate transfer from {source.Name} to {dest.Name} for {netCargoDiff} cargo and {netFuelDiff} fuel");

                EventManager.PublishCargoTransferredEvent(source, dest);
                // zero it out for next time
                netCargoDiff = new Cargo();
                netFuelDiff = 0;
            }

            Hide();
        }

        void OnSourceCargoTransferRequested(Cargo newCargo, int fuel)
        {
            var cargoDiff = sourceCargoTransfer.Cargo - newCargo;
            AttemptSourceTransfer(cargoDiff, fuel);
        }

        void AttemptSourceTransfer(Cargo cargoDiff, int fuel)
        {
            if (destCargoTransfer.AttemptTransfer(cargoDiff, fuel))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                sourceCargoTransfer.Cargo = sourceCargoTransfer.Cargo - cargoDiff;
                netCargoDiff -= cargoDiff;
                netFuelDiff -= fuel;
            }
        }

        void OnDestCargoTransferRequested(Cargo newCargo, int fuel)
        {
            var cargoDiff = destCargoTransfer.Cargo - newCargo;
            AttemptDestTransfer(cargoDiff, fuel);
        }

        void AttemptDestTransfer(Cargo cargoDiff, int fuel)
        {
            if (sourceCargoTransfer.AttemptTransfer(cargoDiff, fuel))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                destCargoTransfer.Cargo = destCargoTransfer.Cargo - cargoDiff;
                netCargoDiff += cargoDiff;
                netFuelDiff += fuel;
            }

        }


    }
}