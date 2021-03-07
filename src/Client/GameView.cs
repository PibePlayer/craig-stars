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
    public class GameView : Node
    {
        ILog log = LogManager.GetLogger(typeof(GameView));

        /// <summary>
        /// The game node creates a server in single player or host mode
        /// </summary>
        public Game Game { get; private set; }

        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;

        ProductionQueueDialog productionQueueDialog;
        CargoTransferDialog cargoTransferDialog;
        ResearchDialog researchDialog;
        TechBrowserDialog techBrowserDialog;
        ShipDesignerDialog shipDesignerDialog;

        /// <summary>
        /// When this node enters the tree, setup any server/player stuff
        /// </summary>
        public override void _EnterTree()
        {
            if (PlayersManager.Instance.Players.Count == 0)
            {
                log.Warn("Resetting Players. This probably means you are executing the Game scene directly during development. If not, this is a problem.");
                PlayersManager.Instance.SetupPlayers();
            }

        }

        public override void _Ready()
        {
            Game = new Game() { TechStore = TechStore.Instance };
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
                if (GameSettings.Instance.ShouldContinueGame)
                {
                    GameSaver saver = new GameSaver(Game);
                    saver.LoadGame(GameSettings.Instance.ContinueGame, GameSettings.Instance.ContinueYear, TechStore.Instance);
                    PlayersManager.Instance.InitPlayersFromGame(Game.Players);
                }
                else
                {
                    Game.Init(PlayersManager.Instance.Players.Cast<Player>().ToList(), RulesManager.Rules, TechStore.Instance);
                    Game.GenerateUniverse();
                }
                Signals.PublishPostStartGameEvent(Game.Name, Game.Year);
                if (this.IsServer())
                {
                    // TODO: send each player their turn data
                }

                Signals.SubmitTurnEvent += OnSubmitTurn;
            }
            else
            {
                // if we aren't the server, we come here with our player data already loaded
                // TODO: we need public game data
                OnPostStartGameEvent(Game.Name, PlayersManager.Me.Year);
            }

        }

        public override void _ExitTree()
        {
            Signals.PostStartGameEvent -= OnPostStartGameEvent;
            Signals.ChangeProductionQueuePressedEvent -= OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent -= OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent -= OnResearchDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent -= OnShipDesignerDialogRequestedEvent;
            Signals.TechBrowserDialogRequestedEvent -= OnTechBrowserDialogRequestedEvent;

            if (this.IsServerOrSinglePlayer())
            {
                Signals.SubmitTurnEvent -= OnSubmitTurn;
            }
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        async void OnSubmitTurn(Player player)
        {
            Game.SubmitTurn(player);
            if (Game.AllPlayersSubmitted())
            {
                // once everyone is submitted, generate a new turn
                Signals.PublishTurnGeneratingEvent();
                await Game.GenerateTurn();
                Signals.PublishTurnPassedEvent(Game.Year);
            }
        }

        /// <summary>
        /// When the game is ready to go, init the scanner
        /// </summary>
        /// <param name="year"></param>
        void OnPostStartGameEvent(String name, int year)
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

    }
}
