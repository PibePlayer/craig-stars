using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
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
        private NetworkServer server;
        private SinglePlayerServer singlePlayerServer;

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
            serverTree?.Finish();
        }

        #region Single Player

        /// <summary>
        /// Create a new single player server and generate a new game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public PublicGameInfo NewGame(GameSettings<Player> settings)
        {
            singlePlayerServer = GD.Load<PackedScene>("res://src/Client/Singletons/SinglePlayerServer.tscn").Instance<SinglePlayerServer>();
            var game = singlePlayerServer.CreateNewGame(settings);
            AddChild(singlePlayerServer);

            PlayersManager.Me = game.Players[0];

            return game.GameInfo;
        }

        /// <summary>
        /// Create a new single player server and load a saved game game
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public PublicGameInfo ContinueGame(String gameName, int year, int playerIndex = 0)
        {
            singlePlayerServer = GD.Load<PackedScene>("res://src/Client/Singletons/SinglePlayerServer.tscn").Instance<SinglePlayerServer>();
            var game = singlePlayerServer.LoadGame(gameName, year);
            AddChild(singlePlayerServer);

            // check for a player specific save to load
            PlayersManager.Me = game.Players[playerIndex];

            if (GamesManager.Instance.HasPlayerSave(PlayersManager.Me))
            {
                GamesManager.Instance.LoadPlayerSave(PlayersManager.Me, game.GameInfo.Players);
            }

            return game.GameInfo;
        }

        public void ExitGame()
        {
            if (singlePlayerServer != null)
            {
                RemoveChild(singlePlayerServer);
                singlePlayerServer.QueueFree();
            }
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
            var rpc = GD.Load<PackedScene>("res://src/Client/Singletons/RPC.tscn").Instance<RPC>();
            rpc.Name = "RPC";
            serverTree.Root.AddChild(rpc);
            server = GD.Load<PackedScene>("res://src/Client/NetworkServer.tscn").Instance<NetworkServer>();
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
                serverTree.Finish();
                serverTree = null;
            }
            SetProcess(false);
        }

        #endregion

    }
}