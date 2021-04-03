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

        LineEdit nameLineEdit;
        OptionButton sizeOptionButton;
        OptionButton densityOptionButton;
        Container playersContainer;
        Button addPlayerButton;

        public override void _Ready()
        {
            nameLineEdit = (LineEdit)FindNode("NameLineEdit");
            sizeOptionButton = (OptionButton)FindNode("SizeOptionButton");
            densityOptionButton = (OptionButton)FindNode("DensityOptionButton");
            addPlayerButton = (Button)FindNode("AddPlayerButton");
            playersContainer = (Container)FindNode("PlayersContainer");

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

            addPlayerButton.Connect("pressed", this, nameof(OnAddPlayerButtonPressed));
            ((Button)FindNode("BackButton")).Connect("pressed", this, nameof(OnBackPressed));
            ((Button)FindNode("StartButton")).Connect("pressed", this, nameof(OnStartPressed));

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
            GetTree().ChangeScene("res://src/Client/GameView.tscn");
        }

    }
}