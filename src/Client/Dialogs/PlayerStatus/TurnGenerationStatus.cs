using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
using System.Linq;

namespace CraigStars.Client
{
    public class TurnGenerationStatus : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnGenerationStatus));

        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }
        Player Me { get => PlayersManager.Me; }

        Label yearLabel;
        PlayersTable playerStatusTable;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            yearLabel = GetNode<Label>("VBoxContainer/YearLabel");
            playerStatusTable = GetNode<PlayersTable>("VBoxContainer/ScrollContainer/PlayerStatusTable");

            playerStatusTable.Data.Clear();
            playerStatusTable.Data.AddColumns("Num", "Name", "Race");
            playerStatusTable.Data.AddColumn(new Column<PublicPlayerInfo>("Status")
            {
                CellProvider = (col, cell, row) => new PlayerStatusButtonCell(col, cell, row,
                    (CSButtonCell<PublicPlayerInfo> buttonCell) =>
                    {
                        if (buttonCell.Row.Metadata.SubmittedTurn)
                        {
                            EventManager.PublishUnsubmitTurnRequestedEvent(buttonCell.Row.Metadata);
                        }
                        else
                        {
                            // if we want to play a different player's turn, send that event
                            EventManager.PublishPlayTurnRequestedEvent(buttonCell.Row.Metadata.Num);
                        }
                    })
            });
            playerStatusTable.RowSelectedEvent += OnPlayerStatusRowSelected;

            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                playerStatusTable.RowSelectedEvent -= OnPlayerStatusRowSelected;
                EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
                EventManager.TurnGeneratingEvent -= OnTurnGenerating;
                EventManager.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            }
        }

        void OnPlayerStatusRowSelected(int rowIndex, int colIndex, Cell cell, object metadata)
        {
            if (colIndex == 3 && cell.Metadata is PublicPlayerInfo player)
            {
                log.Debug("Player status butotn clicked");
            }
        }

        void OnTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo submittingPlayer)
        {
            if (IsVisibleInTree())
            {
                UpdatePlayerStatuses();
            }
        }

        void OnTurnGenerating()
        {
            if (IsVisibleInTree())
            {
                UpdatePlayerStatuses();
            }
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
            bool resetTable = playerStatusTable.Data.Rows.Count() != GameInfo.Players.Count;
            playerStatusTable.Data.ClearRows();
            GameInfo.Players.ForEach(player =>
            {
                playerStatusTable.Data.AddRowAdvanced(metadata: player, color: Colors.White, italic: false,

                    player.Num,
                    player.Name,
                    player.RacePluralName,
                    player.SubmittedTurn ? "Submitted" : "Waiting to Submit"
                );
            });

            if (resetTable)
            {
                playerStatusTable.ResetTable();
            }
            else
            {
                playerStatusTable.UpdateRows();
            }
        }
    }

}
