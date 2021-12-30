using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class Scanner : Node2D
    {
        static CSLog log = LogProvider.GetLogger(typeof(Scanner));

        [Export]
        public int ScreenshotSize { get; set; } = 256;

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        /// <summary>
        /// Queue a screenshot for the next time we are visible
        /// </summary>
        bool queueScreenshot;

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
        public Dictionary<Guid, MapObjectSprite> MapObjectsByGuid { get; set; } = new();
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
            waypointAreaScene = CSResourceLoader.GetPackedScene("WaypointArea.tscn");
            scannerCoverageScene = CSResourceLoader.GetPackedScene("ScannerCoverage.tscn");
            planetScene = CSResourceLoader.GetPackedScene("PlanetSprite.tscn");
            fleetScene = CSResourceLoader.GetPackedScene("FleetSprite.tscn");
            salvageScene = CSResourceLoader.GetPackedScene("SalvageSprite.tscn");
            mineFieldScene = CSResourceLoader.GetPackedScene("MineFieldSprite.tscn");
            mineralPacketScene = CSResourceLoader.GetPackedScene("MineralPacketSprite.tscn");
            wormholeScene = CSResourceLoader.GetPackedScene("WormholeSprite.tscn");

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
            EventManager.FleetViewStateUpdatedEvent += OnFleetViewStateUpdated;
            EventManager.ViewStateUpdatedEvent += OnViewStateUpdated;
            EventManager.PacketDestinationToggleEvent += OnPacketDestinationToggle;
            EventManager.GameExitingEvent += OnGameExiting;
            EventManager.SaveScreenshotEvent += OnSaveScreenshot;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
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
                EventManager.FleetViewStateUpdatedEvent -= OnFleetViewStateUpdated;
                EventManager.ViewStateUpdatedEvent -= OnViewStateUpdated;
                EventManager.PacketDestinationToggleEvent -= OnPacketDestinationToggle;
                EventManager.GameExitingEvent -= OnGameExiting;
                EventManager.SaveScreenshotEvent -= OnSaveScreenshot;
            }
        }

        async void OnSaveScreenshot()
        {
            queueScreenshot = true;
            if (GameInfo != null && Me != null)
            {
                var viewport = GetViewport();
                if (viewport != null)
                {
                    await ToSignal(VisualServer.Singleton, "frame_post_draw");
                    GamesManager.Instance.SavePlayerScreenshot(viewport, GameInfo.Name, GameInfo.Year, PlayersManager.Me.Num, ScreenshotSize);
                    queueScreenshot = false;
                }
            }
        }

        /// <summary>
        /// Called after the scanner is initialized
        /// </summary>
        public void AfterScannerReady()
        {
            if (queueScreenshot)
            {
                CallDeferred(nameof(OnSaveScreenshot));
            }
        }

        // Leave this commented out in case I need to debug the universe area/camera again
        // public override void _Draw()
        // {
        //     base._Draw();
        //     if (GameInfo != null)
        //     {
        //         var area = GameInfo.Rules.GetArea(GameInfo.Size);
        //         DrawRect(new Rect2(0, 0, area, area), Colors.Green, false, width: 2);
        //         DrawRect(new Rect2(-50, -50, area + 50 * 2, area + 50 * 2), Colors.Yellow, false, width: 2);
        //     }
        // }

        /// <summary>
        /// Return any pooled resources to the node pool
        /// </summary>
        /// <param name="obj"></param>
        void OnGameExiting(PublicGameInfo obj)
        {
            // give these objects back to the NodePool
            mapObjects.ForEach(mo => { if (IsInstanceValid(mo)) ReturnNode(mo); });
            Scanners.ForEach(s => NodePool.Return(s));
            PenScanners.ForEach(s => NodePool.Return(s));

            // clear our local lists
            mapObjects.Clear();
            Scanners.Clear();
            PenScanners.Clear();
        }

        void ReturnNode(MapObjectSprite value)
        {
            switch (value)
            {
                case PlanetSprite planetSprite:
                    NodePool.Return(planetSprite);
                    break;
                case FleetSprite fleetSprite:
                    NodePool.Return(fleetSprite);
                    break;
                case WormholeSprite wormholeSprite:
                    NodePool.Return(wormholeSprite);
                    break;
                default:
                    value.QueueFree();
                    break;
            }
        }

        #region Scanner MapObject Init

        /// <summary>
        /// Initialize the scanner with new universe data. This is called after the game world has been generated or a new turn has been generated
        /// </summary>
        public void Init()
        {
            AddMapObjectsToViewport();
            UpdateScanners();
        }

        /// <summary>
        /// Update all sprites in the viewport so they are at their correct state
        /// </summary>
        public void UpdateSprites()
        {
            mapObjects.ForEach(mo =>
            {
                // log.Debug($"Updating {mo.GetType()} {mo}");
                mo.UpdateSprite();
            });
        }

        /// <summary>
        /// Add all map objects to the viewport
        /// </summary>
        void AddMapObjectsToViewport()
        {
            log.Debug($"{GameInfo.Year} Refreshing transient viewport objects.");
            CommandedFleet = null;
            CommandedPlanet = null;
            commandedMapObject = null;
            selectedMapObject = null;

            if (Planets.Count == 0)
            {
                Planets.AddRange(Me.AllPlanets.Select(planet =>
                {
                    var planetSprite = NodePool.Get<PlanetSprite>(planetScene);
                    planetSprite.Planet = planet;
                    planetSprite.Position = planet.Position;
                    return planetSprite;
                }));
                var planetsNode = GetNode("Planets");
                Planets.ForEach(p => planetsNode.AddChild(p));
                Planets.ForEach(p => p.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { p }));
                Planets.ForEach(p => p.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { p }));
                Planets.ForEach(p => p.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { p }));
            }
            else
            {
                // reset each planet with data for this player
                Me.AllPlanets.Each((planet, index) =>
                {
                    var planetSprite = Planets[index];
                    planetSprite.Planet = planet;
                    planetSprite.Position = planet.Position;
                    planetSprite.OrbitingFleets.Clear();
                    planetSprite.PacketTarget = null;
                    planetSprite.UpdateSprite();
                });
            }

            log.Debug($"{GameInfo.Year} Refreshed Planets.");

            // rebuild our MapObjectsByGuid
            RemoveMapObjects(transientMapObjects);
            log.Debug($"{GameInfo.Year} Removed previous MapObjects.");
            MapObjectsByGuid = Planets.Cast<MapObjectSprite>().ToLookup(p => p.MapObject.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            // clear out existing waypoint areas
            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();

            // rebuild our list of transient map objects nad the guid
            transientMapObjects.Clear();
            transientMapObjects.AddRange(AddFleetsToViewport());
            log.Debug($"{GameInfo.Year} Refreshed Fleets.");
            transientMapObjects.AddRange(AddMapObjectsToViewport<Salvage, SalvageSprite>(Me.AllSalvage, salvageScene, GetNode("Salvage")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<Wormhole, WormholeSprite>(Me.Wormholes, wormholeScene, GetNode("Wormholes")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<MineField, MineFieldSprite>(Me.AllMineFields, mineFieldScene, GetNode("MineFields")));
            transientMapObjects.AddRange(AddMapObjectsToViewport<MineralPacket, MineralPacketSprite>(Me.AllMineralPackets, mineralPacketScene, GetNode("MineralPackets")));

            GetNode<Node2D>("MineFields").Visible = Me.UISettings.ShowMineFields;

            mapObjects.Clear();
            mapObjects.AddRange(Planets);
            mapObjects.AddRange(transientMapObjects);

            mapObjectsByLocation = mapObjects.ToLookup(mo => mo.MapObject.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());

            // add planet packet destination lines
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

            log.Debug($"{GameInfo.Year} Done refreshing transient viewport objects.");
        }

        /// <summary>
        /// Remove and Disconnect mapObjects from the scanner
        /// </summary>
        /// <param name="mapObjects"></param>
        /// <typeparam name="T"></typeparam>
        void RemoveMapObjects<T>(List<T> mapObjects) where T : MapObjectSprite
        {
            log.Debug($"Removing {mapObjects.Count} {typeof(T)} from viewport.");
            mapObjects.ForEach(mo =>
            {
                if (IsInstanceValid(mo))
                {
                    mo.GetParent().RemoveChild(mo);
                    mo.Disconnect("input_event", this, nameof(OnInputEvent));
                    mo.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
                    mo.Disconnect("mouse_exited", this, nameof(OnMouseExited));
                    MapObjectsByGuid.Remove(mo.MapObject.Guid);
                    ReturnNode(mo);
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
                var sprite = NodePool.Get<U>(spriteScene);
                sprite.MapObject = mapObject;
                sprite.Position = mapObject.Position;
                MapObjectsByGuid[mapObject.Guid] = sprite;

                root.AddChild(sprite);

                sprite.Connect("input_event", this, nameof(OnInputEvent), new Godot.Collections.Array() { sprite });
                sprite.Connect("mouse_entered", this, nameof(OnMouseEntered), new Godot.Collections.Array() { sprite });
                sprite.Connect("mouse_exited", this, nameof(OnMouseExited), new Godot.Collections.Array() { sprite });
                return sprite;
            }));

            log.Debug($"Added {sprites.Count} {typeof(U)} sprites to viewport.");

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
                fleetSprite.UpdateWaypointsLine();
                var fleet = fleetSprite.Fleet;

                if (fleet.Orbiting != null && MapObjectsByGuid.TryGetValue(fleet.Orbiting.Guid, out var mapObjectSprite) && mapObjectSprite is PlanetSprite planetSprite)
                {
                    planetSprite.OrbitingFleets.Add(fleetSprite);
                    fleetSprite.Orbiting = planetSprite;
                }
                else
                {
                    fleetSprite.Orbiting = null;
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
        /// Get the current player's homeworld, or the first inhabited planet if the homeworld isn't under the player's control anymore
        /// </summary>
        /// <returns></returns>
        PlanetSprite GetHomeWorldOrDefault()
        {
            var homeworld = Planets.Where(p => p.Planet.Homeworld && p.Planet.OwnedBy(Me)).FirstOrDefault();
            if (homeworld == null)
            {
                homeworld = Planets.Where(p => p.Planet.OwnedBy(Me)).FirstOrDefault();
            }
            return homeworld;
        }

        /// <summary>
        /// Focus on the current player's homeworld
        /// </summary>
        public void FocusHomeworld()
        {
            log.Debug("Focusing homeworld");
            var homeworld = GetHomeWorldOrDefault();
            if (homeworld != null)
            {
                selectedWaypoint = null;
                CommandMapObject(homeworld, true);

                SelectHomeworld();
            }
        }

        /// <summary>
        /// Select the Homeworld
        /// </summary>
        void SelectHomeworld()
        {
            var homeworld = GetHomeWorldOrDefault();
            if (homeworld != null)
            {
                selectedWaypoint = null;
                selectedMapObject?.Deselect();
                selectedMapObject = homeworld;

                selectedMapObject.Select();
                UpdateSelectedMapObjectIndicator();
                EventManager.PublishMapObjectSelectedEvent(homeworld);
            }
        }

        /// <summary>
        /// Update the scanners for a new turn.
        /// We only create a ScannerCoverage for the largest scanner at a single location
        /// </summary>
        void UpdateScanners()
        {
            log.Debug("Updating Scanner Scanners");
            // clear out the old scanners
            Scanners.ForEach(s => NodePool.Return<ScannerCoverage>(s, returned =>
            {
                returned.GetParent().RemoveChild(returned);
            }));
            Scanners.Clear();

            PenScanners.ForEach(s => NodePool.Return<ScannerCoverage>(s, returned =>
            {
                returned.GetParent().RemoveChild(returned);
            }));
            PenScanners.Clear();

            foreach (var planet in Planets)
            {
                var range = -1;
                var rangePen = -1;

                // if we own this planet and it has a scanner, include it
                if (planet.OwnedByMe && planet.Planet.Scanner)
                {
                    range = (int)(planet.Planet.Spec.ScanRange);
                    rangePen = planet.Planet.Spec.ScanRangePen;
                }

                foreach (var fleet in planet.OrbitingFleets.Where(f => f.Fleet.OwnedBy(Me)))
                {
                    range = Math.Max(range, fleet.Fleet.Spec.ScanRange);
                    rangePen = Math.Max(rangePen, fleet.Fleet.Spec.ScanRangePen);
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

            foreach (var fleet in Fleets.Where(f => f.OwnedByMe && f.Orbiting == null && f.Fleet.Spec.ScanRange > 0))
            {
                var range = fleet.Fleet.Spec.ScanRange;
                var rangePen = fleet.Fleet.Spec.ScanRangePen;
                if (range > 0)
                {
                    AddScannerCoverage(fleet.Position, range, false);
                }
                if (rangePen > 0)
                {
                    AddScannerCoverage(fleet.Position, rangePen, true);
                }
            }

            if (Me.Race.Spec.PacketBuiltInScanner)
            {
                foreach (var packet in Me.MineralPackets)
                {
                    var rangePen = packet.WarpFactor * packet.WarpFactor;
                    if (rangePen > 0)
                    {
                        AddScannerCoverage(packet.Position, rangePen, true);
                    }
                }
            }

            penScannersNode.Visible = normalScannersNode.Visible = Me.UISettings.ShowScanners;

            log.Debug("Finished updating Scanner Scanners");
        }

        void AddScannerCoverage(Vector2 position, int range, bool pen)
        {
            // grab a scanner from our pool
            ScannerCoverage scanner = NodePool.Get<ScannerCoverage>(scannerCoverageScene);
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
            scanner.Update();
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

                // tell the Fleet we moved its waypoint and notify and client UI elements about it as well
                CommandedFleet.OnWaypointMoved();
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

        void OnFleetViewStateUpdated()
        {
            Fleets.ForEach(f => f.UpdateSprite());
            penScannersNode.Visible = normalScannersNode.Visible = Me.UISettings.ShowScanners;
        }

        void OnViewStateUpdated()
        {
            // misc view states, like hiding minefields
            GetNode<Node2D>("MineFields").Visible = Me.UISettings.ShowMineFields;
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
                CommandMapObject(mapObject, true);
            }
            else
            {
                SelectMapObject(mapObject, true);
            }
        }

        void OnGotoMapObject(MapObject mapObject)
        {
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

            if (mapObjectSprite != null)
            {
                if (mapObjectSprite.OwnedByMe)
                {
                    CommandMapObject(mapObjectSprite, true);
                }
                else
                {
                    SelectMapObject(mapObjectSprite, true);
                }
            }
        }

        void OnCommandNextMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (CommandedPlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.OwnedBy(Me)), CommandedPlanet);
            }
            else if (CommandedFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.OwnedBy(Me)), CommandedFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != CommandedPlanet && mapObjectToActivate != CommandedFleet)
            {
                CommandMapObject(mapObjectToActivate, true);
            }
        }

        void OnCommandPrevMapObject()
        {
            MapObjectSprite mapObjectToActivate = null;
            if (CommandedPlanet != null)
            {
                mapObjectToActivate = FindNextObject(Planets.Where(p => p.Planet.OwnedBy(Me)).Reverse(), CommandedPlanet);
            }
            else if (CommandedFleet != null)
            {
                mapObjectToActivate = FindNextObject(Fleets.Where(f => f.Fleet.OwnedBy(Me)).Reverse(), CommandedFleet);
            }

            // activate this object
            if (mapObjectToActivate != null && mapObjectToActivate != CommandedPlanet && mapObjectToActivate != CommandedFleet)
            {
                CommandMapObject(mapObjectToActivate, true);
            }
        }

        /// <summary>
        /// If an external source asks us to command a map object, look up the sprite by guid and command it
        /// This is published by the Reports dialog
        /// </summary>
        /// <param name="mapObject"></param>
        void OnCommandMapObject(MapObject mapObject, bool centerView)
        {
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

            // activate this object
            if (mapObjectSprite != null)
            {
                CommandMapObject(mapObjectSprite, centerView);
            }
        }

        /// <summary>
        /// If an external source asks us to command a map object, look up the sprite by guid and command it
        /// This is published by the Reports dialog
        /// </summary>
        /// <param name="mapObject"></param>
        void OnSelectMapObject(MapObject mapObject, bool centerView)
        {
            MapObjectsByGuid.TryGetValue(mapObject.Guid, out var mapObjectSprite);

            // activate this object
            if (mapObjectSprite != null)
            {
                SelectMapObject(mapObjectSprite, centerView);
            }
        }

        /// <summary>
        /// Select this map object, deselecting the previous map object
        /// if necessary
        /// </summary>
        /// <param name="mapObject"></param>
        void SelectMapObject(MapObjectSprite mapObject, bool centerView)
        {
            // don't deselect the commanded map object in case we selected something else
            if (selectedMapObject != null && selectedMapObject != commandedMapObject)
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
            if (centerView)
            {
                EventManager.PublishCenterViewOnMapObjectEvent(mapObject.MapObject);
            }
            UpdateSelectedMapObjectIndicator();
        }

        /// <summary>
        /// Command this mapObject, deselecting the previous commandedMapObject if necessary 
        /// </summary>
        /// <param name="mapObject"></param>
        void CommandMapObject(MapObjectSprite mapObject, bool centerView)
        {
            if (mapObject == null || !mapObject.Commandable)
            {
                return;
            }

            if (commandedMapObject != null)
            {
                // deselect the current map object
                commandedMapObject.Deselect();
                commandedMapObject.UpdateSprite();
                // disable the active peer for any planets at the last commanded object's location
                foreach (var peer in mapObjectsByLocation[commandedMapObject.Position])
                {
                    if (peer is PlanetSprite planet)
                    {
                        planet.HasCommandedPeer = false;
                        planet.UpdateSprite();
                    }
                }
            }

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
            bool selectedPlanet = false;
            foreach (var peer in mapObjectsAtLocation)
            {
                if (peer is PlanetSprite planet)
                {
                    planet.HasCommandedPeer = true;
                    planet.UpdateSprite();
                    SelectMapObject(planet, false);
                    selectedPlanet = true;
                }
            }
            if (!selectedPlanet)
            {
                SelectMapObject(mapObject, false);
            }
            commandedMapObject.UpdateSprite();
            EventManager.PublishMapObjectCommandedEvent(mapObject);
            if (centerView)
            {
                EventManager.PublishCenterViewOnMapObjectEvent(mapObject.MapObject);
            }

            UpdateSelectedMapObjectIndicator();
        }

        /// <summary>
        /// When the CommandedFleet changes, 
        /// </summary>
        void OnCommandedFleetChanged()
        {
            log.Debug($"CommandedFleetChanged to {(CommandedFleet == null ? "None" : CommandedFleet)}");
            waypointAreas.ForEach(wpa => { if (IsInstanceValid(wpa)) { RemoveChild(wpa); wpa.DisconnectAll(); wpa.QueueFree(); } });
            waypointAreas.Clear();

            // reset the cursor
            Input.SetDefaultCursorShape(Input.CursorShape.Arrow);
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

                            if (commandedMapObjectIndex >= 0 && commandedMapObjectIndex < mapObjectsAtLocation.Count)
                            {
                                var newCommandedMapObject = mapObjectsAtLocation[commandedMapObjectIndex];
                                log.Debug($"Commanding MapObject {newCommandedMapObject} (index {commandedMapObjectIndex})");
                                EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject, centerView: false);
                            }
                            else
                            {
                                log.Error($"Cannot command next MapObject, index {commandedMapObjectIndex} is out of range [0, {mapObjectsAtLocation.Count}]");
                            }
                        }
                        else if (selectedMapObject == mapObject && selectedMapObject.OwnedByMe)
                        {
                            var newCommandedMapObject = selectedMapObject;
                            commandedMapObjectIndex = 0;
                            log.Debug($"Commanding MapObject {newCommandedMapObject} (index 0)");
                            EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject, centerView: false);
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
                                    EventManager.PublishCommandMapObjectEvent(newCommandedMapObject.MapObject, centerView: false);
                                }
                            }
                        }
                    }
                    else
                    {
                        var newSelectedMapObject = mapObject;
                        selectedMapObjectIndex = 0;
                        log.Debug($"Selecting MapObject {newSelectedMapObject} (index 0)");
                        EventManager.PublishSelectMapObjectEvent(newSelectedMapObject.MapObject, centerView: false);
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
                    // if the selected MapObject is at the same location as a commanded MapObject
                    // the selected object indicator will need to be big
                    if (commandedMapObject != null && mapObjectsByLocation[selectedMapObject.Position].Contains(commandedMapObject))
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
                var fleetSprite = NodePool.Get<FleetSprite>(fleetScene);
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

        void OnFleetDeleted(Fleet fleet)
        {
            log.Debug($"User deleted fleet {fleet.Name} - {fleet.Guid}");
            var fleetSprite = Fleets.Find(fleetSprite => fleetSprite.Fleet == fleet);
            Fleets.Remove(fleetSprite);
            // make sure other fleets don't know about us anymore
            fleet.OtherFleets.ForEach(otherFleet => otherFleet.OtherFleets.Remove(fleet));
            MapObjectsByGuid.Remove(fleet.Guid);
            Me.MapObjectsByLocation[fleet.Position].Remove(fleet);
            mapObjectsByLocation[fleet.Position].Remove(fleetSprite);

            // Goto the homeworld if the current fleet is commanded
            if (fleetSprite == CommandedFleet)
            {
                FocusHomeworld();
            }
            if (fleetSprite == selectedMapObject)
            {
                SelectHomeworld();
            }

            // remove the fleet from the scanner
            RemoveChild(fleetSprite);
            fleetSprite.Disconnect("input_event", this, nameof(OnInputEvent));
            fleetSprite.Disconnect("mouse_entered", this, nameof(OnMouseEntered));
            fleetSprite.Disconnect("mouse_exited", this, nameof(OnMouseExited));
            fleetSprite.QueueFree();
        }

    }
}
