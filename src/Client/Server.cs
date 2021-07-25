using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Threading.Tasks;

namespace CraigStars
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

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnSubmitTurnRequested(Player player)
        {
            Game.SubmitTurn(player);

            if (Game.AllPlayersSubmitted())
            {
                // once everyone is submitted, generate a new turn
                CallDeferred(nameof(GenerateNewTurnDeferred));
            }
        }

        /// <summary>
        /// On next turn, generate a new turn
        /// </summary>
        /// <returns></returns>
        public async void GenerateNewTurnDeferred()
        {
            await GenerateNewTurn();
        }

        /// <summary>
        /// The user unsubmitted their turn
        /// </summary>
        /// <param name="player"></param>
        protected virtual void OnUnsubmitTurnRequested(Player player)
        {
            Game.UnsubmitTurn(player);
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        /// <returns></returns>
        protected virtual async Task GenerateNewTurn()
        {
            await Game.GenerateTurn();
        }

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

            // notify any clients that we have a new game
            Signals.PublishGameStartedEvent(Game.GameInfo);

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

            // notify any clients that we have a new game
            Signals.PublishGameStartedEvent(Game.GameInfo);

            return Game;
        }

    }
}