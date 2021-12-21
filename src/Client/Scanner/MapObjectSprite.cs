using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public abstract class MapObjectSprite : Area2D
    {
        public MapObject MapObject { get; set; }
        public string ObjectName { get => MapObject != null ? MapObject.Name : "Unknown"; }

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        public virtual bool Commandable { get => false; }

        /// <summary>
        /// Get the color of this map object, based on who owns it
        /// </summary>
        /// <value></value>
        public Color PlayerColor { get => MapObject?.PlayerNum != MapObject.Unowned ? GameInfo.Players[MapObject.PlayerNum].Color : Colors.White; }


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
                return MapObject?.PlayerNum != null && MapObject.OwnedBy(PlayersManager.Me);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {ObjectName}";
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            State = ScannerState.None;
            OwnerAllyState = ScannerOwnerAlly.Unknown;
            MapObject = null;
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

    }
}