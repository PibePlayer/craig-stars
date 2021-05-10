using Godot;
using System;

namespace CraigStarsTable
{
    [Tool]
    public class LabelCell : LabelCell<object>
    {
    }

    public class LabelCell<T> : CellControl<T> where T : class
    {
        Label label;

        DynamicFontData italicFont;

        public override void _Ready()
        {
            base._Ready();
            label = GetNode<Label>("Label");
            italicFont = ResourceLoader.Load<DynamicFontData>("res://assets/gui/OpenSans-Italic.ttf");

            UpdateCell();
        }

        protected override void UpdateCell()
        {
            if (label != null)
            {
                label.Text = Cell.Text;
                label.Align = Column.Align;

                if (Row.Italic || Cell.Italic)
                {
                    var font = new DynamicFont()
                    {
                        FontData = italicFont,
                        Size = 14,
                    };
                    label.AddFontOverride("font", font);
                }

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