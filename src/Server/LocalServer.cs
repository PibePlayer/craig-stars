using System;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Client;
using Newtonsoft.Json;

namespace CraigStars.Server
{
    public class LocalServer : Server, IServer
    {
        static CSLog log = LogProvider.GetLogger(typeof(LocalServer));

        protected JsonSerializerSettings playerSerializerSettings;

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

        protected override void PublishGameStartingEvent(PublicGameInfo gameInfo)
        {
            var gameInfoJson = Serializers.Serialize(gameInfo);
            PublicGameInfo gameInfoClone = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            Client.EventManager.PublishGameStartingEvent(gameInfoClone);
        }


        protected async override void PublishGameStartedEvent()
        {
            // we have a new game, so create the player serializer settings
            PublicGameInfo gameInfoClone = null;
            await Task.Run(() =>
            {
                log.Debug($"Creating Player SerializerSettings GameStartedEvent.");
                playerSerializerSettings = Serializers.CreatePlayerSettings(Game.GameInfo.Players, Game.TechStore);
                var gameInfoJson = Serializers.Serialize(Game.GameInfo);
                gameInfoClone = Serializers.DeserializeObject<PublicGameInfo>(gameInfoJson);
            });

            // send a signal per non ai player in the game
            // For hotseat games, the ClientView will store all players that can play
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                var playerClone = new Player();
                await Task.Run(() =>
                {
                    log.Debug($"Creating clone of {player} for GameStartedEvent.");
                    var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                    Serializers.PopulatePlayer(playerJson, playerClone, playerSerializerSettings);
                });

                log.Debug($"{player} GameStartedEvent.");
                Client.EventManager.PublishGameStartedEvent(gameInfoClone, playerClone);
            }
        }

        protected override void PublishTurnSubmittedEvent(PublicPlayerInfo player)
        {
            PublicPlayerInfo playerClone = new PublicPlayerInfo(player);

            log.Debug($"{player} TurnSubmittedEvent.");
            Client.EventManager.PublishTurnSubmittedEvent(playerClone);
        }

        protected override void PublishTurnUnsubmittedEvent(PublicPlayerInfo player)
        {
            PublicPlayerInfo playerClone = new PublicPlayerInfo(player);
            Client.EventManager.PublishTurnUnsubmittedEvent(playerClone);
        }

        protected override void PublishTurnGeneratingEvent()
        {
            Client.EventManager.PublishTurnGeneratingEvent();
        }

        protected override void PublishTurnGeneratorAdvancedEvent(TurnGenerationState state)
        {
            Client.EventManager.PublishTurnGeneratorAdvancedEvent(state);
        }

        protected async override void PublishTurnPassedEvent()
        {
            // notify each non AI player about the new turn
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                var clone = new Player();
                await Task.Run(() =>
                {
                    log.Debug($"Creating clone of {player} for TurnPassedEvent.");
                    var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                    Serializers.PopulatePlayer(playerJson, clone, playerSerializerSettings);
                });

                log.Debug($"Notifying {player} of TurnPassedEvent.");
                Client.EventManager.PublishTurnPassedEvent(Game.GameInfo, clone);
            }
        }

        #endregion
    }
}
