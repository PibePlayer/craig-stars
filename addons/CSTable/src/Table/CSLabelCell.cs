using System;
using Godot;

namespace CraigStarsTable
{
    [Tool]
    public class CSLabelCell : CSLabelCell<object>
    {
    }

    public class CSLabelCell<T> : CSMarginContainerCellControl<T> where T : class
    {
        Label label;

        DynamicFontData italicFont;

        public CSLabelCell() : base()
        {
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            RectMinSize = new Vector2(16, 16);
        }

        public CSLabelCell(Column<T> col, Cell cell, Row<T> row) : base(col, cell, row)
        {
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            RectMinSize = new Vector2(16, 16);
        }

        public override void _Ready()
        {
            base._Ready();

            label = new Label()
            {
                SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                SizeFlagsVertical = (int)SizeFlags.Expand | (int)SizeFlags.ShrinkCenter,
            };
            AddChild(label);

            AddConstantOverride("margin_right", 3);
            AddConstantOverride("margin_top", 3);
            AddConstantOverride("margin_left", 3);
            AddConstantOverride("margin_bottom", 3);

            italicFont = ResourceLoader.Load<DynamicFontData>("res://addons/CSTable/assets/OpenSans-Italic.ttf");

            UpdateCell();
        }

        public override void UpdateCell()
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