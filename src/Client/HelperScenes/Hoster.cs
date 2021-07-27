using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;

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
            ServerManager.Instance.HostGame(Settings.Instance.ServerPort);

            // join our own game
            CallDeferred(nameof(GoToLobby));
        }

        void GoToLobby()
        {
            this.ChangeSceneTo<Lobby>("res://src/Client/MenuScreens/Lobby.tscn", (instance) =>
            {
                instance.HostMode = true;
            });
            CallDeferred(nameof(HostJoinNewlyHostedGame));
        }

        /// <summary>
        /// Join our own newly hosted game
        /// </summary>
        void HostJoinNewlyHostedGame()
        {
            NetworkClient.Instance.JoinGame("localhost", Settings.Instance.ServerPort);
        }

    }
}
