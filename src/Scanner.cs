using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;

namespace CraigStars
{
    public class Scanner : Node2D
    {
        PackedScene waypointAreaScene;
        MapObject selectedMapObject;
        MapObject commandedMapObject;
        SelectedMapObjectSprite selectedMapObjectSprite;

        public List<Planet> Planets { get; } = new List<Planet>();
        public List<Fleet> Fleets { get; } = new List<Fleet>();
        public List<WaypointArea> waypointAreas = new List<WaypointArea>();

        public Fleet ActiveFleet
        {
            get => activeFleet; set
            {
                if (activeFleet != value)
                {
                    activeFleet = value;
                    OnActiveFleetChanged();
                }
            }
        }
        Fleet activeFleet;

        public Planet ActivePlanet { get; set; }

        public override void _Ready()
        {
            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/WaypointArea.tscn");
            selectedMapObjectSprite = GetNode<SelectedMapObjectSprite>("SelectedMapObjectSprite");
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent += OnMapObjectWaypointAdded;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent -= OnMapObjectWaypointAdded;
        }

        void OnMapObjectActivated(MapObject mapObject)
        {
            ActiveFleet = mapObject as Fleet;
            ActivePlanet = mapObject as Planet;
        }

        void OnMapObjectWaypointAdded(MapObject mapObject)
        {
            if (ActiveFleet != null)
            {
                var waypoint = ActiveFleet.AddWaypoint(mapObject);
                AddWaypointArea(waypoint);
            }
        }

        /// <summary>
        /// When the ActiveFleet changes, 
        /// </summary>
        void OnActiveFleetChanged()
        {
            waypointAreas.ForEach(wpa => { wpa.QueueFree(); });
            waypointAreas.Clear();
            ActiveFleet?.Waypoints.ForEach(wp => AddWaypointArea(wp));
        }

        void AddWaypointArea(Waypoint waypoint)
        {
            var waypointArea = waypointAreaScene.Instance() as WaypointArea;
            waypointArea.Waypoint = waypoint;
            waypointAreas.Add(waypointArea);
            AddChild(waypointArea);
        }

        public void AddMapObjects(Game game)
        {
            Planets.AddRange(game.Planets);
            Fleets.AddRange(game.Fleets);
            Planets.ForEach(p => AddChild(p));
            Fleets.ForEach(f => AddChild(f));

            Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { p }));
            Fleets.ForEach(f => f.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { f }));

            CallDeferred(nameof(UpdateViewport));
        }

        /// <summary>
        /// Focus on the current player's homeworld
        /// </summary>
        void FocusHomeworld()
        {
            var homeworld = Planets.Where(p => p.Homeworld && p.Player == PlayersManager.Instance.Me).First();
            if (homeworld != null)
            {
                selectedMapObject = homeworld;
                commandedMapObject = homeworld;

                selectedMapObject.Select();
                commandedMapObject.Activate();
                selectedMapObjectSprite.SelectLarge(commandedMapObject.Position);
                Signals.PublishMapObjectSelectedEvent(homeworld);
                Signals.PublishMapObjectActivatedEvent(homeworld);
            }
        }

        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx, MapObject mapObject)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                GD.Print($"Selected {mapObject.ObjectName}");
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is Fleet commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        commandedFleet.AddWaypoint(mapObject);
                    }
                }
                else
                {
                    if (mapObject != selectedMapObject)
                    {
                        // change the selected object
                        selectedMapObject?.Deselect();
                        selectedMapObject = mapObject;
                        selectedMapObject.Select();
                        Signals.PublishMapObjectSelectedEvent(selectedMapObject);

                        // move our selection icon around
                        if (selectedMapObject == commandedMapObject
                        || (commandedMapObject != null && commandedMapObject.GetPeers().Contains(selectedMapObject)))
                        {
                            selectedMapObjectSprite.SelectLarge(selectedMapObject.Position);
                        }
                        else
                        {
                            selectedMapObjectSprite.Select(selectedMapObject.Position);
                        }
                    }
                    else if (mapObject == selectedMapObject)
                    {
                        if (mapObject.OwnedByMe)
                        {
                            if (commandedMapObject != null && commandedMapObject == mapObject)
                            {
                                CommandNextPeer(commandedMapObject);
                            }
                            else
                            {
                                // we aren't commanding anything yet, so command this
                                commandedMapObject?.Deselect();
                                commandedMapObject = mapObject;
                            }
                        }
                        else
                        {
                            // this mapObject isn't owned by me.
                            CommandNextPeer(mapObject);
                        }
                        // move our selection icon around
                        if (selectedMapObject == commandedMapObject
                        || (commandedMapObject != null && commandedMapObject.GetPeers().Contains(selectedMapObject)))
                        {
                            selectedMapObjectSprite.SelectLarge(selectedMapObject.Position);
                        }
                        else
                        {
                            selectedMapObjectSprite.Select(selectedMapObject.Position);
                        }
                        commandedMapObject.Activate();
                        Signals.PublishMapObjectActivatedEvent(commandedMapObject);
                    }
                }

            }
        }

        internal void CommandNextPeer(MapObject mapObject)
        {
            // we selected the object we are commanding again
            // if it has peers, command the next peer
            var peers = mapObject.GetPeers();
            var activePeer = peers.Find(p => p.State == MapObject.States.Active);
            if (peers.Count > 0)
            {
                if (commandedMapObject is Planet)
                {
                    // leave planets selected
                    commandedMapObject.State = MapObject.States.Selected;
                }
                else
                {
                    commandedMapObject.Deselect();
                }

                if (activePeer != null)
                {
                    var activePeerPeers = activePeer.GetPeers();
                    if (activePeerPeers.Count > 0)
                    {
                        // command the next peer
                        commandedMapObject = activePeer.GetPeers()[0];
                    }
                }
                else
                {
                    // command the first peer
                    commandedMapObject = peers[0];
                }
            }
        }

        public void UpdateViewport()
        {
            FocusHomeworld();

            Planets.ForEach(p => p.UpdateSprite());
            Fleets.ForEach(f => f.UpdateSprite());
        }

    }
}
