using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace CraigStarsTable
{
    public class Table : Table<object>
    {

    }


    /// <summary>
    /// A configurable Table control for showing reports as well as simple UI tables
    /// </summary>
    [Tool]
    public class Table<T> : Control where T : class
    {
        public const int NoRowSelected = -1;

        public delegate void RowAction(int rowIndex, int colIndex, Cell cell, T metadata);
        public event RowAction RowSelectedEvent;
        public event RowAction RowActivatedEvent;

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
        public TableData<T> Data { get; set; } = new TableData<T>();

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
        Control[,] cellControls = new Control[0, 0];

        /// <summary>
        /// Keep track of column headers to toggle sort direction icons
        /// </summary>
        List<IColumnHeader<T>> columnHeaders = new List<IColumnHeader<T>>();

        /// <summary>
        /// Property with function that provides column header instances
        /// </summary>
        public virtual Func<Column<T>, Control> ColumnHeaderProvider { get; set; } = (col) => CSTableNodePool.Get<ColumnHeader>(CSTableResourceLoader.Instance.DefaultColumnHeaderScene);

        /// <summary>
        /// Property with function that provides cell instances
        /// </summary>
        public virtual Func<Column<T>, Cell, Row<T>, ICSCellControl<T>> CellProvider { get; set; } = (col, cell, row) =>
        {
            CSLabelCell<T> labelCell = CSTableNodePool.Get<CSLabelCell<T>>();
            labelCell.Column = col;
            labelCell.Cell = cell;
            labelCell.Row = row;
            return labelCell;
        };

        // System.Threading.Mutex tableUpdateMutex;

        public Table() : base()
        {
            SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
            SizeFlagsVertical = (int)SizeFlags.ExpandFill;
        }

        public override void _Ready()
        {

            AddConstantOverride("margin_right", 0);
            AddConstantOverride("margin_top", 0);
            AddConstantOverride("margin_left", 0);
            AddConstantOverride("margin_bottom", 0);

            foreach (Node child in GetChildren())
            {
                child.QueueFree();
            }

            gridContainer = new GridContainer()
            {
                SizeFlagsHorizontal = (int)SizeFlags.ExpandFill,
                SizeFlagsVertical = (int)SizeFlags.ExpandFill,
            };
            gridContainer.AddConstantOverride("vseparation", 0);
            gridContainer.AddConstantOverride("hseparation", 0);

            var panel = new Panel() { ShowBehindParent = true };
            panel.AddStyleboxOverride("panel", new StyleBoxFlat()
            {
                BgColor = new Color("2d2b33")
            });

            AddChild(panel);
            AddChild(gridContainer);

            if (Engine.EditorHint)
            {
                Data.Clear();
                // add two columns
                Data.AddColumn("Item");
                Data.AddColumn("Quantity", align: Label.AlignEnum.Right);

                // add some rows of data
                Data.AddRow("Mine", 5);
                Data.AddRowAdvanced(metadata: null, color: Colors.Green, italic: false, "Factory", 10);
                Data.AddRowAdvanced(metadata: null, color: Colors.Gray, italic: true, "Auto Factory", 15);
            }

            // var _ = ResetTable();

            Connect("resized", this, nameof(OnResized));
            Connect("visibility_changed", this, nameof(OnVisible));
            Connect("item_rect_changed", this, nameof(OnResized));

            Data.SortEvent += OnDataSorted;
            Data.FilterEvent += OnDataFiltered;

            // tableUpdateMutex = new System.Threading.Mutex(true, GetPath());

            ResetTable();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                Data.SortEvent -= OnDataSorted;
                Data.FilterEvent -= OnDataFiltered;
            }
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
                    if (numVisibleColumns > 0 && numVisibleColumns <= gridContainer.GetChildCount() && SelectedRow < cellControls.GetLength(0))
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
                if (child is ColumnHeader<T> columnHeader)
                {
                    columnHeader.SortEvent -= OnSortEvent;
                    if (columnHeader is ColumnHeader poolResource)
                    {
                        columnHeader.Disconnect("item_rect_changed", this, nameof(OnResized));
                        CSTableNodePool.Return<ColumnHeader>(poolResource);
                    }
                    else
                    {
                        child.QueueFree();
                    }
                }
                else if (child is ICSCellControl<T> cell)
                {
                    cell.MouseEnteredEvent -= OnMouseEntered;
                    cell.MouseExitedEvent -= OnMouseExited;
                    cell.CellSelectedEvent -= OnCellSelected;
                    cell.CellActivatedEvent -= OnCellActivated;
                    if (child is CSLabelCell labelCell)
                    {
                        CSTableNodePool.Return<CSLabelCell>(labelCell);
                    }
                    else
                    {
                        child.QueueFree();
                    }
                }
                else
                {
                    child.QueueFree();
                }

            }

            columnHeaders.Clear();
            cellControls = new Control[0, 0];
        }

        /// <summary>
        /// Clear out all the rows (but leave the header)
        /// </summary>
        public void ClearRows()
        {
            // clear out old rows
            foreach (Node child in gridContainer.GetChildren())
            {
                if (child is ColumnHeader)
                {
                    continue;
                }

                if (child is ICSCellControl<T> cell)
                {
                    cell.MouseEnteredEvent -= OnMouseEntered;
                    cell.MouseExitedEvent -= OnMouseExited;
                    cell.CellSelectedEvent -= OnCellSelected;
                    cell.CellActivatedEvent -= OnCellActivated;
                }

                child.QueueFree();
            }

            cellControls = new Control[0, 0];
        }

        /// <summary>
        /// Clear the table and redraw it with new/updated data
        /// </summary>
        public void ResetTable()
        {
            if (gridContainer != null && Data != null)
            {
                using (var mutex = new System.Threading.Mutex())
                {
                    mutex.WaitOne();

                    try
                    {
                        ClearTable();

                        if (Data.VisibleColumns.Count() > 0)
                        {
                            gridContainer.Columns = Data.VisibleColumns.Count();

                            AddColumnHeaders();
                            AddRows();
                            SelectedRow = 0;
                            var firstRow = Data.Rows.FirstOrDefault();
                            if (firstRow != null)
                            {
                                RowSelectedEvent?.Invoke(0, 0, firstRow.Data[0], firstRow.Metadata);
                            }

                            Update();
                        }
                    }
                    catch (Exception e)
                    {
                        // things inside a mutex won't print otherwise, just crash
                        GD.PrintErr($"{e.ToString()}\n{e.StackTrace}");
                        throw e;
                    }

                    mutex.ReleaseMutex();
                }

            }
        }


        /// <summary>
        /// Clear the table and redraw it with new/updated data
        /// </summary>
        public async Task ResetRows()
        {
            if (gridContainer != null && Data != null)
            {
                using (var mutex = new System.Threading.Mutex())
                {
                    mutex.WaitOne();

                    try
                    {
                        RemoveChild(gridContainer);
                        await Task.Run(() =>
                        {
                            try
                            {
                                ClearRows();

                                if (Data.VisibleColumns.Count() > 0)
                                {
                                    AddRows();
                                    // if we are selecting a row that is 
                                    if (SelectedRow > cellControls.GetLength(0))
                                    {
                                        SelectedRow = NoRowSelected;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                GD.PrintErr("Failed to add table rows: ", e);
                                throw e;
                            }
                        });

                        AddChild(gridContainer);

                        if (Data.VisibleColumns.Count() > 0)
                        {
                            Update();

                            if (SelectedRow != NoRowSelected && cellControls.GetLength(0) > SelectedRow)
                            {
                                if (cellControls[SelectedRow, 0] is ICSCellControl<T> cell)
                                {
                                    RowSelectedEvent?.Invoke(SelectedRow, 0, cell.Cell, cell.Row.Metadata);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // things inside a mutex won't print otherwise, just crash
                        GD.PrintErr($"{e.ToString()}\n{e.StackTrace}");
                        throw e;
                    }
                    mutex.ReleaseMutex();
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
                foreach (var col in Data.VisibleColumns)
                {
                    var columnHeaderInstance = ColumnHeaderProvider?.Invoke(col);
                    if (columnHeaderInstance is IColumnHeader<T> header)
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

            // copy the rows so we don't have an issue if the source is modified
            var rows = Data.SourceRows.ToList();
            var cols = Data.Columns.ToList();
            cellControls = new Control[rows.Count, cols.Count];
            foreach (var row in rows)
            {
                for (int columnIndex = 0; columnIndex < row.Data.Count; columnIndex++)
                {
                    var col = cols[columnIndex];
                    if (col.Hidden)
                    {
                        continue;
                    }

                    var cell = row.Data[columnIndex];

                    // instantiate an instance of this cell
                    ICSCellControl<T> node = col.CreateCell(col, cell, row);
                    if (node == null)
                    {
                        node = CellProvider?.Invoke(col, cell, row);
                    }

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
                        GD.PrintErr($"Table Cell {node} ({node.GetType()}) is not a Control and can't be added to the table");
                    }
                }
                rowIndex++;
            }

        }

        public void UpdateRows()
        {
            var rows = Data.Rows.ToList();
            var cols = Data.Columns.ToList();
            var numVisibleColumns = Data.VisibleColumns.Count();
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                for (int columnIndex = 0, nonHiddenColumnIndex = 0; columnIndex < cols.Count; columnIndex++)
                {
                    var col = cols[columnIndex];
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
                    if (node is ICSCellControl<T> cellControl)
                    {
                        cellControl.Row.Metadata = row.Metadata;
                        cellControl.UpdateCell();
                    }
                    nonHiddenColumnIndex++;
                }
            }

            if (rows.Count > 0)
            {
                SelectedRow = rows[0].Index;
                if (rows[0].Data.Count > 0)
                {
                    RowSelectedEvent.Invoke(0, 0, rows[0].Data[0], rows[0].Metadata);
                }
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
            if (IsVisibleInTree())
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

        void OnMouseEntered(ICSCellControl<T> cell)
        {
        }

        void OnMouseExited(ICSCellControl<T> cell)
        {
        }

        /// <summary>
        /// When the user clicks a column header sort button, sort the data in the table
        /// (the sort will trigger a sort event which we will capture to update the table)
        /// </summary>
        /// <param name="columnHeader"></param>
        void OnSortEvent(ColumnHeader<T> columnHeader)
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
        void OnCellSelected(ICSCellControl<T> cell, InputEvent @event)
        {
            var newSelectedRow = cell.Row.Index;

            SelectedRow = newSelectedRow;
            RowSelectedEvent?.Invoke(SelectedRow, cell.Column.Index, cell.Cell, cell.Row.Metadata);
            Update();
        }

        void OnCellActivated(ICSCellControl<T> cell, InputEvent @event)
        {
            var activatedRowIndex = cell.Row.Index;
            RowActivatedEvent?.Invoke(activatedRowIndex, cell.Column.Index, cell.Cell, cell.Row.Metadata);
        }

        #endregion

    }
}