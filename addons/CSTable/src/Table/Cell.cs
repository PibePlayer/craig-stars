using System;
using Godot;

namespace CraigStarsTable
{
    /// <summary>
    /// Cell data for a Table. This defines the text rendered in the cell as well as the
    /// actual value used for sorting.
    /// </summary>
    public class Cell
    {
        public string Text { get; set; } = "";
        public IComparable Value { get; set; }
        public bool Hidden { get; set; }
        public Color Color { get; set; } = Colors.White;
        public bool Italic { get; set; }
        public object Metadata { get; set; }

        public Cell(string text, IComparable value = null, bool hidden = false, Color? color = null, object metadata = null)
        {
            Text = text;
            Hidden = hidden;

            if (value == null)
            {
                Value = text;
            }
            else
            {
                Value = value;
            }
            if (color.HasValue)
            {
                Color = color.Value;
            }

            Metadata = metadata;
        }

        public Cell(int value)
        {
            Text = value.ToString();
            Value = value;
            Hidden = false;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}