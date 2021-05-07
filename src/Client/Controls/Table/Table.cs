using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public delegate void RowAction(int rowIndex, int colIndex, Cell cell, object metadata);
        public event RowAction RowSelectedEvent;
        public event RowAction RowActivatedEvent;

        /// <summary>
        /// The scene that will be used to render each column header
        /// </summary>
        [Export(PropertyHint.File, "*.tscn")]
        public string ColumnHeaderScene
        {
            get => columnHeaderScene; set
            {
                columnHeaderScene = value;
                // var _ = ResetTable();
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
        BorderStyle borderStyle = BorderStyle.Cell;

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
        /// The style of border to put around each cell
        /// </summary>
        [Export]
        public Color SelectColor
        {
            get => selectColor;
            set
            {
                selectColor = value;
                Update();
            }
        }
        Color selectColor = new Color(.5f, .5f, .5f, .5f);

        /// <summary>
        /// The data for the table
        /// </summary>
        public TableData Data { get; set; } = new TableData();

        /// <summary>
        /// The currently selected row index
        /// </summary>
        public int SelectedRow { get; set; } = NoRowSelected;

        /// <summary>
        /// Note, very experimental (i.e. it errors if you have more than one table updating at a time)
        /// </summary>
        public bool Multithreaded { get; set; } = false;

        /// <summary>
        /// Task for resetting the table in a thread (instead of on the main thread)
        /// </summary>
        Task resetTableTask;

        /// <summary>
        /// The GridContainer that we put headers and cells into
        /// </summary>
        GridContainer gridContainer;

        /// <summary>
        /// Keep track of all the control elemeents by row,column so we can easily locate them for drawing
        /// </summary>
        Control[,] cellControls = new Control[0, 0];

        /// <summary>
        /// Keep track of column headers to toggle sort direction icons
        /// </summary>
        List<IColumnHeader> columnHeaders = new List<IColumnHeader>();


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

            // var _ = ResetTable();

            Connect("resized", this, nameof(OnResized));
            Connect("visibility_changed", this, nameof(OnVisible));
            Connect("item_rect_changed", this, nameof(OnResized));

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
            if (gridContainer != null)
            {
                if (BorderStyle == BorderStyle.Cell)
                {
                    // draw a rect around each cell
                    foreach (Node child in gridContainer.GetChildren())
                    {
                        if (child is Control control && control.Visible)
                        {
                            DrawRect(control.GetRect(), BorderColor, false);
                        }
                    }
                }

                if (SelectedRow != NoRowSelected && cellControls.Length > 0)
                {
                    var numVisibleColumns = Data.VisibleColumns.Count();
                    if (numVisibleColumns > 0 && numVisibleColumns < gridContainer.GetChildCount())
                    {
                        // draw a rectangle around the first selected cell to the end of the column x
                        var firstSelectedCell = cellControls[SelectedRow, 0];
                        var lastSelectedColumnHeader = gridContainer.GetChild(numVisibleColumns - 1) as Control;
                        if (firstSelectedCell != null)
                        {
                            DrawRect(new Rect2(gridContainer.RectPosition.x, firstSelectedCell.RectPosition.y, lastSelectedColumnHeader.RectPosition.x + lastSelectedColumnHeader.RectSize.x, firstSelectedCell.RectSize.y), SelectColor);
                        }

                    }
                }
            }

        }

        /// <summary>
        /// Clear out all the nodes in the table (including the column headers
        /// </summary>
        public void ClearTable()
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
                    cell.CellActivatedEvent -= OnCellActivated;
                }

                child.QueueFree();
            }

            columnHeaders.Clear();
            cellControls = new Control[0, 0];
        }

        /// <summary>
        /// Clear the table and redraw it with new/updated data
        /// </summary>
        public async Task ResetTable()
        {
            if (resetTableTask != null)
            {
                await resetTableTask;
            }
            log.Debug("Updating table");

            if (gridContainer != null && Data != null)
            {
                RemoveChild(gridContainer);

                // after container removed, setup the table on a separate thread
                CallDeferred(nameof(ResetTableAsync));
            }
        }

        async void ResetTableAsync()
        {
            if (resetTableTask != null)
            {
                await resetTableTask;
            }

            Action generateTable = () =>
            {
                ClearTable();

                if (Data.VisibleColumns.Count() > 0)
                {
                    gridContainer.Columns = Data.VisibleColumns.Count();

                    AddColumnHeaders();
                    AddRows();
                    SelectedRow = 0;
                    // Update();
                }

            };

            if (Multithreaded)
            {
                resetTableTask = Task.Factory.StartNew(generateTable);
                await resetTableTask;
            }
            else
            {
                generateTable();
            }

            CallDeferred("add_child", gridContainer);
            log.Debug("Done updating table");
            resetTableTask = null;
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
                        columnHeaders.Add(header);
                    }
                    columnHeaderInstance.Connect("item_rect_changed", this, nameof(OnResized));
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
            log.Debug("Adding rows to table");

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
            var rows = Data.SourceRows;
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
                    node.CellActivatedEvent += OnCellActivated;


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

            log.Debug("Done adding rows to table");
        }

        public void UpdateRows()
        {
            log.Debug("Updating rows in table");

            var rows = Data.Rows.ToList();
            var numVisibleColumns = Data.VisibleColumns.Count();
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                for (int columnIndex = 0, nonHiddenColumnIndex = 0; columnIndex < Data.Columns.Count; columnIndex++)
                {
                    var col = Data.Columns[columnIndex];
                    if (col.Hidden)
                    {
                        continue;
                    }

                    // get the control for this cell from our original cellControls list
                    var node = cellControls[row.Index, columnIndex];
                    node.Visible = row.Visible;

                    // move this cell to its new location
                    var newIndex = (row.SortIndex + 1) * numVisibleColumns + nonHiddenColumnIndex;

                    gridContainer.MoveChild(node, newIndex);
                    nonHiddenColumnIndex++;
                }
            }

            log.Debug("Done updating rows to table");
            if (rows.Count > 0)
            {
                SelectedRow = rows[0].Index;
            }
            else
            {
                SelectedRow = NoRowSelected;
            }
            Update();
        }

        #region Table Data Events

        /// <summary>
        /// When the data in the table is sorted, redraw the table
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        void OnDataSorted(int sortColumn, SortDirection sortDirection)
        {
            UpdateRows();
        }

        void OnDataFiltered(string filterText)
        {
            UpdateRows();
        }

        #endregion

        #region Table Control Events

        void OnResized()
        {
            // redraw on any resize event so our grid lines look right
            Update();
        }

        void OnVisible()
        {
            if (Visible)
            {
                var firstRow = Data.Rows.FirstOrDefault();
                if (firstRow != null)
                {
                    SelectedRow = firstRow.SortIndex;
                }
                else
                {
                    SelectedRow = NoRowSelected;
                }
                Update();
            }
        }

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
            var sortDirection = columnHeader.SortDirection;
            // reset all to none so we only have one column sort header active at a time
            columnHeaders.ForEach(ch => ch.SortDirection = SortDirection.None);
            switch (sortDirection)
            {
                case SortDirection.None:
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
                log.Debug($"SelectedRow Updated: {SelectedRow} to {newSelectedRow}");
            }

            SelectedRow = newSelectedRow;
            RowSelectedEvent?.Invoke(SelectedRow, cell.Column.Index, cell.Cell, cell.Row.Metadata);
            Update();
        }

        void OnCellActivated(ICellControl cell, InputEvent @event)
        {
            log.Debug($"Activated {cell.Cell.Text}, Metadata: {cell.Row.Metadata}");
            var activatedRowIndex = cell.Row.Index;
            RowActivatedEvent?.Invoke(activatedRowIndex, cell.Column.Index, cell.Cell, cell.Row.Metadata);
        }


        #endregion

    }
}