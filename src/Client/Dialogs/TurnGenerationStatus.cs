using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class TurnGenerationStatus : MarginContainer
    {
        Player Me { get => PlayersManager.Me; }
        public PublicGameInfo GameInfo { get; set; }

        Label yearLabel;
        Container playerStatusContainer;
        Button cancelButton;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            yearLabel = GetNode<Label>("VBoxContainer/YearLabel");
            playerStatusContainer = (Container)FindNode("PlayerStatusContainer");
            cancelButton = (Button)FindNode("CancelButton");

            cancelButton.Connect("pressed", this, nameof(OnCancel));

            Signals.TurnSubmittedEvent += OnTurnSubmitted;
            Signals.TurnGeneratingEvent += OnTurnGenerating;
            Signals.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
        }

        public override void _ExitTree()
        {
            Signals.TurnSubmittedEvent -= OnTurnSubmitted;
            Signals.TurnGeneratingEvent -= OnTurnGenerating;
            Signals.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
        }

        void OnTurnSubmitted(PublicPlayerInfo submittingPlayer)
        {
            UpdatePlayerStatuses();
        }

        void OnTurnGenerating()
        {
            UpdatePlayerStatuses();
        }

        void OnPlayTurnButtonPressed(int playerNum)
        {
            Signals.PublishPlayTurnRequestedEvent(playerNum);
        }

        void OnUnsubmitButtonPressed(int playerNum)
        {
            if (playerNum == Me.Num)
            {
                Signals.PublishUnsubmitTurnRequestedEvent(Me);
            }
        }

        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            yearLabel.Text = $"Year {GameInfo.Year}";
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
                    Text = $"{player.Num} - {player.RacePluralName} ({player.Name})",
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
