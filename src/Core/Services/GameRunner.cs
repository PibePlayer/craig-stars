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
        /// This event is triggered when universe generation events happen
        /// </summary>
        public event Action<UniverseGenerationState> UniverseGeneratorAdvancedEvent;

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
        private readonly PlayerOrdersValidator playerOrdersValidator;
        private readonly PlayerOrdersConsumer playerOrdersConsumer;
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
            PlayerOrdersValidator playerOrdersValidator,
            PlayerOrdersConsumer playerOrdersConsumer,
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
            this.playerOrdersValidator = playerOrdersValidator;
            this.playerOrdersConsumer = playerOrdersConsumer;
            this.playerScanStep = playerScanStep;
            this.planetService = planetService;
            this.playerTechService = playerTechService;
            this.fleetSpecService = fleetSpecService;

            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent += OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent += OnMapObjectDeleted;
            EventManager.PlayerResearchLevelIncreasedEvent += OnPlayerResearchLevelIncreased;

            universeGenerator.UniverseGeneratorAdvancedEvent += OnUniverseGeneratorAdvanced;
            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            turnGenerator.PurgeDeletedMapObjectsEvent += OnPurgeDeletedMapObjects;
        }

        ~GameRunner()
        {
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent -= OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent -= OnMapObjectDeleted;
            EventManager.PlayerResearchLevelIncreasedEvent -= OnPlayerResearchLevelIncreased;

            universeGenerator.UniverseGeneratorAdvancedEvent -= OnUniverseGeneratorAdvanced;
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
            universeGenerator.PublishUniverseGeneratorAdvancedEvent(UniverseGenerationState.Scan);
            playerScanStep.Execute(new TurnGenerationContext(), Game.OwnedPlanets.ToList());

            AfterTurnGeneration();
        }

        public PlayerOrdersValidatorResult SubmitTurn(PlayerOrders orders)
        {
            var result = ValidatePlayerOrders(orders);
            if (result.IsValid)
            {
                // save the orders for turn generation
                Game.PlayerOrders[orders.PlayerNum] = orders;

                // update info about this player to indicate they have submitted their turn
                var player = Game.Players[orders.PlayerNum];
                log.Info($"{Game.Year}: {player} submitted turn");
                Game.Players[orders.PlayerNum].SubmittedTurn = true;
                Game.GameInfo.Players[orders.PlayerNum].SubmittedTurn = true;

            }
            else
            {
                log.Error($"{orders.PlayerNum} turn submit failed.");
            }

            return result;
        }

        public void UnsubmitTurn(PublicPlayerInfo player)
        {
            if (player.Num >= 0 && player.Num < Game.Players.Count)
            {
                Game.Players[player.Num].SubmittedTurn = false;
                Game.GameInfo.Players[player.Num].SubmittedTurn = false;
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
        /// Propogate universe generator events up to clients
        /// </summary>
        /// <param name="state"></param>
        void OnUniverseGeneratorAdvanced(UniverseGenerationState state)
        {
            UniverseGeneratorAdvancedEvent?.Invoke(state);
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
        /// Validate a player's orders
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public PlayerOrdersValidatorResult ValidatePlayerOrders(PlayerOrders orders)
        {
            return playerOrdersValidator.Validate(orders);
        }

        /// <summary>
        /// Take a player's orders and update the Player for the orders
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public void ConsumePlayerOrders(PlayerOrders orders)
        {
            playerOrdersConsumer.ConsumeOrders(orders);
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        public void GenerateTurn()
        {
            Game.GameInfo.State = GameState.GeneratingTurn;
            log.Info($"{Game.Year} Generating new turn");

            // consume each player's orders before generating the turn
            Game.Players.ForEach(player =>
            {
                if (Game.PlayerOrders[player.Num] != null)
                {
                    ConsumePlayerOrders(Game.PlayerOrders[player.Num]);
                }
            });

            // purge any deleted fleets from fleet merges
            OnPurgeDeletedMapObjects();

            // after new player actions and designs are submitted, we need
            // to compute specs for fleets and designs
            // for turn generation
            ComputeSpecs(recompute: true);

            turnGenerator.GenerateTurn();

            // do any post-turn generation steps
            TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.UpdatingPlayers);
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

            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            Game.UpdateInternalDictionaries();

            // After a turn is generated, update some data on each player 
            // (like their current best planetary scanner)
            Game.Players.ForEach(player =>
            {
                player.SubmittedTurn = false;
                player.PlanetaryScanner = playerTechService.GetBestPlanetaryScanner(player);
                fleetSpecService.ComputePlayerFleetSpecs(player, recompute: true);
                player.AllPlanets
                    .Where(planet => planet.Explored)
                    .ToList()
                    .ForEach(planet => planet.Spec = planetService.ComputePlanetSpec(planet, player));
                player.SetupMapObjectMappings();
                player.UpdateMessageTargets();
            });

            Game.GameInfo.Players.ForEach(player =>
            {
                player.SubmittedTurn = false;
            });
            // clear out the player orders so new ones can be submitted
            Game.PlayerOrders = new PlayerOrders[Game.Players.Count];
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
