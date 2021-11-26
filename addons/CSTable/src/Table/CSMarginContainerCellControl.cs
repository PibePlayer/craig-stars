using Godot;
using System;

namespace CraigStarsTable
{
    public abstract class CSMarginContainerCellControl : CSMarginContainerCellControl<object>
    {
    }

    /// <summary>
    /// This abstract cell control implements ICSCellControl and extends MarginContainer to allow MarginContainer behavior for a cell
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CSMarginContainerCellControl<T> : MarginContainer, ICSCellControl<T> where T : class
    {
        public event Action<ICSCellControl<T>> MouseEnteredEvent;
        public event Action<ICSCellControl<T>> MouseExitedEvent;
        public event Action<ICSCellControl<T>, InputEvent> CellSelectedEvent;
        public event Action<ICSCellControl<T>, InputEvent> CellActivatedEvent;

        public Column<T> Column { get; set; }
        public Cell Cell { get; set; }
        public Row<T> Row { get; set; }

        public CSMarginContainerCellControl() : base()
        {
        }

        public CSMarginContainerCellControl(Column<T> col, Cell cell, Row<T> row) : this()
        {
            Column = col;
            Cell = cell;
            Row = row;
        }

        public override void _Ready()
        {
            Connect("gui_input", this, nameof(OnGuiInput));
            Connect("mouse_entered", this, nameof(OnMouseEntered));
            Connect("mouse_exited", this, nameof(OnMouseExited));
        }

        void OnGuiInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                if (@event is InputEventMouseButton mouseButton && mouseButton.Doubleclick)
                {
                    CellActivatedEvent?.Invoke(this, @event);
                }
                else
                {
                    CellSelectedEvent?.Invoke(this, @event);
                }
            }
        }

        void OnMouseEntered()
        {
            MouseEnteredEvent?.Invoke(this);
        }

        void OnMouseExited()
        {
            MouseEnteredEvent?.Invoke(this);
        }

        protected void PublishCellSelectedEvent()
        {
            CellSelectedEvent?.Invoke(this, null);
        }

        public virtual void UpdateCell()
        {

        }

    }
}