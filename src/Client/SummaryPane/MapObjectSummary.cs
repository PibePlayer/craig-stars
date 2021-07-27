using Godot;
using System;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public abstract class MapObjectSummary<T> : Control where T : MapObjectSprite
    {
        protected Player Me { get => PlayersManager.Me; }

        protected T MapObject
        {
            get => mapObject;
            set
            {
                mapObject = value;
                UpdateControls();
            }
        }
        T mapObject;

        public override void _Ready()
        {
            EventManager.MapObjectSelectedEvent += OnMapObjectSelected;
            EventManager.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            EventManager.MapObjectSelectedEvent -= OnMapObjectSelected;
            EventManager.TurnPassedEvent -= OnTurnPassed;
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            MapObject = mapObject as T;
        }

        void OnTurnPassed(PublicGameInfo gameInfo, Player player)
        {
            MapObject = null;
            UpdateControls();
        }

        protected abstract void UpdateControls();
    }
}