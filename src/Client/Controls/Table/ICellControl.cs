using Godot;
using System;

namespace CraigStars
{
    public interface ICellControl
    {
        event Action<ICellControl> MouseEnteredEvent;
        event Action<ICellControl> MouseExitedEvent;
        event Action<ICellControl, InputEvent> CellSelectedEvent;
        event Action<ICellControl, InputEvent> CellActivatedEvent;
        Row Row { get; set; }
        Cell Cell { get; set; }
        Column Column { get; set; }
    }

}