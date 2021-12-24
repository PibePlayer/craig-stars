using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{

    public class TransferFleetWaypointTaskContainer : VBoxContainer
    {
        Player Me { get => PlayersManager.Me; }
        PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        public Waypoint Waypoint { get; set; } = new();

        OptionButton transferFleetToOptionButton;

        List<int> playerNums = new();

        public override void _Ready()
        {
            base._Ready();
            transferFleetToOptionButton = GetNode<OptionButton>("GridContainer/TransferFleetToOptionButton");

            transferFleetToOptionButton.Connect("item_selected", this, nameof(OnTransferFleetToOptionButtonItemSelected));

            playerNums.Clear();
            foreach (var player in GameInfo.Players.Where(player => player.Num != Me.Num))
            {
                transferFleetToOptionButton.AddItem($"The {player.RacePluralName}");
                playerNums.Add(player.Num);
            }

            UpdateControls();
        }

        void OnTransferFleetToOptionButtonItemSelected(int index)
        {
            Waypoint.TransferToPlayer = playerNums[index];
        }

        void UpdateControls()
        {
            if (transferFleetToOptionButton != null && Waypoint != null)
            {
                transferFleetToOptionButton.Selected = playerNums.FindIndex(playerNum => playerNum == Waypoint.TransferToPlayer);
            }
        }
    }
}