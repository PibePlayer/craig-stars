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
        PackedScene newGamePlayerScene;

        public override void _Ready()
        {
            PlayersContainer = GetNode<Container>("ScrollContainer/MarginContainer/PlayersContainer");
            addPlayerButton = GetNode<Button>("HBoxContainer/AddPlayerButton");
            newGamePlayerScene = ResourceLoader.Load<PackedScene>("res://src/Client/MenuScreens/Components/NewGamePlayer.tscn");

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
            Players[0].Name = Settings.Instance.PlayerName;

            Players.ForEach(player =>
            {
                var playerChooser = newGamePlayerScene.Instance<NewGamePlayer>();
                playerChooser.Player = player;
                playerChooser.PlayerRemovedEvent += OnPlayerRemoved;
                PlayersContainer.AddChild(playerChooser);
            });

        }

        void OnAddPlayerButtonPressed()
        {
            var player = PlayersManager.CreateNewPlayer(Players.Count);
            Players.Add(player);
            var playerChooser = newGamePlayerScene.Instance<NewGamePlayer>();
            playerChooser.Player = player;
            playerChooser.PlayerRemovedEvent += OnPlayerRemoved;

            PlayersContainer.AddChild(playerChooser);
        }

        void OnPlayerRemoved(PlayerChooser<Player> playerChooser, Player player)
        {
            Players.RemoveAt(player.Num);
            PlayersContainer.RemoveChild(playerChooser);
            playerChooser.PlayerRemovedEvent -= OnPlayerRemoved;
            playerChooser.QueueFree();

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