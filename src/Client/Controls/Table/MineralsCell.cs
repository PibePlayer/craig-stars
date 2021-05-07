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
        protected Container container;
        protected Label emptyLabel;

        public override void _Ready()
        {
            base._Ready();

            container = GetNode<Container>("HBoxContainer");
            ironium = GetNode<Label>("HBoxContainer/Ironium");
            boranium = GetNode<Label>("HBoxContainer/Boranium");
            germanium = GetNode<Label>("HBoxContainer/Germanium");
            emptyLabel = GetNode<Label>("EmptyLabel");

            UpdateCell();
        }

        protected override void UpdateCell()
        {
            if (ironium != null)
            {
                if (Cell?.Metadata is Mineral mineral)
                {
                    if (mineral == Mineral.Empty)
                    {
                        container.Visible = false;
                        emptyLabel.Visible = true;
                    }
                    else
                    {
                        container.Visible = true;
                        emptyLabel.Visible = false;

                        ironium.Text = $"{mineral.Ironium}";
                        boranium.Text = $"{mineral.Boranium}";
                        germanium.Text = $"{mineral.Germanium}";
                    }
                }
                else if (Cell?.Metadata is Cargo cargo)
                {
                    if (cargo == Cargo.Empty)
                    {
                        container.Visible = false;
                        emptyLabel.Visible = true;
                    }
                    else
                    {
                        container.Visible = true;
                        emptyLabel.Visible = false;

                        ironium.Text = $"{cargo.Ironium}";
                        boranium.Text = $"{cargo.Boranium}";
                        germanium.Text = $"{cargo.Germanium}";
                    }
                }
            }
        }
    }
}