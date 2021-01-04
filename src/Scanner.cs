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
        PackedScene scannerCoverageScene;
        PackedScene planetScene;
        PackedScene fleetScene;
        MapObjectSprite selectedMapObject;
        MapObjectSprite commandedMapObject;
        SelectedMapObjectSprite selectedMapObjectSprite;
        Node2D normalScannersNode;
        Node2D penScannersNode;

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

            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/WaypointArea.tscn");
            scannerCoverageScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/ScannerCoverage.tscn");
            planetScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/PlanetSprite.tscn");
            fleetScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/FleetSprite.tscn");

            // get some nodes
            selectedMapObjectSprite = GetNode<SelectedMapObjectSprite>("SelectedMapObjectSprite");
            normalScannersNode = GetNode<Node2D>("Scanners/Normal");
            penScannersNode = GetNode<Node2D>("Scanners/Pen");

            // wire up events
            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
        }

        void OnTurnPassed(int year)
        {

            Fleets.ForEach(f => { RemoveChild(f); f.QueueFree(); });
            Fleets.Clear();
            Planets.ForEach(p => p.OrbitingFleets.Clear());
            ActiveFleet = null;
            ActivePlanet = null;

            // update all the sprites
            CallDeferred(nameof(AddFleetsToViewport));
            CallDeferred(nameof(UpdateViewport));
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

        void AddWaypointArea(Waypoint waypoint)
        {
            var waypointArea = waypointAreaScene.Instance() as WaypointArea;
            waypointArea.Waypoint = waypoint;
            waypointAreas.Add(waypointArea);
            AddChild(waypointArea);
        }

        public void AddMapObjects(Player player)
        {

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

            CallDeferred(nameof(UpdateViewport));
        }

        void OnInputEvent(Node viewport, InputEvent @event, int shapeIdx, MapObjectSprite mapObject)
        {
            if (@event.IsActionPressed("viewport_select"))
            {
                GD.Print($"Selected {mapObject.ObjectName}");
                if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Shift)
                {
                    if (commandedMapObject is FleetSprite commandedFleet)
                    {
                        // this was shift+clicked, so let the viewport know it's supposed to be added as a waypoint
                        commandedFleet.AddWaypoint(mapObject.MapObject);
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

        internal void CommandNextPeer(MapObjectSprite mapObject)
        {
            // we selected the object we are commanding again
            // if it has peers, command the next peer
            var peers = mapObject.GetPeers();
            var activePeer = peers.Find(p => p.State == ScannerState.Active);
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
        }

        void AddFleetsToViewport()
        {
            var player = PlayersManager.Instance.Me;


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

        public void UpdateViewport()
        {
            FocusHomeworld();
            UpdateScanners();
            Planets.ForEach(p => p.UpdateSprite());
            Fleets.ForEach(f => f.UpdateSprite());
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
                selectedMapObject = homeworld;
                commandedMapObject = homeworld;

                selectedMapObject.Select();
                commandedMapObject.Activate();
                selectedMapObjectSprite.SelectLarge(commandedMapObject.Position);
                Signals.PublishMapObjectSelectedEvent(homeworld);
                Signals.PublishMapObjectActivatedEvent(homeworld);
            }
        }

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
                    ScannerCoverage scanner = scannerCoverageScene.Instance() as ScannerCoverage;
                    scanner.Position = planet.Position;
                    normalScannersNode.AddChild(scanner);
                    scanner.ScanRange = range;
                    Scanners.Add(scanner);
                }
                if (rangePen > 0)
                {
                    ScannerCoverage scanner = scannerCoverageScene.Instance() as ScannerCoverage;
                    scanner.Position = planet.Position;
                    penScannersNode.AddChild(scanner);
                    scanner.ScanRange = rangePen;
                    scanner.Pen = true;
                    Scanners.Add(scanner);
                }

            }

            foreach (var fleet in Fleets.Where(f => f.OwnedByMe && f.Orbiting == null && f.Fleet.Aggregate.ScanRange > 0))
            {
                var range = fleet.Fleet.Aggregate.ScanRange;
                var rangePen = fleet.Fleet.Aggregate.ScanRangePen;
                if (range > 0)
                {
                    ScannerCoverage scanner = scannerCoverageScene.Instance() as ScannerCoverage;
                    scanner.Position = fleet.Position;
                    normalScannersNode.AddChild(scanner);
                    scanner.ScanRange = range;
                    Scanners.Add(scanner);
                }
                if (rangePen > 0)
                {
                    ScannerCoverage scanner = scannerCoverageScene.Instance() as ScannerCoverage;
                    scanner.Position = fleet.Position;
                    penScannersNode.AddChild(scanner);
                    scanner.ScanRange = rangePen;
                    scanner.Pen = true;
                    Scanners.Add(scanner);
                }
            }
        }

        #endregion
    }
}
