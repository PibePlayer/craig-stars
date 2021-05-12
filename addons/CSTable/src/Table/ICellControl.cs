using Godot;
using System;

namespace CraigStarsTable
{
    public interface ICSCellControl<T> where T : class
    {
        event Action<ICSCellControl<T>> MouseEnteredEvent;
        event Action<ICSCellControl<T>> MouseExitedEvent;
        event Action<ICSCellControl<T>, InputEvent> CellSelectedEvent;
        event Action<ICSCellControl<T>, InputEvent> CellActivatedEvent;
        Row<T> Row { get; set; }
        Cell Cell { get; set; }
        Column Column { get; set; }
    }

    public interface ICellControl : ICSCellControl<object>
    {
    }

}