using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// This is a generic table class that supports adding columns and rows to a table
    /// </summary>
    public abstract class Table<T> : VBoxContainer
    {
        public enum SortDirection { Ascending, Descending };
        protected Tree tree;
        protected TreeItem root;
        protected LineEdit searchLineEdit;
        protected int sortColumn = 0;
        protected SortDirection sortDirection;

        public readonly struct ColumnData
        {
            public readonly string text;
            public readonly IComparable value;
            public readonly Guid guid;

            public ColumnData(string text, IComparable value = null, Guid? guid = null)
            {
                this.text = text;
                if (value == null)
                {
                    this.value = text;
                }
                else
                {
                    this.value = value;
                }
                if (guid.HasValue)
                {
                    this.guid = guid.Value;
                }
                else
                {
                    this.guid = Guid.Empty;
                }
            }

            public ColumnData(int value)
            {
                this.text = value.ToString();
                this.value = value;
                this.guid = Guid.Empty;
            }
        }

        protected List<string> Columns { get; set; } = new List<string>();

        public override void _Ready()
        {
            tree = GetNode<Tree>("Tree");
            searchLineEdit = GetNode<LineEdit>("SearchLineEdit");

            // our tree has visible columns
            tree.SetColumnTitlesVisible(true);

            searchLineEdit.Connect("text_changed", this, nameof(OnSearchLineEditTextChanged));
            tree.Connect("column_title_pressed", this, nameof(OnColumnTitlePressed));
            tree.Connect("cell_selected", this, nameof(OnCellSelected));
            Connect("visibility_changed", this, nameof(OnVisible));
        }

        protected void OnVisible()
        {
            ClearTree();
            AddHeader();
            AddRows();
        }

        void OnColumnTitlePressed(int column)
        {
            if (sortColumn == column)
            {
                sortDirection = sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
            }
            else
            {
                sortDirection = SortDirection.Ascending;
                sortColumn = column;
            }

            ClearTree();
            AddHeader();
            AddRows(searchLineEdit.Text, sortColumn, sortDirection);
        }

        void OnCellSelected()
        {
            int col = tree.GetSelectedColumn();
            var row = tree.GetSelected();
            if (col != -1 && row != null)
            {
                ItemSelected(row, col);
            }
        }

        void OnSearchLineEditTextChanged(string newText)
        {
            ClearTree();
            AddHeader();
            AddRows(newText, sortColumn, sortDirection);
        }

        /// <summary>
        /// You can't hide items, so we clear the tree when we change items
        /// </summary>
        protected void ClearTree()
        {
            tree.Clear();
            root = tree.CreateItem();
        }

        protected virtual void AddHeader()
        {
            tree.Columns = Columns.Count;
            Columns.Each((col, index) =>
            {
                tree.SetColumnTitle(index, col);
            });

            tree.SetColumnTitle(0, $"{Columns[0]}");
        }

        /// <summary>
        /// Create column data for an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract ColumnData[] CreateColumnData(T item);

        /// <summary>
        /// Get an enumerable of items in the table
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<T> GetItems();

        /// <summary>
        /// Create a new Comparer based on the sortColumn, for sorting rows
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        protected virtual IComparer<ColumnData[]> CreateComparer(int sortColumn)
        {
            return Comparer<ColumnData[]>.Create((row1, row2) =>
            {
                return row1[sortColumn].value.CompareTo(row2[sortColumn].value);
            });
        }

        /// <summary>
        /// Get a list of ColumnDatas that represent the rows
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        protected List<ColumnData[]> GetRows(string filter = "", int sortColumn = 0, SortDirection sortDirection = SortDirection.Ascending)
        {
            var items = GetItems();

            var comparer = CreateComparer(sortColumn);

            IEnumerable<ColumnData[]> sortedRows;
            if (sortDirection == SortDirection.Ascending)
            {
                sortedRows = items.Select((item) => CreateColumnData(item)).OrderBy((p) => p, comparer);
            }
            else
            {
                sortedRows = items.Select((item) => CreateColumnData(item)).OrderByDescending((p) => p, comparer);
            }

            return sortedRows.ToList();
        }

        /// <summary>
        /// Add rows to the table. This sorts, filters, etc.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        protected virtual void AddRows(string filter = "", int sortColumn = 0, SortDirection sortDirection = SortDirection.Ascending)
        {
            foreach (var row in GetRows(filter, sortColumn, sortDirection))
            {
                var item = tree.CreateItem(root);
                item.SetMetadata(0, row[0].guid.ToString());

                for (int col = 0; col < Columns.Count; col++)
                {
                    item.SetText(col, row[col].text);
                }
            }
        }

        protected virtual void ItemSelected(TreeItem row, int col)
        {
            // Child classes will fill this out if they want to respond to clicks
        }


    }
}