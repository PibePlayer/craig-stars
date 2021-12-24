using System;
using System.Linq;
using CraigStars.Singletons;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    public class TurnGenerationStatus : MarginContainer
    {
        static CSLog log = LogProvider.GetLogger(typeof(TurnGenerationStatus));

        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }
        Player Me { get => PlayersManager.Me; }

        Label yearLabel;
        PlayersTable playerStatusTable;
        ProgressStatus progressStatus;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
            yearLabel = GetNode<Label>("VBoxContainer/YearLabel");
            playerStatusTable = GetNode<PlayersTable>("VBoxContainer/ScrollContainer/PlayerStatusTable");
            progressStatus = GetNode<ProgressStatus>("VBoxContainer/ProgressStatus");

            playerStatusTable.Data.Clear();
            playerStatusTable.Data.AddColumns("Num", "Name", "Race");
            playerStatusTable.Data.AddColumn(new Column<PublicPlayerInfo>("Status")
            {
                CellProvider = (col, cell, row) => new PlayerStatusButtonCell(col, cell, row,
                    (CSButtonCell<PublicPlayerInfo> buttonCell) =>
                    {
                        if (buttonCell.Row.Metadata.SubmittedTurn)
                        {
                            buttonCell.Row.Metadata.SubmittedTurn = false;
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

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            EventManager.GameViewResetEvent += OnGameViewResetEvent;
            EventManager.TurnSubmittedEvent += OnTurnSubmitted;
            EventManager.TurnUnsubmittedEvent += OnTurnUnsubmitted;
            EventManager.PlayTurnRequestedEvent += OnPlayTurnRequested;
            EventManager.TurnGeneratingEvent += OnTurnGenerating;
            EventManager.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            EventManager.UniverseGeneratorAdvancedEvent += OnUniverseGeneratorAdvanced;

            CSResourceLoader.ResourceLoadingEvent += OnResourceLoading;

        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                playerStatusTable.RowSelectedEvent -= OnPlayerStatusRowSelected;
                EventManager.GameViewResetEvent -= OnGameViewResetEvent;
                EventManager.TurnSubmittedEvent -= OnTurnSubmitted;
                EventManager.TurnUnsubmittedEvent -= OnTurnUnsubmitted;
                EventManager.PlayTurnRequestedEvent -= OnPlayTurnRequested;
                EventManager.TurnGeneratingEvent -= OnTurnGenerating;
                EventManager.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
                EventManager.UniverseGeneratorAdvancedEvent -= OnUniverseGeneratorAdvanced;
                CSResourceLoader.ResourceLoadingEvent -= OnResourceLoading;
            }
        }

        void OnVisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                UpdateControls();
            }
        }

        void OnPlayerStatusRowSelected(int rowIndex, int colIndex, Cell cell, object metadata)
        {
            if (colIndex == 3 && cell.Metadata is PublicPlayerInfo player)
            {
                log.Debug("Player status butotn clicked");
            }
        }

        void OnGameViewResetEvent(PublicGameInfo gameInfo)
        {
            if (IsVisibleInTree())
            {
                PlayersManager.GameInfo = gameInfo;
                UpdateControls();
            }
        }

        void OnTurnSubmitted(PublicGameInfo gameInfo, PublicPlayerInfo submittingPlayer)
        {
            if (IsVisibleInTree())
            {
                PlayersManager.GameInfo = gameInfo;
                UpdateControls();
            }
        }

        void OnTurnUnsubmitted(PublicGameInfo gameInfo, PublicPlayerInfo submittingPlayer)
        {
            if (IsVisibleInTree())
            {
                PlayersManager.GameInfo = gameInfo;
                UpdateControls();
            }
        }

        void OnPlayTurnRequested(int playerNum)
        {
            if (IsVisibleInTree())
            {
                UpdateControls();
            }
        }

        void OnTurnGenerating()
        {
            if (IsVisibleInTree())
            {
                UpdateControls();
            }
        }

        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            yearLabel.Text = $"Year {GameInfo.Year}";
            progressStatus.Visible = state != TurnGenerationState.Finished;
            progressStatus.Progress = 100 * (((int)state + 1) / (float)(Enum.GetValues(typeof(TurnGenerationState)).Length));

            switch (state)
            {
                case TurnGenerationState.WaitingForPlayers:
                    progressStatus.ProgressLabel = "Waiting for Players";
                    progressStatus.ProgressSubLabel = "";
                    break;
                default:
                    progressStatus.ProgressLabel = "Generating Turn";
                    progressStatus.ProgressSubLabel = state.ToString();
                    break;
            }
        }

        void OnUniverseGeneratorAdvanced(UniverseGenerationState state)
        {
            yearLabel.Text = $"Year {GameInfo.Year}";
            progressStatus.Visible = state != UniverseGenerationState.Finished;
            progressStatus.Progress = 100 * (((int)state + 1) / (float)(Enum.GetValues(typeof(UniverseGenerationState)).Length));

            progressStatus.ProgressLabel = "Generating Universe";
            progressStatus.ProgressSubLabel = state.ToString();
        }

        private void OnResourceLoading(string resource)
        {
            progressStatus.Visible = true;
            progressStatus.ProgressSubLabel = $"Loading {resource}";
            progressStatus.Progress = (float)CSResourceLoader.Loaded / CSResourceLoader.TotalResources;
        }



        /// <summary>
        /// Update the player statuses in the dialog
        /// </summary>
        public void UpdateControls()
        {
            if (GameInfo == null)
            {
                return;
            }

            yearLabel.Text = $"Year {GameInfo.Year}";

            if (CSResourceLoader.TotalResources > 0 && CSResourceLoader.Loaded < CSResourceLoader.TotalResources)
            {
                progressStatus.Visible = true;
                progressStatus.ProgressLabel = "Loading client resources";
                progressStatus.Progress = (float)CSResourceLoader.Loaded / CSResourceLoader.TotalResources;
            }
            else
            {
                progressStatus.Visible = GameInfo.State != GameState.WaitingForPlayers;
            }

            bool resetTable = playerStatusTable.Data.Rows.Count() != GameInfo.Players.Count;
            playerStatusTable.Data.ClearRows();
            GameInfo.Players.ForEach(player =>
            {
                playerStatusTable.Data.AddRowAdvanced(metadata: player, color: Colors.White, italic: false,

                    player.Num + 1,
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
