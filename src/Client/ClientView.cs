using Godot;
using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class ClientView : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameView));

        /// <summary>
        /// The GameInfo passed in when we're created
        /// </summary>
        /// <value></value>
        public PublicGameInfo GameInfo { get; set; }

        Player Me { get => PlayersManager.Me; }

        Game Game { get; set; }
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

            Signals.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            Signals.TurnSubmittedEvent += OnTurnSubmitted;
            Signals.PlayTurnRequestedEvent += OnPlayTurnRequested;
            Signals.TurnPassedEvent += OnTurnPassed;

            // set the game info for the turn generation status to our GameInfo (whether we are a client or the host)
            turnGenerationStatus.GameInfo = GameInfo;
            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year}");

            CallDeferred(nameof(ReloadGameView));
        }

        public override void _ExitTree()
        {
            base._ExitTree();

            Signals.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            Signals.TurnSubmittedEvent -= OnTurnSubmitted;
            Signals.PlayTurnRequestedEvent -= OnPlayTurnRequested;
            Signals.TurnPassedEvent -= OnTurnPassed;

            // TODO: figure out turn generator events with servers
            // if (this.IsSinglePlayer())
            // {
            //     Game.TurnGeneratorAdvancedEvent -= OnGameTurnGeneratorAdvanced;
            // }
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
                RemoveGameViewAndShow();
                // this was us, show the dialog
                turnGenerationStatus.Visible = true;
            }
        }

        void OnTurnSubmitted(PublicPlayerInfo player)
        {
            if (player == PlayersManager.Me)
            {
                // we just submitted our turn, remove the game view and show this container
                RemoveGameViewAndShow();
                // this was us, show the dialog
                turnGenerationStatus.Visible = true;

                // TODO: figure out fast hotseat with singleplayer server
                // if we are on fast hot seat mode, switch to the next available player
                // if (Settings.Instance.FastHotseat)
                // {
                //     var nextUnsubmittedPlayer = Game.Players.Find(player => !player.AIControlled && !player.SubmittedTurn);
                //     if (nextUnsubmittedPlayer != null)
                //     {
                //         Signals.PublishPlayTurnRequestedEvent(nextUnsubmittedPlayer.Num);
                //     }
                // }
            }

        }

        /// <summary>
        /// Every time we create a new game, load a game, or generate a new turn we "reload" the game view as a new scene object
        /// This allows us to update our progress bar
        /// </summary>
        void ReloadGameView()
        {
            label.Text = "Refreshing Scanner";
            subLabel.Text = "";
            progressBar.Value = 0;
            loader = ResourceLoader.LoadInteractive("res://src/Client/GameView.tscn");
            SetProcess(true);
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
                Resource resource = loader.GetResource();
                label.Text = "Ready to Play";
                progressBar.Value = 100;
                loader = null;
                // fire this off, don't wait
                CallDeferred(nameof(SetNewGameView), resource);
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
        /// When a new gameview is loaded, show it
        /// </summary>
        /// <param name="scene"></param>
        void SetNewGameView(PackedScene scene)
        {
            OS.SetWindowTitle($"{projectName} - {GameInfo.Name}: Year {GameInfo.Year} - {Me.Name}");

            gameView = scene.Instance<GameView>();
            gameView.GameInfo = GameInfo;
            container.Visible = false;
            turnGenerationStatus.Visible = false;

            AddChild(gameView);

            Signals.PublishGameViewResetEvent(GameInfo);
        }

        /// <summary>
        /// While a turn is being generated, this will update the progress bar
        /// </summary>
        /// <param name="state"></param>
        void OnGameTurnGeneratorAdvanced(TurnGenerationState state)
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
            Signals.PublishTurnGeneratorAdvancedEvent(state);
        }

        /// <summary>
        /// Remove the GameView and free it, then show the progress
        /// container (and the TurnStatus)
        /// </summary>
        void RemoveGameViewAndShow()
        {
            container.Visible = true;
            // remove the old game view and re-add it
            if (gameView != null && IsInstanceValid(gameView))
            {
                RemoveChild(gameView);
                gameView.QueueFree();
            }

        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
            OS.SetWindowTitle($"{projectName} - {gameInfo.Name}: Year {gameInfo.Year}");
            CallDeferred(nameof(ReloadGameView));
        }

        /// <summary>
        /// Join as the next player and reset the UI
        /// </summary>
        /// <param name="playerNum"></param>
        void OnPlayTurnRequested(int playerNum)
        {
            // TODO: figure out how to do hotseat
            PlayersManager.Instance.ActivePlayer = playerNum;

            RemoveGameViewAndShow();
            CallDeferred(nameof(ReloadGameView));
        }


    }
}