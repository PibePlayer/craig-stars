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
        MapObject selectedMapObject;
        MapObject commandedMapObject;
        SelectedMapObjectSprite selectedMapObjectSprite;
        Node2D normalScannersNode;
        Node2D penScannersNode;

        public List<Planet> Planets { get; } = new List<Planet>();
        public List<Fleet> Fleets { get; } = new List<Fleet>();
        public List<WaypointArea> waypointAreas = new List<WaypointArea>();
        public List<ScannerCoverage> Scanners { get; set; } = new List<ScannerCoverage>();
        public List<ScannerCoverage> PenScanners { get; set; } = new List<ScannerCoverage>();

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
        public Player Me { get; set; }

        public override void _Ready()
        {
            // setup the current player
            Me = PlayersManager.Instance.Me;

            waypointAreaScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/WaypointArea.tscn");
            scannerCoverageScene = ResourceLoader.Load<PackedScene>("res://src/GameObjects/ScannerCoverage.tscn");

            // get some nodes
            selectedMapObjectSprite = GetNode<SelectedMapObjectSprite>("SelectedMapObjectSprite");
            normalScannersNode = GetNode<Node2D>("Scanners/Normal");
            penScannersNode = GetNode<Node2D>("Scanners/Pen");

            // wire up events
            Signals.TurnPassedEvent += OnTurnPassed;
            Signals.MapObjectActivatedEvent += OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent += OnMapObjectWaypointAdded;
        }

        public override void _ExitTree()
        {
            Signals.TurnPassedEvent -= OnTurnPassed;
            Signals.MapObjectActivatedEvent -= OnMapObjectActivated;
            Signals.MapObjectWaypointAddedEvent -= OnMapObjectWaypointAdded;
        }

        void OnTurnPassed(int year)
        {
            FocusHomeworld();
            UpdateScanners();
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
                if (planet.Player == Me && planet.Scanner && Me.PlanetaryScanner != null)
                {
                    range = Me.PlanetaryScanner.ScanRange;
                    rangePen = Me.PlanetaryScanner.ScanRangePen;
                }

                foreach (var fleet in planet.OrbitingFleets.Where(f => f.Player == Me))
                {
                    range = Math.Max(range, fleet.Aggregate.ScanRange);
                    rangePen = Math.Max(rangePen, fleet.Aggregate.ScanRangePen);
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

            foreach (var fleet in Fleets.Where(f => f.Player == Me && f.Orbiting == null && f.Aggregate.ScanRange > 0))
            {
                var range = fleet.Aggregate.ScanRange;
                var rangePen = fleet.Aggregate.ScanRangePen;
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
