using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using log4net;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// A popup menu to show other objects at a map location and allow the user to select them.
    /// </summary>
    public class OtherObjectsPopupMenu : PopupMenu
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OtherObjectsPopupMenu));

        protected Player Me { get => PlayersManager.Me; }

        List<MapObject> otherObjectsAtLocation = new List<MapObject>();

        public override void _Ready()
        {
            base._Ready();

            Connect("id_pressed", this, nameof(OnIdPressed));

            Signals.ViewportAlternateSelectEvent += OnViewportAlternateSelect;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.ViewportAlternateSelectEvent += OnViewportAlternateSelect;
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
                if (selectedObject.Player == Me)
                {
                    log.Debug($"Commanding {selectedObject}");
                    Signals.PublishCommandMapObjectEvent(selectedObject);
                }
                else
                {
                    log.Debug($"Selecting {selectedObject}");
                    Signals.PublishSelectMapObjectEvent(selectedObject);
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
                otherObjectsAtLocation.Each((mo, index) => AddItem(mo.Name, index));
                RectGlobalPosition = GetGlobalMousePosition();
                log.Debug($"Showing Other Objects Popup with {otherObjectsAtLocation.Count} items.");
                ShowModal();
            }
        }
    }
}