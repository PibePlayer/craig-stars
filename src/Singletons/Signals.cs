using System;
using System.Collections.Generic;
using Godot;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The Signals csharp class services as a way to bridge csharp and gdscript until 
    /// everything is rewritten in .net
    /// </summary>
    public class Signals : Node
    {
        ILog log = LogManager.GetLogger(typeof(Signals));

        public delegate void YearUpdate(int year);
        public static event YearUpdate TurnPassedEvent;

        #region Viewport Events

        public static event Action<MapObjectSprite> MapObjectHighlightedEvent;
        public static event Action<MapObjectSprite> MapObjectSelectedEvent;
        public static event Action<MapObjectSprite> MapObjectActivatedEvent;
        public static event Action<Fleet, Waypoint> WaypointAddedEvent;
        public static event Action<Waypoint> WaypointSelectedEvent;
        public static event Action<Waypoint> WaypointDeletedEvent;

        #endregion

        #region UI Events

        public static event Action<Player> SubmitTurnEvent;
        public static event Action<PlanetSprite> ChangeProductionQueuePressedEvent;
        public static event Action ResearchDialogRequestedEvent;
        public static event Action TechBrowserDialogRequestedEvent;
        public static event Action PlanetViewStateUpdatedEvent;
        public static event Action<Planet> ProductionQueueChangedEvent;
        public delegate void CargoTransferRequested(ICargoHolder source, ICargoHolder dest);
        public static event CargoTransferRequested CargoTransferRequestedEvent;

        #endregion

        #region Server Events

        public static event Action<Fleet> FleetBuiltEvent;

        #endregion

        #region Network Events

        public static event Action ServerDisconnectedEvent;

        public delegate void PreStartGame(List<Player> players);
        public static event PreStartGame PreStartGameEvent;

        public static event YearUpdate PostStartGameEvent;

        #endregion

        #region Player Connection Events

        public delegate void PlayerUpdated(Player player);
        public static event PlayerUpdated PlayerUpdatedEvent;

        public delegate void PlayerJoined(int networkId);
        public static event PlayerJoined PlayerJoinedEvent;

        public delegate void PlayerLeft(int networkId);
        public static event PlayerLeft PlayerLeftEvent;

        public delegate void PlayerReadyToStart(int networkId, bool ready);
        public static event PlayerReadyToStart PlayerReadyToStartEvent;

        public static event Action<PlayerMessage> PlayerMessageEvent;

        #endregion

        // The GDScript signals object
        public static Signals Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            ConfigureLogging();
        }

        /// <summary>
        /// configure the logger we will use
        /// TODO: this should probably be in a different function
        /// </summary>
        void ConfigureLogging()
        {
            const string logLayoutPattern =
                "[%date %timestamp][%level][%stacktracedetail{1}] %message %newline" +
                "%exception %newline";

            var logger = (Logger)log.Logger;
            logger.Hierarchy.Root.Level = Level.All;

            var consoleAppender = new ConsoleAppender
            {
                Name = "ConsoleAppender",
                Layout = new PatternLayout(logLayoutPattern)
            };

            logger.Hierarchy.Root.AddAppender(consoleAppender);
            logger.Hierarchy.Configured = true;

            log.Info("Logging Configured");
        }

        #region Event Publishers

        public static void PublishPreStartGameEvent(List<Player> players)
        {
            PreStartGameEvent?.Invoke(players);
        }

        /// <summary>
        /// Publish a player updated event for any listeners
        /// </summary>
        /// <param name="player"></param>
        /// <param name="notifyPeers">True if we should notify peers of this player data</param>
        public static void PublishPlayerUpdatedEvent(Player player, bool notifyPeers = false)
        {
            PlayerUpdatedEvent?.Invoke(player);
            if (notifyPeers)
            {
                RPC.Instance.SendPlayerUpdated(player);
            }
        }

        public static void PublishPlayerJoinedEvent(int networkId)
        {
            PlayerJoinedEvent?.Invoke(networkId);
        }

        public static void PublishPlayerLeftEvent(int networkId)
        {
            PlayerLeftEvent?.Invoke(networkId);
        }

        public static void PublishPlayerReadyToStart(int networkId, bool ready)
        {
            PlayerReadyToStartEvent?.Invoke(networkId, ready);
        }

        public static void PublishPlayerMessageEvent(PlayerMessage message)
        {
            PlayerMessageEvent?.Invoke(message);
        }

        public static void PublishPostStartGameEvent(int year)
        {
            PostStartGameEvent?.Invoke(year);
        }

        public static void PublishTurnPassedEvent(int year)
        {
            TurnPassedEvent?.Invoke(year);
        }

        # region Scanner Objects

        public static void PublishMapObjectHightlightedEvent(MapObjectSprite mapObjectSprite)
        {
            MapObjectHighlightedEvent?.Invoke(mapObjectSprite);
        }

        public static void PublishMapObjectSelectedEvent(MapObjectSprite mapObjectSprite)
        {
            MapObjectSelectedEvent?.Invoke(mapObjectSprite);
        }

        public static void PublishMapObjectActivatedEvent(MapObjectSprite mapObjectSprite)
        {
            MapObjectActivatedEvent?.Invoke(mapObjectSprite);
        }

        #endregion

        public static void PublishWaypointAddedEvent(Fleet fleet, Waypoint waypoint)
        {
            WaypointAddedEvent?.Invoke(fleet, waypoint);
        }

        public static void PublishServerDisconnectedEvent()
        {
            ServerDisconnectedEvent?.Invoke();
        }

        #region Waypoints

        public static void PublishWaypointSelectedEvent(Waypoint waypoint)
        {
            WaypointSelectedEvent?.Invoke(waypoint);
        }

        public static void PublishWaypointDeletedEvent(Waypoint waypoint)
        {
            WaypointDeletedEvent?.Invoke(waypoint);
        }

        #endregion


        #region Dialog Publishers

        public static void PublishChangeProductionQueuePressedEvent(PlanetSprite planet)
        {
            ChangeProductionQueuePressedEvent?.Invoke(planet);
        }

        public static void PublishResearchDialogRequestedEvent()
        {
            ResearchDialogRequestedEvent?.Invoke();
        }

        public static void PublishTechBrowserDialogRequestedEvent()
        {
            TechBrowserDialogRequestedEvent?.Invoke();
        }

        public static void PublishPlanetViewStateUpdatedEvent()
        {
            PlanetViewStateUpdatedEvent?.Invoke();
        }

        public static void PublishProductionQueueChangedEvent(Planet planet)
        {
            ProductionQueueChangedEvent?.Invoke(planet);
        }

        public static void PublishCargoTransferRequestedEvent(ICargoHolder source, ICargoHolder dest)
        {
            CargoTransferRequestedEvent?.Invoke(source, dest);
        }


        public static void PublishSubmitTurnEvent(Player player)
        {
            SubmitTurnEvent?.Invoke(player);
        }

        internal static void PublishFleetBuiltEvent(Fleet fleet)
        {
            FleetBuiltEvent?.Invoke(fleet);
        }

        #endregion

        #endregion
    }
}
