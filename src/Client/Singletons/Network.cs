using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    /// <summary>
    /// A server for managing the game
    /// </summary>
    public class Network : Node
    {

        public bool Started { get; private set; } = false;
        public Game Game { get; set; } = new Game();

        /// <summary>
        /// If any player isn't ready to start, the server isn't ready
        /// </summary>
        /// <returns></returns>
        public bool ReadyToStart { get => PlayersManager.Instance.Players.Find(p => !p.Ready) == null; }

        /// <summary>
        /// Server is a singleton
        /// </summary>
        private static Network instance;
        public static Network Instance
        {
            get
            {
                return instance;
            }
        }

        Network()
        {
            instance = this;
        }

        public override void _Ready()
        {
        }

        public override void _ExitTree()
        {
        }

        public void Reset()
        {
            Started = false;
        }

        void ConnectServerEvents()
        {
            // signals for when a player connects to us
            GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Connect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            // These are signals that are only triggered for clients. In a single player game they don't really have any
            // purpose. They are here to differentiate code paths that only clients act upon (like activating/deactivating a shield without a timer)
            // ClientSignals.SomeClientEvent += OnSomeClientEvent;

            // these are game events that our server listens for in order to send notifications to clients
            // Signals.SomeServerEvent += OnSomeServerEvent;
        }

        /// <summary>
        /// When our server shuts down, disconnect all the events
        /// </summary>
        void DisconnectServerEvents()
        {
            GetTree().Disconnect("network_peer_connected", this, nameof(OnPlayerConnected));
            GetTree().Disconnect("network_peer_disconnected", this, nameof(OnPlayerDisconnected));

            // ClientSignals.SomeClientEvent -= OnSomeClientEvent;
            // Signals.SomeServerEvent -= OnSomeServerEvent;

        }


        #region Player Join/Leave Events

        void OnPlayerConnected(int id)
        {
            if (this.IsServer())
            {
                // if we are the server, we know a new player has connected
                GD.Print($"Server: Player {id} connected to server.");
                Signals.PublishPlayerJoinedEvent(id);
                var player = PlayersManager.Instance.GetNetworkPlayer(id);
                RPC.Instance.SendAllMessages(id);
                RPC.Instance.SendMessage($"{player} has joined the game");
            }
        }

        void OnPlayerDisconnected(int id)
        {
            if (this.IsServer())
            {
                // if we are the server, we know a new player has connected
                GD.Print($"Server: Player {id} disconnected from server.");
                var player = PlayersManager.Instance.GetNetworkPlayer(id);
                RPC.Instance.SendMessage($"{player} has left the game");
                Signals.PublishPlayerLeftEvent(id);
            }
        }

        #endregion

        #region Network Host/Close 

        /// <summary>
        /// Host a new game, starting a server
        /// </summary>
        public void HostGame(int port = 3000)
        {
            ConnectServerEvents();
            var peer = new NetworkedMultiplayerENet();
            var error = peer.CreateServer(port, PlayersManager.Instance.NumPlayers);
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to create network server: Error: {error.ToString()}");
                return;
            }
            GetTree().NetworkPeer = peer;

            GD.Print("Hosting new game");
        }

        /// <summary>
        /// Close the connection to all clients
        /// </summary>
        public void CloseConnection()
        {
            DisconnectServerEvents();

            var peer = GetTree().NetworkPeer as NetworkedMultiplayerENet;
            if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
            {
                GD.Print("Closing connection");
                peer.CloseConnection();
            }
            GetTree().NetworkPeer = null;
        }

        #endregion

        #region Game State Changes

        public void BeginGame()
        {
            if (this.IsServer())
            {
                // join our own game
                Signals.PublishPlayerJoinedEvent(GetTree().GetNetworkUniqueId());
            }
        }

        public void PostBeginGame()
        {
            // for single player games, we control player 1
            if (this.IsSinglePlayer())
            {
                PlayersManager.Instance.Players[0].AIControlled = false;
            }
            if (this.IsServerOrSinglePlayer())
            {
                Started = true;
            }
        }

        #endregion

        #region Game Events
        // put any game events that need an RPC method here
        #endregion
    }
}