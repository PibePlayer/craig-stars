using Godot;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        public List<SplitFleetOrder> SplitFleetOrders { get; set; } = new List<SplitFleetOrder>();

        [JsonProperty(ItemIsReference = true)]
        public List<FleetOrder> FleetOrders { get; set; } = new List<FleetOrder>();

        #endregion

        #region Intel

        public Intel<ShipDesign> DesignIntel { get; set; } = new Intel<ShipDesign>();
        public Intel<Planet> PlanetIntel { get; set; } = new Intel<Planet>();
        public Intel<Fleet> FleetIntel { get; set; } = new Intel<Fleet>();

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

        public void ComputeAggregates()
        {
            Designs.ForEach(d => d.ComputeAggregate(this));
            Fleets.ForEach(f => f.ComputeAggregate());
            Planets.ForEach(p => p.Starbase?.ComputeAggregate());
        }

        #endregion

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
        /// This function will be called recursively until no more levels are passed
        /// From starsfaq
        ///   The cost of a tech level depends on four things: 
        ///  1) Your research setting for that field (cheap, normal, or expensive) 
        ///  2) The level you are researching (higher level, higher cost) 
        ///  3) The total number of tech levels you have already have in all fields (you can add it up yourself, or look at 'tech levels' on the 'score' screen). 
        ///  4) whether 'slow tech advance' was selected as a game parameter.
        ///
        ///  in general,
        ///
        ///  totalCost=(baseCost + (totalLevels * 10)) * costFactor
        ///
        ///  where  totalLevels=the sum of your current levels in all fields 
        ///    costFactor =.5 if your setting for the field is '50% less' 
        ///                       =1 if your setting for the field is 'normal' 
        ///                       =1.75 if your setting for the field is '75% more expensive'
        ///
        //  If 'slow tech advance' is a game parameter, totalCost should be doubled.
        ///
        ///  Below is a table showing the base cost of each level. 
        ///
        ///  1     50              14    18040 
        ///  2     80              15    22440 
        ///  3     130             16    27050 
        ///  4     210             17    31870 
        ///  5     340             18    36900 
        ///  6     550             19    42140 
        ///  7     890             20    47590 
        ///  8     1440            21    53250 
        ///  9     2330            22    59120 
        ///  10    3770            23    65200 
        ///  11    6100            24    71490 
        ///  12    9870            25    77990 
        ///  13    13850           26    84700
        /// 
        /// 
        /// </summary>
        /// <param name="resourcesToSpend">The amount of resources to spend on research</param>
        public void ResearchNextLevel(int resourcesToSpend)
        {
            // add the resourcesToSpend to how much we've currently spent
            TechLevelsSpent[Researching] += resourcesToSpend;
            var spentOnCurrentLevel = TechLevelsSpent[Researching];

            // don't research more than the max on this level
            // TODO: If we get to max level, automatically switch to the lowest field
            var level = TechLevels[Researching];
            if (level >= Rules.TechBaseCost.Length - 1)
            {
                Message.Info(this, $"You are already at level {level} in {Researching} and cannot research further.");
                return;
            }

            // figure out our total levels
            var totalLevels = TechLevels.Sum();

            // figure out the cost to advance to the next level
            var baseCost = Rules.TechBaseCost[level + 1];
            var researchCost = Race.ResearchCost[Researching];
            var costFactor = 1f;
            switch (researchCost)
            {
                case ResearchCostLevel.Extra:
                    costFactor = 1.75f;
                    break;
                case ResearchCostLevel.Less:
                    costFactor = .5f;
                    break;
            }

            // from starsfaq
            int totalCost = (int)((baseCost + (totalLevels * 10)) * costFactor);

            if (spentOnCurrentLevel >= totalCost)
            {
                // increase a level
                TechLevels[Researching]++;

                // figure out how many leftover points we have
                var leftoverResources = spentOnCurrentLevel - totalCost;

                // reset the amount we spent to zero
                TechLevelsSpent[Researching] = 0;

                // determine the next field to research
                var nextField = GetNextResearchField();

                // notify our player that we got a new level
                Message.TechLevel(this, Researching, TechLevels[Researching], nextField);

                // setup the next level
                Researching = nextField;

                if (leftoverResources > 0)
                {
                    // we have leftover resources, so call ourselves again
                    // to apply them to the next level
                    ResearchNextLevel(leftoverResources);
                }
            }
        }

        /// <summary>
        /// Get the next TechField to research based on the NextResearchField setting
        /// </summary>
        /// <returns></returns>
        public TechField GetNextResearchField()
        {
            var nextField = Researching;
            switch (NextResearchField)
            {
                case NextResearchField.Energy:
                    nextField = TechField.Energy;
                    break;
                case NextResearchField.Weapons:
                    nextField = TechField.Weapons;
                    break;
                case NextResearchField.Propulsion:
                    nextField = TechField.Propulsion;
                    break;
                case NextResearchField.Construction:
                    nextField = TechField.Construction;
                    break;
                case NextResearchField.Electronics:
                    nextField = TechField.Electronics;
                    break;
                case NextResearchField.Biotechnology:
                    nextField = TechField.Biotechnology;
                    break;
                case NextResearchField.LowestField:
                    nextField = TechLevels.Lowest();
                    break;
            }

            return nextField;
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
