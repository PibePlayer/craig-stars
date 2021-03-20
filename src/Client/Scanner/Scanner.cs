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
        public Dictionary<Guid, FleetSprite> FleetsByGuid { get; set; } = new Dictionary<Guid, FleetSprite>();
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
        Player Me { get => PlayersManager.Me; }

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
            Signals.GotoMapObjectEvent += OnGotoMapObject;
            Signals.GotoMapObjectSpriteEvent += OnGotoMapObjectSprite;
            Signals.ActiveNextMapObjectEvent += OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent += OnActivePrevMapObject;
            Signals.CommandMapObjectEvent += OnCommandMapObject;
            Signals.SelectMapObjectEvent += OnSelectMapObject;
            Signals.FleetDeletedEvent += OnFleetDeleted;
            Signals.WaypointAddedEvent += OnWaypointAdded;
            Signals.WaypointSelectedEvent += OnWaypointSelected;
            Signals.WaypointDeletedEvent += OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent += OnPlanetViewStateUpdatedEvent;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.GotoMapObjectEvent -= OnGotoMapObject;
            Signals.GotoMapObjectSpriteEvent -= OnGotoMapObjectSprite;
            Signals.ActiveNextMapObjectEvent -= OnActiveNextMapObject;
            Signals.ActivePrevMapObjectEvent -= OnActivePrevMapObject;
            Signals.CommandMapObjectEvent -= OnCommandMapObject;
            Signals.SelectMapObjectEvent -= OnSelectMapObject;
            Signals.FleetDeletedEvent -= OnFleetDeleted;
            Signals.WaypointAddedEvent -= OnWaypointAdded;
            Signals.WaypointSelectedEvent -= OnWaypointSelected;
            Signals.WaypointDeletedEvent -= OnWaypointDeleted;
            Signals.PlanetViewStateUpdatedEvent -= OnPlanetViewStateUpdatedEvent;
        }

        void OnPlanetViewStateUpdatedEvent()
        {
            Planets.ForEach(p => p.UpdateSprite());
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            // update all the sprites
            mapObjectsUnderMouse.Clear();
            waypointAreas.Clear();
            CallDeferred(nameof(AddFleetsToViewport));
            CallDeferred(nameof(ResetScannerToHome));
        }

        void OnMapObjectActivated(MapObjectSprite mapObject)
        {
            ActiveFleet = mapObject as FleetSprite;
            ActivePlanet = mapObject as PlanetSprite;
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
            if (mapObjectToActivate != null && mapObjectToActivate != ActivePlanet && mapObjectToActivate != ActiveFleet)
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
            if (mapObjectToSelect != null && mapObjectToSelect != ActivePlanet && mapObjectToSelect != ActiveFleet)
            {
                SelectMapObject(mapObjectToSelect);
            }
        }

        void SelectMapObject(MapObjectSprite mapObject)
        {
            selectedMapObject.Deselect();
            selectedMapObject = mapObject;
            selectedMapObject.Select();
            selectedMapObject.UpdateSprite();
            Signals.PublishMapObjectSelectedEvent(mapObject);
            UpdateSelectedIndicator();
        }

        // TODO: We need to share this functionality with the various mouse click stuff
        // It's not good for it to be all over
        void CommandMapObject(MapObjectSprite mapObject)
        {
            selectedMapObject.Deselect();
            selectedMapObject = mapObject;
            commandedMapObject = mapObject;
            commandedMapObject.Command();
            commandedMapObject.UpdateSprite();
            Signals.PublishMapObjectSelectedEvent(mapObject);
            Signals.PublishMapObjectActivatedEvent(mapObject);
            UpdateSelectedIndicator();
        }

        /// <summary>
        /// When the ActiveFleet changes, 
        /// </summary>
        void OnActiveFleetChanged()
        {
            log.Info("ActiveFleetChanged begin");
            waypointAreas.ForEach(wpa =>
            {
                if (IsInstanceValid(wpa))
                {
                    RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree();
                }
            });
            waypointAreas.Clear();
            selectedWaypoint = null;
            ActiveFleet?.Fleet?.Waypoints.Each((wp, index) => AddWaypointArea(wp));
            log.Info("ActiveFleetChanged end");
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
            AddChild(waypointArea);
        }

        public void InitMapObjects()
        {
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
            if (@event.IsActionPressed("delete") && selectedWaypoint != null && activeFleet != null)
            {
                activeFleet.DeleteWaypoint(selectedWaypoint);
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
        /// Respond to mouse events on our map objects in the Scanner
        /// If the player clicks an object, it should select it. If they click it again, and the player
        /// owns the object, it should activate it. Subsequent clicks should cycle through all mapObjects
        /// at the same point on the scanner
        /// </summary>
        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx, MapObjectSprite mapObjectSprite)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                log.Debug("viewport_select event");
                var localMousePosition = GetLocalMousePosition();
                MapObjectSprite closest = mapObjectsUnderMouse.Aggregate((curMin, mo) => (curMin == null || (mo.Position.DistanceSquaredTo(localMousePosition)) < curMin.Position.DistanceSquaredTo(localMousePosition) ? mo : curMin));

                log.Debug($"Clicked {closest.ObjectName}");
                if (mapObjectSprite != closest)
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

            log.Debug($"Commanding Next Peer {commandedMapObject.ObjectName}");
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

            waypointAreas.ForEach(wpa => { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); });
            waypointAreas.Clear();
            Planets.ForEach(p => p.OrbitingFleets.Clear());
            ActiveFleet = null;
            ActivePlanet = null;

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

        void OnFleetDeleted(FleetSprite fleet)
        {
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
