using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using log4net;

namespace CraigStars.Server
{
    public class LocalServer : Server
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
            Signals.PublishPlayerUpdatedEvent(player);
        }

        protected override void PublishGameStartedEvent()
        {
            // send a signal per non ai player in the game
            // For hotseat games, the ClientView will store all players that can play
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                Signals.PublishGameStartedEvent(Game.GameInfo, player);
            }
        }

        protected override void PublishTurnSubmittedEvent(PublicPlayerInfo player)
        {
            Signals.PublishPlayerUpdatedEvent(player);
        }

        protected override void PublishTurnUnsubmittedEvent(PublicPlayerInfo player)
        {
            Signals.PublishTurnUnsubmittedEvent(player);
        }

        protected override void PublishTurnGeneratingEvent()
        {
            Signals.PublishTurnGeneratingEvent();
        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {
            Signals.PublishTurnGeneratorAdvancedEvent(state);
        }

        protected override void PublishTurnPassedEvent()
        {
            // notify each non AI player about the new turn
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                Signals.PublishTurnPassedEvent(Game.GameInfo, player);
            }
        }

        #endregion
    }
}
