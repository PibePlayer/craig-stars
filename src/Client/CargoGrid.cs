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
                cargo = cargo.WithIronium(value);
                UpdateControls();
            }
        }

        [Export]
        public int Boranium
        {
            get => Cargo.Boranium;
            set
            {
                cargo = cargo.WithBoranium(value);
                UpdateControls();
            }
        }

        [Export]
        public int Germanium
        {
            get => Cargo.Germanium;
            set
            {
                cargo = cargo.WithGermanium(value);
                UpdateControls();
            }
        }

        [Export]
        public int Colonists
        {
            get => Cargo.Colonists;
            set
            {
                cargo = cargo.WithColonists(value);
                UpdateControls();
            }
        }

        [Export]
        public bool ShowColonists
        {
            get => showColonists;
            set
            {
                showColonists = value;
                UpdateControls();
            }
        }
        bool showColonists = true;

        Label ironiumAmountLabel;
        Label boraniumAmountLabel;
        Label germaniumAmountLabel;
        Label colonistsLabel;
        Label colonistsAmountLabel;

        public override void _Ready()
        {
            ironiumAmountLabel = GetNode<Label>("IroniumAmount");
            boraniumAmountLabel = GetNode<Label>("BoraniumAmount");
            germaniumAmountLabel = GetNode<Label>("GermaniumAmount");
            colonistsLabel = GetNode<Label>("Colonists");
            colonistsAmountLabel = GetNode<Label>("ColonistsAmount");
        }

        void UpdateControls()
        {

            if (ironiumAmountLabel != null)
            {
                colonistsAmountLabel.Visible = colonistsLabel.Visible = ShowColonists;
                ironiumAmountLabel.Text = $"{Ironium}kT";
                boraniumAmountLabel.Text = $"{Boranium}kT";
                germaniumAmountLabel.Text = $"{Germanium}kT";
                colonistsAmountLabel.Text = $"{Colonists}kT";
            }
        }

    }
}