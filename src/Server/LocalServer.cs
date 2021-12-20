using System;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Client;
using CraigStars.Singletons;
using Newtonsoft.Json;

namespace CraigStars.Server
{
    public class LocalServer : Server
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
        protected override void OnSubmitTurnRequested(PlayerOrders orders)
        {
            if (orders.PlayerNum >= 0 && orders.PlayerNum < Game.Players.Count && Game.Players[orders.PlayerNum].AIControlled)
            {
                base.OnSubmitTurnRequested(orders);
            }
            else
            {
                base.OnSubmitTurnRequested(orders);
            }
        }


        #region Publishers

        protected async override void PublishPlayerDataEvent(Player player)
        {
            Player playerClone = null;
            await Task.Run(() =>
            {
                log.Debug($"Creating clone of {player} for GameStartedEvent.");
                var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                playerClone = Serializers.DeserializeObject<Player>(playerJson, playerSerializerSettings);
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
            log.Debug($"Creating clone of {player} for GameStartedEvent.");
            var playerJson = Serializers.Serialize(player, playerSerializerSettings);
            // log.Debug($"Player json: \n {playerJson}");
            var playerClone = Serializers.DeserializeObject<Player>(playerJson, playerSerializerSettings);
            return playerClone;
        }

        protected override void PublishGameStartedEvent()
        {
            playerSerializerSettings = Serializers.CreatePlayerSettings(TechStore.Instance);

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
            playerSerializerSettings = Serializers.CreatePlayerSettings(TechStore.Instance);
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

        protected override void PublishUniverseGeneratorAdvancedEvent(UniverseGenerationState state)
        {
            Client.EventManager.PublishUniverseGeneratorAdvancedEvent(state);
        }

        protected override void PublishTurnPassedEvent()
        {
            // notify each non AI player about the new turn
            foreach (var player in Game.Players.Where(player => !player.AIControlled))
            {
                log.Debug($"Creating clone of {player} for TurnPassedEvent.");
                var playerJson = Serializers.Serialize(player, playerSerializerSettings);
                var clone = Serializers.DeserializeObject<Player>(playerJson, playerSerializerSettings);

                log.Debug($"Notifying {player} of TurnPassedEvent.");
                Client.EventManager.PublishTurnPassedEvent(Game.GameInfo, clone);
            }

        }

        #endregion
    }
}

