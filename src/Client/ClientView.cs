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

        Player Me { get => PlayersManager.Me; }

        /// <summary>
        /// For hotseat games, this is the list of all non ai players
        /// </summary>
        public List<Player> LocalPlayers { get; set; } = new List<Player>();

        PackedScene gameViewScene;
        string projectName;
        Control container;
        ProgressBar progressBar;
        Label label;
        Label subLabel;
        GameView gameView;
        TurnGenerationStatus turnGenerationStatus;
        ResourceInteractiveLoader loader;

        public override void _Ready()
        {
            base._Ready();
            container = GetNode<Container>("CanvasLayer/Container");
            progressBar = GetNode<ProgressBar>("CanvasLayer/Container/PanelContainer/VBoxContainer/ProgressBar");
            label = GetNode<Label>("CanvasLayer/Container/PanelContainer/VBoxContainer/Label");
            subLabel = GetNode<Label>("CanvasLayer/Container/PanelContainer/VBoxContainer/SubLabel");
            turnGenerationStatus = GetNode<TurnGenerationStatus>("CanvasLayer/Container/PanelContainer/VBoxContainer/TurnGenerationStatus");

            projectName = ProjectSettings.GetSetting("application/config/name").ToString();
            PlayerName ??= Settings.Instance.PlayerName;

            EventManager.GameStartedEvent += OnGameStarted;
            EventManager.GameContinuedEvent += OnGameContinued;
            EventManager.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            EventManager.PlayTurnRequestedEvent += OnPlayTurnRequested;
            EventManager.PlayerDataEvent += OnPlayerData;
            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnUnsubmittedEvent += OnTurnUnsubmitted;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
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
            turnGenerationStatus.UpdatePlayerStatuses();
            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year}");
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
                EventManager.PlayTurnRequestedEvent -= OnPlayTurnRequested;
                EventManager.PlayerDataEvent -= OnPlayerData;
                EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
                EventManager.TurnUnsubmittedEvent -= OnTurnUnsubmitted;
                EventManager.TurnGeneratingEvent -= OnTurnGenerating;
                EventManager.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
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
            label.Text = "Loading client resources";
            subLabel.Text = "";
            if (CSResourceLoader.TotalResources > 0)
            {
                progressBar.Value = CSResourceLoader.Loaded / CSResourceLoader.TotalResources;
            }
            gameViewScene = CSResourceLoader.GetPackedScene("GameView.tscn");
            CallDeferred(nameof(SetNewGameView));
        }

        /// <summary>
        /// Remove the GameView and free it, then show the progress
        /// container (and the TurnStatus)
        /// </summary>
        void RemoveGameViewAndShowTurnGeneration()
        {
            container.Visible = true;
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
            label.Text = "Refreshing Scanner";

            PlayersManager.GameInfo = GameInfo;
            // Create a new instande of GameView. Note, we have to create it
            // new, otherwise _Ready() won't be called if we re-use the same scene
            // TODO: we should probably re-use this scene
            if (gameView == null)
            {
                gameView = gameViewScene.Instance<GameView>();
                AddChild(gameView);
            }

            gameView.Visible = true;
            container.Visible = false;
            turnGenerationStatus.Visible = false;

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
            GamesManager.Instance.SavePlayer(player);
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
            if (player == PlayersManager.Me)
            {
                player.SubmittedTurn = true;
                if (this.IsMultiplayer())
                {
                    // submit our turn to the server
                    NetworkClient.Instance.SubmitTurnToServer(player);
                }

                // we just submitted our turn, remove the game view and show this container
                RemoveGameViewAndShowTurnGeneration();

                // this was us, show the dialog
                turnGenerationStatus.Visible = true;
                turnGenerationStatus.UpdatePlayerStatuses();

                // save our game
                GamesManager.Instance.SavePlayer(player);
            }
        }

        void OnTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;
            PlayersManager.GameInfo.Players.ForEach(p => log.Debug($"{PlayersManager.GameInfo}: Player: {p}, Submitted: {p.SubmittedTurn}"));

            var localPublicPlayer = GameInfo.Players.Find(p => p.Num == player.Num);
            localPublicPlayer.Update(player);

            if (PlayersManager.Me != null)
            {
                GamesManager.Instance.SavePlayerGameInfo(gameInfo, PlayersManager.Me.Num);
            }

            if (player == PlayersManager.Me)
            {
                // we just submitted our turn, remove the game view and show this container
                RemoveGameViewAndShowTurnGeneration();
                // this was us, show the dialog
                turnGenerationStatus.Visible = true;
                turnGenerationStatus.UpdatePlayerStatuses();

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
            else
            {
                turnGenerationStatus.UpdatePlayerStatuses();
            }

        }

        void OnTurnUnsubmitted(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PlayersManager.GameInfo = GameInfo = gameInfo;
            // put this player back into rotation so they can be played
            var localPlayer = LocalPlayers.Find(p => p == player);
            if (localPlayer != null)
            {
                localPlayer.SubmittedTurn = false;
                // save our game
                GamesManager.Instance.SavePlayer(localPlayer);
            }

            if (PlayersManager.Me != null)
            {
                GamesManager.Instance.SavePlayerGameInfo(gameInfo, PlayersManager.Me.Num);
            }

            var localPublicPlayer = GameInfo.Players.Find(p => p.Num == player.Num);
            localPublicPlayer.Update(player);
        }

        void OnTurnGenerating()
        {
            // we just submitted our turn, remove the game view and show this container
            RemoveGameViewAndShowTurnGeneration();

            // this was us, show the dialog
            turnGenerationStatus.Visible = true;
            turnGenerationStatus.UpdatePlayerStatuses();
        }


        /// <summary>
        /// While a turn is being generated, this will update the progress bar
        /// </summary>
        /// <param name="state"></param>
        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            progressBar.Value = 100 * (((int)state + 1) / (float)(Enum.GetValues(typeof(TurnGenerationState)).Length));

            switch (state)
            {
                case TurnGenerationState.WaitingForPlayers:
                    label.Text = "Waiting for Players";
                    subLabel.Text = "";
                    break;
                default:
                    label.Text = "Generating Turn";
                    subLabel.Text = state.ToString();
                    break;
            }

            // let any client listeners know the game advanced the turn state
            // EventManager.PublishTurnGeneratorAdvancedEvent(state);
        }

        void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            // replace this player in the list
            // this uses our magic HashCode override that uses playerNum as the Equals
            LocalPlayers.Remove(player);
            LocalPlayers.Add(player);

            GamesManager.Instance.SavePlayer(player);

            // if we don't already have a local player, play this player
            if (PlayersManager.Me == null)
            {
                PlayersManager.GameInfo = GameInfo = gameInfo;
                PlayersManager.Me = player;
                PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
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
            LocalPlayers.Remove(player);
            LocalPlayers.Add(player);

            // if we don't already have a local player, play this player
            if (PlayersManager.Me == null)
            {
                GameInfo = gameInfo;
                PlayersManager.GameInfo = GameInfo = gameInfo;
                PlayersManager.Me = player;
                PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
                OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
                LoadGameView();
            }
        }



        #endregion

    }
}