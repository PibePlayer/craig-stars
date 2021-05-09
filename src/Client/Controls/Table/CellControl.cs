using Godot;
using System;

namespace CraigStars
{
    public abstract class CellControl : CellControl<object>
    {

    }

    public abstract class CellControl<T> : Control, ICellControl<T> where T : class
    {
        public event Action<ICellControl<T>> MouseEnteredEvent;
        public event Action<ICellControl<T>> MouseExitedEvent;
        public event Action<ICellControl<T>, InputEvent> CellSelectedEvent;
        public event Action<ICellControl<T>, InputEvent> CellActivatedEvent;

        public Column Column { get; set; }
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

        protected abstract void UpdateCell();
    }
}