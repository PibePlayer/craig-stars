using System;
using System.Collections.Generic;

namespace CraigStars.Client
{
    /// <summary>
    /// The Client EventManager handles all client side events, like opening dialogs, new turn requests, changing view mode, etc.
    /// </summary>
    public static class EventManager
    {
        static CSLog log = LogProvider.GetLogger(typeof(EventManager));

        #region Turn Generation events

        public static event Action<GameSettings<Player>> StartNewGameRequestedEvent;
        public static event Action<string, int> ContinueGameRequestedEvent;
        public static event Action<PublicGameInfo> GameStartingEvent;
        public static event Action<PublicGameInfo, Player> GameStartedEvent;
        public static event Action<PublicGameInfo> GameContinuedEvent;
        public static event Action<PublicGameInfo> GameExitingEvent;
        public static event Action<PublicPlayerInfo> PlayerDataRequestedEvent;
        public static event Action<Player> SubmitTurnRequestedEvent;
        public static event Action<PublicGameInfo, PublicPlayerInfo> TurnSubmittedEvent;
        public static event Action<PublicPlayerInfo> UnsubmitTurnRequestedEvent;
        public static event Action<PublicGameInfo, PublicPlayerInfo> TurnUnsubmittedEvent;
        public static event Action<int> PlayTurnRequestedEvent;

        public static event Action<PublicGameInfo, Player> PlayerDataEvent;
        public static event Action TurnGeneratingEvent;
        public static event Action<TurnGenerationState> TurnGeneratorAdvancedEvent;
        public static event Action<UniverseGenerationState> UniverseGeneratorAdvancedEvent;
        public static event Action<PublicGameInfo, Player> TurnPassedEvent;
        public static event Action<PublicGameInfo> GameViewResetEvent;

        public static void PublishStartNewGameRequestedEvent(GameSettings<Player> settings) => StartNewGameRequestedEvent?.Invoke(settings);
        public static void PublishContinueGameRequestedEvent(string gameName, int year) => ContinueGameRequestedEvent?.Invoke(gameName, year);
        public static void PublishPlayerDataRequestedEvent(PublicPlayerInfo player) => PlayerDataRequestedEvent?.Invoke(player);
        public static void PublishGameStartingEvent(PublicGameInfo gameInfo) => GameStartingEvent?.Invoke(gameInfo);
        public static void PublishGameExitingEvent(PublicGameInfo gameInfo) => GameExitingEvent?.Invoke(gameInfo);
        public static void PublishGameStartedEvent(PublicGameInfo gameInfo, Player player) => GameStartedEvent?.Invoke(gameInfo, player);
        public static void PublishGameContinuedEvent(PublicGameInfo gameInfo) => GameContinuedEvent?.Invoke(gameInfo);
        public static void PublishSubmitTurnRequestedEvent(Player player) => SubmitTurnRequestedEvent?.Invoke(player);
        public static void PublishTurnSubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player) => TurnSubmittedEvent?.Invoke(gameInfo, player);
        public static void PublishUnsubmitTurnRequestedEvent(PublicPlayerInfo player) => UnsubmitTurnRequestedEvent?.Invoke(player);
        public static void PublishTurnUnsubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player) => TurnUnsubmittedEvent?.Invoke(gameInfo, player);
        public static void PublishPlayTurnRequestedEvent(int playerNum) => PlayTurnRequestedEvent?.Invoke(playerNum);

        public static void PublishPlayerDataEvent(PublicGameInfo gameInfo, Player player) => PlayerDataEvent?.Invoke(gameInfo, player);
        public static void PublishUniverseGeneratorAdvancedEvent(UniverseGenerationState state) => UniverseGeneratorAdvancedEvent?.Invoke(state);
        public static void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state) => TurnGeneratorAdvancedEvent?.Invoke(state);
        public static void PublishTurnGeneratingEvent() => TurnGeneratingEvent?.Invoke();
        public static void PublishTurnPassedEvent(PublicGameInfo gameInfo, Player player) => TurnPassedEvent?.Invoke(gameInfo, player);
        public static void PublishGameViewResetEvent(PublicGameInfo gameInfo) => GameViewResetEvent?.Invoke(gameInfo);

        #endregion

        #region Race Editor

        public static event Action<Race, string> RaceSavedEvent;
        public static void PublishRaceSavedEvent(Race race, string filename) => RaceSavedEvent?.Invoke(race, filename);

        #endregion

        #region Viewport Events

        public static event Action<MapObjectSprite> MapObjectHighlightedEvent;
        public static event Action<MapObjectSprite> MapObjectSelectedEvent;
        public static event Action<MapObjectSprite> MapObjectCommandedEvent;
        public static event Action<MapObject> CommandMapObjectEvent;
        public static event Action<MapObject> SelectMapObjectEvent;
        public static event Action CommandNextMapObjectEvent;
        public static event Action CommandPrevMapObjectEvent;
        public static event Action<MapObjectSprite> GotoMapObjectSpriteEvent;
        public static event Action<MapObject> GotoMapObjectEvent;

        #endregion

        #region UI State Change Events

        public static event Action PlayerDirtyChangedEvent;
        public static event Action PlanetViewStateUpdatedEvent;
        public static event Action FleetViewStateUpdatedEvent;
        public static event Action ScannerScaleUpdatedEvent;
        public static event Action ViewStateUpdatedEvent;
        public static event Action<Planet> ProductionQueueChangedEvent;

        public static void PublishPlayerDirtyEvent() => PlayerDirtyChangedEvent?.Invoke();
        public static void PublishPlanetViewStateUpdatedEvent() => PlanetViewStateUpdatedEvent?.Invoke();
        public static void PublishFleetViewStateUpdatedEvent() => FleetViewStateUpdatedEvent?.Invoke();
        public static void PublishScannerScaleUpdatedEvent() => ScannerScaleUpdatedEvent?.Invoke();
        public static void PublishViewStateUpdatedEvent() => ViewStateUpdatedEvent?.Invoke();
        public static void PublishProductionQueueChangedEvent(Planet planet) => ProductionQueueChangedEvent?.Invoke(planet);

        #endregion

        #region Network Events


        #endregion

        #region Scanner Objects

        public static void PublishMapObjectHightlightedEvent(MapObjectSprite mapObjectSprite) => MapObjectHighlightedEvent?.Invoke(mapObjectSprite);
        public static void PublishMapObjectSelectedEvent(MapObjectSprite mapObjectSprite) => MapObjectSelectedEvent?.Invoke(mapObjectSprite);
        public static void PublishMapObjectCommandedEvent(MapObjectSprite mapObjectSprite) => MapObjectCommandedEvent?.Invoke(mapObjectSprite);
        public static void PublishCommandMapObjectEvent(MapObject mapObject) => CommandMapObjectEvent?.Invoke(mapObject);
        public static void PublishSelectMapObjectEvent(MapObject mapObject) => SelectMapObjectEvent?.Invoke(mapObject);
        public static void PublishCommandNextMapObjectEvent() => CommandNextMapObjectEvent?.Invoke();
        public static void PublishCommandPrevMapObjectEvent() => CommandPrevMapObjectEvent?.Invoke();
        public static void PublishGotoMapObjectEvent(MapObjectSprite mapObjectSprite) => GotoMapObjectSpriteEvent?.Invoke(mapObjectSprite);
        public static void PublishGotoMapObjectEvent(MapObject mapObject) => GotoMapObjectEvent?.Invoke(mapObject);

        #endregion



        #region Waypoints

        public static event Action<Fleet, Waypoint> WaypointAddedEvent;
        public static event Action<Fleet, Waypoint> WaypointMovedEvent;
        public static event Action<Waypoint> WaypointSelectedEvent;
        public static event Action<Waypoint> WaypointDeletedEvent;

        public static void PublishWaypointAddedEvent(Fleet fleet, Waypoint waypoint) => WaypointAddedEvent?.Invoke(fleet, waypoint);
        public static void PublishWaypointMovedEvent(Fleet fleet, Waypoint waypoint) => WaypointMovedEvent?.Invoke(fleet, waypoint);
        public static void PublishWaypointSelectedEvent(Waypoint waypoint) => WaypointSelectedEvent?.Invoke(waypoint);
        public static void PublishWaypointDeletedEvent(Waypoint waypoint) => WaypointDeletedEvent?.Invoke(waypoint);

        #endregion

        #region Fleets

        public static event Action<Fleet> FleetDeletedEvent;
        public static event Action<Fleet, ICargoHolder> CargoTransferredEvent;
        public static event Action<List<Fleet>> FleetsCreatedEvent;

        public static void PublishFleetDeletedEvent(Fleet fleet) => FleetDeletedEvent?.Invoke(fleet);
        public static void PublishCargoTransferredEvent(Fleet source, ICargoHolder dest) => CargoTransferredEvent?.Invoke(source, dest);
        public static void PublishFleetsCreatedEvent(List<Fleet> fleets) => FleetsCreatedEvent?.Invoke(fleets);

        #endregion

        #region Packets

        public static event Action PacketDestinationToggleEvent;
        public static event Action<Planet, Planet> PacketDestinationChangedEvent;

        public static void PublishPacketDestinationToggleEvent() => PacketDestinationToggleEvent?.Invoke();
        public static void PublishPacketDestinationChangedEvent(Planet planet, Planet target) => PacketDestinationChangedEvent?.Invoke(planet, target);

        #endregion


        #region Dialog And Hotkey Publishers

        public delegate void CargoTransferDialogRequest(ICargoHolder source, ICargoHolder dest);

        public static event Action<PlanetSprite> ProductionQueueDialogRequestedEvent;
        public static event Action<FleetSprite> RenameFleetDialogRequestedEvent;
        public static event CargoTransferDialogRequest CargoTransferDialogRequestedEvent;
        public static event Action ResearchDialogRequestedEvent;
        public static event Action BattlePlansDialogRequestedEvent;
        public static event Action TransportPlansDialogRequestedEvent;
        public static event Action ProductionPlansDialogRequestedEvent;
        public static event Action ReportsDialogRequestedEvent;
        public static event Action TechBrowserDialogRequestedEvent;
        public static event Action RaceDesignerDialogRequestedEvent;
        public static event Action ShipDesignerDialogRequestedEvent;
        public static event Action PlayerStatusDialogRequestedEvent;
        public static event Action PlayerRelationsDialogRequestedEvent;
        public static event Action<FleetSprite> MergeFleetsDialogRequestedEvent;
        public static event Action<BattleRecord> BattleViewerDialogRequestedEvent;
        public static event Action<MapObjectSprite> ViewportAlternateSelectEvent;

        public static void PublishProductionQueueDialogRequestedEvent(PlanetSprite planet) => ProductionQueueDialogRequestedEvent?.Invoke(planet);
        public static void PublishRenameFleetDialogRequestedEvent(FleetSprite fleetSprite) => RenameFleetDialogRequestedEvent?.Invoke(fleetSprite);
        public static void PublishCargoTransferDialogRequestedEvent(ICargoHolder source, ICargoHolder dest) => CargoTransferDialogRequestedEvent?.Invoke(source, dest);
        public static void PublishResearchDialogRequestedEvent() => ResearchDialogRequestedEvent?.Invoke();
        public static void PublishBattlePlansDialogRequestedEvent() => BattlePlansDialogRequestedEvent?.Invoke();
        public static void PublishTransportPlansDialogRequestedEvent() => TransportPlansDialogRequestedEvent?.Invoke();
        public static void PublishProductionPlansDialogRequestedEvent() => ProductionPlansDialogRequestedEvent?.Invoke();
        public static void PublishReportsDialogRequestedEvent() => ReportsDialogRequestedEvent?.Invoke();
        public static void PublishShipDesignerDialogRequestedEvent() => ShipDesignerDialogRequestedEvent?.Invoke();
        public static void PublishPlayerStatusDialogRequestedEvent() => PlayerStatusDialogRequestedEvent?.Invoke();
        public static void PublishPlayerRelationsDialogRequestedEvent() => PlayerRelationsDialogRequestedEvent?.Invoke();
        public static void PublishTechBrowserDialogRequestedEvent() => TechBrowserDialogRequestedEvent?.Invoke();
        public static void PublishRaceDesignerDialogRequestedEvent() => RaceDesignerDialogRequestedEvent?.Invoke();
        public static void PublishMergeFleetsDialogRequestedEvent(FleetSprite sourceFleet) => MergeFleetsDialogRequestedEvent?.Invoke(sourceFleet);
        public static void PublishBattleViewerDialogRequestedEvent(BattleRecord battle) => BattleViewerDialogRequestedEvent?.Invoke(battle);
        public static void PublishViewportAlternateSelect(MapObjectSprite mapObject) => ViewportAlternateSelectEvent?.Invoke(mapObject);

        #endregion

    }
}
