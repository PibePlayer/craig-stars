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

            // create a new turn submitter to handle submitted turns
            turnSubmitter = new TurnSubmitter(this);

            Signals.PublishPostStartGameEvent(Year);
            Signals.SubmitTurnEvent += OnSubmitTurn;
            Signals.FleetBuiltEvent += OnFleetBuilt;
        }

        void OnFleetBuilt(Fleet fleet)
        {
            Fleets.Add(fleet);
            FleetsByGuid[fleet.Guid] = fleet;
        }

        /// <summary>
        /// Tell the server to Unsubscribe from events so it won't trigger any unecessary calls
        /// </summary>
        public void Shutdown()
        {
            Signals.SubmitTurnEvent -= OnSubmitTurn;
        }

        /// <summary>
        /// The player has submitted a new turn.
        /// Copy any data from this to the main game
        /// </summary>
        /// <param name="player"></param>
        void OnSubmitTurn(Player player)
        {
            SubmitTurn(player);
            if (AllPlayersSubmitted())
            {
                // once everyone is submitted, generate a new turn
                GenerateTurn();
            }
        }

        void UpdateDictionaries()
        {
            // build each players dictionary of planets by id
            PlanetsByGuid = Planets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            FleetsByGuid = Fleets.ToLookup(p => p.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
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
            TurnGenerator generator = new TurnGenerator(this);
            generator.GenerateTurn();
            generator.UpdatePlayerReports();
            UpdateDictionaries();
            Signals.PublishTurnPassedEvent(Year);
        }

    }
}
