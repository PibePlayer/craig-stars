using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class CargoGrid : GridContainer
    {
        public Cargo Cargo
        {
            get => cargo; set
            {
                cargo = value;
                UpdateControls();
            }
        }
        Cargo cargo = new Cargo(3, 3, 2, 8);

        [Export]
        public int Ironium
        {
            get => Cargo.Ironium;
            set
            {
                Cargo.Ironium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Boranium
        {
            get => Cargo.Boranium;
            set
            {
                Cargo.Boranium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Germanium
        {
            get => Cargo.Germanium;
            set
            {
                Cargo.Germanium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Colonists
        {
            get => Cargo.Colonists;
            set
            {
                Cargo.Colonists = value;
                UpdateControls();
            }
        }

        Label ironiumAmountLabel;
        Label boraniumAmountLabel;
        Label germaniumAmountLabel;
        Label colonistsAmountLabel;

        public override void _Ready()
        {
            ironiumAmountLabel = GetNode<Label>("IroniumAmount");
            boraniumAmountLabel = GetNode<Label>("BoraniumAmount");
            germaniumAmountLabel = GetNode<Label>("GermaniumAmount");
            colonistsAmountLabel = GetNode<Label>("ColonistsAmount");
        }

        void UpdateControls()
        {
            if (ironiumAmountLabel != null)
            {
                ironiumAmountLabel.Text = $"{Ironium}kT";
                boraniumAmountLabel.Text = $"{Boranium}kT";
                germaniumAmountLabel.Text = $"{Germanium}kT";
                colonistsAmountLabel.Text = $"{Colonists}kT";
            }
        }

    }
}