using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// This is a generic table class that supports adding columns and rows to a table
    /// </summary>
    public abstract class ReportTable<T> : VBoxContainer where T : class
    {
        static CSLog log = LogProvider.GetLogger(typeof(ReportTable<T>));

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        protected CSTable table;
        protected LineEdit searchLineEdit;

        ToolButton showOwnedButton;
        ToolButton showAllButton;

        protected List<string> Columns { get; set; } = new List<string>();

        public override void _Ready()
        {
            table = GetNode<CSTable>("ScrollContainer/CSTable");
            searchLineEdit = (LineEdit)FindNode("SearchLineEdit");

            showOwnedButton = (ToolButton)FindNode("ShowOwnedButton");
            showAllButton = (ToolButton)FindNode("ShowAllButton");

            // our tree has visible columns
            table.ShowHeader = true;

            searchLineEdit.Connect("text_changed", this, nameof(OnSearchLineEditTextChanged));
            showOwnedButton.Connect("pressed", this, nameof(OnShowOwnedPressed));
            showAllButton.Connect("pressed", this, nameof(OnShowAllPressed));

            table.RowSelectedEvent += OnRowSelected;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                table.RowSelectedEvent -= OnRowSelected;
            }
        }

        void OnRowSelected(int rowIndex, int colIndex, Cell cell, object metadata)
        {
            if (metadata == null || metadata is T)
            {
                ItemSelected(metadata as T, cell);
            }
        }

        public void ResetTableData()
        {
            log.Debug("Resetting table data");
            table.Data.Clear();

            AddColumns();
            foreach (T item in GetItems())
            {
                table.Data.AddRowWithMetadata(CreateCellsForItem(item), item);
            }
            table.ResetTable();
        }

        protected virtual void OnShowOwnedPressed()
        {
            ResetTableData();
        }

        protected virtual void OnShowAllPressed()
        {
            ResetTableData();

        }

        void OnSearchLineEditTextChanged(string newText)
        {
            table.Data.Filter(newText);
        }

        /// <summary>
        /// Must be overridden to add columns to the table using table.Data.AddColumns()
        /// </summary>
        protected abstract void AddColumns();

        /// <summary>
        /// Create column data for an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract List<Cell> CreateCellsForItem(T item);

        /// <summary>
        /// Get an enumerable of items in the table
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<T> GetItems();

        protected virtual void ItemSelected(T item, Cell cell)
        {

        }


    }
}