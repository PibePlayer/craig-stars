using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Client
{
    /// <summary>
    /// A popup menu to show other objects at a map location and allow the user to select them.
    /// </summary>
    public class OtherObjectsPopupMenu : PopupMenu
    {
        static CSLog log = LogProvider.GetLogger(typeof(OtherObjectsPopupMenu));

        protected Player Me { get => PlayersManager.Me; }

        List<MapObject> otherObjectsAtLocation = new List<MapObject>();

        public override void _Ready()
        {
            base._Ready();

            Connect("id_pressed", this, nameof(OnIdPressed));

            EventManager.ViewportAlternateSelectEvent += OnViewportAlternateSelect;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.ViewportAlternateSelectEvent -= OnViewportAlternateSelect;
            }
        }

        /// <summary>
        /// When the user selects an option from the right-click "Other objects" popup menu, select/or command it
        /// </summary>
        /// <param name="id"></param>
        void OnIdPressed(int id)
        {
            log.Debug($"Other Object {id} selected.");
            if (otherObjectsAtLocation != null && id >= 0 && id < otherObjectsAtLocation.Count)
            {
                var selectedObject = otherObjectsAtLocation[id];

                // TODO: Whether something is Commandable is part of MapObjectSprite, but we are using regular MapObjects...
                // this isn't terrible, but it'd be nice if it weren't in two places
                if (selectedObject.OwnedBy(Me) && (selectedObject is Planet || selectedObject is Fleet))
                {
                    log.Debug($"Commanding {selectedObject}");
                    EventManager.PublishCommandMapObjectEvent(selectedObject);
                }
                else
                {
                    log.Debug($"Selecting {selectedObject}");
                    EventManager.PublishSelectMapObjectEvent(selectedObject);
                }
            }
        }

        void OnViewportAlternateSelect(MapObjectSprite mapObject)
        {
            if (mapObject != null && Me.MapObjectsByLocation.TryGetValue(mapObject.MapObject.Position, out var mapObjects) && mapObjects.Count > 1)
            {
                Clear();
                otherObjectsAtLocation.Clear();
                otherObjectsAtLocation.AddRange(mapObjects);

                for (int i = 0; i < otherObjectsAtLocation.Count; i++)
                {
                    var mo = otherObjectsAtLocation[i];
                    if (i > 0 && otherObjectsAtLocation[i - 1].GetType() != mo.GetType())
                    {
                        AddSeparator();
                    }
                    AddItem(mo.Name, i);
                }                
                RectGlobalPosition = GetGlobalMousePosition();
                log.Debug($"Showing Other Objects Popup with {otherObjectsAtLocation.Count} items.");
                ShowModal();
            }
        }
    }
}