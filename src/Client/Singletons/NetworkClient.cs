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
            GD.Print("NetworkClient: connected to server");
            // subscribe to any server events we want to listen for
            Signals.SubmitTurnRequestedEvent += OnSubmitTurnRequested;
        }

        /// <summary>
        /// Called when the server disconnects us, a networkclient.
        /// </summary>
        public void OnServerDisconnected()
        {
            Signals.PublishServerDisconnectedEvent();
            Signals.SubmitTurnRequestedEvent -= OnSubmitTurnRequested;
        }

        /// <summary>
        /// Called when our networkclient connection to the server fails
        /// </summary>
        public void OnConnectionFailed()
        {
            GD.Print("NetworkClient: connecting to server failed");
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
                GD.PrintErr($"Failed to connect to server: {address}:{port} Error: {error.ToString()}");
                return;
            }
            GetTree().NetworkPeer = peer;

            GD.Print($"Joined game at {address}:{port}");
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
                GD.Print("Closing connection");
                peer.CloseConnection();
            }
            GetTree().NetworkPeer = null;
        }

        void OnSubmitTurnRequested(Player player)
        {
            if (!this.IsServerOrSinglePlayer())
            {
                // tell the server we submitted our turn
                RPC.Instance.SendSubmitTurn(player);
            }
        }

    }
}