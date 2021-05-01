using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class MineralsCell : CellControl
    {
        protected Label ironium;
        protected Label boranium;
        protected Label germanium;

        public override void _Ready()
        {
            base._Ready();

            ironium = GetNode<Label>("HBoxContainer/Ironium");
            boranium = GetNode<Label>("HBoxContainer/Boranium");
            germanium = GetNode<Label>("HBoxContainer/Germanium");

            UpdateCell();
        }

        protected override void UpdateCell()
        {
            if (ironium != null)
            {
                if (Cell?.Metadata is Mineral mineral)
                {
                    ironium.Text = $"{mineral.Ironium}";
                    boranium.Text = $"{mineral.Boranium}";
                    germanium.Text = $"{mineral.Germanium}";
                }
                else if (Cell?.Metadata is Cargo cargo)
                {
                    ironium.Text = $"{cargo.Ironium}";
                    boranium.Text = $"{cargo.Boranium}";
                    germanium.Text = $"{cargo.Germanium}";
                }
            }
        }
    }
}