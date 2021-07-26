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
    public abstract class Server : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(Server));

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

            clientEventPublisher.GameStartRequestedEvent += OnGameStartRequested;
            clientEventPublisher.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            clientEventPublisher.GameStartRequestedEvent += OnGameStartRequested;
            clientEventPublisher.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            clientEventPublisher.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
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
                LoadGame(settings.Name, settings.Year);
            }
            else
            {
                CreateNewGame(settings);
            }
            // notify each player of a game start event
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
            PublishTurnSubmittedEvent(player);

            if (Game.AllPlayersSubmitted())
            {
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
            PublishTurnUnsubmittedEvent(player);
        }

        #endregion

        #region Turn Generation

        /// <summary>
        /// On next turn, generate a new turn
        /// </summary>
        /// <returns></returns>
        public async void GenerateNewTurnDeferred()
        {
            await Game.GenerateTurn();
            PublishTurnPassedEvent();
        }

        #endregion

        /// <summary>
        /// Create a new game and generate the universe
        /// </summary>
        public Game CreateNewGame(GameSettings<Player> settings)
        {
            Game = new Game()
            {
                Name = settings.Name,
                Multithreaded = Settings.Multithreaded,
                SaveToDisk = Settings.SaveToDisk,
                GameInfo = settings
            };
            if (GamesManager.Instance.GameExists(Game.Name))
            {
                GamesManager.Instance.DeleteGame(Game.Name);
            }
            // PlayersManager.Instance.NumPlayers = PlayersManager.Instance.Players.Count;
            Game.Init(settings.Players, RulesManager.Rules, TechStore.Instance, GamesManager.Instance, TurnProcessorManager.Instance);
            Game.GenerateUniverse();

            // TODO: remove this turn process stuff later
            Game.Players.ForEach(player => player.Settings.TurnProcessors.AddRange(TurnProcessorManager.Instance.TurnProcessors.Select(p => p.Name)));

            return Game;
        }

        /// <summary>
        /// Load a game from disk into the Game property
        /// </summary>
        public Game LoadGame(string gameName, int year)
        {
            Game = GamesManager.Instance.LoadGame(TechStore.Instance, TurnProcessorManager.Instance, gameName, year);
            Game.Multithreaded = Settings.Multithreaded;
            Game.SaveToDisk = Settings.SaveToDisk;

            return Game;
        }

    }
}