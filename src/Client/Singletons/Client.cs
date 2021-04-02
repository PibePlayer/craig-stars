using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    /// <summary>
    /// A client to connect to servers
    /// </summary>
    public class Client : Node
    {
        /// <summary>
        /// Client is a singleton
        /// </summary>
        private static Client instance;
        public static Client Instance
        {
            get
            {
                return instance;
            }
        }

        Client()
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
            GD.Print("Client: connected to server");
            // subscribe to any server events we want to listen for
            Signals.SubmitTurnEvent += OnSubmitTurn;
        }

        /// <summary>
        /// Called when the server disconnects us, a client.
        /// </summary>
        public void OnServerDisconnected()
        {
            Signals.PublishServerDisconnectedEvent();
            Signals.SubmitTurnEvent -= OnSubmitTurn;
        }

        /// <summary>
        /// Called when our client connection to the server fails
        /// </summary>
        public void OnConnectionFailed()
        {
            GD.Print("Client: connecting to server failed");
        }

        /// <summary>
        /// Join an existing game by address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public void JoinGame(string address, int port)
        {
            // hook up to client specific network events
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
        /// Close the connection to a server or all clients
        /// </summary>
        public void CloseConnection()
        {
            // hook up to client specific network events
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

        void OnSubmitTurn(Player player)
        {
            if (!this.IsServerOrSinglePlayer())
            {
                // tell the server we submitted our turn
                RPC.Instance.SendSubmitTurn(player);
            }
        }

    }
}