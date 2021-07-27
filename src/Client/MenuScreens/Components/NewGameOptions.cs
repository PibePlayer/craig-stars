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
        public Size Size { get; set; } = Size.Small;

        [Export]
        public Density Density { get; set; } = Density.Normal;

        LineEdit nameLineEdit;
        OptionButton sizeOptionButton;
        OptionButton densityOptionButton;

        public override void _Ready()
        {
            nameLineEdit = GetNode<LineEdit>("NameLineEdit");
            sizeOptionButton = GetNode<OptionButton>("SizeOptionButton");
            densityOptionButton = GetNode<OptionButton>("DensityOptionButton");

            sizeOptionButton.PopulateOptionButton<Size>();
            densityOptionButton.PopulateOptionButton<Density>();

            sizeOptionButton.Selected = (int)Size;
            densityOptionButton.Selected = (int)Density;
        }

        public GameSettings<Player> GetGameSettings()
        {
            return new GameSettings<Player>()
            {
                Name = nameLineEdit.Text,
                Size = (Size)sizeOptionButton.Selected,
                Density = (Density)densityOptionButton.Selected,
            };

        }

    }
}
