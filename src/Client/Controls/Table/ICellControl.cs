using Godot;
using System;

namespace CraigStars
{
    public interface ICellControl<T> where T : class
    {
        event Action<ICellControl<T>> MouseEnteredEvent;
        event Action<ICellControl<T>> MouseExitedEvent;
        event Action<ICellControl<T>, InputEvent> CellSelectedEvent;
        event Action<ICellControl<T>, InputEvent> CellActivatedEvent;
        Row<T> Row { get; set; }
        Cell Cell { get; set; }
        Column Column { get; set; }
    }

    public interface ICellControl : ICellControl<object>
    {
    }

}