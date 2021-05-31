using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using CraigStars.Client;

namespace CraigStars
{
    public class GameView : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameView));

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
        ScoreDialog scoreDialog;

        /// <summary>
        /// When this node enters the tree, setup any server/player stuff
        /// </summary>
        public override void _EnterTree()
        {
            if (PlayersManager.Instance.Players.Count == 0)
            {
                log.Warn("Resetting Players. This probably means you are executing the Game scene directly during development. If not, this is a problem.");
                PlayersManager.Instance.SetupPlayers();
                if (Settings.Instance.ContinueGame != null && GamesManager.Instance.GameExists(Settings.Instance.ContinueGame))
                {
                    Settings.Instance.ShouldContinueGame = true;
                }
                // don't save on debug games
                // Settings.SaveToDisk = false;
                // PlayersManager.Me.Race.PRT = PRT.PP;
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
            scoreDialog = GetNode<ScoreDialog>("CanvasLayer/ScoreDialog");
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
            Signals.ScoreDialogRequestedEvent += OnScoreDialogRequested;
            Signals.TechBrowserDialogRequestedEvent += OnTechBrowserDialogRequested;
            Signals.RaceDesignerDialogRequestedEvent += OnRaceDesignerDialogRequested;
            Signals.MergeFleetsDialogRequestedEvent += OnMergeFleetsDialogRequested;
            Signals.BattleViewerDialogRequestedEvent += OnBattleViewerDialogRequested;

            // if we are the server (or a single player game)
            // init the server and send a notice to all players that it's time to start
            if (this.IsServerOrSinglePlayer())
            {
                if (Settings.Instance.ShouldContinueGame)
                {
                    Game = GamesManager.Instance.LoadGame(TechStore.Instance, TurnProcessorManager.Instance, Settings.Instance.ContinueGame, Settings.Instance.ContinueYear);
                    Game.Multithreaded = Settings.Multithreaded;
                    Game.SaveToDisk = Settings.SaveToDisk;
                    PlayersManager.Instance.InitPlayersFromGame(Game.Players);

                    if (GamesManager.Instance.HasPlayerSave(PlayersManager.Me))
                    {
                        GamesManager.Instance.LoadPlayerSave(PlayersManager.Me);
                    }
                }
                else
                {

                    Game = new Game()
                    {
                        Name = Settings.Instance.GameSettings.Name,
                        Multithreaded = Settings.Multithreaded,
                        SaveToDisk = Settings.SaveToDisk,
                        GameInfo = Settings.Instance.GameSettings
                    };
                    if (GamesManager.Instance.GameExists(Game.Name))
                    {
                        GamesManager.Instance.DeleteGame(Game.Name);
                    }
                    PlayersManager.Instance.NumPlayers = PlayersManager.Instance.Players.Count;
                    Game.Init(PlayersManager.Instance.Players.Cast<Player>().ToList(), RulesManager.Rules, TechStore.Instance, GamesManager.Instance, TurnProcessorManager.Instance);
                    Game.GenerateUniverse();

                    PlayersManager.Me.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name));
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
            Signals.ScoreDialogRequestedEvent -= OnScoreDialogRequested;
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

        void OnTurnGeneratorAdvanced(TurnGenerationState state)
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
                // once everyone is submitted, generate a new turn
                Signals.PublishTurnGeneratingEvent();
                CallDeferred(nameof(GenerateNewTurn));
            }
        }

        async void GenerateNewTurn()
        {
            await Game.GenerateTurn();

            if (this.IsServer())
            {
                // send players their data
                RPC.Instance.SendPlayerDataUpdated(Game);
                RPC.Instance.SendTurnPassed(Game.GameInfo);
            }

            log.Debug($"{Game.Year} Publishing client side turn passed event.");
            Signals.PublishTurnPassedEvent(Game.GameInfo);
            log.Debug($"{Game.Year} Done publishing client side turn passed event.");
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
            PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
            // add the universe to the viewport
            scanner.Init();
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
            OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
        }

        void OnTechBrowserDialogRequested()
        {
            techBrowserDialog.PopupCentered();
        }

        void OnRaceDesignerDialogRequested()
        {
            raceDesignerDialog.Race = PlayersManager.Me.Race;
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

        void OnScoreDialogRequested()
        {
            scoreDialog.PopupCentered();
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
