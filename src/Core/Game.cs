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
        GameSaver gameSaver;
        Task savingGameTask;

        public Game()
        {
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);
            gameSaver = new GameSaver(this);

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratedAdvanced;
            EventManager.FleetCreatedEvent += OnFleetCreated;
            EventManager.FleetDeletedEvent += OnFleetDeleted;
            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
        }

        ~Game()
        {
            EventManager.FleetCreatedEvent -= OnFleetCreated;
            EventManager.FleetDeletedEvent -= OnFleetDeleted;
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratedAdvanced;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // compute aggregates on load
            ComputeAggregates();

            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            UpdateDictionaries();
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

            RunTurnProcessors();

            SaveGame();
        }

        public async Task SubmitTurn(Player player)
        {
            if (savingGameTask != null)
            {
                await savingGameTask;
            }
            turnSubmitter.SubmitTurn(player);
        }

        public Boolean AllPlayersSubmitted()
        {
            return Players.All(p => p.SubmittedTurn);
        }

        /// <summary>
        /// Propogate turn generator events up to clients
        /// </summary>
        /// <param name="state"></param>
        void OnTurnGeneratedAdvanced(TurnGeneratorState state)
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
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // after new player actions and designs are submitted, we need
                // to compute aggregates for fleets and designs
                // for turn generation
                ComputeAggregates();

                turnGenerator.GenerateTurn();

                // do any post-turn generation steps
                AfterTurnGeneration();

                SaveGame();

                TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Finished);

                stopwatch.Stop();

                log.Debug($"Turn Generated ({stopwatch.ElapsedMilliseconds}ms)");
            });


            if (Year < Rules.StartingYear + Rules.QuickStartTurns)
            {
                Players.ForEach(p =>
                {
                    if (!p.AIControlled)
                    {
                        SubmitTurn(p);
                    }
                });
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

            // run AI turn processors
            TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.RunningTurnProcessors);
            RunTurnProcessors();

            // first round, we have to submit AI turns
            SubmitAITurns();

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
        internal void RunTurnProcessors()
        {
            List<TurnProcessor> processors = new List<TurnProcessor>() {
                new ShipDesignerTurnProcessor(),
                new ScoutTurnProcessor(),
                new ColonyTurnProcessor(),
                new BomberTurnProcessor(),
                new PlanetProductionTurnProcessor()
            };
            Players.ForEach(player =>
            {
                // TODO: make turn processors configurable
                if (player.AIControlled || true)
                {
                    processors.ForEach(processor => processor.Process(Year, player));
                }
            });

        }

        internal async Task SaveGame()
        {
            if (SaveToDisk && Year >= Rules.StartingYear + Rules.QuickStartTurns)
            {
                TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Saving);
                // save the game to disk
                savingGameTask = gameSaver.SaveGame(this);
                await savingGameTask;
                savingGameTask = null;
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
        /// </summary>
        void SubmitAITurns()
        {
            // submit AI turns
            Players.ForEach(async p =>
            {
                if (p.AIControlled)
                {
                    await SubmitTurn(p as Player);
                }
            });
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
