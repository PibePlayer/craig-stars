using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.UniverseGeneration;

namespace CraigStars
{
    /// <summary>
    /// The Game manages generating a universe, submitting turns, and storing the source of
    /// truth for all planets, fleets, designs, etc
    /// </summary>
    public class GameRunner
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameRunner));

        /// <summary>
        /// This event is triggered when turn events happen
        /// </summary>
        public event Action<TurnGenerationState> TurnGeneratorAdvancedEvent;

        /// <summary>
        /// The TechStore is set by the client on load (or the StaticTechStore for tests )
        /// </summary>
        public ITechStore TechStore { get; set; }

        /// <summary>
        /// We purge deleted map objects after every turn step
        /// </summary>
        List<MapObject> deletedMapObjects = new List<MapObject>();

        private readonly IProvider<Game> gameProvider;
        private readonly UniverseGenerator universeGenerator;
        private readonly TurnGenerator turnGenerator;
        private readonly TurnSubmitter turnSubmitter;
        private readonly PlayerScanStep playerScanStep;
        private readonly PlanetService planetService;
        private readonly PlayerTechService playerTechService;

        public Game Game { get => gameProvider.Item; }
        public IProvider<Game> GameProvider { get => gameProvider; }

        public GameRunner(
            IProvider<Game> gameProvider,
            UniverseGenerator universeGenerator,
            TurnSubmitter turnSubmitter,
            TurnGenerator turnGenerator,
            PlayerScanStep playerScanStep,
            PlanetService planetService,
            PlayerTechService playerTechService)
        {
            this.gameProvider = gameProvider;
            this.universeGenerator = universeGenerator;
            this.turnGenerator = turnGenerator;
            this.turnSubmitter = turnSubmitter;
            this.playerScanStep = playerScanStep;
            this.planetService = planetService;
            this.playerTechService = playerTechService;

            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent += OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent += OnMapObjectDeleted;

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            turnGenerator.PurgeDeletedMapObjectsEvent += OnPurgeDeletedMapObjects;
        }

        ~GameRunner()
        {
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent -= OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent -= OnMapObjectDeleted;

            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            turnGenerator.PurgeDeletedMapObjectsEvent -= OnPurgeDeletedMapObjects;
        }


        /// <summary>
        /// Generate a new Universe and update all players with turn 0 scan knowledge
        /// </summary>
        public void GenerateUniverse()
        {
            // generate a new univers
            universeGenerator.Generate();

            Game.ComputeAggregates();

            Game.UpdateInternalDictionaries();

            // update player intel with new universe
            playerScanStep.Execute(new TurnGenerationContext(), Game.OwnedPlanets.ToList());

            AfterTurnGeneration();

        }

        public void SubmitTurn(Player player)
        {
            if (player.Num >= 0 && player.Num < Game.Players.Count)
            {
                log.Info($"{Game.Year}: {player} submitted turn");
                Game.Players[player.Num] = player;
                Game.Players[player.Num].SubmittedTurn = true;
                Game.Players[player.Num].SubmittedTurn = true;
            }
            else
            {
                log.Error($"{player} not found in game.");
            }
        }

        public void UnsubmitTurn(PublicPlayerInfo player)
        {
            if (player.Num >= 0 && player.Num < Game.Players.Count)
            {
                Game.Players[player.Num].SubmittedTurn = false;
                Game.Players[player.Num].SubmittedTurn = false;
            }
            else
            {
                log.Error($"{player} not found in game.");
            }
        }

        public Boolean AllPlayersSubmitted()
        {
            return Game.Players.All(p => p.SubmittedTurn);
        }

        /// <summary>
        /// Propogate turn generator events up to clients
        /// </summary>
        /// <param name="state"></param>
        void OnTurnGeneratorAdvanced(TurnGenerationState state)
        {
            TurnGeneratorAdvancedEvent?.Invoke(state);
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        public void GenerateTurn()
        {
            Game.GameInfo.State = GameState.GeneratingTurn;
            log.Info($"{Game.Year} Generating new turn");

            // submit these actions to the actual game objects
            Game.Players.ForEach(player => turnSubmitter.SubmitTurn(player));

            // after new player actions and designs are submitted, we need
            // to compute aggregates for fleets and designs
            // for turn generation
            Game.ComputeAggregates();

            turnGenerator.GenerateTurn();

            // do any post-turn generation steps
            AfterTurnGeneration();

            TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.Finished);

            log.Info($"{Game.Year} Generating turn complete");
        }


        /// <summary>
        /// Method for updating player reports and doing any other stuff required after a turn (or universe)
        /// is generated
        /// </summary>
        internal void AfterTurnGeneration()
        {
            log.Debug($"{Game.Year} Updating internal dictionaries and player dictionaries");
            TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.UpdatingPlayers);

            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            Game.UpdateInternalDictionaries();

            // After a turn is generated, update some data on each player 
            // (like their current best planetary scanner)
            Game.Players.ForEach(p =>
            {
                p.PlanetaryScanner = playerTechService.GetBestPlanetaryScanner(p);
                p.ComputeAggregates();
                p.SetupMapObjectMappings();
                p.UpdateMessageTargets();
            });
        }

        #region Event Handlers


        /// <summary>
        /// Remove all deleted map objects from the ame
        /// </summary>
        internal void OnPurgeDeletedMapObjects()
        {
            deletedMapObjects.ForEach(mapObject =>
            {
                Game.DeleteMapObject(mapObject);
            });
            deletedMapObjects.Clear();
        }

        void OnMapObjectCreated(MapObject mapObject)
        {
            Game.CreateMapObject(mapObject);
        }

        void OnMapObjectDeleted(MapObject mapObject)
        {
            // add these to the deletedMapObjects list for future deletion
            deletedMapObjects.Add(mapObject);
        }

        void OnPlanetPopulationEmptied(Planet planet)
        {
            planetService.EmptyPlanet(planet);
        }

        #endregion

    }
}
