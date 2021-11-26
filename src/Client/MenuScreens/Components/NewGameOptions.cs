using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars.Client
{
    /// <summary>
    /// Control to set options for a new game
    /// </summary>
    public class NewGameOptions : GridContainer
    {
        [Export]
        public string GameName { get; set; } = "A Barefoot Jaywalk";

        [Export]
        public Size Size { get; set; } = Size.Small;

        [Export]
        public Density Density { get; set; } = Density.Normal;

        [Export]
        public PlayerPositions PlayerPositions { get; set; } = PlayerPositions.Moderate;

        LineEdit nameLineEdit;
        OptionButton sizeOptionButton;
        OptionButton densityOptionButton;
        OptionButton playerPositionsOptionButton;

        public override void _Ready()
        {
            nameLineEdit = GetNode<LineEdit>("NameLineEdit");
            sizeOptionButton = GetNode<OptionButton>("SizeOptionButton");
            densityOptionButton = GetNode<OptionButton>("DensityOptionButton");
            playerPositionsOptionButton = GetNode<OptionButton>("PlayerPositionsOptionButton");

            sizeOptionButton.PopulateOptionButton<Size>();
            densityOptionButton.PopulateOptionButton<Density>();
            playerPositionsOptionButton.PopulateOptionButton<PlayerPositions>();

            sizeOptionButton.Selected = (int)Size;
            densityOptionButton.Selected = (int)Density;
            playerPositionsOptionButton.Selected = (int)PlayerPositions;
            nameLineEdit.Text = GameName;
            nameLineEdit.GrabFocus();
        }

        public GameSettings<Player> GetGameSettings()
        {
            return new GameSettings<Player>()
            {
                Name = nameLineEdit.Text,
                Size = (Size)sizeOptionButton.Selected,
                Density = (Density)densityOptionButton.Selected,
                PlayerPositions = (PlayerPositions)playerPositionsOptionButton.Selected
            };

        }

    }
}
