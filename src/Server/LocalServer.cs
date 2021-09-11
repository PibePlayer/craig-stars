using System;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Client;
using Newtonsoft.Json;

namespace CraigStars.Server
{
    public class LocalServer : Server
    {
        static CSLog log = LogProvider.GetLogger(typeof(LocalServer));

        protected PlayerJsonSerializerSettings playerSerializerSettings;

        protected override IClientEventPublisher CreateClientEventPublisher()
        {
            return new LocalClientEventPublisher();
        }

        /// <summary>
        /// Override OnSubmitTurnRequested to clone the player
        /// </summary>
        /// <param name="player"></param>
        protected override void OnSubmitTurnRequested(Player player)
        {
            if (player.AIControlled)
            {
                base.OnSubmitTurnRequested(player);
            }
            else
            {
                // before submitting this turn to the server, clone the player
                var clone = new Player();
                var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                Serializers.PopulatePlayer(playerJson, clone, playerSerializerSettings);

                base.OnSubmitTurnRequested(clone);
            }
        }


        #region Publishers

        protected async override void PublishPlayerDataEvent(Player player)
        {
            var playerClone = new Player();
            await Task.Run(() =>
            {
                log.Debug($"Creating clone of {player} for GameStartedEvent.");
                var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                Serializers.PopulatePlayer(playerJson, playerClone, playerSerializerSettings);
            });

            log.Debug($"{player} GameStartedEvent.");
            Client.EventManager.PublishPlayerDataEvent(Game.GameInfo, playerClone);
        }

        protected override void PublishGameStartingEvent(PublicGameInfo gameInfo)
        {
            var gameInfoClone = new PublicGameInfo(gameInfo);
            Client.EventManager.PublishGameStartingEvent(gameInfoClone);
        }

        Player GetPlayerClone(Player player)
        {
            var playerClone = new Player() { Num = player.Num };
            log.Debug($"Creating clone of {player} for GameStartedEvent.");
            var playerJson = Serializers.Serialize(player, playerSerializerSettings);
            playerSerializerSettings.UpdatePlayer(playerClone);
            Serializers.PopulatePlayer(playerJson, playerClone, playerSerializerSettings);
            return playerClone;
        }

        protected override void PublishGameStartedEvent()
        {
            playerSerializerSettings = Serializers.CreatePlayerSettings(Game.GameInfo.Players, Game.TechStore);

            // send a signal per non ai player in the game
            // For hotseat games, the ClientView will store all players that can play
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                var playerClone = GetPlayerClone(player);

                // we have a new game, so create the player serializer settings
                PublicGameInfo gameInfoClone = new PublicGameInfo(Game.GameInfo);
                log.Debug($"{player} GameStartedEvent.");

                Client.EventManager.PublishGameStartedEvent(gameInfoClone, playerClone);
            }
        }

        protected override void PublishGameContinuedEvent()
        {
            playerSerializerSettings = Serializers.CreatePlayerSettings(Game.GameInfo.Players, Game.TechStore);
            // we have a new game, so create the player serializer settings
            PublicGameInfo gameInfoClone = new PublicGameInfo(Game.GameInfo);

            log.Debug($"GameContinuedEvent.");
            Client.EventManager.PublishGameContinuedEvent(gameInfoClone);
        }

        protected override void PublishTurnSubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PublicGameInfo gameInfoClone = new PublicGameInfo(gameInfo);
            PublicPlayerInfo playerClone = new PublicPlayerInfo(player);

            log.Debug($"{player} TurnSubmittedEvent.");
            Client.EventManager.PublishTurnSubmittedEvent(gameInfo, playerClone);
        }

        protected override void PublishTurnUnsubmittedEvent(PublicGameInfo gameInfo, PublicPlayerInfo player)
        {
            PublicGameInfo gameInfoClone = new PublicGameInfo(gameInfo);
            PublicPlayerInfo playerClone = new PublicPlayerInfo(player);
            Client.EventManager.PublishTurnUnsubmittedEvent(gameInfoClone, playerClone);
        }

        protected override void PublishTurnGeneratingEvent()
        {
            Client.EventManager.PublishTurnGeneratingEvent();
        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {
            Client.EventManager.PublishTurnGeneratorAdvancedEvent(state);
        }

        protected override void PublishTurnPassedEvent()
        {
            // notify each non AI player about the new turn
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                var clone = new Player() { Num = player.Num };
                log.Debug($"Creating clone of {player} for TurnPassedEvent.");
                var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                playerSerializerSettings.UpdatePlayer(clone);
                Serializers.PopulatePlayer(playerJson, clone, playerSerializerSettings);

                log.Debug($"Notifying {player} of TurnPassedEvent.");
                Client.EventManager.PublishTurnPassedEvent(Game.GameInfo, clone);
            }

        }

        #endregion
    }
}

