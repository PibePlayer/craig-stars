using System;
using System.Linq;
using CraigStars.Client;

namespace CraigStars.Server
{
    public class LocalServer : Server, IServer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LocalServer));

        #region Server events

        // The SinglePlayerServer class recieves network messages and publishes events to it's local instance
        // This way, a server can listen for events only on the RPC instance in its scene tree

        public event Action GameStartRequestedEvent;
        public event Action<Player> SubmitTurnRequestedEvent;

        #endregion

        protected override IClientEventPublisher CreateClientEventPublisher()
        {
            return new LocalClientEventPublisher();
        }

        #region Publishers

        protected override void PublishPlayerUpdatedEvent(PublicPlayerInfo player)
        {
            // do nothing, no need to notify players about other player updates
            // in a LocalServer
        }

        protected override void PublishGameStartedEvent()
        {
            // send a signal per non ai player in the game
            // For hotseat games, the ClientView will store all players that can play
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                Client.EventManager.PublishGameStartedEvent(Game.GameInfo, player);
            }
        }

        protected override void PublishTurnSubmittedEvent(PublicPlayerInfo player)
        {
            Client.EventManager.PublishTurnSubmittedEvent(player);
        }

        protected override void PublishTurnUnsubmittedEvent(PublicPlayerInfo player)
        {
            Client.EventManager.PublishTurnUnsubmittedEvent(player);
        }

        protected override void PublishTurnGeneratingEvent()
        {
            Client.EventManager.PublishTurnGeneratingEvent();
        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {
            Client.EventManager.PublishTurnGeneratorAdvancedEvent(state);
        }

        protected override void PublishTurnPassedEvent()
        {
            // notify each non AI player about the new turn
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                Client.EventManager.PublishTurnPassedEvent(Game.GameInfo, player);
            }
        }

        #endregion
    }
}
