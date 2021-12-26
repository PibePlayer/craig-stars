using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Client;
using CraigStars.Server;
using CraigStars.Utils;
using Godot;
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
            else if (what == NotificationWmQuitRequest)
            {
                ExitGame();
            }
        }

        #region Single Player

        GameSettings<Player> settings;
        PublicGameInfo gameInfo;

        /// <summary>
        /// Create a new single player server and generate a new game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public void NewGame(GameSettings<Player> settings)
        {
            this.settings = settings;
            server = ResourceLoader.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
            server.Init(GodotTaskFactory, GamesManager.Instance, TurnProcessorManager.Instance);
            AddChild(server);

            CallDeferred(nameof(PublishLocalStartNewGameRequest));
        }

        List<Player> continuePlayers;

        public void ContinueLocalGame(PublicGameInfo gameInfo, int year)
        {
            server = ResourceLoader.Load<PackedScene>("res://src/Server/LocalServer.tscn").Instance<LocalServer>();
            server.Init(GodotTaskFactory, GamesManager.Instance, TurnProcessorManager.Instance);
            AddChild((Node)server);

            CallDeferred(nameof(PublishLocalContinueGameRequest), gameInfo.Name, gameInfo.Year);
        }

        public void ExitGame()
        {
            if (server != null)
            {
                CloseConnection();
                server.GetParent()?.RemoveChild(server);
                server.QueueFree();
                server = null;
            }
        }

        /// <summary>
        /// For local games, we publish a GameStartRequestedEvent
        /// </summary>
        void PublishLocalStartNewGameRequest()
        {
            Client.EventManager.PublishStartNewGameRequestedEvent(settings);
        }

        /// <summary>
        /// For local games, we publish a GameStartRequestedEvent
        /// </summary>
        void PublishLocalContinueGameRequest(string gameName, int year)
        {
            Client.EventManager.PublishContinueGameRequestedEvent(gameName, year);
        }

        #endregion


        #region Network Host/Close 

        /// <summary>
        /// Host a new game, starting a server
        /// </summary>
        public async void HostGame(int port = 3000, string continueGameName = null, int continueGameYear = -1)
        {
            if (serverTree != null)
            {
                CloseConnection();
            }

            serverTree = new();
            serverTree.Init();
            serverTree.Root.RenderTargetUpdateMode = Viewport.UpdateMode.Disabled;

            // add singletons (multipletons)
            var serverRPC = ResourceLoader.Load<PackedScene>("res://src/Shared/ServerRPC.tscn").Instance<ServerRPC>();
            serverRPC.Name = "ServerRPC";
            serverTree.Root.AddChild(serverRPC);
            var clientRPC = ResourceLoader.Load<PackedScene>("res://src/Shared/ClientRPC.tscn").Instance<ClientRPC>();
            clientRPC.Name = "ClientRPC";
            serverTree.Root.AddChild(clientRPC);
            server = ResourceLoader.Load<PackedScene>("res://src/Server/NetworkServer.tscn").Instance<NetworkServer>();
            server.Init(GodotTaskFactory, GamesManager.Instance, TurnProcessorManager.Instance);
            if (continueGameName != null && continueGameYear != -1)
            {
                await server.ContinueGame(continueGameName, continueGameYear);
            }
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

            log.Info("Started network server to host game");

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

