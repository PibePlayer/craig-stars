using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;

namespace CraigStars
{
    /// <summary>
    /// This class manages handling player turn submittals
    /// </summary>
    public class Server
    {
        public UniverseSettings Settings { get; private set; } = new UniverseSettings();
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

        public void Init(List<Player> players, UniverseSettings settings, ITechStore techStore)
        {
            TechStore = techStore;
            turnGenerator = new TurnGenerator(this);
            turnSubmitter = new TurnSubmitter(this);

            Players.AddRange(players);
            Settings = settings;

            // generate a new univers
            UniverseGenerator generator = new UniverseGenerator(this);
            generator.Generate();

            UpdateDictionaries();

            // update our player information as if we'd just generated a new turn
            turnGenerator = new TurnGenerator(this);
            turnGenerator.UpdatePlayerReports();
            turnGenerator.RunTurnProcessors();

            // create a new turn submitter to handle submitted turns
            turnSubmitter = new TurnSubmitter(this);

            // first round, we have to submit AI turns
            SubmitAITurns();

            EventManager.FleetBuiltEvent += OnFleetBuilt;
        }

        void OnFleetBuilt(Fleet fleet)
        {
            Fleets.Add(fleet);
            FleetsByGuid[fleet.Guid] = fleet;
            CargoHoldersByGuid[fleet.Guid] = fleet;
        }

        /// <summary>
        /// Tell the server to Unsubscribe from events so it won't trigger any unecessary calls
        /// </summary>
        public void Shutdown()
        {
            EventManager.FleetBuiltEvent -= OnFleetBuilt;
        }

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

        public void SubmitTurn(Player player)
        {
            turnSubmitter.SubmitTurn(player);
        }

        public Boolean AllPlayersSubmitted()
        {
            return Players.All(p => p.SubmittedTurn);
        }

        public void GenerateTurn()
        {
            turnGenerator.GenerateTurn();
            turnGenerator.UpdatePlayerReports();
            turnGenerator.RunTurnProcessors();
            UpdateDictionaries();
            SubmitAITurns();
        }
    }
}
