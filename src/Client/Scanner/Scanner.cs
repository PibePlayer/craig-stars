using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using log4net;
using CraigStars.Utils;

namespace CraigStars
{
    public class Scanner : Node2D
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Scanner));

        PackedScene waypointAreaScene;
        PackedScene scannerCoverageScene;
        PackedScene planetScene;
        PackedScene fleetScene;

        /// <summary>
        /// The index of the currently selected map object (if there are more than one at a location)
        /// </summary>
        int selectedMapObjectIndex;
        MapObjectSprite selectedMapObject;

        /// <summary>
        /// The index of the currently commanded map object (if there are more than one at a location)
        /// </summary>
        int commandedMapObjectIndex;
        MapObjectSprite commandedMapObject;
        MapObjectSprite highlightedMapObject;
        Waypoint selectedWaypoint;

        List<MapObjectSprite> mapObjectsUnderMouse = new List<MapObjectSprite>();
        List<MapObjectSprite> mapObjects = new List<MapObjectSprite>();
        Dictionary<Vector2, List<MapObjectSprite>> mapObjectsByLocation = new Dictionary<Vector2, List<MapObjectSprite>>();

        SelectedMapObjectIndicatorSprite selectedMapObjectIndicatorSprite;
        Node2D normalScannersNode;
        Node2D penScannersNode;
        Camera2D camera2D;

        public List<PlanetSprite> Planets { get; } = new List<PlanetSprite>();
        public List<FleetSprite> Fleets { get; } = new List<FleetSprite>();
        public Dictionary<Guid, PlanetSprite> PlanetsByGuid { get; set; } = new Dictionary<Guid, PlanetSprite>();
        public Dictionary<Guid, FleetSprite> FleetsByGuid { get; set; } = new Dictionary<Guid, FleetSprite>();
        public List<WaypointArea> waypointAreas = new List<WaypointArea>();
        public List<ScannerCoverage> Scanners { get; set; } = new List<ScannerCoverage>();
        public List<ScannerCoverage> PenScanners { get; set; } = new List<ScannerCoverage>();

        public FleetSprite CommandedFleet
        {
            get => commandedFleet; set
            {
                if (commandedFleet != value)
                {
                    commandedFleet = value;
                    OnCommandedFleetChanged();
                }
            }
        }
        FleetSprite commandedFleet;

        public PlanetSprite CommandedPlanet { get; set; }
        Player Me { get => PlayersManager.Me; }

        bool movingWaypoint = false;
        WaypointArea activeWaypointArea;

        public override void _Ready()
        {
            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/WaypointArea.tscn");
            scannerCoverageScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/ScannerCoverage.tscn");
            planetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/PlanetSprite.tscn");
            fleetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/FleetSprite.tscn");

            // get some nodes
            selectedMapObjectIndicatorSprite = GetNode<SelectedMapObjectIndicatorSprite>("SelectedMapObjectIndicatorSprite");
            normalScannersNode = GetNode<Node2D>("Scanners/Normal");
            penScannersNode = GetNode<Node2D>("Scanners/Pen");
            camera2D = GetNode<Camera2D>("Camera2D");

            // wire up events
            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.MapObjectCommandedEvent += OnMapObjectCommanded;
            Signals.GotoMapObjectEvent += OnGotoMapObject;
            Signals.GotoMapObjectSpriteEvent += OnGotoMapObjectSprite;
            Signals.ActiveNextMapObjectEvent += OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent += OnActivePrevMapObject;
            Signals.CommandMapObjectEvent += OnCommandMapObject;
            Signals.SelectMapObjectEvent += OnSelectMapObject;
            Signals.FleetDeletedEvent += OnFleetDeleted;
            Signals.FleetsCreatedEvent += OnFleetsCreated;
            Signals.WaypointAddedEvent += OnWaypointAdded;
            Signals.WaypointSelectedEvent += OnWaypointSelected;
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent += OnPlanetViewStateUpdatedEvent;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectCommandedEvent -= OnMapObjectCommanded;
            Signals.GotoMapObjectEvent -= OnGotoMapObject;
            Signals.GotoMapObjectSpriteEvent -= OnGotoMapObjectSprite;
            Signals.ActiveNextMapObjectEvent -= OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent -= OnActivePrevMapObject;
            Signals.CommandMapObjectEvent -= OnCommandMapObject;
            Signals.SelectMapObjectEvent -= OnSelectMapObject;
            Signals.FleetDeletedEvent -= OnFleetDeleted;
            Signals.FleetsCreatedEvent -= OnFleetsCreated;
            Signals.WaypointAddedEvent -= OnWaypointAdded;
            Signals.WaypointSelectedEvent -= OnWaypointSelected;
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent -= OnPlanetViewStateUpdatedEvent;
        }

        /// <summary>
        /// During process we handle constant update stuff like dragging waypoints around
        /// </summary>
        /// <param name="delta"></param>
        public override void _Process(float delta)
        {
            // if we are currently moving a waypoint, and the viewport_select action is held down, and we have an active waypoint, drag it around
            if (movingWaypoint && Input.IsActionPressed("viewport_select") && activeWaypointArea != null && IsInstanceValid(activeWaypointArea))
            {
                // without the shift key we snap to objects
                MapObjectSprite closest = GetClosestMapObjectUnderMouse();
                if (Input.IsKeyPressed((int)Godot.KeyList.Shift) || closest == null)
                {
                    // shift key we just move to a position
                    activeWaypointArea.GlobalPosition = GetGlobalMousePosition();
                    activeWaypointArea.Waypoint.Position = activeWaypointArea.Position;
                    activeWaypointArea.Waypoint.Target = null;
                }
                else
                {
                    // snap to the closest object
                    activeWaypointArea.Position = closest.Position;
                    activeWaypointArea.Waypoint.Position = closest.Position;
                    activeWaypointArea.Waypoint.Target = closest.MapObject;
                }

                Signals.PublishWaypointMovedEvent(activeWaypointArea.Fleet, activeWaypointArea.Waypoint);

            }
            else if (Input.IsActionJustReleased("viewport_select"))
            {
                movingWaypoint = false;
                UpdateSelectedMapObjectIndicator();
            }
        }

        void OnPlanetViewStateUpdatedEvent()
        {
            Planets.ForEach(p => p.UpdateSprite());
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            // update all the sprites
            mapObjectsUnderMouse.Clear();
            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();
            selectedMapObject = null;
            selectedWaypoint = null;
            commandedFleet = null;
            activeWaypointArea = null;
            commandedMapObjectIndex = 0;
            selectedMapObjectIndex = 0;

            CallDeferred(nameof(AddFleetsToViewport));
            CallDeferred(nameof(ResetScannerToHome));
        }

        void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            CommandedFleet = mapObject as FleetSprite;
            CommandedPlanet = mapObject as PlanetSprite;
        }

        /// <summary>
        /// Find the next object in a list after our currently active object. This loops back to the beginning
        /// </summary>
        /// <typeparam name="T">The type of MapObjectSprite</typeparam>
        /// <param name="items">A list of items  to iterate over</param>
        /// <param name="current">The current item to find the "next" item for</param>
        /// <returns>The next item in a list, looping back </returns>
        T FindNextObject<T>(IEnumerable<T> items, T current) where T : MapObjectSprite
        {
            return items
              .SkipWhile(item => item != current)
              .FirstOrDefault(item => item != current)
              ?? items.First();
        }

        void OnGotoMapObjectSprite(MapObjectSprite mapObject)
        {
            if (mapObject.OwnedByMe)
            {
                CommandMapObject(mapObject);
            }
            else
            {
                SelectMapObject(mapObject);
            }
        }

        void OnGotoMapObject(MapObject mapObject)
        {
            MapObjectSprite mapObjectSprite = null;

            if (mapObject is Planet planet && PlanetsByGuid.TryGetValue(planet.Guid, out var planetSprite))
            {
                mapObjectSprite = planetSprite;
            }
            else if (mapObject is Fleet fleet && FleetsByGuid.TryGetValue(fleet.Guid, out var fleetSprite))
            {
                mapObjectSprite = fleetSprite;
            }

            if (mapObjectSprite != null)
            {
                if (mapObjectSprite.OwnedByMe)
                {
                    CommandMapObject(mapObjectSprite);
                }
                else
                {
                    SelectMapObject(mapObjectSprite);
                }
            }
        }

        void OnActiveNextMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (CommandedPlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.Player == Me), CommandedPlanet);
            }
            else if (CommandedFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.Player == Me), CommandedFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != CommandedPlanet && mapObjectToActivate != CommandedFleet)
            {
                CommandMapObject(mapObjectToActivate);
            }
        }

        void OnActivePrevMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (CommandedPlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.Player == Me).Reverse(), CommandedPlanet);
            }
            else if (CommandedFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.Player == Me).Reverse(), CommandedFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != CommandedPlanet && mapObjectToActivate != CommandedFleet)
            {
                CommandMapObject(mapObjectToActivate);
            }
        }

        /// <summary>
        /// If an external source asks us to command a map object, look up the sprite by guid and command it
        /// This is published by the Reports dialog
        /// </summary>
        /// <param name="mapObject"></param>
        void OnCommandMapObject(MapObject mapObject)
        {
            MapObjectSprite mapObjectToActivate = null;
            if (mapObject is Planet planet && PlanetsByGuid.TryGetValue(planet.Guid, out var planetSprite))
            {
                mapObjectToActivate = planetSprite;
            }
            else if (mapObject is Fleet fleet && FleetsByGuid.TryGetValue(fleet.Guid, out var fleetSprite))
            {
                mapObjectToActivate = fleetSprite;
            }

            // activate this object
            if (mapObjectToActivate != null)
            {
                CommandMapObject(mapObjectToActivate);
            }
        }

        /// <summary>
        /// If an external source asks us to command a map object, look up the sprite by guid and command it
        /// This is published by the Reports dialog
        /// </summary>
        /// <param name="mapObject"></param>
        void OnSelectMapObject(MapObject mapObject)
        {
            MapObjectSprite mapObjectToSelect = null;
            if (mapObject is Planet planet && PlanetsByGuid.TryGetValue(planet.Guid, out var planetSprite))
            {
                mapObjectToSelect = planetSprite;
            }
            else if (mapObject is Fleet fleet && FleetsByGuid.TryGetValue(fleet.Guid, out var fleetSprite))
            {
                mapObjectToSelect = fleetSprite;
            }

            // activate this object
            if (mapObjectToSelect != null)
            {
                SelectMapObject(mapObjectToSelect);
            }
        }

        /// <summary>
        /// Select this map object, deselecting the previous map object
        /// if necessary
        /// </summary>
        /// <param name="mapObject"></param>
        void SelectMapObject(MapObjectSprite mapObject)
        {
            // don't deselect the commanded map object in case we selected something else
            if (selectedMapObject != commandedMapObject)
            {
                selectedMapObject.Deselect();
                selectedMapObject.UpdateSprite();
            }
            selectedWaypoint = null;

            // now we have selected something else, don't toggle the "selected" mode
            // on this newly selected if object if it's the currently commanded mapObject
            // i.e. if we select off and back onto the currently commanded planet, don't
            // "select" the planet because that will make it small
            selectedMapObject = mapObject;
            if (selectedMapObject != commandedMapObject)
            {
                selectedMapObject.Select();
                selectedMapObject.UpdateSprite();
            }

            var mapObjectsAtLocation = mapObjectsByLocation[mapObject.MapObject.Position];
            for (int i = 0; i < mapObjectsAtLocation.Count; i++)
            {
                if (mapObjectsAtLocation[i] == selectedMapObject)
                {
                    selectedMapObjectIndex = i;
                    break;
                }
            }

            Signals.PublishMapObjectSelectedEvent(mapObject);
            UpdateSelectedMapObjectIndicator();
        }

        /// <summary>
        /// Command this mapObject, deselecting the previous commandedMapObject if necessary 
        /// </summary>
        /// <param name="mapObject"></param>
        void CommandMapObject(MapObjectSprite mapObject)
        {
            // deselect the current map object
            commandedMapObject.Deselect();
            commandedMapObject.UpdateSprite();

            // update and select the new one
            commandedMapObject = mapObject;
            var mapObjectsAtLocation = mapObjectsByLocation[mapObject.MapObject.Position];
            for (int i = 0; i < mapObjectsAtLocation.Count; i++)
            {
                if (mapObjectsAtLocation[i] == commandedMapObject)
                {
                    commandedMapObjectIndex = i;
                    break;
                }
            }

            commandedMapObject.Command();
            commandedMapObject.UpdateSprite();
            Signals.PublishMapObjectCommandedEvent(mapObject);
            UpdateSelectedMapObjectIndicator();
        }

        /// <summary>
        /// When the CommandedFleet changes, 
        /// </summary>
        void OnCommandedFleetChanged()
        {
            log.Debug($"CommandedFleetChanged to {CommandedFleet}");
            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();
            selectedWaypoint = null;
            CommandedFleet?.Fleet?.Waypoints.Each((wp, index) => AddWaypointArea(CommandedFleet.Fleet, wp));
        }

        #region Waypoints

        /// <summary>
        /// When a waypoint is added, add an area for it so we can
        /// select it
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="waypoint"></param>
        void OnWaypointAdded(Fleet fleet, Waypoint waypoint)
        {
            AddWaypointArea(fleet, waypoint);
            selectedWaypoint = waypoint;
            UpdateSelectedMapObjectIndicator();
        }

        void OnWaypointDeleted(Waypoint waypoint)
        {
            if (selectedWaypoint == waypoint)
            {
                selectedWaypoint = null;
            }
        }

        void OnWaypointSelected(Waypoint waypoint)
        {
            selectedWaypoint = waypoint;
            UpdateSelectedMapObjectIndicator();
        }

        void AddWaypointArea(Fleet fleet, Waypoint waypoint)
        {
            var waypointArea = waypointAreaScene.Instance() as WaypointArea;
            if (waypoint != fleet.Waypoints[0])
            {
                waypointArea.Connect("mouse_entered", this, nameof(OnWaypointAreaMouseEntered), new Godot.Collections.Array() { waypointArea });
                waypointArea.Connect("mouse_exited", this, nameof(OnWaypointAreaMouseExited), new Godot.Collections.Array() { waypointArea });
                waypointArea.Connect("input_event", this, nameof(OnWaypointAreaInputEvent), new Godot.Collections.Array() { waypointArea });
            }

            waypointArea.Fleet = fleet;
            waypointArea.Waypoint = waypoint;
            waypointAreas.Add(waypointArea);
            AddChild(waypointArea);
        }

        void OnWaypointAreaMouseEntered(WaypointArea waypointArea)
        {
            Input.SetDefaultCursorShape(Input.CursorShape.Drag);
        }

        void OnWaypointAreaMouseExited(WaypointArea waypointArea)
        {
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
        }

        void OnWaypointAreaInputEvent(Node viewport, InputEvent @event, int shapeIdx, WaypointArea waypointArea)
        {
            if (IsInstanceValid(waypointArea))
            {
                if (@event.IsActionPressed("viewport_select"))
                {
                    log.Debug($"Selecting waypoint {waypointArea.Position}");
                    Signals.PublishWaypointSelectedEvent(waypointArea.Waypoint);
                    activeWaypointArea = waypointArea;
                    movingWaypoint = true;
                }
            }
            else
            {
                log.Error("Got input event for invalid waypoint.");
            }
        }

        #endregion

        public void InitMapObjects()
        {
            // remove old planets, in case we switched players
            Planets.ForEach(oldPlanet =>
            {
                RemoveChild(oldPlanet);
                oldPlanet.Disconnect("input_event", this, nameof(OnInputEvent));
                oldPlanet.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
                oldPlanet.Disconnect("mouse_exited", this, nameof(OnMouseExited));
                oldPlanet.QueueFree();
            });
            Planets.Clear();

            Planets.AddRange(Me.AllPlanets.Select(planet =>
            {
                var planetSprite = planetScene.Instance() as PlanetSprite;
                planetSprite.Planet = planet;
                planetSprite.Position = planet.Position;
                return planetSprite;
            }));
            Planets.ForEach(p => AddChild(p));
            Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { p }));
            Planets.ForEach(p => p.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { p }));
            Planets.ForEach(p => p.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { p }));
            PlanetsByGuid = Planets.ToLookup(p => p.Planet.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            AddFleetsToViewport();

            CallDeferred(nameof(ResetScannerToHome));
        }

        /// <summary>
        /// Handle clicks on teh scanner itself to 
        /// </summary>
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // don't put a waypoint in space unless it's empty space
                        if (mapObjectsUnderMouse.Count == 0)
                        {
                            log.Debug($"Adding waypoint in space for {commandedFleet.Name} to {GetLocalMousePosition()}");
                            commandedFleet?.AddWaypoint(GetLocalMousePosition(), selectedWaypoint);
                            PlayersManager.Me.Dirty = true;
                            Signals.PublishPlayerDirtyEvent();
                        }
                    }
                }
            }
            if (@event.IsActionPressed("delete") && selectedWaypoint != null && commandedFleet != null)
            {
                commandedFleet.DeleteWaypoint(selectedWaypoint);
                PlayersManager.Me.Dirty = true;
                Signals.PublishPlayerDirtyEvent();
            }
        }

        void OnMouseEntered(MapObjectSprite mapObject)
        {
            if (IsInstanceValid(mapObject))
            {
                log.Debug($"Highlighted map object {mapObject.ObjectName}");
                mapObjectsUnderMouse.Add(mapObject);
                Signals.PublishMapObjectHightlightedEvent(mapObject);
            }
            else
            {
                log.Error("OnMouseEntered called for invalid mapObject");
            }
        }

        void OnMouseExited(MapObjectSprite mapObject)
        {
            if (IsInstanceValid(mapObject))
            {
                log.Debug($"Mouse Left map object {mapObject.ObjectName}");
                mapObjectsUnderMouse.Remove(mapObject);
                Signals.PublishMapObjectHightlightedEvent(mapObject);
            }
            else
            {
                log.Error("OnMouseExited called for invalid mapObject");
            }
        }

        /// <summary>
        /// Get the closest map object sprite under our mouse
        /// </summary>
        /// <returns></returns>
        MapObjectSprite GetClosestMapObjectUnderMouse()
        {
            var localMousePosition = GetLocalMousePosition();
            var validMapObjects = mapObjectsUnderMouse.Where(mo => IsInstanceValid(mo)).ToList();
            if (validMapObjects.Count > 0)
            {
                return validMapObjects.Aggregate((curMin, mo) => (curMin == null || (mo.Position.DistanceSquaredTo(localMousePosition)) < curMin.Position.DistanceSquaredTo(localMousePosition) ? mo : curMin));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Respond to mouse events on our map objects in the Scanner
        /// If the player clicks an object, it should select it. If they click it again, and the player
        /// owns the object, it should activate it. Subsequent clicks should cycle through all mapObjects
        /// at the same point on the scanner
        /// </summary>
        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx, MapObjectSprite mapObject)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                log.Debug($"viewport_select event {mapObject}");
                MapObjectSprite closest = GetClosestMapObjectUnderMouse();

                log.Debug($"Closest: {closest?.ObjectName}");
                if (mapObject != closest)
                {
                    // ignore events from all but the closest mapobject
                    return;
                }

                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        log.Debug($"Adding waypoint for {commandedFleet.Name} to {closest.Name}");
                        commandedFleet.AddWaypoint(closest.MapObject, selectedWaypoint);
                        PlayersManager.Me.Dirty = true;
                        Signals.PublishPlayerDirtyEvent();
                    }
                }
                else
                {
                    // we have clicked on a map object
                    var mapObjectsAtLocation = mapObjectsByLocation[mapObject.MapObject.Position];
                    log.Debug($"{mapObjectsAtLocation.Count} MapObjects at {mapObject.MapObject.Position} with {mapObject}");
                    if (mapObjectsAtLocation.Contains(selectedMapObject))
                    {
                        // the currently selected mapObject is at the same location as the mapObject we just clicked
                        if (mapObjectsAtLocation.Contains(commandedMapObject))
                        {
                            if (commandedMapObjectIndex + 1 == mapObjectsAtLocation.Count)
                            {
                                // we are at the end of the list, go back to the beginning
                                commandedMapObjectIndex = 0;
                            }
                            else
                            {
                                // go to the next one
                                commandedMapObjectIndex++;
                            }
                            // the commanded map object also exists at the same location as the mapObject we just clicked
                            // we should command the next mapObject we own at this location
                            for (int i = commandedMapObjectIndex; i < mapObjectsAtLocation.Count; i++)
                            {
                                if (mapObjectsAtLocation[i].OwnedByMe)
                                {
                                    commandedMapObjectIndex = i;
                                    break;
                                }
                            }

                            var newCommandedMapObject = mapObjectsAtLocation[commandedMapObjectIndex];
                            log.Debug($"Commanding MapObject {newCommandedMapObject} (index {commandedMapObjectIndex})");
                            Signals.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
                        }
                        else if (selectedMapObject == mapObject && selectedMapObject.OwnedByMe)
                        {
                            var newCommandedMapObject = selectedMapObject;
                            commandedMapObjectIndex = 0;
                            log.Debug($"Commanding MapObject {newCommandedMapObject} (index 0)");
                            Signals.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
                        }
                        else if (selectedMapObject == mapObject && !selectedMapObject.OwnedByMe && mapObjectsAtLocation.Count > 1)
                        {
                            log.Debug($"Selected MapObject we don't own {mapObject}");
                            for (int i = 1; i < mapObjectsAtLocation.Count; i++)
                            {
                                var otherMapObject = mapObjectsAtLocation[i];
                                if (otherMapObject.OwnedByMe)
                                {
                                    var newCommandedMapObject = otherMapObject;
                                    commandedMapObjectIndex = i;
                                    log.Debug($"Commanding MapObject {newCommandedMapObject} (index 0)");
                                    Signals.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        var newSelectedMapObject = mapObject;
                        selectedMapObjectIndex = 0;
                        log.Debug($"Selecting MapObject {newSelectedMapObject} (index 0)");
                        Signals.PublishSelectMapObjectEvent(newSelectedMapObject.MapObject);
                    }
                }

            }
        }

        /// <summary>
        /// Update the sprite showing which object is selected. If the object is commanded, or it has a commanded fleet, we show the bigger icon
        /// </summary>
        void UpdateSelectedMapObjectIndicator()
        {
            if (selectedMapObjectIndicatorSprite != null)
            {

                if (selectedWaypoint != null)
                {
                    selectedMapObjectIndicatorSprite.Select(selectedWaypoint.Position);
                }
                else if (selectedMapObject != null)
                {
                    // if the selected object (or one of its peers) is currently being commanded, it'll need a large
                    // selected object indicator
                    if (selectedMapObject == commandedMapObject ||
                    (commandedMapObject != null && commandedMapObject.GetPeers().Contains(selectedMapObject)) ||
                    (commandedMapObject is FleetSprite commandedFleet && commandedFleet.Orbiting == selectedMapObject))
                    {
                        selectedMapObjectIndicatorSprite.SelectLarge(selectedMapObject.Position);
                    }
                    else
                    {
                        selectedMapObjectIndicatorSprite.Select(selectedMapObject.Position);
                    }

                }
            }

        }

        /// <summary>
        /// Add new fleet data to the viewport, clearing out the old data first
        /// </summary>
        void AddFleetsToViewport()
        {
            log.Debug("Resetting viewport Fleets");

            // clear out any existing fleets
            Fleets.ForEach(f =>
            {
                RemoveChild(f);
                f.Disconnect("input_event", this, nameof(OnInputEvent));
                f.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
                f.Disconnect("mouse_exited", this, nameof(OnMouseExited));
                f.QueueFree();
            });
            Fleets.Clear();
            FleetsByGuid.Clear();

            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();
            Planets.ForEach(p => p.OrbitingFleets.Clear());
            CommandedFleet = null;
            CommandedPlanet = null;

            // add in new fleets
            Fleets.AddRange(Me.AllFleets.Select(fleet =>
            {
                var fleetSprite = fleetScene.Instance() as FleetSprite;
                fleetSprite.Fleet = fleet;
                fleetSprite.Position = fleet.Position;
                if (fleet.Orbiting != null && PlanetsByGuid.TryGetValue(fleet.Orbiting.Guid, out var planetSprite))
                {
                    planetSprite.OrbitingFleets.Add(fleetSprite);
                    fleetSprite.Orbiting = planetSprite;
                }
                return fleetSprite;
            }));

            FleetsByGuid = Fleets.ToLookup(f => f.Fleet.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            Fleets.ForEach(f =>
            {
                f.OtherFleets.Clear();
                if (f.Fleet.OtherFleets?.Count > 0)
                {
                    foreach (var fleet in f.Fleet.OtherFleets)
                    {
                        var fleetSprite = FleetsByGuid[fleet.Guid];
                        f.OtherFleets.Add(fleetSprite);
                    }
                }
            });

            Fleets.ForEach(f => AddChild(f));
            Fleets.ForEach(f => f.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { f }));
            Fleets.ForEach(f => f.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { f }));
            Fleets.ForEach(f => f.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { f }));
        }

        /// <summary>
        /// Add a new set of fleets to the viewport. This happens when doing a split
        /// on the client side
        /// </summary>
        /// <param name="fleets"></param>
        void OnFleetsCreated(List<Fleet> fleets)
        {
            // add in new fleets
            var newFleetSprites = fleets.Select(fleet =>
            {
                log.Debug($"User created new fleet {fleet.Name} - {fleet.Guid}");
                var fleetSprite = fleetScene.Instance() as FleetSprite;
                fleetSprite.Fleet = fleet;
                fleetSprite.Position = fleet.Position;
                if (fleet.Orbiting != null && PlanetsByGuid.TryGetValue(fleet.Orbiting.Guid, out var planetSprite))
                {
                    planetSprite.OrbitingFleets.Add(fleetSprite);
                    fleetSprite.Orbiting = planetSprite;
                }

                // Add these fleets to our dictionaries
                FleetsByGuid[fleet.Guid] = fleetSprite;
                if (mapObjectsByLocation.TryGetValue(fleet.Position, out var mapObjectsAtLocation))
                {
                    mapObjectsAtLocation.Add(fleetSprite);
                }
                else
                {
                    mapObjectsByLocation[fleet.Position] = new List<MapObjectSprite>() { fleetSprite };
                }
                return fleetSprite;
            }).ToList();

            Fleets.AddRange(newFleetSprites);


            Fleets.ForEach(f =>
            {
                f.OtherFleets.Clear();
                if (f.Fleet.OtherFleets?.Count > 0)
                {
                    foreach (var fleet in f.Fleet.OtherFleets)
                    {
                        if (FleetsByGuid.TryGetValue(fleet.Guid, out var fleetSprite))
                        {
                            f.OtherFleets.Add(fleetSprite);
                        }
                        else
                        {
                            log.Error($"Couldn't map up fleet's OtherFleet by guid: {fleet.Guid}");
                        }
                    }
                }
            });

            newFleetSprites.ForEach(f => AddChild(f));
            newFleetSprites.ForEach(f => f.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { f }));
            newFleetSprites.ForEach(f => f.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { f }));
            newFleetSprites.ForEach(f => f.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { f }));

        }

        void OnFleetDeleted(FleetSprite fleet)
        {
            log.Debug($"User deleted fleet {fleet.Fleet.Name} - {fleet.Fleet.Guid}");
            Fleets.Remove(fleet);
            // make sure other fleets don't know about us anymore
            fleet.OtherFleets.ForEach(otherFleet => otherFleet.OtherFleets.Remove(fleet));

            // make sure any planets we are orbiting don't know about us anymore
            if (fleet.Orbiting != null)
            {
                fleet.Orbiting.OrbitingFleets.Remove(fleet);
            }
            FleetsByGuid.Remove(fleet.Fleet.Guid);

            // remove the fleet from the scanner
            RemoveChild(fleet);
            fleet.Disconnect("input_event", this, nameof(OnInputEvent));
            fleet.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
            fleet.Disconnect("mouse_exited", this, nameof(OnMouseExited));
            fleet.QueueFree();
        }

        public void ResetScannerToHome()
        {
            log.Debug("Resetting Scanner to homeview");
            FocusHomeworld();
            log.Debug("Updating scanners");
            UpdateScanners();
            log.Debug("Updating Sprites");
            Planets.ForEach(p => p.UpdateSprite());
            Fleets.ForEach(f => f.UpdateSprite());
            log.Debug("Finished Updating Sprites");

            // build a list of all map objects and all map objects per location
            mapObjects.Clear();
            mapObjects.AddRange(Planets);
            mapObjects.AddRange(Fleets);
            mapObjectsByLocation = mapObjects.ToLookup(mo => mo.MapObject.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());

        }

        #region New Turn functions

        /// <summary>
        /// Focus on the current player's homeworld
        /// </summary>
        void FocusHomeworld()
        {
            var homeworld = Planets.Where(p => p.Planet.Homeworld && p.Planet.Player == Me).FirstOrDefault();
            if (homeworld != null)
            {
                selectedWaypoint = null;
                selectedMapObject?.Deselect();
                commandedMapObject?.Deselect();
                selectedMapObject = homeworld;
                commandedMapObject = homeworld;

                selectedMapObject.Select();
                commandedMapObject.Command();
                UpdateSelectedMapObjectIndicator();
                Signals.PublishMapObjectSelectedEvent(homeworld);
                Signals.PublishMapObjectCommandedEvent(homeworld);
            }
        }

        /// <summary>
        /// Update the scanners for a new turn.
        /// We only create a ScannerCoverage for the largest scanner at a single location
        /// </summary>
        void UpdateScanners()
        {
            // clear out the old scanners
            Scanners.ForEach(s => { s.GetParent()?.RemoveChild(s); s.QueueFree(); });
            Scanners.Clear();

            foreach (var planet in Planets)
            {
                var range = -1;
                var rangePen = -1;

                // if we own this planet and it has a scanner, include it
                if (planet.OwnedByMe && planet.Planet.Scanner && Me.PlanetaryScanner != null)
                {
                    range = Me.PlanetaryScanner.ScanRange;
                    rangePen = Me.PlanetaryScanner.ScanRangePen;
                }

                foreach (var fleet in planet.OrbitingFleets.Where(f => f.Fleet.Player == Me))
                {
                    range = Math.Max(range, fleet.Fleet.Aggregate.ScanRange);
                    rangePen = Math.Max(rangePen, fleet.Fleet.Aggregate.ScanRangePen);
                }

                if (range > 0)
                {
                    AddScannerCoverage(planet.Position, range, false);
                }
                if (rangePen > 0)
                {
                    AddScannerCoverage(planet.Position, rangePen, true);
                }

            }

            foreach (var fleet in Fleets.Where(f => f.OwnedByMe && f.Orbiting == null && f.Fleet.Aggregate.ScanRange > 0))
            {
                var range = fleet.Fleet.Aggregate.ScanRange;
                var rangePen = fleet.Fleet.Aggregate.ScanRangePen;
                if (range > 0)
                {
                    AddScannerCoverage(fleet.Position, range, false);
                }
                if (rangePen > 0)
                {
                    AddScannerCoverage(fleet.Position, rangePen, true);
                }
            }
        }

        void AddScannerCoverage(Vector2 position, int range, bool pen)
        {
            ScannerCoverage scanner = scannerCoverageScene.Instance() as ScannerCoverage;
            scanner.Position = position;
            if (pen)
            {
                penScannersNode.AddChild(scanner);
            }
            else
            {
                normalScannersNode.AddChild(scanner);
            }
            scanner.ScanRange = range;
            scanner.Pen = pen;
            Scanners.Add(scanner);

        }


        #endregion
    }
}
