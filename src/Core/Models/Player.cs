using Godot;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CraigStars
{
    public class Player : PublicPlayerInfo
    {
        static ILog log = LogManager.GetLogger(typeof(Player));

        /// <summary>
        /// /// The player needs to know information about the game
        /// </summary>
        public PublicGameInfo Game { get; set; } = new PublicGameInfo();

        /// <summary>
        /// true if this player has made changes and needs to save
        /// </summary>
        /// <value></value>
        [JsonIgnore] public bool Dirty { get; set; }

        public override String RaceName { get => Race.Name; }
        public override String RacePluralName { get => Race.PluralName; }

        /// <summary>
        /// Each player gets a copy of rules from the server. These rules are used
        /// for computing various values both for the UI and during turn generation
        /// </summary>
        public Rules Rules { get => Game.Rules; }

        /// <summary>
        /// Each player gets a TechStore from the server (or client)
        /// Note: this defaults to StaticTechStore for testing
        /// </summary>
        [JsonIgnore]
        public ITechStore TechStore { get; set; } = StaticTechStore.Instance;

        public Race Race { get; set; } = new Race();
        public int DefaultHullSet { get; set; } = 0;
        public PlayerStats Stats { get; set; } = new PlayerStats();

        #region Research

        /// <summary>
        /// Each player has a certain level of each tech.
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevels { get; set; } = new TechLevel();

        /// <summary>
        /// The amount spent on each tech level
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevelsSpent { get; set; } = new TechLevel();

        /// <summary>
        /// The percentage of resources to spend on research
        /// </summary>
        /// <value></value>
        public int ResearchAmount { get; set; } = 15;
        public TechField Researching { get; set; } = TechField.Energy;
        public NextResearchField NextResearchField { get; set; } = NextResearchField.LowestField;

        #endregion

        #region Turn Actions

        public List<CargoTransferOrder> CargoTransferOrders { get; set; } = new List<CargoTransferOrder>();
        public List<MergeFleetOrder> MergeFleetOrders { get; set; } = new List<MergeFleetOrder>();
        public List<SplitAllFleetOrder> SplitFleetOrders { get; set; } = new List<SplitAllFleetOrder>();

        [JsonProperty(ItemIsReference = true)]
        public List<FleetOrder> FleetOrders { get; set; } = new List<FleetOrder>();

        #endregion

        #region Intel

        public Intel<ShipDesign> DesignIntel { get; set; } = new Intel<ShipDesign>();
        public Intel<Planet> PlanetIntel { get; set; } = new Intel<Planet>();
        public Intel<Fleet> FleetIntel { get; set; } = new Intel<Fleet>();
        public List<BattleRecord> Battles { get; set; } = new List<BattleRecord>();

        #endregion

        #region Designs

        [JsonIgnore] public List<ShipDesign> Designs { get => DesignIntel.Owned; }
        [JsonIgnore] public List<ShipDesign> ForeignDesigns { get => DesignIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<ShipDesign> AllDesigns { get => DesignIntel.All; }
        [JsonIgnore] public Dictionary<Guid, ShipDesign> DesignsByGuid { get => DesignIntel.ItemsByGuid; }

        [JsonProperty(ItemIsReference = true)]
        public List<ShipDesign> DeletedDesigns { get; set; } = new List<ShipDesign>();

        #endregion

        #region Universe Data

        public PlayerUISettings UISettings { get; set; } = new PlayerUISettings();

        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

        [JsonIgnore] public List<Planet> Planets { get => PlanetIntel.Owned; }
        [JsonIgnore] public List<Planet> ForeignPlanets { get => PlanetIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Planet> AllPlanets { get => PlanetIntel.All; }
        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get => PlanetIntel.ItemsByGuid; }

        [JsonIgnore] public List<Fleet> Fleets { get => FleetIntel.Owned; }
        [JsonIgnore] public List<Fleet> ForeignFleets { get => FleetIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Fleet> AllFleets { get => FleetIntel.All; }
        [JsonIgnore] public Dictionary<Guid, Fleet> FleetsByGuid { get => FleetIntel.ItemsByGuid; }

        /// <summary>
        /// These fleets have been merged into other fleets and no longer exist
        /// We might not need this field. 
        /// TODO: Delete this if we don't actually use it
        /// </summary>
        /// <typeparam name="Fleet"></typeparam>
        /// <returns></returns>
        [JsonProperty(ItemIsReference = true)]
        public List<Fleet> MergedFleets { get; set; } = new List<Fleet>();

        [JsonProperty(ItemIsReference = true)]
        public List<Message> Messages { get; set; } = new List<Message>();
        [JsonIgnore] public IEnumerable<Message> FilteredMessages { get => Messages.Where(m => ((ulong)m.Type & UISettings.MessageTypeFilter) > 0); }

        [JsonProperty(IsReference = true)]
        public Planet Homeworld { get; set; }

        #endregion

        #region Calculated Values

        /// <summary>
        /// The current PlanetaryScanner tech this player has researched
        /// </summary>
        /// <value></value>
        public TechPlanetaryScanner PlanetaryScanner { get; set; }

        #endregion

        #region Serializer Helpers

        /// <summary>
        /// When a player is deserialized, we have to calculate all the computed values
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            SetupMapObjectMappings();
            ComputeAggregates();
        }

        public void SetupMapObjectMappings()
        {
            DesignIntel.SetupItemsByGuid();
            PlanetIntel.SetupItemsByGuid();
            FleetIntel.SetupItemsByGuid();

            List<MapObject> mapObjects = new List<MapObject>();
            mapObjects.AddRange(PlanetIntel.All);
            mapObjects.AddRange(FleetIntel.All);
            MapObjectsByLocation = mapObjects.ToLookup(mo => mo.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());

            Fleets.ForEach(fleet =>
            {
                if (MapObjectsByLocation.TryGetValue(fleet.Position, out var otherMapObjects))
                {
                    foreach (var otherFleet in otherMapObjects.Where(mo => mo is Fleet && mo != fleet))
                    {
                        fleet.OtherFleets.Add((Fleet)otherFleet);
                    }
                }
            });
        }

        /// <summary>
        /// Compute design and fleet aggregates so the UI will show correct values
        /// </summary>
        public void ComputeAggregates()
        {
            Designs.ForEach(d => d.ComputeAggregate(this));
            Fleets.ForEach(f => f.ComputeAggregate());
            Planets.ForEach(p => p.Starbase?.ComputeAggregate());
        }

        #endregion

        /// <summary>
        /// Return true if this other player is our enemy
        /// </summary>
        /// <param name="otherPlayer"></param>
        /// <returns></returns>
        public bool IsEnemy(PublicPlayerInfo otherPlayer)
        {
            if (otherPlayer == null)
            {
                return false;
            }
            return this.Num != otherPlayer.Num;
        }

        /// <summary>
        /// Are we neutral to this player
        /// </summary>
        /// <param name="otherPlayer"></param>
        /// <returns></returns>
        public bool IsNeutral(PublicPlayerInfo otherPlayer)
        {
            // TODO: add some player relations...
            return false;
        }

        /// <summary>
        /// Are we friends?
        /// </summary>
        /// <param name="otherPlayer"></param>
        /// <returns></returns>
        public bool IsFriend(PublicPlayerInfo otherPlayer)
        {
            // TODO: add some player relations...
            // WE HAVE NO FRIENDS
            return false;
        }

        /// <summary>
        /// Delete this design and also delete any tokens that use the design
        /// </summary>
        /// <param name="design"></param>
        public void DeleteDesign(ShipDesign design)
        {
            var designIndex = Designs.FindIndex(d => d == design);

            DeletedDesigns.Add(Designs[designIndex]);
            Designs.RemoveAt(designIndex);

            foreach (var fleet in Fleets)
            {
                fleet.Tokens = fleet.Tokens.Where(token => token.Design != design).ToList();
            }
        }

        /// <summary>
        /// Get the next available fleet id
        /// TODO: with 30k fleets this will take a while...
        /// </summary>
        /// <returns></returns>
        public int GetNextFleetId()
        {
            int i = 1;
            foreach (var fleet in Fleets.OrderBy(f => f.Id))
            {
                if (fleet.Id != i)
                {
                    // find the first available id, starting at 1 and counting up
                    break;
                }
                i++;
            }
            return i;
        }

        /// <summary>
        /// Returns true if the player has this tech
        /// </summary>
        /// <param name="tech">The tech to check requirements for</param>
        /// <returns>True if this player has access to this tech</returns>
        public bool HasTech(Tech tech)
        {
            // we made it here, if we have the levels, we have the tech
            return CanLearnTech(tech) && TechLevels.HasRequiredLevels(tech.Requirements);
        }

        /// <summary>
        /// Can the player ever learn this tech?
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public bool CanLearnTech(Tech tech)
        {
            TechRequirements requirements = tech.Requirements;
            if (requirements.PRTRequired != PRT.None && requirements.PRTRequired != Race.PRT)
            {
                return false;
            }
            if (requirements.PRTDenied != PRT.None && Race.PRT == requirements.PRTDenied)
            {
                return false;
            }

            foreach (LRT lrt in requirements.LRTsRequired)
            {
                if (!Race.HasLRT(lrt))
                {
                    return false;
                }
            }

            foreach (LRT lrt in requirements.LRTsDenied)
            {
                if (Race.HasLRT(lrt))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the best planetary scanner this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechPlanetaryScanner GetBestPlanetaryScanner()
        {
            return GetBestTech<TechPlanetaryScanner>(TechStore, TechCategory.PlanetaryScanner);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechDefense GetBestDefense()
        {
            return GetBestTech<TechDefense>(TechStore, TechCategory.PlanetaryDefense);
        }

        /// <summary>
        /// Get the best engine this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechEngine GetBestEngine()
        {
            return GetBestTech<TechEngine>(TechStore, TechCategory.Engine);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestScanner()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Scanner);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestShield()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Shield);
        }

        /// <summary>
        /// Get the best armor this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestArmor()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Armor);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestBeamWeapon()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.BeamWeapon);
        }

        /// <summary>
        /// Get the best torpedo this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestTorpedo()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Torpedo);
        }

        /// <summary>
        /// Get the best bomb this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestBomb()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.Bomb);
        }

        /// <summary>
        /// Get the best mine robot this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestMineRobot()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.MineRobot);
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestMineLayer()
        {
            return GetBestTech<TechHullComponent>(TechStore, TechCategory.MineLayer);
        }

        /// <summary>
        /// Get the best tech by category
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="techStore"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public T GetBestTech<T>(ITechStore techStore, TechCategory category) where T : Tech
        {
            var techs = techStore.GetTechsByCategory(category);
            techs.Sort((t1, t2) => t2.Ranking.CompareTo(t1.Ranking));
            var tech = techs.Find(t => HasTech(t));
            return tech as T;
        }

        public ShipDesign GetDesign(string name)
        {
            return Designs.Find(d => d.Name == name);
        }

        /// <summary>
        /// Merge
        /// </summary>
        /// <param name="order"></param>
        public void MergeFleet(MergeFleetOrder order)
        {
            order.Source.Merge(order);
            foreach (var fleet in order.Source.OtherFleets)
            {
                MergedFleets.Add(fleet);
                Fleets.Remove(fleet);
            }
        }

        /// <summary>
        /// Go through each message and update the target to a value from our reports
        /// </summary>
        public void UpdateMessageTargets()
        {
            foreach (var message in Messages)
            {
                if (message.Target != null)
                {
                    if (message.Target is Fleet)
                    {
                        if (FleetsByGuid.TryGetValue(message.Target.Guid, out var fleet))
                        {
                            message.Target = fleet;
                        }
                        else
                        {
                            log.Error($"Found a Message Target with a fleet the player hasn't discovered: {message.Target.Name}");
                        }
                    }
                    else if (message.Target is Planet)
                    {
                        message.Target = PlanetsByGuid[message.Target.Guid];
                    }
                    else
                    {
                        log.Error("Found a Message Target with an unknown type, setting to null.");
                        message.Target = null;
                    }
                }
            }
        }


    }
}
