using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// A client to connect to servers
    /// This client will automatically try and reconnect if the connection with the server is lost
    /// </summary>
    public class NetworkClient : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(NetworkClient));

        public event Action ServerDisconnectedEvent;
        public event Action ServerConnectedEvent;
        public event Action<PublicPlayerInfo> PlayerUpdatedEvent;

        /// <summary>
        /// The player this network client is playing as
        /// </summary>
        /// <value></value>
        public Player Player { get; set; }

        public String Host { get; set; }
        public int Port { get; set; }

        public bool Connected { get; set; }

        public Timer reconnectTimer;

        ServerRPC rpc;

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
            base._Ready();
            rpc = ServerRPC.Instance(GetTree());
            reconnectTimer = GetNode<Timer>("ReconnectTimer");
            reconnectTimer.Connect("timeout", this, nameof(OnReconnectTimerTimeout));
        }

        /// <summary>
        /// Called when the we have connected to a server
        /// </summary>
        public void OnConnectedToServer()
        {
            log.Info("Connected to server");
            reconnectTimer.Stop();
            if (Player != null)
            {
                // we have connected to this server
                // make sure the server updates our records
                rpc.SendPlayerRejoinedGame(Player);
            }

            Connected = true;
            ServerConnectedEvent?.Invoke();
        }

        /// <summary>
        /// Called when the server disconnects us, a networkclient.
        /// </summary>
        public void OnServerDisconnected()
        {
            log.Info("Server disconnected");
            Connected = false;
            ServerDisconnectedEvent?.Invoke();

            // try and reconnect every 5 seconds (configured in the Godot editor)
            reconnectTimer.Start();
        }

        /// <summary>
        /// If we lose connection to the server, try and reconnect
        /// </summary>
        void OnReconnectTimerTimeout()
        {
            // try and connect again
            ConnectToServer(Host, Port);
        }

        /// <summary>
        /// Called when our networkclient connection to the server fails
        /// </summary>
        public void OnConnectionFailed()
        {
            log.Info("Connecting to server failed");
            reconnectTimer.Start();
        }

        /// <summary>
        /// Connect up to a remote server
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        void ConnectToServer(string host, int port)
        {
            CloseConnection();

            Host = host;
            Port = port;

            // hook up to networkclient specific network events
            GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
            GetTree().Connect("connected_to_server", this, nameof(OnConnectedToServer));
            GetTree().Connect("connection_failed", this, nameof(OnConnectionFailed));

            var peer = new NetworkedMultiplayerENet();
            var error = peer.CreateClient(host, port);

            if (error != Error.Ok)
            {
                log.Error($"Failed to connect to server: {host}:{port} Error: {error.ToString()}");
                return;
            }
            GetTree().NetworkPeer = peer;

            log.Info($"Joined game (as {GetTree().GetNetworkUniqueId()}) at {host}:{port}");

        }

        /// <summary>
        /// Join an existing game by address and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void JoinNewGame(string host, int port)
        {
            ConnectToServer(host, port);
        }

        /// <summary>
        /// Join an existing game by address and port
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void JoinExistingGame(string host, int port, Player player)
        {
            // on connection, we tell the server we are resuming play as a player
            Player = player;
            ConnectToServer(host, port);
        }


        /// <summary>
        /// Close the connection to a server or all networkclients
        /// </summary>
        public void CloseConnection()
        {
            if (GetTree().IsConnected("server_disconnected", this, nameof(OnServerDisconnected)))
            {
                // disconnect networkclient specific network events
                GetTree().Disconnect("server_disconnected", this, nameof(OnServerDisconnected));
                GetTree().Disconnect("connected_to_server", this, nameof(OnConnectedToServer));
                GetTree().Disconnect("connection_failed", this, nameof(OnConnectionFailed));
            }

            var peer = GetTree().NetworkPeer as NetworkedMultiplayerENet;
            if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
            {
                log.Info("Closing connection");
                peer.CloseConnection();
            }
            GetTree().NetworkPeer = null;
        }

        /// <summary>
        /// Publish a player updated event for any listeners
        /// </summary>
        /// <param name="player"></param>
        /// <param name="notifyPeers">True if we should notify peers of this player data</param>
        public void PublishPlayerUpdatedEvent(PublicPlayerInfo player, bool notifyPeers = false, SceneTree sceneTree = null)
        {
            PlayerUpdatedEvent?.Invoke(player);
            if (notifyPeers && sceneTree != null)
            {
                rpc.SendPlayerUpdated(player);
            }
        }

        public void StartGame(GameSettings<Player> settings)
        {
            // Note: The server already knows about the game's players, so no need to pass
            // those along here
            rpc.SendStartNewGameRequest(settings);
        }

        /// <summary>
        /// Tell a headless server to continue an existing game (not currently being used)
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="year"></param>
        public void ContinueGame(string gameName, int year)
        {
            // Note: The server already knows about the game's players, so no need to pass
            // those along here
            rpc.SendContinueGameRequested(gameName, year);
        }

        /// <summary>
        /// Submit a turn to the server
        /// </summary>
        /// <param name="player"></param>
        public void SubmitTurnToServer(string token, PublicGameInfo gameInfo, PlayerOrders orders)
        {
            // tell the server we submitted our turn
            rpc.SendSubmitTurn(token, gameInfo, orders);
        }

        /// <summary>
        /// Unsubmit a turn to the server
        /// </summary>
        /// <param name="player"></param>
        public void UnsubmitTurnToServer(PublicGameInfo gameInfo, Player player)
        {
            // tell the server we submitted our turn
            rpc.SendUnsubmitTurn(gameInfo, player);
        }

        /// <summary>
        /// Submit a race to the server
        /// </summary>
        /// <param name="player"></param>
        public void UpdateRaceOnServer(Race race)
        {
            // tell the server we submitted our turn
            rpc.SendUpdateRace(race);
        }

        /// <summary>
        /// Tell the server to add an AI player
        /// </summary>
        /// <param name="player"></param>
        public void AddAIPlayer()
        {
            // tell the server we submitted our turn
            rpc.SendAddAIPlayer();
        }

        /// <summary>
        /// Tell the server to kick out a player
        /// </summary>
        /// <param name="player"></param>
        public void KickPlayer(int playerNum)
        {
            // tell the server we submitted our turn
            rpc.SendKickPlayer(playerNum);
        }

        /// <summary>
        /// Submit a turn to the server
        /// </summary>
        /// <param name="player"></param>
        public void GenerateTurn(PublicGameInfo gameInfo, Player player)
        {
            // tell the server we submitted our turn
            rpc.SendGenerateTurn(gameInfo, player);
        }

    }
}