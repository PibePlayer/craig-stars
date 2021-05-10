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
        /// Override the scene path for this cell
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