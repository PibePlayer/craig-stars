using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using log4net;
using System.Collections.Generic;

namespace CraigStars
{
    public class NewGameMenu : MarginContainer
    {
        [Export]
        public PackedScene PlayerChooserScene { get; set; }

        [Export]
        public Size Size { get; set; }

        [Export]
        public Density Density { get; set; }

        Loader loader;
        CheckButton fastHotseatCheckButton;
        LineEdit nameLineEdit;
        OptionButton sizeOptionButton;
        OptionButton densityOptionButton;
        Container playersContainer;
        Button addPlayerButton;
        Button startButton;
        Button backButton;

        List<Player> players;

        public override void _Ready()
        {
            base._Ready();
            loader = GetNode<Loader>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BottomHBoxContainer/Loader");
            startButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/StartButton");
            backButton = GetNode<Button>("VBoxContainer/CenterContainer/Panel/MarginContainer/HBoxContainer/MenuButtons/BackButton");

            nameLineEdit = (LineEdit)FindNode("NameLineEdit");
            sizeOptionButton = (OptionButton)FindNode("SizeOptionButton");
            densityOptionButton = (OptionButton)FindNode("DensityOptionButton");
            addPlayerButton = (Button)FindNode("AddPlayerButton");
            playersContainer = (Container)FindNode("PlayersContainer");
            fastHotseatCheckButton = (CheckButton)FindNode("FastHotseatCheckButton");

            sizeOptionButton.PopulateOptionButton<Size>();
            densityOptionButton.PopulateOptionButton<Density>();

            sizeOptionButton.Selected = (int)Size;
            densityOptionButton.Selected = (int)Density;

            foreach (Node node in playersContainer.GetChildren())
            {
                playersContainer.RemoveChild(node);
                node.QueueFree();
            }

            players = PlayersManager.CreateNewPlayersList(2);

            players.ForEach(player =>
            {
                var playerChooser = (PlayerChooser)PlayerChooserScene.Instance();
                playerChooser.Player = player;
                playerChooser.PlayerRemovedEvent += OnPlayerRemoved;
                playersContainer.AddChild(playerChooser);
            });


            fastHotseatCheckButton.Pressed = Settings.Instance.FastHotseat;
            fastHotseatCheckButton.Connect("toggled", this, nameof(OnFastHotseatToggled));
            addPlayerButton.Connect("pressed", this, nameof(OnAddPlayerButtonPressed));
            backButton.Connect("pressed", this, nameof(OnBackPressed));
            startButton.Connect("pressed", this, nameof(OnStartPressed));

            backButton.Disabled = startButton.Disabled = false;

            Signals.GameStartedEvent += OnGameStarted;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.GameStartedEvent -= OnGameStarted;
        }

        void OnFastHotseatToggled(bool toggled)
        {
            Settings.Instance.FastHotseat = toggled;
        }

        void OnAddPlayerButtonPressed()
        {
            var player = PlayersManager.CreateNewPlayer(players.Count);
            players.Add(player);
            var playerChooser = (PlayerChooser)PlayerChooserScene.Instance();
            playerChooser.Player = player;
            playerChooser.PlayerRemovedEvent += OnPlayerRemoved;

            playersContainer.AddChild(playerChooser);
        }

        void OnPlayerRemoved(PlayerChooser playerChooser, Player player)
        {
            players.Remove(player);
            playersContainer.RemoveChild(playerChooser);
            playerChooser.QueueFree();
            playerChooser.PlayerRemovedEvent -= OnPlayerRemoved;

            // reassign player numbers in case we deleted a player out of the middle
            players.Each((player, index) => player.Num = index);
            foreach (Node node in playersContainer.GetChildren())
            {
                if (node is PlayerChooser chooser)
                {
                    chooser.UpdateControls();
                }
            }
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }

        void OnStartPressed()
        {
            GameSettings<Player> settings = new GameSettings<Player>()
            {
                Name = nameLineEdit.Text,
                Size = (Size)sizeOptionButton.Selected,
                Density = (Density)densityOptionButton.Selected,
                Players = players,
            };

            // start a new game and change to the client view
            Action startGame = () =>
            {
                ServerManager.Instance.NewGame(settings);

                backButton.Disabled = startButton.Disabled = true;
            };

            if (GamesManager.Instance.GameExists(settings.Name))
            {
                CSConfirmDialog.Show($"A game named {settings.Name} already exists. Are you sure you want to overwrite it?", () =>
                {
                    // delete the existing game
                    GamesManager.Instance.DeleteGame(settings.Name);
                    startGame.Invoke();
                });
            }
            else
            {
                startGame.Invoke();
            }

        }

        /// <summary>
        /// The server will notify us when the game is ready
        /// </summary>
        /// <param name="gameInfo"></param>
        void OnGameStarted(PublicGameInfo gameInfo, Player player)
        {
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (client) =>
            {
                PlayersManager.Me = player;
                client.GameInfo = gameInfo;
            });
        }

    }
}