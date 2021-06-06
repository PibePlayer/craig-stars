using Godot;
using System;

namespace CraigStars
{
    public class MineralTooltip : CSTooltip
    {
        public MineralType Type { get; set; } = MineralType.Ironium;

        Label typeLabel;
        Label onSurfaceValueLabel;
        Label mineralConcentrationValueLabel;
        Label miningRateValueLabel;

        public override void _Ready()
        {
            typeLabel = GetNode<Label>("MarginContainer/VBoxContainer/TypeLabel");
            onSurfaceValueLabel = GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/OnSurfaceValueLabel");
            mineralConcentrationValueLabel = GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/MineralConcentrationValueLabel");
            miningRateValueLabel = GetNode<Label>("MarginContainer/VBoxContainer/GridContainer/MiningRateValueLabel");
        }

        public void ShowAtMouse(Planet planet, MineralType type)
        {
            Type = type;
            ShowAtMouse(planet);
        }


        protected override void UpdateControls()
        {
            typeLabel.Text = Type.ToString();
            if (Planet != null)
            {
                onSurfaceValueLabel.Text = $"{Planet.Cargo[Type]}kT";
                mineralConcentrationValueLabel.Text = $"{Planet.MineralConcentration[Type]}";
                miningRateValueLabel.Text = $"{Planet.MineralOutput[Type]}kT/yr";
            }
        }

    }
}