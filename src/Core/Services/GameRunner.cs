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
        private readonly RaceService raceService;
        private readonly UniverseGenerator universeGenerator;
        private readonly TurnGenerator turnGenerator;
        private readonly TurnSubmitter turnSubmitter;
        private readonly PlayerScanStep playerScanStep;
        private readonly PlanetService planetService;
        private readonly PlayerTechService playerTechService;
        private readonly FleetSpecService fleetSpecService;

        public Game Game { get => gameProvider.Item; }
        public IProvider<Game> GameProvider { get => gameProvider; }

        public GameRunner(
            IProvider<Game> gameProvider,
            RaceService raceService,
            UniverseGenerator universeGenerator,
            TurnSubmitter turnSubmitter,
            TurnGenerator turnGenerator,
            PlayerScanStep playerScanStep,
            PlanetService planetService,
            PlayerTechService playerTechService,
            FleetSpecService fleetSpecService)
        {
            this.gameProvider = gameProvider;
            this.raceService = raceService;
            this.universeGenerator = universeGenerator;
            this.turnGenerator = turnGenerator;
            this.turnSubmitter = turnSubmitter;
            this.playerScanStep = playerScanStep;
            this.planetService = planetService;
            this.playerTechService = playerTechService;
            this.fleetSpecService = fleetSpecService;

            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent += OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent += OnMapObjectDeleted;
            EventManager.PlayerResearchLevelIncreasedEvent += OnPlayerResearchLevelIncreased;

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            turnGenerator.PurgeDeletedMapObjectsEvent += OnPurgeDeletedMapObjects;
        }

        ~GameRunner()
        {
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent -= OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent -= OnMapObjectDeleted;
            EventManager.PlayerResearchLevelIncreasedEvent -= OnPlayerResearchLevelIncreased;

            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            turnGenerator.PurgeDeletedMapObjectsEvent -= OnPurgeDeletedMapObjects;
        }


        /// <summary>
        /// Generate a new Universe and update all players with turn 0 scan knowledge
        /// </summary>
        public void GenerateUniverse()
        {
            // update all of our player race specs before we generate.
            Game.Players.ForEach(player => player.Race.Spec = raceService.ComputeRaceSpecs(player.Race));

            // generate a new univers
            universeGenerator.Generate();

            ComputeSpecs();

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
            // to compute specs for fleets and designs
            // for turn generation
            ComputeSpecs(recompute: true);

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
            Game.Players.ForEach(player =>
            {
                player.PlanetaryScanner = playerTechService.GetBestPlanetaryScanner(player);
                fleetSpecService.ComputePlayerFleetSpecs(player, recompute: true);
                player.Planets.ForEach(planet => planet.Spec = planetService.ComputePlanetSpec(planet, player));
                player.SetupMapObjectMappings();
                player.UpdateMessageTargets();
            });
        }

        /// <summary>
        /// After game load or creating a new game, loop through all designs, fleets, and starbases and build
        /// </summary>
        public void ComputeSpecs(bool recompute = false)
        {
            // No cheating, make sure only the server updates the race spec!
            Game.Players.ForEach(player => player.Race.Spec = raceService.ComputeRaceSpecs(player.Race));
            Game.OwnedPlanets.ToList().ForEach(planet => planet.Spec = planetService.ComputePlanetSpec(planet, Game.Players[planet.PlayerNum]));
            Game.Designs.ForEach(design => fleetSpecService.ComputeDesignSpec(Game.Players[design.PlayerNum], design, recompute));
            Game.Fleets.ForEach(fleet => fleetSpecService.ComputeFleetSpec(Game.Players[fleet.PlayerNum], fleet, recompute));
            foreach (var planet in Game.OwnedPlanets.Where(p => p.HasStarbase))
            {
                fleetSpecService.ComputeStarbaseSpec(Game.Players[planet.PlayerNum], planet.Starbase, recompute);
            }
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

        /// <summary>
        /// When a player's tech level increases their designs (and therefore fleets) get cheaper. JoaT races also
        /// improve built in scan ranges
        /// </summary>
        /// <param name="player"></param>
        /// <param name="field"></param>
        /// <param name="level"></param>
        void OnPlayerResearchLevelIncreased(Player player, TechField field, int level)
        {
            var updateBuiltInScanners = field == TechField.Electronics && player.Race.Spec.BuiltInScannerMultiplier > 0;
            foreach (var design in Game.Designs.Where(d => d.PlayerNum == player.Num))
            {
                if (updateBuiltInScanners && design.Hull.BuiltInScanner)
                {
                    fleetSpecService.ComputeDesignScanRanges(player, design);
                }
                fleetSpecService.ComputeDesignCost(player, design);
            }

            foreach (var fleet in Game.Fleets.Where(f => f.PlayerNum == player.Num))
            {
                fleetSpecService.ComputeFleetSpec(player, fleet, recompute: true);
            }

        }
        #endregion

    }
}
