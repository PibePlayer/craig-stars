using CraigStars.Singletons;
using Godot;
using log4net;
using System;

namespace CraigStars
{
    public class CargoTransferDialog : WindowDialog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CargoTransferDialog));

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

        Button okButton;

        int quantityModifier = 1;

        /// <summary>
        /// This is the net cargo difference we record when the OK button is pressed
        /// </summary>
        Cargo netCargoDiff = new Cargo();

        public override void _Ready()
        {
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


            okButton = FindNode("OKButton") as Button;

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
            okButton.Connect("pressed", this, nameof(OnOK));
        }

        /// <summary>
        /// Set the quantity modifier for the dialog
        /// if the user holds shift, we multipy by 10, if they press control we multiply by 100
        /// both multiplies by 1000
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key)
            {
                if (key.Pressed && key.Scancode == (uint)KeyList.Shift)
                {
                    quantityModifier *= 10;
                }
                else if (key.Pressed && key.Scancode == (uint)KeyList.Control)
                {
                    quantityModifier *= 100;
                }
                else
                {
                    quantityModifier = 1;
                }
            }
        }


        void OnSourceButtonPressed(CargoType type)
        {
            AttemptSourceTransfer(Cargo.OfAmount(type, -quantityModifier));
        }

        void OnDestButtonPressed(CargoType type)
        {
            AttemptDestTransfer(Cargo.OfAmount(type, -quantityModifier));
        }

        /// <summary>
        /// Wire up events
        /// </summary>
        void OnAboutToShow()
        {
            // clear out the cargoDiff
            netCargoDiff = new Cargo();
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

        void OnOK()
        {
            if (netCargoDiff != Cargo.Empty)
            {
                var me = PlayersManager.Me;
                var source = sourceCargoTransfer.CargoHolder;
                var dest = destCargoTransfer.CargoHolder;
                me.CargoTransferOrders.Add(new CargoTransferOrder()
                {
                    Source = source,
                    Dest = dest,
                    Transfer = netCargoDiff
                });
                if (source is Fleet sourceFleet)
                {
                    sourceFleet.ComputeAggregate();
                }
                if (dest is Fleet destFleet)
                {
                    destFleet.ComputeAggregate();
                }
                log.Info($"{me.Name} made immediate transfer from {source.Name} to {dest.Name} for {netCargoDiff} cargo");

                // zero it out for next time
                netCargoDiff = new Cargo();
            }
            Hide();
        }

        void OnSourceCargoTransferRequested(Cargo newCargo)
        {
            var cargoDiff = sourceCargoTransfer.Cargo - newCargo;
            AttemptSourceTransfer(cargoDiff);
        }

        void AttemptSourceTransfer(Cargo cargoDiff)
        {
            if (destCargoTransfer.AttemptTransfer(cargoDiff))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                sourceCargoTransfer.Cargo = sourceCargoTransfer.Cargo - cargoDiff;
                netCargoDiff -= cargoDiff;
            }
        }

        void OnDestCargoTransferRequested(Cargo newCargo)
        {
            var cargoDiff = destCargoTransfer.Cargo - newCargo;
            AttemptDestTransfer(cargoDiff);
        }

        void AttemptDestTransfer(Cargo cargoDiff)
        {
            if (sourceCargoTransfer.AttemptTransfer(cargoDiff))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                destCargoTransfer.Cargo = destCargoTransfer.Cargo - cargoDiff;
                netCargoDiff += cargoDiff;
            }

        }


    }
}