
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStarsTable
{
    public class TableData : TableData<object>
    {

    }

    /// <summary>
    /// Data for a table. This is used to render the table headers, rows and cells. The data
    /// is what drives the table
    /// </summary>
    public class TableData<T> where T : class
    {
        public delegate void SortAction(int sortColumn, SortDirection sortDirection);
        public delegate void FilterAction(string filterText);

        /// <summary>
        /// Triggered when the rows are sorted
        /// </summary>
        public event SortAction SortEvent;

        /// <summary>
        /// Rows have been filtered by some value
        /// </summary>
        public event FilterAction FilterEvent;

        public List<Column<T>> Columns { get; set; } = new List<Column<T>>();
        public List<Row<T>> SourceRows { get; set; } = new List<Row<T>>();
        public int SortColumn { get; set; }
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
        public string FilterText { get; set; } = "";

        /// <summary>
        /// Clear all data in the table
        /// </summary>
        public void Clear()
        {
            SourceRows.Clear();
            Columns.Clear();
            SortColumn = 0;
            SortDirection = SortDirection.Ascending;
        }

        /// <summary>
        /// Clear just the row data (leaving the columns as is)
        /// </summary>
        public void ClearRows()
        {
            SourceRows.Clear();
        }

        public void AddColumn(Column<T> column)
        {
            Columns.Add(column);
        }

        public void AddColumn(string name, bool sortable = true, bool hidden = false, Label.AlignEnum align = Label.AlignEnum.Left)
        {
            Columns.Add(new Column<T>(name, Columns.Count, sortable, hidden, align));
        }

        public void AddColumns(IEnumerable<Column<T>> columns)
        {
            foreach (var column in columns)
            {
                Columns.Add(column);
            }
        }

        /// <summary>
        /// Add columns, either as a string or Column object
        /// </summary>
        /// <param name="columns"></param>
        public void AddColumns(params object[] columns)
        {
            foreach (var column in columns)
            {
                if (column is string name)
                {
                    Columns.Add(new Column<T>(name, Columns.Count));
                }
                else if (column is Column<T> col)
                {
                    col.Index = Columns.Count;
                    Columns.Add(col);
                }
                else
                {
                    Columns.Add(new Column<T>(column.ToString(), Columns.Count));
                }
            }
        }

        public void AddRow(params object[] cellData)
        {
            AddRowAdvanced(metadata: null, color: Colors.White, italic: false, cellData);
        }

        public void AddRowWithMetadata(List<Cell> cells, T metadata)
        {
            SourceRows.Add(new Row<T>(SourceRows.Count, cells, metadata));
        }

        public void AddRowAdvanced(T metadata, Color color, bool italic, params object[] cellData)
        {
            var cells = new List<Cell>();
            foreach (var cell in cellData)
            {
                if (cell is string s)
                {
                    cells.Add(new Cell(s));
                }
                else if (cell is int i)
                {
                    cells.Add(new Cell(i));
                }
                else if (cell is bool b)
                {
                    cells.Add(new Cell(b.ToString(), metadata: b));
                }
                else if (cell is Cell cellObj)
                {
                    cells.Add(cellObj);
                }
                else
                {
                    throw new InvalidCastException("Table Cells cannot be data of type " + cell.GetType());
                }
            }
            SourceRows.Add(new Row<T>(SourceRows.Count, cells, metadata: metadata, color: color, italic: italic));
        }

        /// <summary>
        /// Create a new Comparer based on the sortColumn, for sorting rows
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        protected virtual IComparer<List<Cell>> CreateComparer(int sortColumn)
        {
            return Comparer<List<Cell>>.Create((row1, row2) =>
            {
                return row1[sortColumn].Value.CompareTo(row2[sortColumn].Value);
            });
        }

        public void Filter(String filterText)
        {
            FilterText = filterText;

            FilterEvent?.Invoke(filterText);
        }

        public void Sort(Column<T> column, SortDirection sortDirection)
        {
            SortColumn = column.Index;
            SortDirection = sortDirection;

            SortEvent?.Invoke(SortColumn, SortDirection);
        }

        public IEnumerable<Column<T>> VisibleColumns { get => Columns.Where(col => !col.Hidden); }

        /// <summary>
        /// Get an enumerable of our rows sorted and filtered
        /// </summary>
        public IEnumerable<Row<T>> Rows
        {
            get
            {
                IEnumerable<Row<T>> rows = SourceRows;
                var comparer = CreateComparer(SortColumn);

                if (FilterText != "")
                {
                    var lowercaseFilterText = FilterText.ToLower();
                    // mark all rows as not visible. we will turn them visible if they exist after filtering
                    rows = rows
                        .Select((row) =>
                        {
                            row.Visible = row.Data.Any(cell => $"{cell.Value}".ToLower().Contains(lowercaseFilterText));
                            return row;
                        });
                }
                else
                {
                    // make all rows visible if we have no filter text
                    rows = rows
                        .Select((row) =>
                        {
                            row.Visible = true;
                            return row;
                        });
                }

                if (SortDirection == SortDirection.Ascending)
                {
                    rows = rows.OrderBy(row => row.Data, comparer);
                }
                else
                {
                    rows = rows.OrderByDescending(row => row.Data, comparer);
                }

                // add an index to each row
                int index = 0;
                rows = rows.Select((row) =>
                {
                    row.SortIndex = index++;
                    return row;
                });

                return rows;
            }
        }
    }

}