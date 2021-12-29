using System;
using Godot;

namespace CraigStars.Client
{
    public class PlanetCargoTransfer : VBoxContainer, ICargoTransferControl
    {
        public event CargoTransferRequested CargoTransferRequestedEvent;

        public ICargoHolder CargoHolder
        {
            get => Planet;
            set
            {
                Planet = value as Planet;
                UpdateControls();
            }
        }

        public Planet Planet { get; set; } = new Planet()
        {
            Name = "UI Test Planet",
            Cargo = new Cargo(100, 200, 300, 400)
        };

        public Cargo Cargo
        {
            get => Planet.Cargo;
            set
            {
                Planet.Cargo = value;
                UpdateControls();
            }
        }

        public int Fuel
        {
            get => Planet.Fuel;
            set
            {
                Planet.Fuel = value;
                UpdateControls();
            }
        }

        Label nameLabel;
        Label ironiumAmountLabel;
        Label boraniumAmountLabel;
        Label germaniumAmountLabel;
        Label colonistsAmountLabel;


        public override void _Ready()
        {
            nameLabel = FindNode("NameLabel") as Label;
            ironiumAmountLabel = FindNode("IroniumAmountLabel") as Label;
            boraniumAmountLabel = FindNode("BoraniumAmountLabel") as Label;
            germaniumAmountLabel = FindNode("GermaniumAmountLabel") as Label;
            colonistsAmountLabel = FindNode("ColonistsAmountLabel") as Label;
            UpdateControls();
        }

        public void UpdateControls()
        {
            if (Planet != null)
            {
                nameLabel.Text = Planet.Name;
                ironiumAmountLabel.Text = $"{Planet.Cargo.Ironium}kT";
                boraniumAmountLabel.Text = $"{Planet.Cargo.Boranium}kT";
                germaniumAmountLabel.Text = $"{Planet.Cargo.Germanium}kT";
                colonistsAmountLabel.Text = $"{Planet.Cargo.Colonists}kT";
            }
        }

    }
}