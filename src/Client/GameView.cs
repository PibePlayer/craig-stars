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
        Game Game { get; set; }
        PublicGameInfo GameInfo { get; set; }

        string projectName;

        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;

        ProductionQueueDialog productionQueueDialog;
        CargoTransferDialog cargoTransferDialog;
        ResearchDialog researchDialog;
        BattlePlansDialog battlePlansDialog;
        TransportPlansDialog transportPlansDialog;
        ReportsDialog reportsDialog;
        TechBrowserDialog techBrowserDialog;
        RaceDesignerDialog raceDesignerDialog;
        ShipDesignerDialog shipDesignerDialog;
        MergeFleetsDialog mergeFleetsDialog;
        BattleViewerDialog battleViewerDialog;

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
            scanner = FindNode("Scanner") as Scanner;
            productionQueueDialog = GetNode<ProductionQueueDialog>("CanvasLayer/ProductionQueueDialog");
            cargoTransferDialog = GetNode<CargoTransferDialog>("CanvasLayer/CargoTransferDialog");
            researchDialog = GetNode<ResearchDialog>("CanvasLayer/ResearchDialog");
            battlePlansDialog = GetNode<BattlePlansDialog>("CanvasLayer/BattlePlansDialog");
            transportPlansDialog = GetNode<TransportPlansDialog>("CanvasLayer/TransportPlansDialog");
            reportsDialog = GetNode<ReportsDialog>("CanvasLayer/ReportsDialog");
            techBrowserDialog = GetNode<TechBrowserDialog>("CanvasLayer/TechBrowserDialog");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("CanvasLayer/RaceDesignerDialog");
            shipDesignerDialog = GetNode<ShipDesignerDialog>("CanvasLayer/ShipDesignerDialog");
            mergeFleetsDialog = GetNode<MergeFleetsDialog>("CanvasLayer/MergeFleetsDialog");
            battleViewerDialog = GetNode<BattleViewerDialog>("CanvasLayer/BattleViewerDialog");
            projectName = ProjectSettings.GetSetting("application/config/name").ToString();

            Signals.PostStartGameEvent += OnPostStartGame;
            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.ChangeProductionQueuePressedEvent += OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent += OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent += OnResearchDialogRequested;
            Signals.BattlePlansDialogRequestedEvent += OnBattlePlansDialogRequested;
            Signals.TransportPlansDialogRequestedEvent += OnTransportPlansDialogRequested;
            Signals.ReportsDialogRequestedEvent += OnReportsDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent += OnShipDesignerDialogRequested;
            Signals.TechBrowserDialogRequestedEvent += OnTechBrowserDialogRequested;
            Signals.RaceDesignerDialogRequestedEvent += OnRaceDesignerDialogRequested;
            Signals.MergeFleetsDialogRequestedEvent += OnMergeFleetsDialogRequested;
            Signals.BattleViewerDialogRequestedEvent += OnBattleViewerDialogRequested;

            // if we are the server (or a single player game)
            // init the server and send a notice to all players that it's time to start
            if (this.IsServerOrSinglePlayer())
            {
                Game = new Game() { TechStore = TechStore.Instance, Mode = Settings.Instance.GameMode, Name = Settings.Instance.GameName };
                if (Settings.Instance.ShouldContinueGame)
                {
                    GameSaver saver = new GameSaver(Game);
                    saver.LoadGame(Settings.Instance.ContinueGame, Settings.Instance.ContinueYear, TechStore.Instance);
                    PlayersManager.Instance.InitPlayersFromGame(Game.Players);
                }
                else
                {
                    Game.Init(PlayersManager.Instance.Players.Cast<Player>().ToList(), RulesManager.Rules, TechStore.Instance);
                    Game.GenerateUniverse();
                }

                GameInfo = Game.GameInfo;
                Signals.PublishPostStartGameEvent(GameInfo);
                if (this.IsServer())
                {
                    // send players their data
                    RPC.Instance.SendPlayerDataUpdated(Game);
                    // tell everyone to start the game
                    RPC.Instance.SendPostStartGame(GameInfo);
                }

                Game.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
                Signals.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
                Signals.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
                Signals.PlayTurnRequestedEvent += OnPlayTurnRequested;
            }
            else
            {
                // if we aren't the server, we come here with our player data already loaded
                // TODO: we need public game data
                OnPostStartGame(PlayersManager.Me.Game);
            }

        }

        public override void _ExitTree()
        {
            Signals.PostStartGameEvent -= OnPostStartGame;
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.ChangeProductionQueuePressedEvent -= OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent -= OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent -= OnResearchDialogRequested;
            Signals.BattlePlansDialogRequestedEvent -= OnBattlePlansDialogRequested;
            Signals.TransportPlansDialogRequestedEvent -= OnTransportPlansDialogRequested;
            Signals.ReportsDialogRequestedEvent -= OnReportsDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent -= OnShipDesignerDialogRequested;
            Signals.TechBrowserDialogRequestedEvent -= OnTechBrowserDialogRequested;
            Signals.RaceDesignerDialogRequestedEvent -= OnRaceDesignerDialogRequested;
            Signals.MergeFleetsDialogRequestedEvent -= OnMergeFleetsDialogRequested;
            Signals.BattleViewerDialogRequestedEvent -= OnBattleViewerDialogRequested;


            if (this.IsServerOrSinglePlayer())
            {
                Game.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
                Signals.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
                Signals.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
                Signals.PlayTurnRequestedEvent -= OnPlayTurnRequested;
            }
        }

        void OnTurnGeneratorAdvanced(TurnGeneratorState state)
        {
            Signals.PublishTurnGeneratorAdvancedEvent(state);
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        void OnSubmitTurnRequested(Player player)
        {
            Game.SubmitTurn(player);
            Signals.PublishTurnSubmittedEvent(player);
            if (Game.AllPlayersSubmitted())
            {
                CallDeferred(nameof(GenerateNewTurn));
            }
        }

        async void GenerateNewTurn()
        {
            // once everyone is submitted, generate a new turn
            Signals.PublishTurnGeneratingEvent();
            await Game.GenerateTurn();

            if (this.IsServer())
            {
                // send players their data
                RPC.Instance.SendPlayerDataUpdated(Game);
                RPC.Instance.SendTurnPassed(Game.GameInfo);
            }

            Signals.PublishTurnPassedEvent(Game.GameInfo);
        }

        void OnUnsubmitTurnRequested(Player player)
        {
            Game.UnsubmitTurn(player);
        }

        /// <summary>
        /// Join as the next player and reset the UI
        /// </summary>
        /// <param name="playerNum"></param>
        void OnPlayTurnRequested(int playerNum)
        {
            PlayersManager.Instance.ActivePlayer = playerNum;
            Signals.PublishPostStartGameEvent(GameInfo);
        }

        /// <summary>
        /// When the game is ready to go, init the scanner
        /// </summary>
        /// <param name="year"></param>
        void OnPostStartGame(PublicGameInfo gameInfo)
        {
            OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
            Game.RunTurnProcessors(PlayersManager.Me);
            // add the universe to the viewport
            scanner.InitMapObjects();
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            Game.RunTurnProcessors(PlayersManager.Me);
            OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
        }


        void OnTechBrowserDialogRequested()
        {
            techBrowserDialog.PopupCentered();
        }

        void OnRaceDesignerDialogRequested()
        {
            raceDesignerDialog.PopupCentered();
        }

        void OnReportsDialogRequested()
        {
            reportsDialog.PopupCentered();
        }

        void OnShipDesignerDialogRequested()
        {
            shipDesignerDialog.PopupCentered();
        }

        void OnResearchDialogRequested()
        {
            researchDialog.PopupCentered();
        }

        void OnBattlePlansDialogRequested()
        {
            battlePlansDialog.PopupCentered();
        }

        void OnTransportPlansDialogRequested()
        {
            transportPlansDialog.PopupCentered();
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

        void OnMergeFleetsDialogRequested(FleetSprite sourceFleet)
        {
            mergeFleetsDialog.SourceFleet = sourceFleet;
            mergeFleetsDialog.PopupCentered();
        }

        void OnBattleViewerDialogRequested(BattleRecord battle)
        {
            battleViewerDialog.BattleRecord = battle;
            battleViewerDialog.PopupCentered();
        }

    }
}
