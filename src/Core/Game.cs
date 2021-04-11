using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// The Game manages generating a universe, submitting turns, and storing the source of
    /// truth for all planets, fleets, designs, etc
    /// </summary>
    public class Game
    {
        static ILog log = LogManager.GetLogger(typeof(Game));

        /// <summary>
        /// This event is triggered when turn events happen
        /// </summary>
        public event Action<TurnGeneratorState> TurnGeneratorAdvancedEvent;

        /// <summary>
        /// The TechStore is set by the client on load (or the StaticTechStore for tests )
        /// </summary>
        [JsonIgnore] public ITechStore TechStore { get; set; }

        #region PublicGameInfo

        /// <summary>
        /// The game gives each player a copy of the PublicGameInfo. The properties
        /// in the game that match up with PublicGameInfo properties are just forwarded
        /// </summary>
        [JsonIgnore] public PublicGameInfo GameInfo { get; set; } = new PublicGameInfo();
        public string Name { get => GameInfo.Name; set => GameInfo.Name = value; }
        public Rules Rules { get => GameInfo.Rules; private set => GameInfo.Rules = value; }
        public int Year { get => GameInfo.Year; set => GameInfo.Year = value; }
        public GameMode Mode { get => GameInfo.Mode; set => GameInfo.Mode = value; }
        public GameLifecycle Lifecycle { get => GameInfo.Lifecycle; set => GameInfo.Lifecycle = value; }

        #endregion

        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))]
        public List<Player> Players { get; set; } = new List<Player>();

        public List<ShipDesign> Designs { get; set; } = new List<ShipDesign>();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public List<MineralPacket> MineralPackets { get; set; } = new List<MineralPacket>();
        public List<MineField> MineFields { get; set; } = new List<MineField>();

        #region Computed Members

        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        [JsonIgnore] public Dictionary<Guid, ShipDesign> DesignsByGuid { get; set; } = new Dictionary<Guid, ShipDesign>();
        [JsonIgnore] public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();
        [JsonIgnore] public Dictionary<Guid, ICargoHolder> CargoHoldersByGuid { get; set; } = new Dictionary<Guid, ICargoHolder>();
        [JsonIgnore] public IEnumerable<Planet> OwnedPlanets { get => Planets.Where(p => p.Player != null); }
        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

        #endregion

        // for tests or fast generation
        [JsonIgnore] public bool SaveToDisk { get; set; } = true;

        TurnGenerator turnGenerator;
        TurnSubmitter turnSubmitter;
        GameSerializer gameSerializer;
        GameSaver gameSaver;
        Task aiSubmittingTask;

        public Game()
        {
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);
            gameSaver = new GameSaver(this);
            gameSerializer = new GameSerializer(this);

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            EventManager.FleetCreatedEvent += OnFleetCreated;
            EventManager.FleetDeletedEvent += OnFleetDeleted;
            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
        }

        ~Game()
        {
            EventManager.FleetCreatedEvent -= OnFleetCreated;
            EventManager.FleetDeletedEvent -= OnFleetDeleted;
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // compute aggregates on load
            ComputeAggregates();

            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            UpdateDictionaries();
            
            // make sure AIs submit their turns
            SubmitAITurns();
        }

        public void ComputeAggregates()
        {
            Designs.ForEach(d => d.ComputeAggregate(d.Player));
            Fleets.ForEach(f => f.ComputeAggregate());
        }

        /// <summary>
        /// Update our dictionary of MapObjectsByLocation. This is used for combat
        /// </summary>
        public void UpdateMapObjectsByLocation()
        {
            List<MapObject> mapObjects = new List<MapObject>();
            mapObjects.AddRange(Planets);
            mapObjects.AddRange(Fleets);
            mapObjects.AddRange(MineralPackets);
            mapObjects.AddRange(MineFields);

            MapObjectsByLocation = mapObjects.ToLookup(mo => mo.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());
        }

        public void Init(List<Player> players, Rules rules, ITechStore techStore)
        {
            Players.Clear();
            Players.AddRange(players);
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // make sure each player knows about the game
            Players.ForEach(player => player.Game = GameInfo);

            TechStore = techStore;
            Rules = rules;
        }

        /// <summary>
        /// Generate a new Universe 
        /// </summary>
        public void GenerateUniverse()
        {
            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator(this);
            generator.Generate();

            ComputeAggregates();

            AfterTurnGeneration();

            // update player intel with new universe
            var scanStep = new PlayerScanStep(this, TurnGeneratorState.Scan);
            scanStep.Execute(new TurnGenerationContext(), OwnedPlanets.ToList());

            UpdatePlayers();

            SaveGame();

            // this can happen in the background
            SubmitAITurns();
        }

        public void SubmitTurn(Player player)
        {
            log.Info($"{Year}: {player} submitted turn");
            turnSubmitter.SubmitTurn(player);
        }

        public void UnsubmitTurn(Player player)
        {
            // TODO: what happens if we are in the middle of generating a turn?
            // it should just be a no-op, but we should tell the player somehow
            player.SubmittedTurn = false;
        }

        public Boolean AllPlayersSubmitted()
        {
            return Players.All(p => p.SubmittedTurn);
        }

        /// <summary>
        /// Propogate turn generator events up to clients
        /// </summary>
        /// <param name="state"></param>
        void OnTurnGeneratorAdvanced(TurnGeneratorState state)
        {
            TurnGeneratorAdvancedEvent?.Invoke(state);
        }

        /// <summary>
        /// Generate a new turn
        /// </summary>
        public async Task GenerateTurn()
        {
            GameInfo.Lifecycle = GameLifecycle.GeneratingTurn;
            await Task.Factory.StartNew(() =>
            {
                log.Info($"{Year} Generating new turn");

                // after new player actions and designs are submitted, we need
                // to compute aggregates for fleets and designs
                // for turn generation
                ComputeAggregates();

                turnGenerator.GenerateTurn();

                // do any post-turn generation steps
                AfterTurnGeneration();

                TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Saving);

                SaveGame();

                TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Finished);

                log.Info($"{Year} Generating turn complete");

            });
            GameInfo.Lifecycle = GameLifecycle.WaitingForPlayers;

            // After we have notified players 
            SubmitAITurns();

            if (Year < Rules.StartingYear + Rules.QuickStartTurns)
            {
                Players.ForEach(p =>
                {
                    if (!p.AIControlled)
                    {
                        RunTurnProcessors(p);
                        SubmitTurn(p);
                    }
                });
                if (aiSubmittingTask != null)
                {
                    await aiSubmittingTask;
                }
                await GenerateTurn();
            }
        }


        /// <summary>
        /// Method for updating player reports and doing any other stuff required after a turn (or universe)
        /// is generated
        /// </summary>
        internal void AfterTurnGeneration()
        {
            TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.UpdatingPlayers);
            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            UpdateDictionaries();

            // update our player information as if we'd just generated a new turn
            UpdatePlayers();

            GameInfo.Lifecycle = GameLifecycle.WaitingForPlayers;
        }

        /// <summary>
        /// After a turn is generated, update some data on each player (like their current best planetary scanner)
        /// </summary>
        internal void UpdatePlayers()
        {
            Players.ForEach(p =>
            {
                p.PlanetaryScanner = p.GetBestPlanetaryScanner();
                p.Fleets.ForEach(f => f.ComputeAggregate());
                p.SetupMapObjectMappings();
                p.UpdateMessageTargets();
            });
        }


        /// <summary>
        /// Run through all the turn processors for each player
        /// </summary>
        public void RunTurnProcessors(Player player)
        {
            List<TurnProcessor> processors = new List<TurnProcessor>() {
                new ShipDesignerTurnProcessor(),
                new ScoutTurnProcessor(),
                new ColonyTurnProcessor(),
                new BomberTurnProcessor(),
                new PlanetProductionTurnProcessor()
            };
            processors.ForEach(processor => processor.Process(Year, player));
        }

        internal void SaveGame()
        {
            if (SaveToDisk && Year >= Rules.StartingYear + Rules.QuickStartTurns)
            {
                // serialize the game to JSON. This must complete before we can
                // modify any state
                var gameJson = gameSerializer.SerializeGame(this);

                // now that we have our json, we can save the game to dis in a separate task
                _ = Task.Run(() =>
                {
                    gameSaver.SaveGame(gameJson);
                });
            }
        }


        /// <summary>
        /// To reference objects between player knowledge and the server's data, we build Dictionaries by Guid
        /// </summary>
        internal void UpdateDictionaries()
        {
            // build game dictionaries by guid
            DesignsByGuid = Designs.ToLookup(d => d.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            PlanetsByGuid = Planets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            FleetsByGuid = Fleets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            CargoHoldersByGuid = new Dictionary<Guid, ICargoHolder>();
            Planets.ForEach(p => CargoHoldersByGuid[p.Guid] = p);
            Fleets.ForEach(f => CargoHoldersByGuid[f.Guid] = f);
            MineralPackets.ForEach(mp => CargoHoldersByGuid[mp.Guid] = mp);

        }

        /// <summary>
        /// Submit any AI turns
        /// This submits all turns in separate threads and returns a Task for them all to complete
        /// </summary>
        void SubmitAITurns()
        {
            var tasks = new List<Task>();
            // submit AI turns
            foreach (var player in Players)
            {
                if (player.AIControlled)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            RunTurnProcessors(player);
                            SubmitTurn(player);
                        }
                        catch (Exception e)
                        {
                            log.Error($"Failed to submit AI turn ${player}", e);
                        }
                    }));
                }
            }
            aiSubmittingTask = Task.Run(() => Task.WaitAll(tasks.ToArray()));
        }

        #region Event Handlers

        void OnFleetCreated(Fleet fleet)
        {
            foreach (var token in fleet.Tokens)
            {
                if (token.Design.Slots.Count == 0)
                {
                    log.Error("Built token with no design slots!");
                }
            }
            log.Debug($"Created new fleet {fleet.Name} - {fleet.Guid}");
            Fleets.Add(fleet);
            FleetsByGuid[fleet.Guid] = fleet;
            CargoHoldersByGuid[fleet.Guid] = fleet;
        }

        void OnFleetDeleted(Fleet fleet)
        {
            if (fleet.Orbiting != null)
            {
                fleet.Orbiting.OrbitingFleets.Remove(fleet);
            }
            Fleets.Remove(fleet);
            FleetsByGuid.Remove(fleet.Guid);
            CargoHoldersByGuid.Remove(fleet.Guid);
            log.Debug($"Deleted fleet {fleet.Name} - {fleet.Guid}");
        }

        void OnPlanetPopulationEmptied(Planet planet)
        {
            // empty this planet
            planet.Player = null;
            planet.Owner = null;
            planet.Starbase = null;
            planet.Scanner = false;
            planet.Defenses = 0;
            planet.ProductionQueue = new ProductionQueue();
        }

        #endregion

    }
}
