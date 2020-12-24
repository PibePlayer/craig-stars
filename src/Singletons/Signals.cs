using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The Signals csharp class services as a way to bridge csharp and gdscript until 
/// everything is rewritten in .net
/// </summary>
public class Signals : Node
{
    public delegate void TurnPassed(int year);
    public static event TurnPassed TurnPassedEvent;

    #region Viewport Events

    public static event Action<MapObject> MapObjectSelectedEvent;

    #endregion

    #region Network Events

    public static event Action ServerDisconnectedEvent;

    public delegate void PreStartGame(List<Player> players);
    public static event PreStartGame PreStartGameEvent;

    public static event Action PostStartGameEvent;

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

    public static void PublishPostStartGameEvent()
    {
        PostStartGameEvent?.Invoke();
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

    public static void PublishTurnPassedEvent(int year)
    {
        TurnPassedEvent?.Invoke(year);
    }

    public static void PublishMapObjectSelectedEvent(MapObject mapObject)
    {
        MapObjectSelectedEvent?.Invoke(mapObject);
    }

    public static void PublishServerDisconnectedEvent()
    {
        ServerDisconnectedEvent?.Invoke();
    }

    #endregion
}
