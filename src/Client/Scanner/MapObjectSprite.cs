using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using CraigStars.Singletons;

namespace CraigStars
{
    public abstract class MapObjectSprite : Area2D
    {
        public MapObject MapObject { get; set; }
        public string ObjectName { get => MapObject != null ? MapObject.ObjectName : "Unknown"; }

        public Player Me { get; set; }

        public ScannerState State
        {
            get => state;
            set
            {
                state = value;
                UpdateSprite();
            }
        }
        ScannerState state = ScannerState.None;

        public ScannerOwnerAlly OwnerAllyState { get; set; } = ScannerOwnerAlly.Unknown;

        public bool OwnedByMe
        {
            get
            {
                return MapObject?.Player != null && MapObject.Player == PlayersManager.Instance?.Me;
            }
        }

        public override void _Ready()
        {
            Me = PlayersManager.Instance?.Me;
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
            State = ScannerState.Selected;
        }

        public virtual void Activate()
        {
            State = ScannerState.Active;
        }

        public virtual void Deselect()
        {
            State = ScannerState.None;
        }

        /// <summary>
        /// True if we have an active peer
        /// </summary>
        /// <param name="peers"></param>
        /// <returns></returns>
        public bool HasActivePeer(List<MapObjectSprite> peers = null)
        {
            if (peers == null)
            {
                peers = GetPeers();
            }
            return peers.Find(p => p.State == ScannerState.Active) != null;
        }

        public virtual List<MapObjectSprite> GetPeers()
        {
            return Enumerable.Empty<MapObjectSprite>().ToList();
        }

        #region Virtuals

        protected virtual void OnMapObjectActivated(MapObjectSprite mapObject)
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