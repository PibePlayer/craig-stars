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
        public event Action<GameSettings<Player>> StartNewGameRequestedEvent;
        public event Action<string, int> ContinueGameRequestedEvent;
        public event Action<PlayerOrders> SubmitTurnRequestedEvent;
        public event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;
        public event Action<PublicPlayerInfo> PlayerDataRequestedEvent;
        public event Action<PublicGameInfo, Player> GenerateTurnRequestedEvent;

        public LocalClientEventPublisher()
        {
            Client.EventManager.StartNewGameRequestedEvent += OnStartNewGameRequested;
            Client.EventManager.ContinueGameRequestedEvent += OnContinueGameRequested;
            Client.EventManager.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            Client.EventManager.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
            Client.EventManager.PlayerDataRequestedEvent += OnPlayerDataRequested;
            Client.EventManager.GenerateTurnRequestedEvent += OnGenerateTurnRequested;
        }

        ~LocalClientEventPublisher()
        {
            Client.EventManager.StartNewGameRequestedEvent -= OnStartNewGameRequested;
            Client.EventManager.ContinueGameRequestedEvent += OnContinueGameRequested;
            Client.EventManager.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            Client.EventManager.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
            Client.EventManager.PlayerDataRequestedEvent -= OnPlayerDataRequested;
            Client.EventManager.GenerateTurnRequestedEvent -= OnGenerateTurnRequested;
        }

        void OnStartNewGameRequested(GameSettings<Player> settings)
        {
            StartNewGameRequestedEvent?.Invoke(settings);
        }

        void OnContinueGameRequested(string gameName, int year)
        {
            ContinueGameRequestedEvent?.Invoke(gameName, year);
        }

        void OnPlayerDataRequested(PublicPlayerInfo player)
        {
            PlayerDataRequestedEvent?.Invoke(player);
        }

        void OnSubmitTurnRequested(PlayerOrders orders)
        {
            SubmitTurnRequestedEvent?.Invoke(orders);
        }

        void OnUnsubmitTurnRequested(PublicPlayerInfo player)
        {
            UnsubmitTurnRequestedEvent?.Invoke(player);
        }

        void OnGenerateTurnRequested(PublicGameInfo gameInfo)
        {
            GenerateTurnRequestedEvent?.Invoke(gameInfo, null);
        }

    }
}