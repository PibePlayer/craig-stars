using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{

    /// <summary>
    /// A configurable Table control for showing reports as well as simple UI tables
    /// </summary>
    [Tool]
    public class Table : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(Table));

        public const int NoRowSelected = -1;

        public delegate void RowSelected(int rowIndex, int colIndex, Cell cell, object metadata);
        public event RowSelected RowSelectedEvent;

        /// <summary>
        /// The scene that will be used to render each column header
        /// </summary>
        [Export(PropertyHint.File, "*.tscn")]
        public string ColumnHeaderScene
        {
            get => columnHeaderScene; set
            {
                columnHeaderScene = value;
                UpdateTable();
            }
        }
        string columnHeaderScene = "res://src/Client/Controls/Table/ColumnHeader.tscn";

        /// <summary>
        /// The scene that will be used to render each cell by default. Note, the control
        /// class for this must implement the ICellControl interface
        /// This defaults to a simple label
        /// </summary>
        [Export(PropertyHint.File, "*.tscn")]
        public string CellControlScene
        {
            get => cellControlScene; set
            {
                cellControlScene = value;
                UpdateTable();
            }
        }
        string cellControlScene = "res://src/Client/Controls/Table/LabelCell.tscn";

        /// <summary>
        /// True to show the column headers
        /// </summary>
        [Export]
        public bool ShowHeader
        {
            get => showHeader;
            set
            {
                showHeader = value;
                UpdateTable();
            }
        }
        bool showHeader = true;

        /// <summary>
        /// The style of border to put around each cell
        /// </summary>
        [Export]
        public BorderStyle BorderStyle
        {
            get => borderStyle;
            set
            {
                borderStyle = value;
                Update();
            }
        }
        BorderStyle borderStyle = BorderStyle.None;

        /// <summary>
        /// The style of border to put around each cell
        /// </summary>
        [Export]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Update();
            }
        }
        Color borderColor = new Color("6e6e6e");

        /// <summary>
        /// The data for the table
        /// </summary>
        public TableData Data { get; set; } = new TableData();

        /// <summary>
        /// The currently selected row index
        /// </summary>
        public int SelectedRow { get; set; } = NoRowSelected;

        /// <summary>
        /// The GridContainer that we put headers and cells into
        /// </summary>
        GridContainer gridContainer;

        /// <summary>
        /// Keep track of all the control elemeents by row,column so we can easily locate them for drawing
        /// </summary>
        Control[,] cellControls;


        public override void _Ready()
        {
            gridContainer = GetNode<GridContainer>("GridContainer");

            if (Engine.EditorHint)
            {
                // add two columns
                Data.AddColumn("Item");
                Data.AddColumn("Quantity", align: Label.AlignEnum.Right);

                // add some rows of data
                Data.AddRow("Mine", 5);
                Data.AddRow(Colors.Green, "Factory", 10);
            }

            UpdateTable();

            Data.SortEvent += OnDataSorted;
            Data.FilterEvent += OnDataFiltered;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Data.SortEvent -= OnDataSorted;
            Data.FilterEvent -= OnDataFiltered;
        }

        public override void _Draw()
        {
            base._Draw();
            if (gridContainer != null)
            {
                if (BorderStyle == BorderStyle.Cell)
                {
                    // draw a rect around each cell
                    foreach (Node child in gridContainer.GetChildren())
                    {
                        if (child is Control control)
                        {
                            DrawRect(control.GetRect(), BorderColor, false);
                        }
                    }
                }

                if (SelectedRow != NoRowSelected)
                {
                    var firstSelectedCell = cellControls[SelectedRow, 0];
                    DrawRect(new Rect2(gridContainer.RectPosition.x, firstSelectedCell.RectPosition.y, gridContainer.RectSize.x, firstSelectedCell.RectSize.y), new Color(1, 1, 1, .25f));
                }
            }

        }

        /// <summary>
        /// Clear out all the nodes in the table (including the column headers
        /// </summary>
        void ClearTable()
        {
            SelectedRow = NoRowSelected;
            // clear out old rows
            foreach (Node child in gridContainer.GetChildren())
            {
                if (child is ColumnHeader columnHeader)
                {
                    columnHeader.SortEvent -= OnSortEvent;
                }

                if (child is ICellControl cell)
                {
                    cell.MouseEnteredEvent -= OnMouseEntered;
                    cell.MouseExitedEvent -= OnMouseExited;
                    cell.CellSelectedEvent -= OnCellSelected;
                }

                gridContainer.RemoveChild(child);
                child.QueueFree();
            }
        }

        /// <summary>
        /// Clear the table and redraw it with new/updated data
        /// </summary>
        public void UpdateTable()
        {
            if (gridContainer != null && Data != null)
            {
                ClearTable();

                if (Data.VisibleColumns.Count() > 0)
                {
                    gridContainer.Columns = Data.VisibleColumns.Count();

                    AddColumnHeaders();
                    AddRows();
                    Update();
                }
            }
        }

        /// <summary>
        /// Add the column headers to this scene
        /// </summary>
        void AddColumnHeaders()
        {
            if (ShowHeader)
            {
                // add headers
                var scene = GD.Load<PackedScene>(ColumnHeaderScene);
                foreach (var col in Data.VisibleColumns)
                {
                    var columnHeaderInstance = scene.Instance() as Control;
                    if (columnHeaderInstance is IColumnHeader header)
                    {
                        header.Column = col;
                        if (Data.SortColumn == col.Index)
                        {
                            header.SortDirection = Data.SortDirection;
                        }
                        header.SortEvent += OnSortEvent;
                    }
                    gridContainer.AddChild(columnHeaderInstance);
                }
            }
        }

        /// <summary>
        /// Add rows to the table built from the TableData
        /// </summary>
        void AddRows()
        {
            int rowIndex = 0;

            // each column could have a cell override
            var defaultScene = GD.Load<PackedScene>(CellControlScene);
            var sceneForColumn = Data.Columns.Select(col =>
            {
                if (col.Scene != null)
                {
                    return GD.Load<PackedScene>(col.Scene);
                }
                else
                {
                    return defaultScene;
                }

            }).ToArray();
            var rows = Data.Rows.ToList();
            cellControls = new Control[rows.Count, Data.Columns.Count];
            foreach (var row in rows)
            {
                for (int columnIndex = 0; columnIndex < row.Data.Count; columnIndex++)
                {
                    var col = Data.Columns[columnIndex];
                    if (col.Hidden)
                    {
                        continue;
                    }

                    var cell = row.Data[columnIndex];

                    // instantiate an instance of this cell
                    ICellControl node = sceneForColumn[columnIndex].Instance() as ICellControl;
                    node.Column = col;
                    node.Cell = cell;
                    node.Row = row;

                    node.MouseEnteredEvent += OnMouseEntered;
                    node.MouseExitedEvent += OnMouseExited;
                    node.CellSelectedEvent += OnCellSelected;


                    if (node is Control control)
                    {
                        gridContainer.AddChild(control);
                        cellControls[rowIndex, columnIndex] = control;
                    }
                    else
                    {
                        // add a label so our table doesn't go all wonky and crash, though this is definitely an error condition...
                        var label = new Label() { Text = cell.Text };
                        cellControls[rowIndex, columnIndex] = label;
                        gridContainer.AddChild(label);
                        log.Error($"Table Cell {node} ({node.GetType()}) is not a Control and can't be added to the table");
                    }
                }
                rowIndex++;
            }
        }

        #region Table Data Events

        /// <summary>
        /// When the data in the table is sorted, redraw the table
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        void OnDataSorted(int sortColumn, SortDirection sortDirection)
        {
            UpdateTable();
        }

        void OnDataFiltered(string filterText)
        {
            UpdateTable();
        }

        #endregion

        #region Table Control Events

        void OnMouseEntered(ICellControl cell)
        {
        }

        void OnMouseExited(ICellControl cell)
        {
        }

        /// <summary>
        /// When the user clicks a column header sort button, sort the data in the table
        /// (the sort will trigger a sort event which we will capture to update the table)
        /// </summary>
        /// <param name="columnHeader"></param>
        void OnSortEvent(ColumnHeader columnHeader)
        {
            switch (columnHeader.SortDirection)
            {
                case SortDirection.Ascending:
                    columnHeader.SortDirection = SortDirection.Descending;
                    break;
                case SortDirection.Descending:
                    columnHeader.SortDirection = SortDirection.Ascending;
                    break;
            }

            Data.Sort(columnHeader.Column, columnHeader.SortDirection);
        }


        /// <summary>
        /// When the user selects a cell, update the SelectedRow and trigger
        /// a draw
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="event"></param>
        void OnCellSelected(ICellControl cell, InputEvent @event)
        {
            log.Debug($"Clicked {cell.Cell.Text}, Metadata: {cell.Row.Metadata}");
            var newSelectedRow = cell.Row.Index;
            if (newSelectedRow != SelectedRow)
            {
                log.Debug($"SelectedRow Updated: {SelectedRow}");
            }

            SelectedRow = cell.Row.Index;
            RowSelectedEvent?.Invoke(SelectedRow, cell.Column.Index, cell.Cell, cell.Row.Metadata);
            Update();
        }
        #endregion

    }
}