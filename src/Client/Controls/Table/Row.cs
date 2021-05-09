using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars
{
    public class Row : Row<object>
    {
        public Row(int index, List<Cell> data, object metadata = null, Color? color = null, bool italic = false) : base(index, data, metadata, color, italic)
        {
        }
    }

    /// <summary>
    /// A row in a table. This is basically a list of cells and some metadata
    /// </summary>
    public class Row<T> where T : class
    {
        /// <summary>
        /// The index of the row, updated during sort
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Metadata associated with the row
        /// </summary>
        public T Metadata { get; set; }

        /// <summary>
        /// The actual cell data used to render the row contents
        /// </summary>
        public List<Cell> Data { get; }

        /// <summary>
        /// The color to render each cell
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// True to use an italic font for labels in the cell
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// The index of the sorted row. For example for 10 rows, if we reverse sort and Index is zero, SortIndex will be 9
        /// we use this to move rows around rather than delete and re-instance them
        /// </summary>
        public int SortIndex { get; set; }

        public bool Visible { get; set; } = true;

        public Row(int index, List<Cell> data, T metadata = null, Color? color = null, bool italic = false)
        {
            Index = index;
            Metadata = metadata;
            Data = data;
            Color = color;
            Italic = italic;
        }
    }

}