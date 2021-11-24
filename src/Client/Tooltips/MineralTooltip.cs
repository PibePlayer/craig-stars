using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars.Client
{
    public class MineralTooltip : CSTooltip
    {
        [Inject] protected PlanetService planetService;
        public MineralType Type { get; set; } = MineralType.Ironium;

        Label typeLabel;
        Label onSurfaceValueLabel;
        Label mineralConcentrationValueLabel;
        Label miningRateValueLabel;

        public override void _Ready()
        {
            this.ResolveDependencies();
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
                miningRateValueLabel.Text = $"{Planet.Spec.MineralOutput[Type]}kT/yr";
            }
        }

    }
}