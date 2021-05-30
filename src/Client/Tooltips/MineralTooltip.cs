using Godot;
using System;

namespace CraigStars
{
    public class MineralTooltip : PopupPanel
    {
        public MineralType Type { get; set; } = MineralType.Ironium;
        public Planet Planet { get; set; }

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

            Connect("about_to_show", this, nameof(OnAboutToShow));
        }

        public void OnAboutToShow()
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