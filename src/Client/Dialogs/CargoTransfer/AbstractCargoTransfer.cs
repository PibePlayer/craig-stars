using System;
using Godot;

namespace CraigStars.Client
{
    
    public class AbstractCargoTransfer<T> : VBoxContainer, ICargoTransferControl where T : ICargoHolder
    {
        public event CargoTransferRequested CargoTransferRequestedEvent;

        public ICargoHolder CargoHolder
        {
            get => cargoHolder;
            set
            {
                cargoHolder = value;
                UpdateControls();
            }
        }
        ICargoHolder cargoHolder;

        public Cargo Cargo
        {
            get => cargoHolder.Cargo;
            set
            {
                cargoHolder.Cargo = value;
                UpdateControls();
            }
        }

        public int Fuel
        {
            get => cargoHolder.Fuel;
            set
            {
                cargoHolder.Fuel = value;
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

        Label nameLabel;
        Label ironiumAmountLabel;
        Label boraniumAmountLabel;
        Label germaniumAmountLabel;
        Control colonistsAmountPanel;
        Label colonistsLabel;
        Label colonistsAmountLabel;


        public override void _Ready()
        {
            nameLabel = FindNode("NameLabel") as Label;
            ironiumAmountLabel = FindNode("IroniumAmountLabel") as Label;
            boraniumAmountLabel = FindNode("BoraniumAmountLabel") as Label;
            germaniumAmountLabel = FindNode("GermaniumAmountLabel") as Label;
            colonistsLabel = FindNode("ColonistsLabel") as Label;
            colonistsAmountPanel = FindNode("ColonistsAmountPanel") as Control;
            colonistsAmountLabel = FindNode("ColonistsAmountLabel") as Label;
            UpdateControls();
        }

        public void UpdateControls()
        {
            if (cargoHolder != null)
            {
                nameLabel.Text = cargoHolder.Name;
                ironiumAmountLabel.Text = $"{cargoHolder.Cargo.Ironium}kT";
                boraniumAmountLabel.Text = $"{cargoHolder.Cargo.Boranium}kT";
                germaniumAmountLabel.Text = $"{cargoHolder.Cargo.Germanium}kT";
                colonistsAmountLabel.Text = $"{cargoHolder.Cargo.Colonists}kT";

                colonistsLabel.Visible = colonistsAmountPanel.Visible = colonistsAmountLabel.Visible = ShowColonists;
            }
        }

    }
}