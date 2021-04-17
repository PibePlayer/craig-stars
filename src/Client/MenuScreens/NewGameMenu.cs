using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using log4net;

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

        public override void _Ready()
        {
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
            }

            PlayersManager.Instance.Reset();
            PlayersManager.Instance.SetupPlayers();
            PlayersManager.Instance.Players.ForEach(player =>
            {
                var playerChooser = (PlayerChooser)PlayerChooserScene.Instance();
                playerChooser.Player = player as Player;
                playersContainer.AddChild(playerChooser);
            });

            fastHotseatCheckButton.Pressed = Settings.Instance.FastHotseat;
            fastHotseatCheckButton.Connect("toggled", this, nameof(OnFastHotseatToggled));
            addPlayerButton.Connect("pressed", this, nameof(OnAddPlayerButtonPressed));
            backButton.Connect("pressed", this, nameof(OnBackPressed));
            startButton.Connect("pressed", this, nameof(OnStartPressed));

            backButton.Disabled = startButton.Disabled = false;
        }

        void OnFastHotseatToggled(bool toggled)
        {
            Settings.Instance.FastHotseat = toggled;
        }

        void OnAddPlayerButtonPressed()
        {
            var player = PlayersManager.Instance.AddNewPlayer();
            var playerChooser = (PlayerChooser)PlayerChooserScene.Instance();
            playerChooser.Player = player as Player;
            playersContainer.AddChild(playerChooser);
        }

        void OnBackPressed()
        {
            GetTree().ChangeScene("res://src/Client/MainMenu.tscn");
        }

        void OnStartPressed()
        {
            RulesManager.Rules.Size = (Size)sizeOptionButton.Selected;
            RulesManager.Rules.Density = (Density)densityOptionButton.Selected;
            Settings.Instance.GameName = nameLineEdit.Text;

            loader.LoadScene("res://src/Client/GameView.tscn");
            backButton.Disabled = startButton.Disabled = true;
        }

    }
}