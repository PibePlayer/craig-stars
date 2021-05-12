using System;
using Godot;

namespace CraigStarsTable
{
    /// <summary>
    /// Represents a column for a table
    /// </summary>
    public class Column
    {
        public string Name { get; set; }
        public bool Sortable { get; set; }
        public bool Hidden { get; set; }
        public int Index { get; set; }
        public Label.AlignEnum Align { get; set; } = Label.AlignEnum.Left;

        /// <summary>
        /// Override the Script path for this column's cells
        /// When rendering a cell, first an override script is checked for, then an override cell.
        /// If neither is present, it will use the default script (LabelCell.cs)
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Override the Sceme path for this column's cells
        /// When rendering a cell, first an override script is checked for, then an override cell.
        /// If neither is present, it will use the default script (LabelCell.cs)
        /// </summary>
        /// <value></value>
        public string Scene { get; set; }        

        /// <summary>
        /// If set, use this scene to render cells instead of the table default
        /// </summary>
        public string CellScene { get; set; }

        public Column(
            string name,
            int index = 0,
            bool sortable = true,
            bool hidden = false,
            Label.AlignEnum align = Label.AlignEnum.Left,
            string scene = null)
        {
            if (name != null)
            {
                Name = name;
            }
            else
            {
                Name = "";
            }
            Index = index;
            Hidden = hidden;
            Sortable = sortable;
            Align = align;
            Scene = scene;
        }
    }
}