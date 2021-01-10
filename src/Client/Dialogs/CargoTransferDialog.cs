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

        Button okButton;

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
            okButton = FindNode("OKButton") as Button;

            if (Source == null)
            {
                log.Warn("No source specified, probably testing the UI");
                Source = sourceFleetCargoTransfer.Fleet;
                Dest = destPlanetCargoTransfer.Planet;
            }

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            okButton.Connect("pressed", this, nameof(OnOK));
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
                var me = PlayersManager.Instance.Me;
                var source = sourceCargoTransfer.CargoHolder;
                var dest = destCargoTransfer.CargoHolder;
                me.CargoTransferOrders.Add(new CargoTransferOrder()
                {
                    Source = source,
                    Dest = dest,
                    Transfer = netCargoDiff
                });
                log.Info($"{me.Name} made immediate transfer from {source.Name} to {dest.Name} for {netCargoDiff} cargo");

                // zero it out for next time
                netCargoDiff = new Cargo();
            }
            Hide();
        }

        void OnSourceCargoTransferRequested(Cargo newCargo)
        {
            var cargoDiff = sourceCargoTransfer.Cargo - newCargo;
            if (destCargoTransfer.AttemptTransfer(cargoDiff))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                sourceCargoTransfer.Cargo = newCargo;
                netCargoDiff -= cargoDiff;
            }
        }

        void OnDestCargoTransferRequested(Cargo newCargo)
        {
            var cargoDiff = destCargoTransfer.Cargo - newCargo;
            if (sourceCargoTransfer.AttemptTransfer(cargoDiff))
            {
                // the cargo transfer was successful, so update the source cargo
                // with their requested value
                destCargoTransfer.Cargo = newCargo;
                netCargoDiff += cargoDiff;
            }
        }


    }
}