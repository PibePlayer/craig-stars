using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Threading.Tasks;

namespace CraigStars.Server
{
    /// <summary>
    /// The Server class manages creating new games, loading save games, accepting turn submittals, and generating new turns.
    /// This class is implemented by a SinglePlayerServer and a NetworkServer. 
    /// </summary>
    public abstract class Server : Node, IServer
    {
        static CSLog log = LogProvider.GetLogger(typeof(Server));

        // for tests or fast generation
        public bool SaveToDisk { get; set; } = true;
        public bool Multithreaded { get; set; } = true;
        Task aiSubmittingTask;

        /// <summary>
        /// The GamesManager is used to save turns
        /// </summary>
        public IGamesManager GamesManager { get; set; }

        /// <summary>
        /// The game that is being created/loaded
        /// </summary>
        /// <value></value>
        protected Game Game { get; set; }

        protected IClientEventPublisher clientEventPublisher;

        public override void _Ready()
        {
            base._Ready();

            clientEventPublisher = CreateClientEventPublisher();
            GamesManager = Singletons.GamesManager.Instance;

            clientEventPublisher.GameStartRequestedEvent += OnGameStartRequested;
            clientEventPublisher.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            clientEventPublisher.GameStartRequestedEvent -= OnGameStartRequested;
            clientEventPublisher.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
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

        protected abstract void PublishPlayerUpdatedEvent(PublicPlayerInfo player);
        protected abstract void PublishGameStartedEvent();
        protected abstract void PublishTurnSubmittedEvent(PublicPlayerInfo player);
        protected abstract void PublishTurnUnsubmittedEvent(PublicPlayerInfo player);
        protected abstract void PublishTurnGeneratingEvent();
        protected abstract void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state);
        protected abstract void PublishTurnPassedEvent();

        #endregion

        #region Client Event Handlers

        protected void OnGameStartRequested(GameSettings<Player> settings)
        {
            if (settings.ContinueGame)
            {
                Game = LoadGame(settings.Name, settings.Year, multithreaded: true, saveToDisk: true);
            }
            else
            {
                Game = CreateNewGame(settings, multithreaded: true, saveToDisk: true);
            }
            // submit the AI player turns
            var _ = SubmitAITurns(Game);

            // notify each player of a game start event
            Game.GameInfo.State = GameState.WaitingForPlayers;
            PublishGameStartedEvent();
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnSubmitTurnRequested(Player player)
        {
            Game.SubmitTurn(player);
            SaveGame(Game);
            PublishTurnSubmittedEvent(player);

            if (Game.AllPlayersSubmitted())
            {
                Game.GameInfo.State = GameState.GeneratingTurn;
                PublishTurnGeneratingEvent();
                // once everyone is submitted, generate a new turn
                CallDeferred(nameof(GenerateNewTurnDeferred));
            }
        }

        /// <summary>
        /// The user unsubmitted their turn
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnUnsubmitTurnRequested(PublicPlayerInfo player)
        {
            Game.UnsubmitTurn(player);
            SaveGame(Game);
            PublishTurnUnsubmittedEvent(player);
        }

        #endregion

        #region Turn Generation

        /// <summary>
        /// On next turn, generate a new turn
        /// </summary>
        /// <returns></returns>
        public void GenerateNewTurnDeferred()
        {
            Action generateTurn = () =>
            {
                Game.GenerateTurn();
                Game.GameInfo.State = GameState.WaitingForPlayers;
                SaveGame(Game);
                var _ = SubmitAITurns(Game);
            };
            // await Task.Factory.StartNew(generateTurn);
            generateTurn();
            PublishTurnPassedEvent();
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
            game.Players.ForEach(player => player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name)));

            Multithreaded = multithreaded;
            SaveToDisk = saveToDisk;

            SaveGame(game);

            return game;
        }

        /// <summary>
        /// Load a game from disk into the Game property
        /// </summary>
        public Game LoadGame(string gameName, int year, bool multithreaded, bool saveToDisk)
        {
            var game = GamesManager.LoadGame(TechStore.Instance, TurnProcessorManager.Instance, gameName, year);

            // TODO: remove this turn process stuff later
            game.Players.ForEach(player => player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name)));

            Multithreaded = multithreaded;
            SaveToDisk = saveToDisk;

            // Make sure on load, AI players submit their turns
            var _ = SubmitAITurns(game);

            return game;
        }

        protected void SaveGame(Game game)
        {
            if (SaveToDisk && game.Year >= game.Rules.StartingYear + game.GameInfo.QuickStartTurns)
            {
                // serialize the game to JSON. This must complete before we can
                // modify any state
                var gameJson = GamesManager.SerializeGame(game, Multithreaded);

                if (Multithreaded)
                {
                    // now that we have our json, we can save the game to dis in a separate task
                    Task.Run(() =>
                    {
                        GamesManager.SaveGame(gameJson, Multithreaded);
                    }).Wait();
                }
                else
                {
                    GamesManager.SaveGame(gameJson, Multithreaded);
                }
            }
        }

        /// <summary>
        /// Submit any AI turns
        /// This submits all turns in separate threads and returns a Task for them all to complete
        /// </summary>
        async Task SubmitAITurns(Game game)
        {
            var tasks = new List<Task>();
            // submit AI turns
            foreach (var player in game.Players)
            {
                if (player.AIControlled && !player.SubmittedTurn)
                {
                    Action submitAITurn = () =>
                    {
                        try
                        {
                            foreach (var processor in TurnProcessorManager.Instance.TurnProcessors)
                            {
                                processor.Process(player);
                            }
                            // Treat this AI player like a player that just submitted their turn
                            OnSubmitTurnRequested(player);
                        }
                        catch (Exception e)
                        {
                            log.Error($"Failed to submit AI turn {player}", e);
                        }
                    };
                    // if (Multithreaded)
                    // {
                    //     tasks.Add(Task.Run(submitAITurn));
                    // }
                    // else
                    // {
                    submitAITurn();
                    // }
                }
            }
            // if (Multithreaded)
            // {
            //     aiSubmittingTask = Task.Run(() => Task.WaitAll(tasks.ToArray()));
            // }
            // else
            // {
            aiSubmittingTask = Task.CompletedTask;
            // }
            await aiSubmittingTask;
        }
    }
}