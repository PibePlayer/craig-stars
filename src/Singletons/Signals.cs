using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The Signals csharp class services as a way to bridge csharp and gdscript until 
    /// everything is rewritten in .net
    /// </summary>
    public class Signals : Node
    {
        public delegate void YearUpdate(int year);
        public static event YearUpdate TurnPassedEvent;

        #region Viewport Events

        public static event Action<MapObjectSprite> MapObjectSelectedEvent;
        public static event Action<MapObjectSprite> MapObjectActivatedEvent;
        public static event Action<Fleet, Waypoint> WaypointAddedEvent;
        public static event Action<Waypoint> WaypointSelectedEvent;
        public static event Action<Waypoint> WaypointDeletedEvent;

        #endregion

        #region UI Events

        public static event Action<Player> SubmitTurnEvent;
        public static event Action<PlanetSprite> ChangeProductionQueuePressedEvent;

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

        Signals()
        {
            Instance = this;
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

        public static void PublishMapObjectSelectedEvent(MapObjectSprite mapObjectSprite)
        {
            MapObjectSelectedEvent?.Invoke(mapObjectSprite);
        }

        public static void PublishMapObjectActivatedEvent(MapObjectSprite mapObjectSprite)
        {
            MapObjectActivatedEvent?.Invoke(mapObjectSprite);
        }

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

        public static void PublishSubmitTurnEvent(Player player)
        {
            SubmitTurnEvent?.Invoke(player);
        }

        #endregion

        #endregion
    }
}
