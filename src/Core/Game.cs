using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class Game
    {
        static ILog log = LogManager.GetLogger(typeof(Game));

        /// <summary>
        /// This event is triggered when turn events happen
        /// </summary>
        public event Action<TurnGeneratorState> TurnGeneratorAdvancedEvent;

        public String Name { get; set; } = "A Barefoot Jaywalk";
        public Rules Rules { get; private set; } = new Rules();

        [JsonIgnore] public ITechStore TechStore { get; set; }
        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))] public List<Player> Players { get; set; } = new List<Player>();
        public List<ShipDesign> Designs { get; set; } = new List<ShipDesign>();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public List<MineralPacket> MineralPackets { get; set; } = new List<MineralPacket>();
        public List<MineField> MineFields { get; set; } = new List<MineField>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Year { get; set; } = 2400;

        #region Computed Members

        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        [JsonIgnore] public Dictionary<Guid, ShipDesign> DesignsByGuid { get; set; } = new Dictionary<Guid, ShipDesign>();
        [JsonIgnore] public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();
        [JsonIgnore] public Dictionary<Guid, ICargoHolder> CargoHoldersByGuid { get; set; } = new Dictionary<Guid, ICargoHolder>();

        #endregion

        TurnGenerator turnGenerator;
        TurnSubmitter turnSubmitter;
        GameSaver gameSaver;

        public Game()
        {
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);
            gameSaver = new GameSaver(this);

            turnGenerator.TurnGeneratorAdvancedEvent += OnTurnGeneratedAdvanced;
            EventManager.FleetBuiltEvent += OnFleetBuilt;
        }

        ~Game()
        {
            EventManager.FleetBuiltEvent -= OnFleetBuilt;
            turnGenerator.TurnGeneratorAdvancedEvent -= OnTurnGeneratedAdvanced;
        }

        public void Init(List<Player> players, Rules rules, ITechStore techStore)
        {
            Players.Clear();
            Players.AddRange(players);
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
            AfterTurnGeneration();
        }

        public void SubmitTurn(Player player)
        {
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
            await Task.Factory.StartNew(() =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                turnGenerator.GenerateTurn();

                // do any post-turn generation steps
                AfterTurnGeneration();

                TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Finished);

                stopwatch.Stop();

                log.Debug($"Turn Generated ({stopwatch.ElapsedMilliseconds}ms)");
            });
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
            turnGenerator.UpdatePlayerReports();
            turnGenerator.RunTurnProcessors();

            // first round, we have to submit AI turns
            SubmitAITurns();

            TurnGeneratorAdvancedEvent?.Invoke(TurnGeneratorState.Saving);
            // save the game to disk
            gameSaver.SaveGame(this);
        }


        /// <summary>
        /// To reference objects between player knowledge and the server's data, we build Dictionaries by Guid
        /// </summary>
        void UpdateDictionaries()
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
            Players.ForEach(p =>
            {
                if (p.AIControlled)
                {
                    SubmitTurn(p as Player);
                }
            });
        }

        #region Event Handlers

        void OnFleetBuilt(Fleet fleet)
        {
            Fleets.Add(fleet);
            FleetsByGuid[fleet.Guid] = fleet;
            CargoHoldersByGuid[fleet.Guid] = fleet;
        }

        #endregion

    }
}
