using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// Special Hoster node for quickly launching a hosted game
    /// </summary>
    public class Hoster : Control
    {
        public override void _Ready()
        {
            PlayersManager.Reset();
            PlayersManager.CreatePlayersForNewGame();
            ServerManager.Instance.HostGame(port: Settings.Instance.ServerPort);

            // join our own game
            CallDeferred(nameof(GoToLobby));
        }

        void GoToLobby()
        {
            this.ChangeSceneTo<LobbyMenu>("res://src/Client/MenuScreens/LobbyMenu.tscn", (instance) =>
            {
                instance.IsHost = true;
            });
            CallDeferred(nameof(HostJoinNewlyHostedGame));
        }

        /// <summary>
        /// Join our own newly hosted game
        /// </summary>
        void HostJoinNewlyHostedGame()
        {
            NetworkClient.Instance.JoinNewGame("localhost", Settings.Instance.ServerPort);
        }

    }
}
