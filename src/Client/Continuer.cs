using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    /// <summary>
    /// Helper class to continue a game
    /// If the game is hosted and the player is the host, it will start hosting again
    /// If the game is a hotseat, all players will be loaded 
    /// </summary>
    public class Continuer : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(Continuer));

        public void Continue(string gameName, int year, int playerNum)
        {
            // load the game info, and then continue
            var gameInfo = GamesManager.Instance.LoadPlayerGameInfo(gameName, year);
            Continue(gameInfo, playerNum);
        }

        public void Continue(PublicGameInfo gameInfo, int playerNum)
        {
            // load the player for this game
            var player = GamesManager.Instance.LoadPlayerSave(gameInfo, playerNum);
            var players = new List<Player>() { player };

            if (gameInfo.Mode == GameMode.HostedMultiplayer && player.Host)
            {
                // start hosting this game
                ServerManager.Instance.HostGame(Settings.Instance.ServerPort, gameInfo.Name, gameInfo.Year);
            }
            else if (gameInfo.Mode == GameMode.HotseatMultiplayer)
            {
                var playersWithSaves = GamesManager.Instance.GetPlayerSaves(gameInfo);
                playersWithSaves.ForEach(playerWithSave =>
                {
                    if (playerWithSave.Num != player.Num)
                    {
                        log.Info($"Loading player save for player {playerWithSave.Num}");
                        players.Add(GamesManager.Instance.LoadPlayerSave(gameInfo, playerWithSave.Num));
                    }
                });
            }

            // for multiplayer games, load only our player save.
            // For singleplayer/hotseat games, load all players
            this.ChangeSceneTo<ClientView>("res://src/Client/ClientView.tscn", (clientView) =>
            {
                clientView.GameInfo = gameInfo;
                clientView.LocalPlayers = players;
            });
        }
    }
}