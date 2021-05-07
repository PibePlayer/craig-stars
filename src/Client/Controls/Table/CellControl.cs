using Godot;
using System;

namespace CraigStars
{
    public abstract class CellControl : Control, ICellControl
    {
        public event Action<ICellControl> MouseEnteredEvent;
        public event Action<ICellControl> MouseExitedEvent;
        public event Action<ICellControl, InputEvent> CellSelectedEvent;
        public event Action<ICellControl, InputEvent> CellActivatedEvent;

        public Column Column { get; set; }
        public Cell Cell { get; set; }
        public Row Row { get; set; }


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