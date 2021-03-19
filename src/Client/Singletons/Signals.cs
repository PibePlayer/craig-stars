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
    public static class Signals
    {
        static ILog log = LogManager.GetLogger(typeof(Signals));

        public delegate void YearUpdate(PublicGameInfo gameInfo);
        public delegate void GameStart(PublicGameInfo gameInfo);
        public static event YearUpdate TurnPassedEvent;
        public static event Action TurnGeneratingEvent;
        public static event Action<TurnGeneratorState> TurnGeneratorAdvancedEvent;

        public static void PublishTurnPassedEvent(PublicGameInfo gameInfo) => TurnPassedEvent?.Invoke(gameInfo);
        public static void PublishTurnGeneratorAdvancedEvent(TurnGeneratorState state) => TurnGeneratorAdvancedEvent?.Invoke(state);
        public static void PublishTurnGeneratingEvent() => TurnGeneratingEvent?.Invoke();

        #region Viewport Events

        public static event Action<MapObjectSprite> MapObjectHighlightedEvent;
        public static event Action<MapObjectSprite> MapObjectSelectedEvent;
        public static event Action<MapObjectSprite> MapObjectActivatedEvent;
        public static event Action<MapObject> CommandMapObjectEvent;
        public static event Action<MapObject> SelectMapObjectEvent;
        public static event Action<Fleet, Waypoint> WaypointAddedEvent;
        public static event Action<Waypoint> WaypointSelectedEvent;
        public static event Action<Waypoint> WaypointDeletedEvent;

        #endregion

        #region UI Events

        public static event Action PlayerDirtyEvent;
        public static event Action<Player> SubmitTurnEvent;
        public static event Action<PlanetSprite> ChangeProductionQueuePressedEvent;
        public static event Action ResearchDialogRequestedEvent;
        public static event Action ReportsDialogRequestedEvent;
        public static event Action TechBrowserDialogRequestedEvent;
        public static event Action ShipDesignerDialogRequestedEvent;

        public static event Action PlanetViewStateUpdatedEvent;
        public static event Action<Planet> ProductionQueueChangedEvent;
        public delegate void CargoTransferRequested(ICargoHolder source, ICargoHolder dest);
        public static event CargoTransferRequested CargoTransferRequestedEvent;

        public static event Action ActiveNextMapObjectEvent;
        public static event Action ActivePrevMapObjectEvent;
        public static event Action<MapObjectSprite> GotoMapObjectSpriteEvent;
        public static event Action<MapObject> GotoMapObjectEvent;
        public static event Action<FleetSprite> RenameFleetRequestedEvent;
        #endregion

        #region Network Events

        public static event Action ServerDisconnectedEvent;

        public delegate void PreStartGame(List<PublicPlayerInfo> players);
        public static event PreStartGame PreStartGameEvent;

        public static event GameStart PostStartGameEvent;
        public static void PublishPostStartGameEvent(PublicGameInfo gameInfo) => PostStartGameEvent?.Invoke(gameInfo);


        #endregion

        #region Player Connection Events

        public delegate void PlayerUpdated(PublicPlayerInfo player);
        public static event PlayerUpdated PlayerUpdatedEvent;

        public delegate void PlayerJoined(int networkId);
        public static event PlayerJoined PlayerJoinedEvent;

        public delegate void PlayerLeft(int networkId);
        public static event PlayerLeft PlayerLeftEvent;

        public delegate void PlayerReadyToStart(int networkId, bool ready);
        public static event PlayerReadyToStart PlayerReadyToStartEvent;

        public static event Action<PlayerMessage> PlayerMessageEvent;

        #endregion

        #region Event Publishers

        public static void PublishPreStartGameEvent(List<PublicPlayerInfo> players) => PreStartGameEvent?.Invoke(players);

        /// <summary>
        /// Publish a player updated event for any listeners
        /// </summary>
        /// <param name="player"></param>
        /// <param name="notifyPeers">True if we should notify peers of this player data</param>
        public static void PublishPlayerUpdatedEvent(PublicPlayerInfo player, bool notifyPeers = false)
        {
            PlayerUpdatedEvent?.Invoke(player);
            if (notifyPeers)
            {
                RPC.Instance.SendPlayerUpdated(player);
            }
        }

        public static void PublishPlayerJoinedEvent(int networkId) => PlayerJoinedEvent?.Invoke(networkId);
        public static void PublishPlayerLeftEvent(int networkId) => PlayerLeftEvent?.Invoke(networkId);
        public static void PublishPlayerReadyToStart(int networkId, bool ready) => PlayerReadyToStartEvent?.Invoke(networkId, ready);
        public static void PublishPlayerMessageEvent(PlayerMessage message) => PlayerMessageEvent?.Invoke(message);


        # region Scanner Objects
        public static void PublishPlayerDirtyEvent() => PlayerDirtyEvent?.Invoke();
        public static void PublishMapObjectHightlightedEvent(MapObjectSprite mapObjectSprite) => MapObjectHighlightedEvent?.Invoke(mapObjectSprite);
        public static void PublishMapObjectSelectedEvent(MapObjectSprite mapObjectSprite) => MapObjectSelectedEvent?.Invoke(mapObjectSprite);
        public static void PublishMapObjectActivatedEvent(MapObjectSprite mapObjectSprite) => MapObjectActivatedEvent?.Invoke(mapObjectSprite);
        public static void PublishCommandMapObjectEvent(MapObject mapObject) => CommandMapObjectEvent?.Invoke(mapObject);
        public static void PublishSelectMapObjectEvent(MapObject mapObject) => SelectMapObjectEvent?.Invoke(mapObject);
        public static void PublishActiveNextMapObjectEvent() => ActiveNextMapObjectEvent?.Invoke();
        public static void PublishActivePrevMapObjectEvent() => ActivePrevMapObjectEvent?.Invoke();
        public static void PublishRenameFleetRequestedEvent(FleetSprite fleetSprite) => RenameFleetRequestedEvent?.Invoke(fleetSprite);
        public static void PublishGotoMapObjectEvent(MapObjectSprite mapObjectSprite) => GotoMapObjectSpriteEvent?.Invoke(mapObjectSprite);
        public static void PublishGotoMapObjectEvent(MapObject mapObject) => GotoMapObjectEvent?.Invoke(mapObject);

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

        public static void PublishChangeProductionQueuePressedEvent(PlanetSprite planet) => ChangeProductionQueuePressedEvent?.Invoke(planet);
        public static void PublishResearchDialogRequestedEvent() => ResearchDialogRequestedEvent?.Invoke();
        public static void PublishReportsDialogRequestedEvent() => ReportsDialogRequestedEvent?.Invoke();
        public static void PublishShipDesignerDialogRequestedEvent() => ShipDesignerDialogRequestedEvent?.Invoke();
        public static void PublishTechBrowserDialogRequestedEvent() => TechBrowserDialogRequestedEvent?.Invoke();
        public static void PublishPlanetViewStateUpdatedEvent() => PlanetViewStateUpdatedEvent?.Invoke();
        public static void PublishProductionQueueChangedEvent(Planet planet) => ProductionQueueChangedEvent?.Invoke(planet);
        public static void PublishCargoTransferRequestedEvent(ICargoHolder source, ICargoHolder dest) => CargoTransferRequestedEvent?.Invoke(source, dest);
        public static void PublishSubmitTurnEvent(Player player) => SubmitTurnEvent?.Invoke(player);

        #endregion

        #endregion
    }
}
