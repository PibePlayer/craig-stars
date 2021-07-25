using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    /// <summary>
    /// A client to connect to servers
    /// </summary>
    public class NetworkClient : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(NetworkClient));

        /// <summary>
        /// NetworkClient is a singleton
        /// </summary>
        private static NetworkClient instance;
        public static NetworkClient Instance
        {
            get
            {
                return instance;
            }
        }

        NetworkClient()
        {
            instance = this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        /// <summary>
        /// Called when the we have connected to a server
        /// </summary>
        public void OnConnectedToServer()
        {
            log.Info("Connected to server");
        }

        /// <summary>
        /// Called when the server disconnects us, a networkclient.
        /// </summary>
        public void OnServerDisconnected()
        {
            log.Info("Server disconnected");
            Signals.PublishServerDisconnectedEvent();
        }

        /// <summary>
        /// Called when our networkclient connection to the server fails
        /// </summary>
        public void OnConnectionFailed()
        {
            log.Info("Connecting to server failed");
        }

        /// <summary>
        /// Join an existing game by address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void JoinGame(string address, int port)
        {
            // hook up to networkclient specific network events
            GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
            GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer));
            GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));

            var peer = new NetworkedMultiplayerENet();
            var error = peer.CreateClient(address, port);

            if (error != Error.Ok)
            {
                log.Error($"Failed to connect to server: {address}:{port} Error: {error.ToString()}");
                return;
            }
            GetTree().NetworkPeer = peer;

            log.Info($"Joined game (as {GetTree().GetNetworkUniqueId()}) at {address}:{port}");
        }

        /// <summary>
        /// Close the connection to a server or all networkclients
        /// </summary>
        public void CloseConnection()
        {
            // hook up to networkclient specific network events
            GetTree().Disconnect("server_disconnected", this, nameof(OnServerDisconnected));
            GetTree().Disconnect("connected_to_server", this, nameof(OnConnectedToServer));
            GetTree().Disconnect("connection_failed", this, nameof(OnConnectionFailed));

            var peer = GetTree().NetworkPeer as NetworkedMultiplayerENet;
            if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
            {
                log.Info("Closing connection");
                peer.CloseConnection();
            }
            GetTree().NetworkPeer = null;
        }

        /// <summary>
        /// Submit a turn to the server
        /// </summary>
        /// <param name="player"></param>
        public void SubmitTurnToServer(Player player)
        {
            // tell the server we submitted our turn
            RPC.Instance(GetTree()).SendSubmitTurn(player);
        }
    }
}