using System;
using CraigStars.Client;

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
        public event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;
        public event Action<PublicPlayerInfo> PlayerDataRequestedEvent;

        public LocalClientEventPublisher()
        {
            Client.EventManager.GameStartRequestedEvent += OnGameStartRequested;
            Client.EventManager.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            Client.EventManager.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
            Client.EventManager.PlayerDataRequestedEvent += OnPlayerDataRequested;
        }


        ~LocalClientEventPublisher()
        {
            Client.EventManager.GameStartRequestedEvent -= OnGameStartRequested;
            Client.EventManager.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            Client.EventManager.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
            Client.EventManager.PlayerDataRequestedEvent -= OnPlayerDataRequested;
        }

        void OnGameStartRequested(GameSettings<Player> settings)
        {
            GameStartRequestedEvent?.Invoke(settings);
        }

        void OnPlayerDataRequested(PublicPlayerInfo player)
        {
            PlayerDataRequestedEvent?.Invoke(player);
        }

        void OnSubmitTurnRequested(Player player)
        {
            SubmitTurnRequestedEvent?.Invoke(player);
        }

        void OnUnsubmitTurnRequested(PublicPlayerInfo player)
        {
            UnsubmitTurnRequestedEvent?.Invoke(player);
        }
    }
}