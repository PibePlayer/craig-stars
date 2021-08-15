using System;
using Godot;

namespace CraigStarsTable
{
    public class Column : Column<object>
    {
        public Column(
            string name,
            int index = 0,
            bool sortable = true,
            bool hidden = false,
            Label.AlignEnum align = Label.AlignEnum.Left,
            string scene = null,
            string script = null) : base(name, index, sortable, hidden, align, scene, script)
        {

        }
    }

    /// <summary>
    /// Represents a column for a table
    /// </summary>
    public class Column<T> where T : class
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

        public Func<Column<T>, Cell, Row<T>, ICSCellControl<T>> OnCreateCellControl { get; set; }

        public ICSCellControl<T> CreateCell(Column<T> col, Cell cell, Row<T> row)
        {
            return OnCreateCellControl?.Invoke(col, cell, row);
        }

        public Column()
        {
        }

        public Column(
            string name,
            int index = 0,
            bool sortable = true,
            bool hidden = false,
            Label.AlignEnum align = Label.AlignEnum.Left,
            string scene = null,
            string script = null)
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
            Script = script;
        }
    }
}