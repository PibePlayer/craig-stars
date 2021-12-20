using System;

namespace CraigStars.Server
{
    /// <summary>
    /// Servers subscribe to these events either from a ClientView in single player
    /// or from RPC calls in multiplayer
    /// </summary>
    public interface IClientEventPublisher
    {
        event Action<PublicPlayerInfo> PlayerDataRequestedEvent;
        event Action<GameSettings<Player>> StartNewGameRequestedEvent;
        event Action<string, int> ContinueGameRequestedEvent;
        event Action<PlayerOrders> SubmitTurnRequestedEvent;
        event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;
        event Action<PublicGameInfo, Player> GenerateTurnRequestedEvent;
    }
}

