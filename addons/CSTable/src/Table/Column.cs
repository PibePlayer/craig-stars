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
            Label.AlignEnum align = Label.AlignEnum.Left) : base(name, index, sortable, hidden, align)
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
        /// If set, use this scene to render cells instead of the table default
        /// </summary>
        public string CellScene { get; set; }

        /// <summary>
        /// Optional property with function that provides cell instances. this allows clients to override
        /// behavior for creating cells for a single column
        /// </summary>
        public virtual Func<Column<T>, Cell, Row<T>, ICSCellControl<T>> CellProvider { get; set; }

        public ICSCellControl<T> CreateCell(Column<T> col, Cell cell, Row<T> row)
        {
            return CellProvider?.Invoke(col, cell, row);
        }

        public Column()
        {
        }

        public Column(
            string name,
            int index = 0,
            bool sortable = true,
            bool hidden = false,
            Label.AlignEnum align = Label.AlignEnum.Left)
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
        }
    }
}