using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Singletons;
using log4net;

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
            // setup the current player
            Me = PlayersManager.Instance.Me;

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
            Signals.WaypointAddedEvent += OnWaypointAdded;
            Signals.PlanetViewStateUpdatedEvent += OnPlanetViewStateUpdatedEvent;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
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

        /// <summary>
        /// When the ActiveFleet changes, 
        /// </summary>
        void OnActiveFleetChanged()
        {
            waypointAreas.ForEach(wpa => { wpa.QueueFree(); });
            waypointAreas.Clear();
            ActiveFleet?.Fleet?.Waypoints.ForEach(wp => AddWaypointArea(wp));
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
            var player = PlayersManager.Instance.Me;
            Planets.AddRange(player.Planets.Select(planet =>
            {
                var planetSprite = planetScene.Instance() as PlanetSprite;
                planetSprite.Planet = planet;
                planetSprite.Position = planet.Position;
                return planetSprite;
            }));
            Planets.ForEach(p => AddChild(p));
            Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { p }));
            PlanetsByGuid = Planets.ToLookup(p => p.Planet.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            AddFleetsToViewport();

            CallDeferred(nameof(ResetScannerToHome));
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
                log.Debug($"Clicked {mapObject.ObjectName}");
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        log.Debug($"Adding waypoint for {commandedFleet.Name} to {mapObject.Name}");
                        commandedFleet.AddWaypoint(mapObject.MapObject);
                    }
                }
                else
                {
                    if (mapObject != selectedMapObject)
                    {
                        // We selected a new MapObject. Select it and update our sprite
                        if (selectedMapObject?.State == ScannerState.Selected)
                        {
                            log.Debug($"Deselected map object {selectedMapObject.ObjectName}");
                            // deselect the old one if it's selected.
                            selectedMapObject?.Deselect();
                        }
                        selectedMapObject = mapObject;
                        if (selectedMapObject.State != ScannerState.Commanded)
                        {
                            log.Debug($"Selected map object {selectedMapObject.ObjectName}");
                            selectedMapObject.Select();
                        }
                        UpdateSelectedIndicator();


                        Signals.PublishMapObjectSelectedEvent(selectedMapObject);
                    }
                    else if (mapObject == selectedMapObject)
                    {
                        if (mapObject.OwnedByMe)
                        {
                            if (commandedMapObject != null && commandedMapObject == mapObject || mapObject.HasActivePeer())
                            {
                                CommandNextPeer(commandedMapObject);
                            }
                            else
                            {
                                // we aren't commanding anything yet, so command this
                                commandedMapObject?.Deselect();
                                commandedMapObject = mapObject;

                                log.Debug($"Commanded map object {commandedMapObject.ObjectName}");
                            }
                        }
                        else if (mapObject.GetPeers().Count > 0)
                        {
                            // this mapObject isn't owned by me.
                            CommandNextPeer(mapObject);
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
            if (selectedMapObject == commandedMapObject || (commandedMapObject != null && commandedMapObject.GetPeers().Contains(selectedMapObject)))
            {
                selectedMapObjectSprite.SelectLarge(selectedMapObject.Position);
            }
            else
            {
                selectedMapObjectSprite.Select(selectedMapObject.Position);
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
            Fleets.Clear();
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
            Scanners.ForEach(s => s.QueueFree());
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
