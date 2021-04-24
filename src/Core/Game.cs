using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CraigStars.Singletons;
using CraigStars.UniverseGeneration;
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
        public event Action<TurnGenerationState> TurnGeneratorAdvancedEvent;

        /// <summary>
        /// The TechStore is set by the client on load (or the StaticTechStore for tests )
        /// </summary>
        [JsonIgnore] public ITechStore TechStore { get; set; }

        /// <summary>
        /// The GamesManager is used to save turns
        /// </summary>
        [JsonIgnore] public IGamesManager GamesManager { get; set; }

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
        public List<Salvage> Salvage { get; set; } = new List<Salvage>();
        public List<Wormhole> Wormholes { get; set; } = new List<Wormhole>();
        public List<MysteryTrader> MysteryTraders { get; set; } = new List<MysteryTrader>();

        #region Computed Members

        [JsonIgnore] public Dictionary<Guid, MapObject> MapObjectsByGuid { get; set; } = new Dictionary<Guid, MapObject>();
        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        [JsonIgnore] public Dictionary<Guid, ShipDesign> DesignsByGuid { get; set; } = new Dictionary<Guid, ShipDesign>();
        [JsonIgnore] public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();
        [JsonIgnore] public Dictionary<Guid, MineField> MineFieldsByGuid { get; set; } = new Dictionary<Guid, MineField>();
        [JsonIgnore] public Dictionary<Guid, MineralPacket> MineralPacketsByGuid { get; set; } = new Dictionary<Guid, MineralPacket>();
        [JsonIgnore] public Dictionary<Guid, Salvage> SalvageByGuid { get; set; } = new Dictionary<Guid, Salvage>();
        [JsonIgnore] public Dictionary<Guid, Wormhole> WormholesByGuid { get; set; } = new Dictionary<Guid, Wormhole>();
        [JsonIgnore] public Dictionary<Guid, MysteryTrader> MysteryTradersByGuid { get; set; } = new Dictionary<Guid, MysteryTrader>();
        [JsonIgnore] public Dictionary<Guid, ICargoHolder> CargoHoldersByGuid { get; set; } = new Dictionary<Guid, ICargoHolder>();
        [JsonIgnore] public IEnumerable<Planet> OwnedPlanets { get => Planets.Where(p => p.Player != null); }
        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

        #endregion

        /// <summary>
        /// We purge deleted map objects after every turn step
        /// </summary>
        [JsonIgnore]
        List<MapObject> deletedMapObjects = new List<MapObject>();

        // for tests or fast generation
        [JsonIgnore] public bool SaveToDisk { get; set; } = true;

        TurnGenerator turnGenerator;
        TurnSubmitter turnSubmitter;
        Task aiSubmittingTask;

        public Game()
        {
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratorAdvanced;
            EventManager.PlanetPopulationEmptiedEvent += OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent += OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent += OnMapObjectDeleted;
        }

        ~Game()
        {
            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratorAdvanced;
            EventManager.PlanetPopulationEmptiedEvent -= OnPlanetPopulationEmptied;
            EventManager.MapObjectCreatedEvent -= OnMapObjectCreated;
            EventManager.MapObjectDeletedEvent -= OnMapObjectDeleted;
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
            foreach (var planet in OwnedPlanets.Where(p => p.HasStarbase))
            {
                planet.Starbase.ComputeAggregate();
            }

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
            mapObjects.AddRange(Salvage);
            mapObjects.AddRange(Wormholes);
            mapObjects.AddRange(MysteryTraders);

            MapObjectsByLocation = mapObjects.ToLookup(mo => mo.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());
        }

        public void Init(List<Player> players, Rules rules, ITechStore techStore, IGamesManager gamesManager)
        {
            Players.Clear();
            Players.AddRange(players);
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // make sure each player knows about the game
            Players.ForEach(player => player.Game = GameInfo);

            TechStore = techStore;
            GamesManager = gamesManager;
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

            UpdateDictionaries();

            // update player intel with new universe
            var scanStep = new PlayerScanStep(this);
            scanStep.Execute(new TurnGenerationContext(), OwnedPlanets.ToList());

            AfterTurnGeneration();

            SaveGame();

            // this can happen in the background
            SubmitAITurns();
        }

        public void SubmitTurn(Player player)
        {
            log.Info($"{Year}: {player} submitted turn");
            turnSubmitter.SubmitTurn(player);
            SaveGame();
        }

        public void UnsubmitTurn(Player player)
        {
            // TODO: what happens if we are in the middle of generating a turn?
            // it should just be a no-op, but we should tell the player somehow
            player.SubmittedTurn = false;
            SaveGame();
        }

        public Boolean AllPlayersSubmitted()
        {
            return Players.All(p => p.SubmittedTurn);
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
        public async Task GenerateTurn()
        {
            await aiSubmittingTask;
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

                TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.Saving);

                SaveGame();

                TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.Finished);

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
            log.Debug($"{Year} Updating internal dictionaries and player dictionaries");
            TurnGeneratorAdvancedEvent?.Invoke(TurnGenerationState.UpdatingPlayers);

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
                p.ComputeAggregates();
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
                new ShipDesignerTurnProcessor(GameInfo),
                new ScoutTurnProcessor(GameInfo),
                new ColonyTurnProcessor(GameInfo),
                new BomberTurnProcessor(GameInfo),
                new MineLayerTurnProcessor(GameInfo),
                new PlanetProductionTurnProcessor(GameInfo)
            };
            processors.ForEach(processor => processor.Process(player));
        }

        internal void SaveGame()
        {
            if (SaveToDisk && Year >= Rules.StartingYear + Rules.QuickStartTurns)
            {
                // serialize the game to JSON. This must complete before we can
                // modify any state
                var gameJson = GamesManager.SerializeGame(this);

                // now that we have our json, we can save the game to dis in a separate task
                _ = Task.Run(() =>
                {
                    GamesManager.SaveGame(gameJson);
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
            FleetsByGuid = Fleets.ToLookup(f => f.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            MineFieldsByGuid = MineFields.ToLookup(mf => mf.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            MineralPacketsByGuid = MineralPackets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            SalvageByGuid = Salvage.ToLookup(s => s.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            WormholesByGuid = Wormholes.ToLookup(w => w.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            MysteryTradersByGuid = MysteryTraders.ToLookup(mt => mt.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

            MapObjectsByGuid.Clear();
            Planets.ForEach(p => MapObjectsByGuid[p.Guid] = p);
            Fleets.ForEach(f => MapObjectsByGuid[f.Guid] = f);
            MineFields.ForEach(mf => MapObjectsByGuid[mf.Guid] = mf);
            Salvage.ForEach(s => MapObjectsByGuid[s.Guid] = s);
            Wormholes.ForEach(w => MapObjectsByGuid[w.Guid] = w);
            MysteryTraders.ForEach(mt => MapObjectsByGuid[mt.Guid] = mt);

            CargoHoldersByGuid = new Dictionary<Guid, ICargoHolder>();
            Planets.ForEach(p => CargoHoldersByGuid[p.Guid] = p);
            Fleets.ForEach(f => CargoHoldersByGuid[f.Guid] = f);
            MineralPackets.ForEach(mp => CargoHoldersByGuid[mp.Guid] = mp);
            Salvage.ForEach(s => CargoHoldersByGuid[s.Guid] = s);

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
                if (player.AIControlled && !player.SubmittedTurn)
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

        void OnMapObjectCreated<T>(T mapObject, List<T> items, Dictionary<Guid, T> itemsByGuid) where T : MapObject
        {
            items.Add(mapObject);
            itemsByGuid[mapObject.Guid] = mapObject;
            MapObjectsByGuid[mapObject.Guid] = mapObject;
            if (mapObject is ICargoHolder cargoHolder)
            {
                CargoHoldersByGuid[cargoHolder.Guid] = cargoHolder;
            }

            log.Debug($"Created new {typeof(T)} {mapObject.Name} - {mapObject.Guid}");
        }

        void DeleteMapObject<T>(T mapObject, List<T> items, Dictionary<Guid, T> itemsByGuid) where T : MapObject
        {
            items.Remove(mapObject);
            itemsByGuid.Remove(mapObject.Guid);
            MapObjectsByGuid.Remove(mapObject.Guid);

            if (mapObject is ICargoHolder cargoHolder)
            {
                CargoHoldersByGuid.Remove(cargoHolder.Guid);
            }

            log.Debug($"Deleted {typeof(T)} {mapObject.Name} - {mapObject.Guid}");
        }

        /// <summary>
        /// Remove all deleted map objects from the ame
        /// </summary>
        public void PurgeDeletedMapObjects()
        {
            deletedMapObjects.ForEach(mapObject =>
            {
                if (mapObject is Fleet fleet)
                {
                    if (fleet.Orbiting != null)
                    {
                        fleet.Orbiting.OrbitingFleets.Remove(fleet);
                    }

                    DeleteMapObject(fleet, Fleets, FleetsByGuid);
                }
                else if (mapObject is MineField mineField)
                {
                    DeleteMapObject(mineField, MineFields, MineFieldsByGuid);
                }
                else if (mapObject is Salvage salvage)
                {
                    DeleteMapObject(salvage, Salvage, SalvageByGuid);
                }
                else if (mapObject is MineralPacket packet)
                {
                    DeleteMapObject(packet, MineralPackets, MineralPacketsByGuid);
                }
                else if (mapObject is Wormhole wormhole)
                {
                    DeleteMapObject(wormhole, Wormholes, WormholesByGuid);
                    Players.ForEach(player =>
                    {
                        if (player.WormholesByGuid.TryGetValue(wormhole.Guid, out var playerWormhole))
                        {
                            if (playerWormhole.Destination != null)
                            {
                                // make the other wormhole forget about this one
                                playerWormhole.Destination.Destination = null;
                            }
                            player.Wormholes.Remove(playerWormhole);
                        }
                    });
                }
                else if (mapObject is MysteryTrader mysteryTrader)
                {
                    DeleteMapObject(mysteryTrader, MysteryTraders, MysteryTradersByGuid);
                }
            });
            deletedMapObjects.Clear();
        }

        void OnMapObjectCreated(MapObject mapObject)
        {
            if (mapObject is Fleet fleet)
            {
                OnMapObjectCreated(fleet, Fleets, FleetsByGuid);
            }
            else if (mapObject is MineField mineField)
            {
                OnMapObjectCreated(mineField, MineFields, MineFieldsByGuid);
            }
            else if (mapObject is MineralPacket mineralPacket)
            {
                OnMapObjectCreated(mineralPacket, MineralPackets, MineralPacketsByGuid);
            }
            else if (mapObject is Salvage salvage)
            {
                OnMapObjectCreated(salvage, Salvage, SalvageByGuid);
            }
            else if (mapObject is Wormhole wormhole)
            {
                OnMapObjectCreated(wormhole, Wormholes, WormholesByGuid);
            }
            else if (mapObject is MysteryTrader mysteryTrader)
            {
                OnMapObjectCreated(mysteryTrader, MysteryTraders, MysteryTradersByGuid);
            }
        }

        void OnMapObjectDeleted(MapObject mapObject)
        {
            // add these to the deletedMapObjects list for future deletion
            deletedMapObjects.Add(mapObject);
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
