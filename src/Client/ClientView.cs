using Godot;
using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CraigStars.Client
{
    public class ClientView : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(ClientView));

        [Inject] TurnProcessorRunner turnProcessorRunner;

        Player Me { get => PlayersManager.Me; }

        /// <summary>
        /// The GameInfo passed in when we're created
        /// </summary>
        /// <value></value>
        public PublicGameInfo GameInfo { get; set; }

        public string PlayerName { get; set; }

        /// <summary>
        /// Set to true to automatically try and become the player that matches your name
        /// </summary>
        /// <value></value>
        public bool AutoJoin { get; set; }

        /// <summary>
        /// For hotseat games, this is the list of all non ai players
        /// </summary>
        public List<Player> LocalPlayers { get; set; } = new List<Player>();

        InGameMenu inGameMenu;
        PackedScene gameViewScene;
        string projectName;
        GameView gameView;
        ResourceInteractiveLoader loader;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();

            inGameMenu = GetNode<InGameMenu>("CanvasLayer/InGameMenu");

            projectName = ProjectSettings.GetSetting("application/config/name").ToString();
            PlayerName ??= Settings.Instance.PlayerName;

            EventManager.GameStartedEvent += OnGameStarted;
            EventManager.GameContinuedEvent += OnGameContinued;
            EventManager.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            EventManager.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
            EventManager.GenerateTurnRequestedEvent += OnGenerateTurnRequested;
            EventManager.PlayTurnRequestedEvent += OnPlayTurnRequested;
            EventManager.PlayerDataEvent += OnPlayerData;
            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnUnsubmittedEvent += OnTurnUnsubmitted;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnPassedEvent += OnTurnPassed;

            // set the game info for the turn generation status to our GameInfo (whether we are a client or the host)
            // turnGenerationStatus.GameInfo = GameInfo;
            PlayersManager.GameInfo = GameInfo;
            if (LocalPlayers.Count > 0)
            {
                // if we are starting with a player preloaded, go right into the game
                PlayersManager.Me = LocalPlayers[0];

                if (GameInfo.Mode == GameMode.NetworkedMultiPlayer)
                {
                    NetworkClient.Instance.JoinExistingGame(Settings.Instance.ClientHost, Settings.Instance.ClientPort, PlayersManager.Me);
                }

                // don't show the game if we already submitted
                if (!PlayersManager.Me.SubmittedTurn)
                {
                    CallDeferred(nameof(LoadGameView));
                }

            }
            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year}");
            inGameMenu.PopupCentered();
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            PlayersManager.Me = null;
            PlayersManager.GameInfo = null;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameStartedEvent -= OnGameStarted;
                EventManager.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
                EventManager.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
                EventManager.PlayTurnRequestedEvent -= OnPlayTurnRequested;
                EventManager.GenerateTurnRequestedEvent -= OnGenerateTurnRequested;
                EventManager.PlayerDataEvent -= OnPlayerData;
                EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
                EventManager.TurnUnsubmittedEvent -= OnTurnUnsubmitted;
                EventManager.TurnGeneratingEvent -= OnTurnGenerating;
                EventManager.TurnPassedEvent -= OnTurnPassed;
            }
        }

        #region GameView Loading/Reloading

        /// <summary>
        /// Every time we create a new game, load a game, or generate a new turn we "reload" the game view as a new scene object
        /// This allows us to update our progress bar
        /// </summary>
        void LoadGameView()
        {
            log.Debug("Reloading GameView");
            inGameMenu.PopupCentered();
            gameViewScene = CSResourceLoader.GetPackedScene("GameView.tscn");
            CallDeferred(nameof(SetNewGameView));
        }

        /// <summary>
        /// Remove the GameView and free it, then show the progress
        /// container (and the TurnStatus)
        /// </summary>
        void RemoveGameViewAndShowTurnGeneration()
        {
            inGameMenu.PopupCentered();
            // remove the old game view and re-add it
            if (gameView != null && IsInstanceValid(gameView))
            {
                log.Debug("Removing GameView from tree");
                gameView.Visible = false;
            }
        }

        /// <summary>
        /// When a new gameview is loaded, show it
        /// </summary>
        /// <param name="scene"></param>
        void SetNewGameView()
        {
            log.Debug("Setting new GameView and hiding turn generation view");

            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year} - {Me?.Name}");
            // inGameMenu.ProgressStatus.ProgressLabel = "Refreshing Scanner";

            PlayersManager.GameInfo = GameInfo;

            // Create a new instance of GameView if we haven't already.
            if (gameView == null)
            {
                gameView = gameViewScene.Instance<GameView>();
                AddChild(gameView);
            }

            gameView.Visible = true;
            inGameMenu.Visible = false;

            EventManager.PublishGameViewResetEvent(GameInfo);
        }

        #endregion

        #region Turn/Game Generation Events

        /// <summary>
        /// This will be triggered if we are a client connecting to a server, or if
        /// we are playing a hotseat game and the server tells us about additional players
        /// that we can play as
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="player"></param>
        void OnGameStarted(PublicGameInfo gameInfo, Player player)
        {
            log.Info("OnGameStarted");
            LocalPlayers.Add(player);

            PlayersManager.GameInfo = GameInfo = gameInfo;
            PlayersManager.GameInfo.Players.ForEach(p => log.Debug($"{PlayersManager.GameInfo}: Player: {p}, Submitted: {p.SubmittedTurn}"));
            var _ = GamesManager.Instance.SavePlayer(gameInfo, player);
            if (PlayersManager.Me == null)
            {
                PlayersManager.Me = player;
                CallDeferred(nameof(LoadGameView));
            }
            else
            {
                CallDeferred(nameof(LoadGameView));
            }
        }

        void OnGameContinued(PublicGameInfo gameInfo)
        {
            // the server is ready
            PlayersManager.GameInfo = GameInfo = gameInfo;
        }


        void OnSubmitTurnRequested(Player player)
        {
            // we submitted our turn, switch to turn submitter view
            if (player.Num == PlayersManager.Me.Num)
            {
                var gameInfo = PlayersManager.GameInfo;
                player.SubmittedTurn = true;
                if (this.IsMultiplayer())
                {
                    // submit our turn to the server
                    NetworkClient.Instance.SubmitTurnToServer(PlayersManager.GameInfo, player);
                }

                // we just submitted our turn, remove the game view and show this container
                RemoveGameViewAndShowTurnGeneration();

                // this was us, show the dialog
                inGameMenu.Visible = true;

                // save our game
                var _ = GamesManager.Instance.SavePlayer(gameInfo, player);
            }
        }

        void OnUnsubmitTurnRequested(PublicPlayerInfo player)
        {
            var gameInfo = PlayersManager.GameInfo;
            player.SubmittedTurn = true;
            if (this.IsMultiplayer())
            {
                // make sure we are unsubmitting ourself
                var localPlayer = LocalPlayers.Find(p => p.Num == player.Num);
                if (localPlayer != null)
                {
                    // unsubmit our turn to the server
                    NetworkClient.Instance.UnsubmitTurnToServer(PlayersManager.GameInfo, localPlayer);
                }
                else
                {
                    log.Error($"Cannot unsubmit {player}, they are not a LocalPlayer.");
                }
            }
        }

        async void OnTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;
            PlayersManager.GameInfo.Players.ForEach(p => log.Debug($"{PlayersManager.GameInfo}: Player: {p}, Submitted: {p.SubmittedTurn}"));

            if (PlayersManager.Me != null)
            {
                await GamesManager.Instance.SavePlayerGameInfo(gameInfo, PlayersManager.Me.Num);
            }

            if (PlayersManager.Me != null && PlayersManager.Me.Num == player.Num)
            {
                // we just submitted our turn, remove the game view and show this container
                RemoveGameViewAndShowTurnGeneration();
                // this was us, show the dialog
                inGameMenu.Visible = true;

                // Mark the current player as submitted and clear it out
                // so a new play can be assumed
                PlayersManager.Me.SubmittedTurn = true;
                PlayersManager.Me = null;

                // if we are on fast hot seat mode, switch to the next available player
                if (Settings.Instance.FastHotseat && LocalPlayers.Count > 0)
                {
                    var nextUnsubmittedPlayer = LocalPlayers.FirstOrDefault(p => !p.SubmittedTurn);
                    if (nextUnsubmittedPlayer != null)
                    {
                        EventManager.PublishPlayTurnRequestedEvent(nextUnsubmittedPlayer.Num);
                    }
                }
            }
        }

        async void OnTurnUnsubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;

            // update the local player to unsubmitted
            var localPlayer = LocalPlayers.Find(p => p.Num == player.Num);
            if (localPlayer != null)
            {
                localPlayer.SubmittedTurn = false;
                // save our game
                await GamesManager.Instance.SavePlayer(gameInfo, localPlayer);
            }

            if (PlayersManager.Me != null)
            {
                await GamesManager.Instance.SavePlayerGameInfo(gameInfo, PlayersManager.Me.Num);
            }
        }

        void OnGenerateTurnRequested(PublicGameInfo gameInfo)
        {
            if (this.IsMultiplayer())
            {
                // make sure we are unsubmitting ourself
                var localPlayer = LocalPlayers.FirstOrDefault();
                if (localPlayer != null && localPlayer.Host)
                {
                    // unsubmit our turn to the server
                    NetworkClient.Instance.GenerateTurn(PlayersManager.GameInfo, localPlayer);
                }
                else
                {
                    log.Error($"Cannot force generate turn, we are not the host.");
                }
            }
        }

        void OnTurnGenerating()
        {
            PlayersManager.Me = null;
            
            // we just submitted our turn, remove the game view and show this container
            RemoveGameViewAndShowTurnGeneration();

            // this was us, show the dialog
            inGameMenu.Visible = true;
        }

        async void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;
            // replace this player in the list
            // this uses our magic HashCode override that uses playerNum as the Equals
            LocalPlayers.RemoveAll(p => p.Num == player.Num);
            LocalPlayers.Add(player);

            await GamesManager.Instance.SavePlayer(gameInfo, player);

            // if we don't already have a local player, play this player
            if (PlayersManager.Me == null)
            {
                PlayersManager.Me = player;
                turnProcessorRunner.RunTurnProcessors(PlayersManager.GameInfo, PlayersManager.Me, TurnProcessorManager.Instance);
                OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
                LoadGameView();
            }
        }

        /// <summary>
        /// Join as the next player and reset the UI
        /// </summary>
        /// <param name="playerNum"></param>
        void OnPlayTurnRequested(int playerNum)
        {
            var player = LocalPlayers.Find(player => player.Num == playerNum);
            if (player != null)
            {
                PlayersManager.Me = player;
                PlayersManager.Me.SubmittedTurn = false;
                RemoveGameViewAndShowTurnGeneration();
                CallDeferred(nameof(LoadGameView));
            }
        }

        /// <summary>
        /// The server sent us player data
        /// </summary>
        /// <param name="player"></param>
        void OnPlayerData(PublicGameInfo gameInfo, Player player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;
            LocalPlayers.RemoveAll(p => p.Num == player.Num);
            LocalPlayers.Add(player);

            // if we don't already have a local player, play this player
            if (PlayersManager.Me == null)
            {
                PlayersManager.Me = player;
                turnProcessorRunner.RunTurnProcessors(PlayersManager.GameInfo, PlayersManager.Me, TurnProcessorManager.Instance);
                OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
                LoadGameView();
            }
        }



        #endregion

    }
}