using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CraigStars.Server
{
    /// <summary>
    /// The Server class manages creating new games, loading save games, accepting turn submittals, and generating new turns.
    /// This class is implemented by a SinglePlayerServer and a NetworkServer. 
    /// 
    /// TODO: 
    /// * Server shoud send client player data
    /// * Clients save player file locally (with token from server)
    /// * All requests to server include token (to authenticate)
    /// * Server can re-send/regenerate token to be given to player if they lose it
    /// ** player can redownload turn, if necessary
    /// Continuing a game will load player save, and launch the server in the background to connect
    /// </summary>
    public abstract class Server : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(Server));

        public bool SaveToDisk { get; set; } = true;
        public bool Multithreaded { get; set; } = true;

        /// <summary>
        /// The GamesManager is used to save turns
        /// </summary>
        public GamesManager GamesManager { get; set; }

        /// <summary>
        /// This TaskFactory is configured to schedule executions on the main thread so it's safe for sending events to
        /// the client
        /// </summary>
        /// <value></value>
        public TaskFactory GodotTaskFactory { get; set; }

        /// <summary>
        /// The game that is being created/loaded
        /// </summary>
        /// <value></value>
        protected Game Game { get; set; }

        protected AITurnSubmitter aiTurnSubmitter;

        protected IClientEventPublisher clientEventPublisher;

        protected System.Threading.Mutex saveGameMutex = new System.Threading.Mutex();

        public override void _Ready()
        {
            base._Ready();

            clientEventPublisher = CreateClientEventPublisher();
            aiTurnSubmitter.TurnSubmitRequestedEvent += OnAITurnSubmitRequested;

            clientEventPublisher.PlayerDataRequestedEvent += OnPlayerDataRequested;
            clientEventPublisher.StartNewGameRequestedEvent += OnStartNewGameRequested;
            clientEventPublisher.ContinueGameRequestedEvent += OnContinueGameRequested;
            clientEventPublisher.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;

        }

        public void Init(TaskFactory godotTaskFactory, GamesManager gamesManager, TurnProcessorManager turnProcessorManager)
        {
            GamesManager = gamesManager;
            GodotTaskFactory = godotTaskFactory;
            aiTurnSubmitter = new AITurnSubmitter(turnProcessorManager, Multithreaded);
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                aiTurnSubmitter.TurnSubmitRequestedEvent -= OnAITurnSubmitRequested;
                clientEventPublisher.PlayerDataRequestedEvent -= OnPlayerDataRequested;
                clientEventPublisher.StartNewGameRequestedEvent -= OnStartNewGameRequested;
                clientEventPublisher.ContinueGameRequestedEvent -= OnContinueGameRequested;
                clientEventPublisher.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
                clientEventPublisher.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
            }
        }

        /// <summary>
        /// Create a new IServerEventPublisher that will pass events
        /// from the client to the server
        /// </summary>
        /// <returns></returns>
        protected abstract IClientEventPublisher CreateClientEventPublisher();

        #region Publish Functions

        // These abstract functions must be overridden by the single player or network server
        // To notify clients (through RPC or signals) about game state changes

        protected abstract void PublishPlayerDataEvent(Player player);
        protected abstract void PublishGameStartedEvent();
        protected abstract void PublishGameContinuedEvent();
        protected abstract void PublishGameStartingEvent(PublicGameInfo gameInfo);
        protected abstract void PublishTurnSubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player);
        protected abstract void PublishTurnUnsubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player);
        protected abstract void PublishTurnGeneratingEvent();
        protected abstract void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state);
        protected abstract void PublishTurnPassedEvent();

        #endregion

        #region Client Event Handlers

        protected void OnPlayerDataRequested(PublicPlayerInfo player)
        {
            if (Game != null && player.Num >= 0 && player.Num < Game.Players.Count)
            {
                var fullPlayer = Game.Players[player.Num];
                PublishPlayerDataEvent(fullPlayer);
            }
            else
            {
                log.Error($"PlayerDataRequested for invalid player/game state: Player: {player}, Game: {Game}");
            }
        }

        protected async void OnStartNewGameRequested(GameSettings<Player> settings)
        {

            await Task.Run(() =>
            {
                try
                {
                    GodotTaskFactory.StartNew(() => PublishGameStartingEvent(settings));
                    Game = CreateNewGame(settings, multithreaded: true, saveToDisk: true);

                    // notify each player of a game start event
                    Game.GameInfo.State = GameState.WaitingForPlayers;
                }
                catch (Exception e)
                {
                    log.Error("Failed to create new game or load game.", e);
                    throw e;
                }
            });

            await GodotTaskFactory.StartNew(() => PublishGameStartedEvent());

            // submit the AI player turns
            aiTurnSubmitter.SubmitAITurns(Game);
        }

        public async Task ContinueGame(string gameName, int year)
        {
            await Task.Run(() =>
            {
                try
                {
                    PublicGameInfo gameInfo = GamesManager.LoadServerGameInfo(gameName, year);
                    GodotTaskFactory.StartNew(() => PublishGameStartingEvent(gameInfo));
                    Game = LoadGame(gameInfo.Name, gameInfo.Year, multithreaded: true, saveToDisk: true);

                    // notify each player of a game start event
                    Game.GameInfo.State = GameState.WaitingForPlayers;
                }
                catch (Exception e)
                {
                    log.Error("Failed to create new game or load game.", e);
                    throw e;
                }
            });

            await GodotTaskFactory.StartNew(() => PublishGameContinuedEvent());

            // submit the AI player turns
            aiTurnSubmitter.SubmitAITurns(Game);
        }

        protected async void OnContinueGameRequested(string gameName, int year)
        {
            await ContinueGame(gameName, year);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnAITurnSubmitRequested(Player player)
        {
            // AI players don't need to save the game
            SubmitTurn(player, saveGame: false);
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnSubmitTurnRequested(Player player)
        {
            SubmitTurn(player, saveGame: true);
        }

        private void SubmitTurn(Player player, bool saveGame)
        {
            Task.Run(async () =>
            {
                try
                {
                    log.Debug($"{Game.Year}: Submitting turn for {player}");
                    Game.SubmitTurn(player);
                    await GodotTaskFactory.StartNew(() => PublishTurnSubmittedEvent(Game.GameInfo, player));

                    SaveGame(Game);

                    if (Game.AllPlayersSubmitted())
                    {
                        Game.GameInfo.State = GameState.GeneratingTurn;
                        await GodotTaskFactory.StartNew(() => PublishTurnGeneratingEvent());

                        // once everyone is submitted, generate a new turn
                        GenerateNewTurn();
                    }
                }
                catch (Exception e)
                {
                    log.Error("Failed to process OnSubmitTurnRequested event.", e);
                    throw e;
                }
            });
        }

        /// <summary>
        /// The user unsubmitted their turn
        /// </summary>
        /// <param name="player"></param>
        protected async virtual void OnUnsubmitTurnRequested(PublicPlayerInfo player)
        {
            await Task.Run(() =>
            {
                try
                {
                    Game.UnsubmitTurn(player);
                    SaveGame(Game);
                }
                catch (Exception e)
                {
                    log.Error($"Failed to unsubmit turn for {player}", e);
                    throw e;
                }
            });

            await GodotTaskFactory.StartNew(() => PublishTurnUnsubmittedEvent(Game.GameInfo, player));
        }

        #endregion

        #region Turn Generation

        /// <summary>
        /// Generate a new turn
        /// </summary>
        /// <returns></returns>
        public async void GenerateNewTurn()
        {
            Game.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;

            Action generateTurn = () =>
            {
                try
                {
                    Game.GenerateTurn();
                    Game.GameInfo.State = GameState.WaitingForPlayers;
                }
                catch (Exception e)
                {
                    log.Error("Failed to generate new turn.", e);
                    throw e;
                }
            };
            if (Multithreaded)
            {
                await Task.Run(generateTurn);
            }
            else
            {
                generateTurn();
            }

            Game.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;

            await GodotTaskFactory.StartNew(() => PublishTurnPassedEvent());

            SaveGame(Game);
            aiTurnSubmitter.SubmitAITurns(Game);
        }

        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            GodotTaskFactory.StartNew(() => PublishTurnGeneratorAdvancedEvent(state));
        }

        #endregion

        /// <summary>
        /// Create a new game and generate the universe
        /// </summary>
        public Game CreateNewGame(GameSettings<Player> settings, bool multithreaded, bool saveToDisk)
        {
            var game = new Game()
            {
                Name = settings.Name,
                GameInfo = settings
            };
            if (GamesManager.Exists(game.Name))
            {
                GamesManager.DeleteGame(game.Name);
            }
            // PlayersManager.Instance.NumPlayers = PlayersManager.Instance.Players.Count;
            game.Init(settings.Players, RulesManager.Rules, TechStore.Instance);
            game.GenerateUniverse();

            // TODO: remove this turn process stuff later
            // game.Players.ForEach(player => player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name)));

            Multithreaded = multithreaded;
            SaveToDisk = saveToDisk;

            SaveGame(game);

            return game;
        }

        /// <summary>
        /// Load just the PublicGameInfo for a game from disk
        /// </summary>
        public PublicGameInfo LoadGameInfo(string gameName, int year)
        {
            var gameInfo = GamesManager.LoadServerGameInfo(gameName, year);
            return gameInfo;
        }

        /// <summary>
        /// Load a game from disk into the Game property
        /// </summary>
        public virtual Game LoadGame(string gameName, int year, bool multithreaded, bool saveToDisk)
        {
            log.Debug($"Loading {gameName}:{year} from disk");
            var game = GamesManager.LoadGame(TechStore.Instance, gameName, year);
            log.Debug($"Loaded {gameName}:{year} from disk");

            // TODO: remove this turn process stuff later
            // game.Players.ForEach(player => player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name)));

            Multithreaded = multithreaded;
            SaveToDisk = saveToDisk;

            return game;
        }

        protected void SaveGame(Game game)
        {
            try
            {
                saveGameMutex.WaitOne();

                if (SaveToDisk && game.Year >= game.Rules.StartingYear + game.GameInfo.QuickStartTurns)
                {
                    if (Multithreaded)
                    {
                        // now that we have our json, we can save the game to dis in a separate task
                        log.Debug($"{game.Year}: Saving game {game.Name} to disk.");
                        // serialize the game to JSON. This must complete before we can
                        // modify any state
                        var gameJson = GamesManager.SerializeGame(game, Multithreaded);

                        GamesManager.SaveGame(gameJson, Multithreaded);
                        log.Debug($"{game.Year}: Finished saving game {game.Name} to disk.");
                    }
                    else
                    {
                        // serialize the game to JSON. This must complete before we can
                        // modify any state
                        var gameJson = GamesManager.SerializeGame(game, Multithreaded);

                        GamesManager.SaveGame(gameJson, Multithreaded);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error($"{game.Year}: Failed to save game {game.Name}.", e);
                throw e;
            }
            finally
            {
                saveGameMutex.ReleaseMutex();
            }
        }
    }
}