using System;
using CraigStars.Singletons;

namespace CraigStars.Server
{
    /// <summary>
    /// For single player games, we intercept events from the Signals event manager and propagate them to the server
    /// This allows our single player and multiplayer servers to use a common interface for subscribing to client side
    /// events. 
    /// </summary>
    public class LocalClientEventPublisher : IClientEventPublisher
    {
        public event Action<GameSettings<Player>> GameStartRequestedEvent;
        public event Action<Player> SubmitTurnRequestedEvent;
        public event Action<Player> UnsubmitTurnRequestedEvent;

        public LocalClientEventPublisher()
        {
            Signals.GameStartRequestedEvent += OnGameStartRequested;
            Signals.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            Signals.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
        }

        ~LocalClientEventPublisher()
        {
            Signals.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
        }

        void OnGameStartRequested(GameSettings<Player> settings)
        {
            GameStartRequestedEvent?.Invoke(settings);
        }

        void OnSubmitTurnRequested(Player player)
        {
            SubmitTurnRequestedEvent?.Invoke(player);
        }

        void OnUnsubmitTurnRequested(Player player)
        {
            UnsubmitTurnRequestedEvent?.Invoke(player);
        }
    }
}