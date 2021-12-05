using System;
using Godot;

namespace CraigStars.Client
{
    [Tool]
    public class CargoCell : MineralsCell
    {
        protected Label colonists;
        protected Label colonistskTLabel;

        public override void _Ready()
        {
            colonists = GetNode<Label>("HBoxContainer/Colonists");
            colonistskTLabel = GetNode<Label>("HBoxContainer/ColonistskTLabel");

            base._Ready();
        }

        public override void UpdateCell()
        {
            if (ironium != null)
            {
                if (Cell?.Metadata is Mineral mineral)
                {
                    colonistskTLabel.Visible = colonists.Visible = false;
                    ironium.Text = $"{mineral.Ironium}";
                    boranium.Text = $"{mineral.Boranium}";
                    germanium.Text = $"{mineral.Germanium}";
                }
                else if (Cell?.Metadata is Cargo cargo)
                {
                    var fleet = Row.Metadata as Fleet;
                    if (fleet != null && fleet.Spec.CargoCapacity == 0)
                    {
                        // if this cell is for a fleet with no cargo capacity, hide these controls
                        container.Visible = false;
                        emptyLabel.Visible = true;
                    }
                    else
                    {
                        container.Visible = true;
                        emptyLabel.Visible = false;
                        colonistskTLabel.Visible = colonists.Visible = true;
                        ironium.Text = $"{cargo.Ironium}";
                        boranium.Text = $"{cargo.Boranium}";
                        germanium.Text = $"{cargo.Germanium}";
                        colonists.Text = $"{cargo.Colonists}";
                    }
                }
            }
        }
    }
}