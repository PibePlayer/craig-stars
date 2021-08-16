using Godot;
using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using System.Collections.Generic;

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
            SetProcess(false);

            EventManager.GameStartedEvent += OnGameStarted;
            EventManager.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            EventManager.PlayTurnRequestedEvent += OnPlayTurnRequested;
            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnUnsubmittedEvent += OnTurnUnsubmitted;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            EventManager.TurnPassedEvent += OnTurnPassed;

            // set the game info for the turn generation status to our GameInfo (whether we are a client or the host)
            // turnGenerationStatus.GameInfo = GameInfo;
            PlayersManager.GameInfo = GameInfo;
            turnGenerationStatus.UpdatePlayerStatuses();
            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year}");

            CallDeferred(nameof(ReloadGameView));
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            EventManager.GameStartedEvent -= OnGameStarted;
            EventManager.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            EventManager.PlayTurnRequestedEvent -= OnPlayTurnRequested;
            EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
            EventManager.TurnUnsubmittedEvent -= OnTurnUnsubmitted;
            EventManager.TurnGeneratingEvent -= OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            EventManager.TurnPassedEvent -= OnTurnPassed;
        }

        #region GameView Loading/Reloading

        /// <summary>
        /// Every time we create a new game, load a game, or generate a new turn we "reload" the game view as a new scene object
        /// This allows us to update our progress bar
        /// </summary>
        void ReloadGameView()
        {
            log.Debug("Reloading GameView");
            label.Text = "Refreshing Scanner";
            subLabel.Text = "";
            progressBar.Value = 0;
            if (gameViewScene == null)
            {
                loader = ResourceLoader.LoadInteractive("res://src/Client/GameView.tscn");
                SetProcess(true);
            }
            else
            {
                CallDeferred(nameof(SetNewGameView));
            }
        }

        public override void _Process(float delta)
        {
            if (loader == null)
            {
                SetProcess(false);
                return;
            }

            Error err = loader.Poll();

            if (err == Error.FileEof)
            {
                gameViewScene = loader.GetResource() as PackedScene;
                label.Text = "Ready to Play";
                progressBar.Value = 100;
                loader = null;
                // fire this off, don't wait
                CallDeferred(nameof(SetNewGameView));
                return;
            }
            else if (err == Error.Ok)
            {
                // update the progress bar
                var loadProgress = ((float)loader.GetStage()) / loader.GetStageCount();
                progressBar.Value = loadProgress * 100;
                return;
            }
            else
            {
                log.Error($"Failed to load GameView scene, Error: {err}");
                label.Text = "Failed to load GameView scene";
                loader = null;
            }
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
                RemoveChild(gameView);
                gameView.QueueFree();
                gameView = null;
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

            PlayersManager.GameInfo = GameInfo;
            gameView = gameViewScene.Instance<GameView>();
            container.Visible = false;
            turnGenerationStatus.Visible = false;

            AddChild(gameView);

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
            LocalPlayers.Add(player);
            if (PlayersManager.Me == null)
            {
                GameInfo = gameInfo;
                PlayersManager.Me = player;
                PlayersManager.GameInfo = gameInfo;
            }
        }

        void OnSubmitTurnRequested(Player player)
        {
            // we submitted our turn, switch to turn submitter view
            if (player == PlayersManager.Me)
            {
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
            }
        }

        void OnTurnSubmitted(PublicPlayerInfo player)
        {
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

        }

        void OnTurnUnsubmitted(PublicPlayerInfo player)
        {
            // put this player back into rotation so they can be played
            var localPlayer = LocalPlayers.Find(p => p == player);
            if (localPlayer != null)
            {
                localPlayer.SubmittedTurn = false;
            }
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
            EventManager.PublishTurnGeneratorAdvancedEvent(state);
        }

        void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            // replace this player in the list
            // this uses our magic HashCode override that uses playerNum as the Equals
            LocalPlayers.Remove(player);
            LocalPlayers.Add(player);

            // if we don't already have a local player, play this player
            if (PlayersManager.Me == null)
            {
                PlayersManager.GameInfo = gameInfo;
                PlayersManager.Me = player;
                PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
                OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
                ReloadGameView();
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
                CallDeferred(nameof(ReloadGameView));
            }
        }

        #endregion

    }
}