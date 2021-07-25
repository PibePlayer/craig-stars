using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Utils;
using Godot;
using log4net;

namespace CraigStars.Singletons
{
    public class SinglePlayerServer : Server
    {
        static CSLog log = LogProvider.GetLogger(typeof(SinglePlayerServer));

        #region Server events

        // The SinglePlayerServer class recieves network messages and publishes events to it's local instance
        // This way, a server can listen for events only on the RPC instance in its scene tree

        public event Action GameStartRequestedEvent;
        public event Action<Player> SubmitTurnRequestedEvent;

        #endregion

        public override void _Ready()
        {
            base._Ready();
            Signals.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
            Signals.UnsubmitTurnRequestedEvent += OnUnsubmitTurnRequested;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
            Signals.UnsubmitTurnRequestedEvent -= OnUnsubmitTurnRequested;
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        /// <returns></returns>
        protected override async Task GenerateNewTurn()
        {
            Signals.PublishTurnGeneratingEvent();

            await base.GenerateNewTurn();

            Signals.PublishTurnPassedEvent(Game.GameInfo);
        }

    }
}
