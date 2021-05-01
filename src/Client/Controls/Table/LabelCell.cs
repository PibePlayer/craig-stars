using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class LabelCell : CellControl
    {
        Label label;


        public override void _Ready()
        {
            base._Ready();
            label = GetNode<Label>("Label");

            UpdateCell();
        }

        protected override void UpdateCell()
        {
            if (label != null)
            {
                label.Text = Cell.Text;
                label.Align = Column.Align;

                // use a cell color override or a row color override
                if (Cell.Color != Colors.White)
                {
                    label.Modulate = Cell.Color;
                }
                else if (Row.Color.HasValue && Row.Color != Colors.White)
                {
                    label.Modulate = Row.Color.Value;
                }
                else
                {
                    label.Modulate = Colors.White;
                }
            }
        }
    }
}