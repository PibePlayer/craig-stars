using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// PopupMenu to show a list of targets at a location
    /// </summary>
    public class OtherWaypointTargetsPopupMenu : PopupMenu
    {
        static CSLog log = LogProvider.GetLogger(typeof(OtherWaypointTargetsPopupMenu));

        public Player Me { get => PlayersManager.Me; }

        List<MapObject> otherObjectsAtLocation = new List<MapObject>();

        Action<MapObject> onSelected;

        public override void _Ready()
        {
            base._Ready();
            Connect("id_pressed", this, nameof(OnIdPressed));
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

                onSelected?.Invoke(selectedObject);
            }
        }

        /// <summary>
        /// Show other mapObjects at this location, if there are any.
        /// </summary>
        /// <param name="fleetSprite"></param>
        /// <param name="waypoint"></param>
        /// <param name="onSelected"></param>
        public void Show(FleetSprite fleetSprite, Waypoint waypoint, Action<MapObject> onSelected)
        {
            this.onSelected = onSelected;
            Clear();

            if (Me.MapObjectsByLocation.TryGetValue(waypoint.Position, out var otherMapObjectsAtWaypoint))
            {
                otherObjectsAtLocation.Clear();
                otherObjectsAtLocation.AddRange(otherMapObjectsAtWaypoint.Where(_ => _ != fleetSprite.Fleet));

                for (int i = 0; i < otherObjectsAtLocation.Count; i++)
                {
                    var mo = otherObjectsAtLocation[i];
                    if (i > 0 && otherObjectsAtLocation[i - 1].GetType() != mo.GetType())
                    {
                        AddSeparator();
                    }
                    AddItem(mo.Name, i);
                }

                if (otherObjectsAtLocation.Count > 1)
                {
                    RectGlobalPosition = GetGlobalMousePosition();
                    ShowModal();
                }
            }
        }
    }
}