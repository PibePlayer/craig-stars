using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class Scanner : Node2D
    {
        static CSLog log = LogProvider.GetLogger(typeof(Scanner));

        protected Player Me { get => PlayersManager.Me; }

        PackedScene waypointAreaScene;
        PackedScene scannerCoverageScene;
        PackedScene planetScene;
        PackedScene fleetScene;
        PackedScene salvageScene;
        PackedScene mineFieldScene;
        PackedScene mineralPacketScene;
        PackedScene wormholeScene;

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
        List<MapObjectSprite> transientMapObjects = new List<MapObjectSprite>();
        Dictionary<Vector2, List<MapObjectSprite>> mapObjectsByLocation = new Dictionary<Vector2, List<MapObjectSprite>>();

        SelectedMapObjectIndicatorSprite selectedMapObjectIndicatorSprite;
        Node2D normalScannersNode;
        Node2D penScannersNode;
        Camera2D camera2D;

        public List<PlanetSprite> Planets { get; } = new List<PlanetSprite>();
        public List<FleetSprite> Fleets { get; } = new List<FleetSprite>();
        public Dictionary<Guid, MapObjectSprite> MapObjectsByGuid { get; set; } = new Dictionary<Guid, MapObjectSprite>();
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

        bool movingWaypoint = false;
        bool pickingPacketDestination = false;
        WaypointArea activeWaypointArea;

        public override void _Ready()
        {
            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/WaypointArea.tscn");
            scannerCoverageScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/ScannerCoverage.tscn");
            planetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/PlanetSprite.tscn");
            fleetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/FleetSprite.tscn");
            salvageScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/SalvageSprite.tscn");
            mineFieldScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/MineFieldSprite.tscn");
            mineralPacketScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/MineralPacketSprite.tscn");
            wormholeScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/WormholeSprite.tscn");

            // get some nodes
            selectedMapObjectIndicatorSprite = GetNode<SelectedMapObjectIndicatorSprite>("SelectedMapObjectIndicatorSprite");
            normalScannersNode = GetNode<Node2D>("Scanners/Normal");
            penScannersNode = GetNode<Node2D>("Scanners/Pen");
            camera2D = GetNode<Camera2D>("Camera2D");

            // we only enable processing if a waypoint is being moved
            SetProcess(false);

            // wire up events
            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
            EventManager.GotoMapObjectEvent += OnGotoMapObject;
            EventManager.GotoMapObjectSpriteEvent += OnGotoMapObjectSprite;
            EventManager.CommandNextMapObjectEvent += OnCommandNextMapObject;
            EventManager.CommandPrevMapObjectEvent += OnCommandPrevMapObject;
            EventManager.CommandMapObjectEvent += OnCommandMapObject;
            EventManager.SelectMapObjectEvent += OnSelectMapObject;
            EventManager.FleetDeletedEvent += OnFleetDeleted;
            EventManager.FleetsCreatedEvent += OnFleetsCreated;
            EventManager.WaypointAddedEvent += OnWaypointAdded;
            EventManager.WaypointSelectedEvent += OnWaypointSelected;
            EventManager.WaypointDeletedEvent += OnWaypointDeleted;
            EventManager.PlanetViewStateUpdatedEvent += OnPlanetViewStateUpdated;
            EventManager.PacketDestinationToggleEvent += OnPacketDestinationToggle;
        }

        public override void _ExitTree()
        {
            EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
            EventManager.GotoMapObjectEvent -= OnGotoMapObject;
            EventManager.GotoMapObjectSpriteEvent -= OnGotoMapObjectSprite;
            EventManager.CommandNextMapObjectEvent -= OnCommandNextMapObject;
            EventManager.CommandPrevMapObjectEvent -= OnCommandPrevMapObject;
            EventManager.CommandMapObjectEvent -= OnCommandMapObject;
            EventManager.SelectMapObjectEvent -= OnSelectMapObject;
            EventManager.FleetDeletedEvent -= OnFleetDeleted;
            EventManager.FleetsCreatedEvent -= OnFleetsCreated;
            EventManager.WaypointAddedEvent -= OnWaypointAdded;
            EventManager.WaypointSelectedEvent -= OnWaypointSelected;
            EventManager.WaypointDeletedEvent -= OnWaypointDeleted;
            EventManager.PlanetViewStateUpdatedEvent -= OnPlanetViewStateUpdated;
            EventManager.PacketDestinationToggleEvent -= OnPacketDestinationToggle;
        }

        #region Scanner MapObject Init

        /// <summary>
        /// Initialize the scanner with new universe data. This is called after the game world has been generated or a new turn has been generated
        /// </summary>
        public void Init()
        {
            AddMapObjectsToViewport();
            UpdateScanners();
            FocusHomeworld();
        }

        /// <summary>
        /// Add all map objects to the viewport
        /// </summary>
        void AddMapObjectsToViewport()
        {
            log.Debug($"{Me.Game.Year} Refreshing transient viewport objects.");
            CommandedFleet = null;
            CommandedPlanet = null;

            // remove old planets, in case we switched players
            Planets.ForEach(oldPlanet =>
            {
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
            var planetsNode = GetNode("Planets");
            Planets.ForEach(p => planetsNode.AddChild(p));
            Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { p }));
            Planets.ForEach(p => p.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { p }));
            Planets.ForEach(p => p.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { p }));

            Planets.ForEach(planetSprite =>
            {
                if (
                    planetSprite.Planet.PacketTarget != null &&
                    MapObjectsByGuid.TryGetValue(planetSprite.Planet.PacketTarget.Guid, out var mapObjectSprite) &&
                    mapObjectSprite is PlanetSprite target)
                {
                    planetSprite.PacketTarget = target;
                }
            });

            // rebuild our MapObjectsByGuid
            Fleets.ForEach(fleet => { if (fleet.Orbiting != null) { fleet.Orbiting.OrbitingFleets.Clear(); } });
            RemoveMapObjects(transientMapObjects);
            MapObjectsByGuid = Planets.Cast<MapObjectSprite>().ToLookup(p => p.MapObject.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            // clear out existing waypoint areas
            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();

            // rebuild our list of transient map objects nad the guid
            transientMapObjects.Clear();
            transientMapObjects.AddRange(AddFleetsToViewport());
            transientMapObjects.AddRange(AddMapObjectsToViewport<Salvage, SalvageSprite>(Me.Salvage, salvageScene, GetNode("Salvage")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<Wormhole, WormholeSprite>(Me.Wormholes, wormholeScene, GetNode("Wormholes")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<MineField, MineFieldSprite>(Me.AllMineFields, mineFieldScene, GetNode("MineFields")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<MineralPacket, MineralPacketSprite>(Me.AllMineralPackets, mineralPacketScene, GetNode("MineralPackets")));

            mapObjects.Clear();
            mapObjects.AddRange(Planets);
            mapObjects.AddRange(transientMapObjects);
            mapObjects.ForEach(mo => mo.UpdateSprite());

            mapObjectsByLocation = mapObjects.ToLookup(mo => mo.MapObject.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());

            log.Debug($"{Me.Game.Year} Done refreshing transient viewport objects.");
        }

        /// <summary>
        /// Remove and Disconnect mapObjects from the scanner
        /// </summary>
        /// <param name="mapObjects"></param>
        /// <typeparam name="T"></typeparam>
        void RemoveMapObjects<T>(List<T> mapObjects) where T : MapObjectSprite
        {
            mapObjects.ForEach(mo =>
            {
                if (IsInstanceValid(mo))
                {
                    mo.GetParent().RemoveChild(mo);
                    mo.Disconnect("input_event", this, nameof(OnInputEvent));
                    mo.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
                    mo.Disconnect("mouse_exited", this, nameof(OnMouseExited));
                    MapObjectsByGuid.Remove(mo.MapObject.Guid);
                    mo.QueueFree();
                }
            });
            mapObjects.Clear();

        }

        /// <summary>
        /// Add map objects to the viewport as sprites
        /// </summary>
        /// <param name="mapObjects"></param>
        /// <param name="spriteScene"></param>
        /// <param name="root"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        List<U> AddMapObjectsToViewport<T, U>(IEnumerable<T> mapObjects, PackedScene spriteScene, Node root)
            where T : MapObject
            where U : MapObjectSprite
        {
            List<U> sprites = new List<U>();

            sprites.AddRange(mapObjects.Select(mapObject =>
            {
                var sprite = spriteScene.Instance() as U;
                sprite.MapObject = mapObject;
                sprite.Position = mapObject.Position;
                MapObjectsByGuid[mapObject.Guid] = sprite;

                root.AddChild(sprite);

                sprite.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { sprite });
                sprite.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { sprite });
                sprite.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { sprite });
                return sprite;
            }));

            return sprites;
        }

        /// <summary>
        /// Add new fleet data to the viewport, clearing out the old data first
        /// </summary>
        List<FleetSprite> AddFleetsToViewport()
        {
            log.Debug("Resetting viewport Fleets");

            Fleets.Clear();
            Fleets.AddRange(AddMapObjectsToViewport<Fleet, FleetSprite>(Me.AllFleets, fleetScene, GetNode("Fleets")));

            Fleets.ForEach(fleetSprite =>
            {
                var fleet = fleetSprite.Fleet;
                if (fleet.Orbiting != null && MapObjectsByGuid.TryGetValue(fleet.Orbiting.Guid, out var mapObjectSprite) && mapObjectSprite is PlanetSprite planetSprite)
                {
                    planetSprite.OrbitingFleets.Add(fleetSprite);
                    fleetSprite.Orbiting = planetSprite;
                }
            });

            Fleets.ForEach(f =>
            {
                f.OtherFleets.Clear();
                if (f.Fleet.OtherFleets?.Count > 0)
                {
                    foreach (var fleet in f.Fleet.OtherFleets)
                    {
                        var fleetSprite = MapObjectsByGuid[fleet.Guid] as FleetSprite;
                        f.OtherFleets.Add(fleetSprite);
                    }
                }
            });

            return Fleets;
        }

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
                EventManager.PublishMapObjectSelectedEvent(homeworld);
                EventManager.PublishMapObjectCommandedEvent(homeworld);
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


        void OnPacketDestinationToggle()
        {
            pickingPacketDestination = !pickingPacketDestination;
            Input.SetDefaultCursorShape(pickingPacketDestination ? Input.CursorShape.Cross : Input.CursorShape.Arrow);
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

                EventManager.PublishWaypointMovedEvent(activeWaypointArea.Fleet, activeWaypointArea.Waypoint);

            }
            else if (Input.IsActionJustReleased("viewport_select"))
            {
                // don't process once a waypoint is no longer being moved
                SetProcess(false);
                movingWaypoint = false;
                UpdateSelectedMapObjectIndicator();
            }
        }

        void OnPlanetViewStateUpdated()
        {
            Planets.ForEach(p => p.UpdateSprite());
            penScannersNode.Visible = normalScannersNode.Visible = Me.UISettings.ShowScanners;
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
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

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

        void OnCommandNextMapObject()
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

        void OnCommandPrevMapObject()
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
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

            // activate this object
            if (mapObjectSprite != null)
            {
                CommandMapObject(mapObjectSprite);
            }
        }

        /// <summary>
        /// If an external source asks us to command a map object, look up the sprite by guid and command it
        /// This is published by the Reports dialog
        /// </summary>
        /// <param name="mapObject"></param>
        void OnSelectMapObject(MapObject mapObject)
        {
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

            // activate this object
            if (mapObjectSprite != null)
            {
                SelectMapObject(mapObjectSprite);
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

            EventManager.PublishMapObjectSelectedEvent(mapObject);
            UpdateSelectedMapObjectIndicator();
        }

        /// <summary>
        /// Command this mapObject, deselecting the previous commandedMapObject if necessary 
        /// </summary>
        /// <param name="mapObject"></param>
        void CommandMapObject(MapObjectSprite mapObject)
        {
            if (mapObject == null || !mapObject.Commandable)
            {
                return;
            }

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
            EventManager.PublishMapObjectCommandedEvent(mapObject);

            SelectMapObject(mapObject);
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
            CommandedFleet?.UpdateSprite();
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
                    EventManager.PublishWaypointSelectedEvent(waypointArea.Waypoint);
                    activeWaypointArea = waypointArea;
                    movingWaypoint = true;
                    SetProcess(true);
                }
            }
            else
            {
                log.Error("Got input event for invalid waypoint.");
            }
        }


        #endregion

        #region Input Events

        /// <summary>
        /// Handle clicks on teh scanner itself to 
        /// </summary>
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                pickingPacketDestination = false;
                Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
            }

            if (@event is InputEventMouse mouse && mouse.Shift)
            {
                pickingPacketDestination = false;
                Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
            }
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
                            Me.Dirty = true;
                            EventManager.PublishPlayerDirtyEvent();
                        }
                    }
                }
            }
            if (@event.IsActionPressed("delete") && selectedWaypoint != null && commandedFleet != null)
            {
                commandedFleet.DeleteWaypoint(selectedWaypoint);
                Me.Dirty = true;
                EventManager.PublishPlayerDirtyEvent();
            }
        }

        void OnMouseEntered(MapObjectSprite mapObject)
        {
            if (IsInstanceValid(mapObject))
            {
                // log.Debug($"Highlighted map object {mapObject.ObjectName}");
                mapObjectsUnderMouse.Add(mapObject);
                EventManager.PublishMapObjectHightlightedEvent(mapObject);
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
                // log.Debug($"Mouse Left map object {mapObject.ObjectName}");
                mapObjectsUnderMouse.Remove(mapObject);
                EventManager.PublishMapObjectHightlightedEvent(mapObject);
            }
            else
            {
                log.Error("OnMouseExited called for invalid mapObject");
            }
        }

        ScannerMapObjectPickerComparer scannerMapObjectPickerComparer = new ScannerMapObjectPickerComparer();
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
                return validMapObjects.OrderBy(mo => mo, scannerMapObjectPickerComparer).Aggregate((curMin, mo) => (curMin == null || (mo.Position.DistanceSquaredTo(localMousePosition)) < curMin.Position.DistanceSquaredTo(localMousePosition) ? mo : curMin));
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

                if (pickingPacketDestination && mapObject is PlanetSprite planet)
                {
                    pickingPacketDestination = false;
                    Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
                    if (CommandedPlanet != null)
                    {
                        CommandedPlanet.Planet.PacketTarget = planet.Planet;
                        CommandedPlanet.PacketTarget = planet;
                        EventManager.PublishPacketDestinationChangedEvent(CommandedPlanet.Planet, planet.Planet);
                        CommandedPlanet.UpdateSprite();
                    }
                }

                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        log.Debug($"Adding waypoint for {commandedFleet.Name} to {closest.Name}");
                        commandedFleet.AddWaypoint(closest.MapObject, selectedWaypoint);
                        Me.Dirty = true;
                        EventManager.PublishPlayerDirtyEvent();
                    }
                }
                else
                {
                    if (mapObjectsByLocation == null)
                    {
                        log.Error("mapObjectsByLocation is null, can't click");
                        return;
                    }
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
                            EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
                        }
                        else if (selectedMapObject == mapObject && selectedMapObject.OwnedByMe)
                        {
                            var newCommandedMapObject = selectedMapObject;
                            commandedMapObjectIndex = 0;
                            log.Debug($"Commanding MapObject {newCommandedMapObject} (index 0)");
                            EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
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
                                    EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        var newSelectedMapObject = mapObject;
                        selectedMapObjectIndex = 0;
                        log.Debug($"Selecting MapObject {newSelectedMapObject} (index 0)");
                        EventManager.PublishSelectMapObjectEvent(newSelectedMapObject.MapObject);
                    }
                }

            }
            else if (@event.IsActionPressed("viewport_alternate_select"))
            {
                log.Debug($"viewport_alternate_select event {mapObject}");
                MapObjectSprite closest = GetClosestMapObjectUnderMouse();

                log.Debug($"Closest: {closest?.ObjectName}");
                if (mapObject != closest)
                {
                    // ignore events from all but the closest mapobject
                    return;
                }

                // let the popupmenu listener know we have a request to pick from objects at this location
                EventManager.PublishViewportAlternateSelect(mapObject);
            }
        }

        #endregion

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
                fleetSprite.MapObject = fleet;
                fleetSprite.Position = fleet.Position;
                MapObjectsByGuid[fleet.Guid] = fleetSprite;
                if (fleet.Orbiting != null && MapObjectsByGuid.TryGetValue(fleet.Orbiting.Guid, out var mapObjectSprite) && mapObjectSprite is PlanetSprite planetSprite)
                {
                    planetSprite.OrbitingFleets.Add(fleetSprite);
                    fleetSprite.Orbiting = planetSprite;
                }

                // Add these fleets to our dictionaries
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

            Fleets.ForEach(fleetSprite =>
            {
                fleetSprite.OtherFleets.Clear();
                if (fleetSprite.Fleet.OtherFleets?.Count > 0)
                {
                    foreach (var fleet in fleetSprite.Fleet.OtherFleets)
                    {
                        if (MapObjectsByGuid.TryGetValue(fleet.Guid, out var mapObjectSprite) && mapObjectSprite is FleetSprite f)
                        {
                            fleetSprite.OtherFleets.Add(f);
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
            MapObjectsByGuid.Remove(fleet.Fleet.Guid);
            Me.MapObjectsByLocation[fleet.Fleet.Position].Remove(fleet.Fleet);
            mapObjectsByLocation[fleet.Fleet.Position].Remove(fleet);

            // make sure any planets we are orbiting don't know about us anymore
            if (fleet.Orbiting != null)
            {
                fleet.Orbiting.OrbitingFleets.Remove(fleet);
            }

            // remove the fleet from the scanner
            RemoveChild(fleet);
            fleet.Disconnect("input_event", this, nameof(OnInputEvent));
            fleet.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
            fleet.Disconnect("mouse_exited", this, nameof(OnMouseExited));
            fleet.QueueFree();
        }

    }
}
