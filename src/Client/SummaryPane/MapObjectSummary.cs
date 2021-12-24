using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public abstract class MapObjectSummary<T> : Control where T : MapObjectSprite
    {
        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

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
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.MapObjectSelectedEvent -= OnMapObjectSelected;
            }
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            MapObject = mapObject as T;
        }

        protected abstract void UpdateControls();
    }
}