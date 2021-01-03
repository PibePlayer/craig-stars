using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public abstract class MapObject : Area2D
    {
        public enum OwnerAlly
        {
            Unknown,
            Known,
            Owned,
            Friend,
            Enemy
        }

        public enum States
        {
            None,
            Selected,
            Active,
        }

        [Export]
        public States State
        {
            get => state;
            set
            {
                state = value;
                UpdateSprite();
            }
        }
        States state = States.None;

        [Export]
        public OwnerAlly OwnerAllyState { get; set; } = OwnerAlly.Unknown;

        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public String ObjectName { get; set; } = "";
        public Player Player { get; set; }
        public String RaceName { get; set; }
        public String RacePluralName { get; set; }

        public bool OwnedByMe
        {
            get
            {
                return Player != null && Player == PlayersManager.Instance?.Me;
            }
        }

        public override void _Ready()
        {
            // wire up signals
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        /// <summary>
        /// Update the sprite of this MapObject to the latest image
        /// This is called automatically when a sprite's state changes
        /// </summary>
        public abstract void UpdateSprite();

        public void Select()
        {
            State = States.Selected;
        }

        public virtual void Activate()
        {
            State = States.Active;
        }

        public virtual void Deselect()
        {
            State = States.None;
        }

        /// <summary>
        /// True if we have an active peer
        /// </summary>
        /// <param name="peers"></param>
        /// <returns></returns>
        internal bool HasActivePeer(List<MapObject> peers = null)
        {
            if (peers == null)
            {
                peers = GetPeers();
            }
            return peers.Find(p => p.State == States.Active) != null;
        }

        #region Virtuals

        internal virtual List<MapObject> GetPeers() { return Enumerable.Empty<MapObject>().ToList(); }

        protected virtual void OnMapObjectActivated(MapObject mapObject)
        {
            // in case one of our peers is activated, update our sprite on activate
            UpdateSprite();
        }

        protected virtual void OnTurnPassed(int year)
        {
            UpdateSprite();
        }

        #endregion

    }
}