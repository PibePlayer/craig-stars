using Godot;
using System;
using CraigStars.Client;
using CraigStars.Server;
using CSServer = CraigStars.Server.Server;
using System.Threading.Tasks;

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

        TaskFactory GodotTaskFactory { get; set; }

        /// <summary>
        /// Servers have their own sceneTree
        /// </summary>
        private SceneTree serverTree;
        private CSServer server;

        public override void _Ready()
        {
            SetProcess(false);
            GodotTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
            serverTree?.Multiplayer?.Poll();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                serverTree?.Free();
                serverTree = null;
            }
        }

        #region Single Player

        GameSettings<Player> settings;

        /// <summary>
        /// Create a new single player server and generate a new game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void NewGame(GameSettings<Player> settings)
        {
            this.settings = settings;
            server = ResourceLoader.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
            server.GodotTaskFactory = GodotTaskFactory;
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
            settings = new GameSettings<Player>()
            {
                ContinueGame = true,
                Name = gameName,
                Year = year,
            };

            var gameInfo = GamesManager.Instance.LoadGameInfo(gameName, year);
            if (gameInfo.Mode == GameMode.MultiPlayer)
            {
                HostGame(settings: settings);

                CallDeferred(nameof(JoinHostedGame));
            }
            else
            {
                server = ResourceLoader.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
                server.GodotTaskFactory = GodotTaskFactory;
                AddChild((Node)server);

                CallDeferred(nameof(PublishLocalGameStartRequest));
            }

        }

        public void ExitGame()
        {
            if (server != null)
            {
                server.GetParent()?.RemoveChild(server);
                server.QueueFree();
                server = null;
            }
        }

        /// <summary>
        /// For local games, we publish a GameStartRequestedEvent
        /// </summary>
        void PublishLocalGameStartRequest()
        {
            Client.EventManager.PublishGameStartRequestedEvent(settings);
        }

        void JoinHostedGame()
        {
            NetworkClient.Instance.JoinGame("localhost", 3000);
        }

        #endregion


        #region Network Host/Close 

        /// <summary>
        /// Host a new game, starting a server
        /// </summary>
        public void HostGame(int port = 3000, GameSettings<Player> settings = null)
        {
            if (serverTree != null)
            {
                CloseConnection();
            }

            serverTree = new();
            serverTree.Init();
            serverTree.Root.RenderTargetUpdateMode = Viewport.UpdateMode.Disabled;

            // add singletons (multipletons)
            var rpc = ResourceLoader.Load<PackedScene>("res://src/Shared/RPC.tscn").Instance<RPC>();
            rpc.Name = "RPC";
            serverTree.Root.AddChild(rpc);
            server = ResourceLoader.Load<PackedScene>("res://src/Server/NetworkServer.tscn").Instance<NetworkServer>();
            server.GodotTaskFactory = GodotTaskFactory;
            server.Settings = settings;
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

            if (settings != null && settings.ContinueGame)
            {
                log.Info("Hosting saved game");
            }
            else
            {
                log.Info("Hosting new game");
            }

        }

        /// <summary>
        /// Close the connection to all clients
        /// </summary>
        public void CloseConnection()
        {
            SetProcess(false);
            if (serverTree != null)
            {
                var peer = serverTree.NetworkPeer as NetworkedMultiplayerENet;
                if (peer != null && peer.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected)
                {
                    log.Info("Closing connection");
                    peer.CloseConnection();
                }

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

