using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars.Client
{
    public class TurnGenerationStatus : MarginContainer
    {
        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        Label yearLabel;
        Container playerStatusContainer;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            yearLabel = GetNode<Label>("VBoxContainer/YearLabel");
            playerStatusContainer = (Container)FindNode("PlayerStatusContainer");

            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
        }

        public override void _ExitTree()
        {
            EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
            EventManager.TurnGeneratingEvent -= OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
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
            EventManager.PublishPlayTurnRequestedEvent(playerNum);
        }

        void OnUnsubmitButtonPressed(int playerNum)
        {
            if (playerNum == Me.Num)
            {
                EventManager.PublishUnsubmitTurnRequestedEvent(Me);
            }
        }

        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            yearLabel.Text = $"Year {GameInfo.Year}";
        }

        /// <summary>
        /// Update the player statuses in the dialog
        /// </summary>
        public void UpdatePlayerStatuses()
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
                    if (player == Me)
                    {
                        if (player.SubmittedTurn && GameInfo.State != GameState.GeneratingTurn)
                        {
                            var button = new Button();
                            button.Text = "Unsubmit";
                            button.Connect("pressed", this, nameof(OnUnsubmitButtonPressed), new Godot.Collections.Array() { player.Num });
                            playerStatusContainer.AddChild(button);
                        }
                        else
                        {
                            playerStatusContainer.AddChild(new Label() { Text = "Waiting to Submit" });
                        }
                    }
                    else
                    {
                        var button = new Button();
                        button.Text = "Play Turn";
                        button.Connect("pressed", this, nameof(OnPlayTurnButtonPressed), new Godot.Collections.Array() { player.Num });
                        playerStatusContainer.AddChild(button);
                    }
                }
            }
        }
    }

}
