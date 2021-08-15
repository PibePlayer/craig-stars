using Godot;
using System;

namespace CraigStarsTable
{
    /// <summary>
    /// A CSButtonCell will show a clickable button
    /// </summary>
    [Tool]
    public class CSButtonCell : CSButtonCell<object>
    {
    }

    public class CSButtonCell<T> : CSMarginContainerCellControl<T> where T : class
    {
        protected Action<CSButtonCell<T>> onPressed;
        protected Button button;

        protected DynamicFontData italicFont;

        public CSButtonCell()
        {
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            RectMinSize = new Vector2(16, 16);
        }

        public CSButtonCell(Column<T> column, Cell cell, Row<T> row, Action<CSButtonCell<T>> onPressed) : base()
        {
            Column = column;
            Cell = cell;
            Row = row;
            this.onPressed = onPressed;
        }

        public override void _Ready()
        {
            base._Ready();

            button = new Button()
            {
                SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                SizeFlagsVertical = (int)SizeFlags.Expand | (int)SizeFlags.ShrinkCenter,
            };
            AddChild(button);

            AddConstantOverride("margin_right", 3);
            AddConstantOverride("margin_top", 3);
            AddConstantOverride("margin_left", 3);
            AddConstantOverride("margin_bottom", 3);

            italicFont = ResourceLoader.Load<DynamicFontData>("res://addons/CSTable/assets/OpenSans-Italic.ttf");

            button.Connect("pressed", this, nameof(OnButtonPressed));

            UpdateCell();
        }

        void OnButtonPressed()
        {
            onPressed?.Invoke(this);
        }

        /// <summary>
        /// Register a callback with the "pressed" signal for this button
        /// </summary>
        /// <param name="onPressed">The action to callback</param>
        public void OnPressed(Action<CSButtonCell<T>> onPressed)
        {
            this.onPressed = onPressed;
        }

        protected override void UpdateCell()
        {
            if (button != null)
            {
                button.Text = Cell.Text;

                if (Row.Italic || Cell.Italic)
                {
                    var font = new DynamicFont()
                    {
                        FontData = italicFont,
                        Size = 14,
                    };
                    button.AddFontOverride("font", font);
                }

                // use a cell color override or a row color override
                if (Cell.Color != Colors.White)
                {
                    button.Modulate = Cell.Color;
                }
                else if (Row.Color.HasValue && Row.Color != Colors.White)
                {
                    button.Modulate = Row.Color.Value;
                }
                else
                {
                    button.Modulate = Colors.White;
                }
            }
        }
    }
}