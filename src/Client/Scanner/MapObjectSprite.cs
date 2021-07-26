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
        public string ObjectName { get => MapObject != null ? MapObject.Name : "Unknown"; }

        protected Player Me { get => PlayersManager.Me; }

        public virtual bool Commandable { get => false; }

        public ScannerState State
        {
            get => state;
            set
            {
                state = value;
                if (state == ScannerState.Selected)
                {
                    ZIndex = 1;
                }
                else
                {
                    ZIndex = 0;
                }
                UpdateSprite();
            }
        }
        ScannerState state = ScannerState.None;

        public ScannerOwnerAlly OwnerAllyState { get; set; } = ScannerOwnerAlly.Unknown;

        public bool OwnedByMe
        {
            get
            {
                return MapObject?.Player != null && MapObject.Player == PlayersManager.Me;
            }
        }

        public override void _Ready()
        {
            // wire up signals
            Signals.MapObjectCommandedEvent += OnMapObjectCommanded;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectCommandedEvent -= OnMapObjectCommanded;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {ObjectName}";
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

        public virtual void Command()
        {
            State = ScannerState.Commanded;
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
            return peers.Find(p => p.State == ScannerState.Commanded) != null;
        }

        public virtual List<MapObjectSprite> GetPeers()
        {
            return Enumerable.Empty<MapObjectSprite>().ToList();
        }

        #region Virtuals

        protected virtual void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            // in case one of our peers is activated, update our sprite on activate
            UpdateSprite();
        }

        protected virtual void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            UpdateSprite();
        }

        #endregion

    }
}