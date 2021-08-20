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
    /// </summary>
    public abstract class Server : Node, IServer
    {
        static CSLog log = LogProvider.GetLogger(typeof(Server));

        public bool SaveToDisk { get; set; } = true;
        public bool Multithreaded { get; set; } = true;

        /// <summary>
        /// The GamesManager is used to save turns
        /// </summary>
        public IGamesManager GamesManager { get; set; }

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

        public override void _Ready()
        {
            base._Ready();

            clientEventPublisher = CreateClientEventPublisher();
            GamesManager = Singletons.GamesManager.Instance;
            aiTurnSubmitter = new AITurnSubmitter(TurnProcessorManager.Instance, Multithreaded);
            aiTurnSubmitter.TurnSubmitRequestedEvent += OnAITurnSubmitRequested;

            clientEventPublisher.GameStartRequestedEvent += OnGameStartRequested;
            clientEventPublisher.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                aiTurnSubmitter.TurnSubmitRequestedEvent -= OnAITurnSubmitRequested;
                clientEventPublisher.GameStartRequestedEvent -= OnGameStartRequested;
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

        protected abstract void PublishGameStartedEvent();
        protected abstract void PublishGameStartingEvent(PublicGameInfo gameInfo);
        protected abstract void PublishTurnSubmittedEvent(PublicPlayerInfo player);
        protected abstract void PublishTurnUnsubmittedEvent(PublicPlayerInfo player);
        protected abstract void PublishTurnGeneratingEvent();
        protected abstract void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state);
        protected abstract void PublishTurnPassedEvent();

        #endregion

        #region Client Event Handlers

        protected async void OnGameStartRequested(GameSettings<Player> settings)
        {

            await Task.Run(() =>
            {
                try
                {
                    if (settings.ContinueGame)
                    {
                        PublicGameInfo gameInfo = LoadGameInfo(settings.Name, settings.Year);
                        GodotTaskFactory.StartNew(() => PublishGameStartingEvent(gameInfo));
                        Game = LoadGame(settings.Name, settings.Year, multithreaded: true, saveToDisk: true);
                    }
                    else
                    {
                        GodotTaskFactory.StartNew(() => PublishGameStartingEvent(settings));
                        Game = CreateNewGame(settings, multithreaded: true, saveToDisk: true);
                    }

                    // notify each player of a game start event
                    Game.GameInfo.State = GameState.WaitingForPlayers;

                    // submit the AI player turns
                    aiTurnSubmitter.SubmitAITurns(Game);
                }
                catch (Exception e)
                {
                    log.Error("Failed to create new game or load game.", e);
                    throw e;
                }
            });

            await GodotTaskFactory.StartNew(() => PublishGameStartedEvent());
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
                    await GodotTaskFactory.StartNew(() => PublishTurnSubmittedEvent(player));
                    if (saveGame)
                    {
                        SaveGame(Game);
                    }

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
                }
            });

            await GodotTaskFactory.StartNew(() => PublishTurnUnsubmittedEvent(player));
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
                    SaveGame(Game);
                }
                catch (Exception e)
                {
                    log.Error("Failed to generate new turn.", e);
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

            aiTurnSubmitter.SubmitAITurns(Game);
            await GodotTaskFactory.StartNew(() => PublishTurnPassedEvent());
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
            var gameInfo = GamesManager.LoadGameInfo(gameName, year);
            return gameInfo;
        }

        /// <summary>
        /// Load a game from disk into the Game property
        /// </summary>
        public Game LoadGame(string gameName, int year, bool multithreaded, bool saveToDisk)
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
            lock (game.Name)
            {
                if (SaveToDisk && game.Year >= game.Rules.StartingYear + game.GameInfo.QuickStartTurns)
                {
                    if (Multithreaded)
                    {
                        // now that we have our json, we can save the game to dis in a separate task
                        Task.Run(() =>
                        {
                            log.Debug($"{Game.Year}: Saving game {Game.Name} to disk.");
                            // serialize the game to JSON. This must complete before we can
                            // modify any state
                            var gameJson = GamesManager.SerializeGame(game, Multithreaded);

                            GamesManager.SaveGame(gameJson, Multithreaded);
                            log.Debug($"{Game.Year}: Finished saving game {Game.Name} to disk.");
                        });
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

        }
    }
}