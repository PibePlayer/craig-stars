using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using System;
using log4net;
using log4net.Core;
using log4net.Appender;

namespace CraigStars
{
    public class Game : Node
    {
        ILog log = LogManager.GetLogger(typeof(Game));

        /// <summary>
        /// The game node creates a server in single player or host mode
        /// </summary>
        public Server Server { get; set; } = new Server();

        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;

        ProductionQueueDialog productionQueueDialog;
        CargoTransferDialog cargoTransferDialog;
        ResearchDialog researchDialog;
        TechBrowserDialog techBrowserDialog;
        ShipDesignerDialog shipDesignerDialog;

        public override void _Ready()
        {
            scanner = FindNode("Scanner") as Scanner;
            productionQueueDialog = GetNode<ProductionQueueDialog>("CanvasLayer/ProductionQueueDialog");
            cargoTransferDialog = GetNode<CargoTransferDialog>("CanvasLayer/CargoTransferDialog");
            researchDialog = GetNode<ResearchDialog>("CanvasLayer/ResearchDialog");
            techBrowserDialog = GetNode<TechBrowserDialog>("CanvasLayer/TechBrowserDialog");
            shipDesignerDialog = GetNode<ShipDesignerDialog>("CanvasLayer/ShipDesignerDialog");

            Signals.PostStartGameEvent += OnPostStartGameEvent;
            Signals.ChangeProductionQueuePressedEvent += OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent += OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent += OnResearchDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent += OnShipDesignerDialogRequestedEvent;
            Signals.TechBrowserDialogRequestedEvent += OnTechBrowserDialogRequestedEvent;

            // if we are the server (or a single player game)
            // init the server and send a notice to all players that it's time to start
            if (this.IsServerOrSinglePlayer())
            {
                if (PlayersManager.Instance.Players.Count == 0)
                {
                    log.Warn("Resetting Players. This probably means you are executing the Game scene directly during development. If not, this is a problem.");
                    PlayersManager.Instance.SetupPlayers();
                }
                Server.Init(PlayersManager.Instance.Players.Cast<Player>().ToList(), SettingsManager.Settings, TechStore.Instance);
                if (this.IsServer())
                {
                    // TODO: send each player their turn data
                }
                // notify everyone (including ourselves) that we're ready to start
                Signals.PublishPostStartGameEvent(Server.Year);
            }
            else
            {
                // if we aren't the server, we come here with our player data already loaded
                OnPostStartGameEvent(PlayersManager.Instance.Me.Year);
            }
        }

        public override void _ExitTree()
        {
            Server.Shutdown();
            Signals.PostStartGameEvent -= OnPostStartGameEvent;
            Signals.ChangeProductionQueuePressedEvent -= OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent -= OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent -= OnResearchDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent -= OnShipDesignerDialogRequestedEvent;
            Signals.TechBrowserDialogRequestedEvent -= OnTechBrowserDialogRequestedEvent;
        }

        /// <summary>
        /// When the game is ready to go, init the scanner
        /// </summary>
        /// <param name="year"></param>
        void OnPostStartGameEvent(int year)
        {
            // add the universe to the viewport
            scanner.InitMapObjects();
        }

        void OnTechBrowserDialogRequestedEvent()
        {
            techBrowserDialog.PopupCentered();
        }

        void OnShipDesignerDialogRequestedEvent()
        {
            shipDesignerDialog.PopupCentered();
        }

        void OnResearchDialogRequested()
        {
            researchDialog.PopupCentered();
        }

        void OnCargoTransferRequested(ICargoHolder source, ICargoHolder dest)
        {
            cargoTransferDialog.Source = source;
            cargoTransferDialog.Dest = dest;
            cargoTransferDialog.PopupCentered();
        }

        void OnChangeProductionQueue(PlanetSprite planetSprite)
        {
            productionQueueDialog.Planet = planetSprite.Planet;
            productionQueueDialog.PopupCentered();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                Signals.PublishSubmitTurnEvent(PlayersManager.Instance.Me);
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                Signals.PublishTechBrowserDialogRequestedEvent();
            }
            if (@event.IsActionPressed("research"))
            {
                Signals.PublishResearchDialogRequestedEvent();
            }
            if (@event.IsActionPressed("ship_designer"))
            {
                Signals.PublishShipDesignerDialogRequestedEvent();
            }
        }

    }
}
