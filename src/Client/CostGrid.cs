using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class CostGrid : GridContainer
    {
        public Cost Cost
        {
            get => cost; set
            {
                cost = value;
                UpdateControls();
            }
        }
        Cost cost = new Cost(3, 3, 2, 8);

        [Export]
        public int Ironium
        {
            get => Cost.Ironium;
            set
            {
                Cost.Ironium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Boranium
        {
            get => Cost.Boranium;
            set
            {
                Cost.Boranium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Germanium
        {
            get => Cost.Germanium;
            set
            {
                Cost.Germanium = value;
                UpdateControls();
            }
        }

        [Export]
        public int Resources
        {
            get => Cost.Resources;
            set
            {
                Cost.Resources = value;
                UpdateControls();
            }
        }

        Label ironiumCostLabel;
        Label boraniumCostLabel;
        Label germaniumCostLabel;
        Label resourcesCostLabel;

        public override void _Ready()
        {
            ironiumCostLabel = GetNode<Label>("IroniumCost");
            boraniumCostLabel = GetNode<Label>("BoraniumCost");
            germaniumCostLabel = GetNode<Label>("GermaniumCost");
            resourcesCostLabel = GetNode<Label>("ResourcesCost");
        }

        void UpdateControls()
        {
            if (ironiumCostLabel != null)
            {
                ironiumCostLabel.Text = $"{Ironium}kT";
                boraniumCostLabel.Text = $"{Boranium}kT";
                germaniumCostLabel.Text = $"{Germanium}kT";
                resourcesCostLabel.Text = $"{Resources}";
            }
        }

    }
}