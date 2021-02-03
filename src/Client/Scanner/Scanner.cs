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
        MapObjectSprite selectedMapObject;
        MapObjectSprite commandedMapObject;
        MapObjectSprite highlightedMapObject;
        Waypoint selectedWaypoint;
        List<MapObjectSprite> mapObjectsUnderMouse = new List<MapObjectSprite>();
        SelectedMapObjectSprite selectedMapObjectSprite;
        Node2D normalScannersNode;
        Node2D penScannersNode;
        Camera2D camera2D;

        public List<PlanetSprite> Planets { get; } = new List<PlanetSprite>();
        public List<FleetSprite> Fleets { get; } = new List<FleetSprite>();
        public Dictionary<Guid, PlanetSprite> PlanetsByGuid { get; set; } = new Dictionary<Guid, PlanetSprite>();
        public List<WaypointArea> waypointAreas = new List<WaypointArea>();
        public List<ScannerCoverage> Scanners { get; set; } = new List<ScannerCoverage>();
        public List<ScannerCoverage> PenScanners { get; set; } = new List<ScannerCoverage>();

        public FleetSprite ActiveFleet
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
        FleetSprite activeFleet;

        public PlanetSprite ActivePlanet { get; set; }
        public Player Me { get; set; }

        public override void _Ready()
        {
            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/WaypointArea.tscn");
            scannerCoverageScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/ScannerCoverage.tscn");
            planetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/PlanetSprite.tscn");
            fleetScene = ResourceLoader.Load<PackedScene>("res://src/Client/Scanner/FleetSprite.tscn");

            // get some nodes
            selectedMapObjectSprite = GetNode<SelectedMapObjectSprite>("SelectedMapObjectSprite");
            normalScannersNode = GetNode<Node2D>("Scanners/Normal");
            penScannersNode = GetNode<Node2D>("Scanners/Pen");
            camera2D = GetNode<Camera2D>("Camera2D");

            // wire up events
            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.ActiveNextMapObjectEvent += OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent += OnActivePrevMapObject;
            Signals.WaypointAddedEvent += OnWaypointAdded;
            Signals.WaypointSelectedEvent += OnWaypointSelected;
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent += OnPlanetViewStateUpdatedEvent;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.ActiveNextMapObjectEvent -= OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent -= OnActivePrevMapObject;
            Signals.WaypointAddedEvent -= OnWaypointAdded;
            Signals.WaypointSelectedEvent -= OnWaypointSelected;
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent -= OnPlanetViewStateUpdatedEvent;
        }

        void OnPlanetViewStateUpdatedEvent()
        {
            Planets.ForEach(p => p.UpdateSprite());
        }

        void OnTurnPassed(int year)
        {
            // update all the sprites
            CallDeferred(nameof(AddFleetsToViewport));
            CallDeferred(nameof(ResetScannerToHome));
        }

        void OnMapObjectActivated(MapObjectSprite mapObject)
        {
            ActiveFleet = mapObject as FleetSprite;
            ActivePlanet = mapObject as PlanetSprite;
        }

        // Find the next object in a list after our currently active object. This loops back to the beginning
        MapObjectSprite FindNextObject<T>(IEnumerable<T> items, T currentlyActive) where T : MapObjectSprite
        {
            var first = items.First();
            var next = items.SkipWhile(item => item != currentlyActive).Skip(1).FirstOrDefault();
            return next != null ? next : first;
        }

        void OnActiveNextMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (ActivePlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.Player == Me), ActivePlanet);
            }
            else if (ActiveFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.Player == Me), ActiveFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != ActivePlanet && mapObjectToActivate != ActiveFleet)
            {
                CommandMapObject(mapObjectToActivate);
            }
        }

        void OnActivePrevMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (ActivePlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.Player == Me).Reverse(), ActivePlanet);
            }
            else if (ActiveFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.Player == Me).Reverse(), ActiveFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != ActivePlanet && mapObjectToActivate != ActiveFleet)
            {
                CommandMapObject(mapObjectToActivate);
            }
        }

        // TODO: We need to share this functionality with the various mouse click stuff
        // It's not good for it to be all over
        void CommandMapObject(MapObjectSprite mapObjectToActivate)
        {
            selectedMapObject.Deselect();
            selectedMapObject = mapObjectToActivate;
            commandedMapObject = mapObjectToActivate;
            commandedMapObject.Command();
            commandedMapObject.UpdateSprite();
            Signals.PublishMapObjectSelectedEvent(mapObjectToActivate);
            Signals.PublishMapObjectActivatedEvent(mapObjectToActivate);
            UpdateSelectedIndicator();
        }

        /// <summary>
        /// When the ActiveFleet changes, 
        /// </summary>
        void OnActiveFleetChanged()
        {
            try
            {
                waypointAreas.ForEach(wpa => { RemoveChild(wpa); wpa.QueueFree(); });
            }
            catch (ObjectDisposedException e)
            {
                log.Error("Failed to free disposed WaypointArea.", e);
            }
            waypointAreas.Clear();
            selectedWaypoint = null;
            ActiveFleet?.Fleet?.Waypoints.Each((wp, index) => AddWaypointArea(wp));
        }

        /// <summary>
        /// When a waypoint is added, add an area for it so we can
        /// select it
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="waypoint"></param>
        void OnWaypointAdded(Fleet fleet, Waypoint waypoint)
        {
            AddWaypointArea(waypoint);
            selectedWaypoint = waypoint;
            UpdateSelectedIndicator();
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
            UpdateSelectedIndicator();
        }

        void AddWaypointArea(Waypoint waypoint)
        {
            var waypointArea = waypointAreaScene.Instance() as WaypointArea;
            waypointArea.Waypoint = waypoint;
            waypointAreas.Add(waypointArea);
            // waypointArea.Connect("input_event", this, nameof(OnInputEvent));
            AddChild(waypointArea);
        }

        public void InitMapObjects()
        {
            // setup the current player
            Me = PlayersManager.Instance.Me;

            Planets.AddRange(Me.Planets.Select(planet =>
            {
                var planetSprite = planetScene.Instance() as PlanetSprite;
                planetSprite.Planet = planet;
                planetSprite.Position = planet.Position;
                return planetSprite;
            }));
            Planets.ForEach(p => AddChild(p));
            Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent)));
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
                        }
                    }
                }
            }
            if (@event.IsActionPressed("delete_waypoint") && selectedWaypoint != null && activeFleet != null)
            {
                activeFleet.DeleteWaypoint(selectedWaypoint);
            }
        }

        void OnMouseEntered(MapObjectSprite mapObject)
        {
            log.Debug($"Highlighted map object {mapObject.ObjectName}");
            mapObjectsUnderMouse.Add(mapObject);
            Signals.PublishMapObjectHightlightedEvent(mapObject);
        }

        void OnMouseExited(MapObjectSprite mapObject)
        {
            mapObjectsUnderMouse.Remove(mapObject);
            Signals.PublishMapObjectHightlightedEvent(mapObject);
        }


        /// <summary>
        /// Respond to mouse events on our map objects in the Scanner
        /// If the player clicks an object, it should select it. If they click it again, and the player
        /// owns the object, it should activate it. Subsequent clicks should cycle through all mapObjects
        /// at the same point on the scanner
        /// </summary>
        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                var localMousePosition = GetLocalMousePosition();
                MapObjectSprite closest = mapObjectsUnderMouse.Aggregate((curMin, mo) => (curMin == null || (mo.Position.DistanceSquaredTo(localMousePosition)) < curMin.Position.DistanceSquaredTo(localMousePosition) ? mo : curMin));

                log.Debug($"Clicked {closest.ObjectName}");
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        log.Debug($"Adding waypoint for {commandedFleet.Name} to {closest.Name}");
                        commandedFleet.AddWaypoint(closest.MapObject, selectedWaypoint);
                    }
                }
                else
                {
                    if (closest != selectedMapObject)
                    {
                        // We selected a new MapObject. Select it and update our sprite
                        if (selectedMapObject?.State == ScannerState.Selected)
                        {
                            log.Debug($"Deselected map object {selectedMapObject.ObjectName}");
                            // deselect the old one if it's selected.
                            selectedMapObject?.Deselect();
                        }
                        selectedMapObject = closest;
                        if (selectedMapObject.State != ScannerState.Commanded)
                        {
                            log.Debug($"Selected map object {selectedMapObject.ObjectName}");
                            selectedMapObject.Select();
                            selectedWaypoint = null;
                        }
                        UpdateSelectedIndicator();


                        Signals.PublishMapObjectSelectedEvent(selectedMapObject);
                    }
                    else if (closest == selectedMapObject)
                    {
                        if (closest.OwnedByMe)
                        {
                            if (commandedMapObject != null && commandedMapObject == closest || closest.HasActivePeer())
                            {
                                CommandNextPeer(commandedMapObject);
                            }
                            else
                            {
                                // we aren't commanding anything yet, so command this
                                commandedMapObject?.Deselect();
                                commandedMapObject = closest;

                                log.Debug($"Commanded map object {commandedMapObject.ObjectName}");
                            }
                        }
                        else if (closest.GetPeers().Count > 0)
                        {
                            // this closest isn't owned by me.
                            CommandNextPeer(closest);
                        }

                        commandedMapObject.Command();
                        UpdateSelectedIndicator();
                        Signals.PublishMapObjectActivatedEvent(commandedMapObject);
                    }
                }

            }
        }

        /// <summary>
        /// Update the sprite showing which object is selected. If the object is commanded, or it has a commanded fleet, we show the bigger icon
        /// </summary>
        void UpdateSelectedIndicator()
        {
            if (selectedWaypoint != null)
            {
                selectedMapObjectSprite.Select(selectedWaypoint.Position);
            }
            else
            {
                if (selectedMapObject == commandedMapObject || (commandedMapObject != null && commandedMapObject.GetPeers().Contains(selectedMapObject)))
                {
                    selectedMapObjectSprite.SelectLarge(selectedMapObject.Position);
                }
                else
                {
                    selectedMapObjectSprite.Select(selectedMapObject.Position);
                }

            }

        }

        void CommandNextPeer(MapObjectSprite mapObject)
        {
            // we selected the object we are commanding again
            // if it has peers, command the next peer
            var peers = mapObject.GetPeers();
            var activePeer = peers.Find(p => p.State == ScannerState.Commanded);
            if (peers.Count > 0)
            {
                if (commandedMapObject is PlanetSprite)
                {
                    // leave planets selected
                    commandedMapObject.State = ScannerState.Selected;
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
                        commandedMapObject = activePeer.GetPeers()[0] as MapObjectSprite;
                    }
                }
                else
                {
                    // command the first peer
                    commandedMapObject = peers[0] as MapObjectSprite;
                }
            }

            GD.Print($"Commanding Next Peer {commandedMapObject.ObjectName}");
        }

        /// <summary>
        /// Add new fleet data to the viewport, clearing out the old data first
        /// </summary>
        void AddFleetsToViewport()
        {
            log.Debug("Resetting viewport Fleets");
            var player = PlayersManager.Instance.Me;

            // clear out any existing fleets
            Fleets.ForEach(f => { RemoveChild(f); f.QueueFree(); });
            waypointAreas.ForEach(wpa => { RemoveChild(wpa); wpa.QueueFree(); });
            Fleets.Clear();
            waypointAreas.Clear();
            Planets.ForEach(p => p.OrbitingFleets.Clear());
            ActiveFleet = null;
            ActivePlanet = null;

            // add in new fleets
            Fleets.AddRange(player.Fleets.Select(fleet =>
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

            Fleets.ForEach(f => AddChild(f));
            Fleets.ForEach(f => f.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { f }));
            Fleets.ForEach(f => f.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { f }));
            Fleets.ForEach(f => f.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { f }));
        }

        public void ResetScannerToHome()
        {
            log.Debug("Resetting Scanner to homeview");
            FocusHomeworld();
            UpdateScanners();
            log.Debug("Updating Sprites");
            Planets.ForEach(p => p.UpdateSprite());
            Fleets.ForEach(f => f.UpdateSprite());
            log.Debug("Finished Updating Sprites");
        }

        #region New Turn functions

        /// <summary>
        /// Focus on the current player's homeworld
        /// </summary>
        void FocusHomeworld()
        {
            var homeworld = Planets.Where(p => p.Planet.Homeworld && p.Planet.Player == PlayersManager.Instance.Me).FirstOrDefault();
            if (homeworld != null)
            {
                selectedWaypoint = null;
                selectedMapObject?.Deselect();
                commandedMapObject?.Deselect();
                selectedMapObject = homeworld;
                commandedMapObject = homeworld;

                selectedMapObject.Select();
                commandedMapObject.Command();
                UpdateSelectedIndicator();
                Signals.PublishMapObjectSelectedEvent(homeworld);
                Signals.PublishMapObjectActivatedEvent(homeworld);
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
