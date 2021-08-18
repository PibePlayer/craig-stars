using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Client
{
    public class NewGamePlayers : VBoxContainer
    {
        public List<Player> Players { get; set; } = new List<Player>();

        Container PlayersContainer;
        Button addPlayerButton;
        PackedScene playerChooserScene;

        public override void _Ready()
        {
            PlayersContainer = GetNode<Container>("ScrollContainer/MarginContainer/PlayersContainer");
            addPlayerButton = GetNode<Button>("HBoxContainer/AddPlayerButton");
            playerChooserScene = CSResourceLoader.GetPackedScene("PlayerChooser.tscn");

            addPlayerButton.Connect("pressed", this, nameof(OnAddPlayerButtonPressed));

            foreach (Node node in PlayersContainer.GetChildren())
            {
                PlayersContainer.RemoveChild(node);
                node.QueueFree();
            }

        }

        public void InitPlayersForSinglePlayerGame()
        {
            Players = PlayersManager.CreatePlayersForNewGame(2);

            Players.ForEach(player =>
            {
                var playerChooser = playerChooserScene.Instance<PlayerChooser>();
                playerChooser.Player = player;
                playerChooser.PlayerRemovedEvent += OnPlayerRemoved;
                PlayersContainer.AddChild(playerChooser);
            });

        }

        void OnAddPlayerButtonPressed()
        {
            var player = PlayersManager.CreateNewPlayer(Players.Count);
            Players.Add(player);
            var playerChooser = playerChooserScene.Instance<PlayerChooser>();
            playerChooser.Player = player;
            playerChooser.PlayerRemovedEvent += OnPlayerRemoved;

            PlayersContainer.AddChild(playerChooser);
        }

        void OnPlayerRemoved(PlayerChooser playerChooser, Player player)
        {
            Players.Remove(player);
            PlayersContainer.RemoveChild(playerChooser);
            playerChooser.QueueFree();
            playerChooser.PlayerRemovedEvent -= OnPlayerRemoved;

            // reassign player numbers in case we deleted a player out of the middle
            Players.Each((player, index) => player.Num = index);
            foreach (Node node in PlayersContainer.GetChildren())
            {
                if (node is PlayerChooser chooser)
                {
                    chooser.UpdateControls();
                }
            }
        }


    }
}