using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class Server
    {
        static ILog log = LogManager.GetLogger(typeof(Server));

        public Rules Rules { get; private set; } = new Rules();
        public ITechStore TechStore { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Planet> Planets { get; set; } = new List<Planet>();
        public List<Fleet> Fleets { get; set; } = new List<Fleet>();
        public Dictionary<Guid, Planet> PlanetsByGuid { get; set; } = new Dictionary<Guid, Planet>();
        public Dictionary<Guid, Fleet> FleetsByGuid { get; set; } = new Dictionary<Guid, Fleet>();
        public Dictionary<Guid, ICargoHolder> CargoHoldersByGuid { get; set; } = new Dictionary<Guid, ICargoHolder>();
        public List<MineralPacket> MineralPackets { get; set; } = new List<MineralPacket>();
        public List<MineField> MineFields { get; set; } = new List<MineField>();
        public int Width { get; set; }
        public int Height { get; set; }
        public int Year { get; set; } = 2400;

        TurnGenerator turnGenerator;
        TurnSubmitter turnSubmitter;

        public void Init(List<Player> players, Rules rules, ITechStore techStore)
        {
            TechStore = techStore;
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);

            Players.AddRange(players);
            Rules = rules;

            EventManager.FleetBuiltEvent += OnFleetBuilt;
        }

        /// <summary>
        /// Generate a new Universe 
        /// </summary>
        public void GenerateUniverse()
        {
            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator(this);
            generator.Generate();
            UpdateDictionaries();
            AfterTurnGeneration();
        }

        /// <summary>
        /// Tell the server to Unsubscribe from events so it won't trigger any unecessary calls
        /// </summary>
        public void Shutdown()
        {
            EventManager.FleetBuiltEvent -= OnFleetBuilt;
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
        /// Generate a new turn
        /// </summary>
        public void GenerateTurn()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            turnGenerator.GenerateTurn();

            // Update our dictionaries 
            UpdateDictionaries();

            // do any post-turn generation steps
            AfterTurnGeneration();

            stopwatch.Stop();

            log.Debug($"Turn Generated ({stopwatch.ElapsedMilliseconds}ms)");

        }

        /// <summary>
        /// Method for updating player reports and doing any other stuff required after a turn (or universe)
        /// is generated
        /// </summary>
        internal void AfterTurnGeneration()
        {
            // update our player information as if we'd just generated a new turn
            turnGenerator.UpdatePlayerReports();
            turnGenerator.RunTurnProcessors();

            // first round, we have to submit AI turns
            SubmitAITurns();
        }


        /// <summary>
        /// To reference objects between player knowledge and the server's data, we build Dictionaries by Guid
        /// </summary>
        void UpdateDictionaries()
        {
            // build each players dictionary of planets by id
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
