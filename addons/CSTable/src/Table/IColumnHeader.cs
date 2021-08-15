using Godot;
using System;

namespace CraigStarsTable
{
    /// <summary>
    /// Control interface for a column header
    /// </summary>
    public interface IColumnHeader<T> where T: class
    {
        /// <summary>
        /// This event is invoked when the user clicks on the sort icon on the column
        /// </summary>
        event Action<ColumnHeader<T>> SortEvent;

        /// <summary>
        /// The Column object from the TableData for the column for this header
        /// </summary>
        Column<T> Column { get; set; }

        /// <summary>
        /// The current SortDirection for this column
        /// </summary>
        SortDirection SortDirection { get; set; }
    }

}