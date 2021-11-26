using Godot;
using System;

namespace CraigStarsTable
{
    public abstract class CSCellControl : CSCellControl<object>
    {

    }

    public abstract class CSCellControl<T> : Control, ICSCellControl<T> where T : class
    {
        public event Action<ICSCellControl<T>> MouseEnteredEvent;
        public event Action<ICSCellControl<T>> MouseExitedEvent;
        public event Action<ICSCellControl<T>, InputEvent> CellSelectedEvent;
        public event Action<ICSCellControl<T>, InputEvent> CellActivatedEvent;

        public Column<T> Column { get; set; }
        public Cell Cell { get; set; }
        public Row<T> Row { get; set; }


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

        public abstract void UpdateCell();
    }
}