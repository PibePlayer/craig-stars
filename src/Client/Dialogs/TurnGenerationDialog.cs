using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class TurnGenerationDialog : GameViewDialog
    {
        PublicGameInfo GameInfo { get; set; }

        Label yearLabel;
        Container playerStatusContainer;
        Button cancelButton;
        Label label;
        ProgressBar progressBar;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            yearLabel = GetNode<Label>("MarginContainer/VBoxContainer/YearLabel");
            playerStatusContainer = (Container)FindNode("PlayerStatusContainer");
            cancelButton = (Button)FindNode("CancelButton");
            label = (Label)FindNode("Label");
            progressBar = (ProgressBar)FindNode("ProgressBar");

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            cancelButton.Connect("pressed", this, nameof(OnCancel));

            Signals.TurnSubmittedEvent += OnTurnSubmitted;
            Signals.PostStartGameEvent += OnGameStart;
            Signals.TurnGeneratingEvent += OnTurnGenerating;
            Signals.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        void OnTurnSubmitted(PublicPlayerInfo submittingPlayer)
        {
            UpdatePlayerStatuses();

            if (submittingPlayer == PlayersManager.Me)
            {
                // if we are on fast hot seat mode, switch to the next available player
                if (Settings.Instance.FastHotseat)
                {
                    var nextUnsubmittedPlayer = PlayersManager.Instance.Players.Find(player => !player.AIControlled && !player.SubmittedTurn);
                    if (nextUnsubmittedPlayer != null)
                    {
                        OnPlayTurnButtonPressed(nextUnsubmittedPlayer.Num);
                    }
                    else
                    {
                        PopupCentered();
                    }
                }
                else
                {
                    // this was us, show the dialog
                    PopupCentered();
                }
            }
        }

        void OnGameStart(PublicGameInfo gameInfo)
        {
            GameInfo = gameInfo;
        }

        public override void _ExitTree()
        {
            Signals.TurnSubmittedEvent -= OnTurnSubmitted;
            Signals.PostStartGameEvent -= OnGameStart;
            Signals.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            Signals.TurnGeneratingEvent -= OnTurnGenerating;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnTurnGenerating()
        {
            UpdatePlayerStatuses();
            PopupCentered();
        }

        void OnPlayTurnButtonPressed(int playerNum)
        {
            Signals.PublishPlayTurnRequestedEvent(playerNum);
            Hide();
        }

        void OnUnsubmitButtonPressed(int playerNum)
        {
            if (playerNum == Me.Num)
            {
                Signals.PublishUnsubmitTurnRequestedEvent(Me);
                Hide();
            }
        }

        void OnTurnGeneratorAdvanced(TurnGeneratorState state)
        {
            yearLabel.Text = $"Year {GameInfo.Year}";
            string labelText;
            switch (state)
            {
                case TurnGeneratorState.WaitingForPlayers:
                    labelText = "Waiting for Players";
                    break;
                default:
                    labelText = state.ToString();
                    break;
            }

            label.Text = labelText;
            progressBar.Value = 100 * ((int)state / (float)(Enum.GetValues(typeof(TurnGeneratorState)).Length));
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            Hide();
        }

        void OnAboutToShow()
        {
            label.Text = "Submitted turn";
            progressBar.Value = 0;

            GetTree().Paused = true;
        }

        void OnPopupHide()
        {
            GetTree().Paused = false;
        }

        void OnCancel()
        {
            // TODO: unsubmit turn
        }

        /// <summary>
        /// Update the player statuses in the dialog
        /// </summary>
        void UpdatePlayerStatuses()
        {
            foreach (Node node in playerStatusContainer.GetChildren())
            {
                playerStatusContainer.RemoveChild(node);
                node.QueueFree();
            }

            foreach (var player in GameInfo.Players)
            {
                playerStatusContainer.AddChild(new Label()
                {
                    Text = $"{player.Num} - {player.RacePluralName}",
                    Modulate = player.Color,
                    SizeFlagsHorizontal = (int)SizeFlags.ExpandFill
                });

                if (player.AIControlled)
                {
                    playerStatusContainer.AddChild(new Label() { Text = $"{(player.SubmittedTurn ? "Submitted" : "Waiting to Submit")}" });
                }
                else
                {
                    var button = new Button();
                    if (player == Me)
                    {
                        button.Text = "Unsubmit";
                        button.Connect("pressed", this, nameof(OnUnsubmitButtonPressed), new Godot.Collections.Array() { player.Num });
                    }
                    else
                    {
                        button.Text = "Play Turn";
                        button.Connect("pressed", this, nameof(OnPlayTurnButtonPressed), new Godot.Collections.Array() { player.Num });
                    }
                    playerStatusContainer.AddChild(button);
                }
            }
        }
    }

}
