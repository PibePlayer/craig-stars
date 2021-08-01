using Godot;
using System;
using CraigStars.Client;
using CraigStars.Server;
using CSServer = CraigStars.Server.Server;


namespace CraigStars.Singletons
{
    /// <summary>
    /// A singleton node for creating new Server scene trees when multiplayer games are hosted
    /// </summary>
    public class ServerManager : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(ServerManager));

        /// <summary>
        /// Server is a singleton
        /// </summary>
        private static ServerManager instance;
        public static ServerManager Instance
        {
            get
            {
                return instance;
            }
        }

        ServerManager()
        {
            instance = this;
        }

        /// <summary>
        /// Servers have their own sceneTree
        /// </summary>
        private SceneTree serverTree;
        private CSServer server;

        public override void _Ready()
        {
            SetProcess(false);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            serverTree?.Multiplayer?.Poll();
        }

        public override void _ExitTree()
        {
            serverTree?.Free();
            serverTree = null;
        }

        #region Single Player

        GameSettings<Player> localGameSettings;

        /// <summary>
        /// Create a new single player server and generate a new game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void NewGame(GameSettings<Player> settings)
        {
            localGameSettings = settings;
            server = GD.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
            AddChild(server);

            CallDeferred(nameof(PublishLocalGameStartRequest));
        }

        /// <summary>
        /// Create a new single player server and load a saved game game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void ContinueGame(String gameName, int year)
        {
            // setup our GameSettings to be a continue game
            localGameSettings = new GameSettings<Player>()
            {
                ContinueGame = true,
                Name = gameName,
                Year = year,
            };

            server = GD.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
            AddChild((Node)server);

            CallDeferred(nameof(PublishLocalGameStartRequest));
        }

        public void ExitGame()
        {
            if (server != null)
            {
                RemoveChild(server);
                server.QueueFree();
                server = null;
            }
        }

        /// <summary>
        /// For local games, we publish a GameStartRequestedEvent
        /// </summary>
        void PublishLocalGameStartRequest()
        {
            Client.EventManager.PublishGameStartRequestedEvent(localGameSettings);
        }

        #endregion


        #region Network Host/Close 

        /// <summary>
        /// Host a new game, starting a server
        /// </summary>
        public void HostGame(int port = 3000)
        {
            if (serverTree != null)
            {
                CloseConnection();
            }

            serverTree = new();
            serverTree.Init();
            serverTree.Root.RenderTargetUpdateMode = Viewport.UpdateMode.Disabled;

            // add singletons (multipletons)
            var rpc = GD.Load<PackedScene>("res://src/Shared/RPC.tscn").Instance<RPC>();
            rpc.Name = "RPC";
            serverTree.Root.AddChild(rpc);
            server = GD.Load<PackedScene>("res://src/Server/NetworkServer.tscn").Instance<NetworkServer>();
            serverTree.Root.AddChild(server);

            var peer = new NetworkedMultiplayerENet();
            var error = peer.CreateServer(port, RulesManager.Rules.MaxPlayers);
            if (error != Error.Ok)
            {
                log.Error($"Failed to create network server: Error: {error.ToString()}");
                return;
            }
            serverTree.NetworkPeer = peer;

            // start updating the serverTree
            SetProcess(true);

            log.Info("Hosting new game");
        }

        /// <summary>
        /// Close the connection to all clients
        /// </summary>
        public void CloseConnection()
        {
            SetProcess(false);
            var peer = serverTree.NetworkPeer as NetworkedMultiplayerENet;
            if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
            {
                log.Info("Closing connection");
                peer.CloseConnection();
            }
            if (serverTree != null)
            {
                foreach (Node child in serverTree.Root.GetChildren())
                {
                    serverTree.Root.RemoveChild(child);
                    child.QueueFree();
                }
                serverTree.NetworkPeer = null;
                serverTree.Free();
                serverTree = null;
            }
        }
        #endregion
    }
}

