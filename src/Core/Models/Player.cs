using Godot;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CraigStars
{
    public class Player : PublicPlayerInfo
    {
        static CSLog log = LogProvider.GetLogger(typeof(Player));

        /// <summary>
        /// /// The player needs to know information about the game
        /// </summary>
        public PublicGameInfo Game { get; set; } = new();

        /// <summary>
        /// true if this player has made changes and needs to save
        /// </summary>
        /// <value></value>
        [JsonIgnore] public bool Dirty { get; set; }

        public override string RaceName { get => Race.Name; }
        public override string RacePluralName { get => Race.PluralName; }

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

        public Race Race { get; set; } = new();
        public int DefaultHullSet { get; set; } = 0;
        public PlayerStats Stats { get; set; } = new();
        public PlayerScore Score { get; set; } = new();
        public List<PlayerScore> ScoreHistory { get; set; } = new();

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
        public int ResearchSpentLastYear { get; set; } = 0;
        public TechField Researching { get; set; } = TechField.Energy;
        public NextResearchField NextResearchField { get; set; } = NextResearchField.LowestField;

        #endregion

        #region Intel

        public Intel<ShipDesign> DesignIntel { get; set; } = new Intel<ShipDesign>();
        public Intel<Planet> PlanetIntel { get; set; } = new Intel<Planet>();
        public Intel<MineField> MineFieldIntel { get; set; } = new Intel<MineField>();
        public Intel<MineralPacket> MineralPacketIntel { get; set; } = new Intel<MineralPacket>();
        public Intel<Salvage> SalvageIntel { get; set; } = new Intel<Salvage>();
        public Intel<Wormhole> WormholeIntel { get; set; } = new Intel<Wormhole>();
        public Intel<MysteryTrader> MysteryTraderIntel { get; set; } = new Intel<MysteryTrader>();
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
        public PlayerSettings Settings { get; set; } = new PlayerSettings();

        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

        [JsonIgnore] public List<Planet> Planets { get => PlanetIntel.Owned; }
        [JsonIgnore] public List<Planet> ForeignPlanets { get => PlanetIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Planet> AllPlanets { get => PlanetIntel.All; }
        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get => PlanetIntel.ItemsByGuid; }

        [JsonIgnore] public List<Salvage> Salvage { get => SalvageIntel.Foriegn; }
        [JsonIgnore] public Dictionary<Guid, Salvage> SalvageByGuid { get => SalvageIntel.ItemsByGuid; }

        [JsonIgnore] public List<Wormhole> Wormholes { get => WormholeIntel.Foriegn; }
        [JsonIgnore] public Dictionary<Guid, Wormhole> WormholesByGuid { get => WormholeIntel.ItemsByGuid; }

        [JsonIgnore] public List<MysteryTrader> MysteryTraders { get => MysteryTraderIntel.Foriegn; }
        [JsonIgnore] public Dictionary<Guid, MysteryTrader> MysteryTradersByGuid { get => MysteryTraderIntel.ItemsByGuid; }

        [JsonIgnore] public List<MineField> MineFields { get => MineFieldIntel.Owned; }
        [JsonIgnore] public List<MineField> ForeignMineFields { get => MineFieldIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<MineField> AllMineFields { get => MineFieldIntel.All; }
        [JsonIgnore] public Dictionary<Guid, MineField> MineFieldsByGuid { get => MineFieldIntel.ItemsByGuid; }

        [JsonIgnore] public List<MineralPacket> MineralPackets { get => MineralPacketIntel.Owned; }
        [JsonIgnore] public List<MineralPacket> ForeignMineralPackets { get => MineralPacketIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<MineralPacket> AllMineralPackets { get => MineralPacketIntel.All; }
        [JsonIgnore] public Dictionary<Guid, MineralPacket> MineralPacketsByGuid { get => MineralPacketIntel.ItemsByGuid; }

        [JsonIgnore] public List<Fleet> Fleets { get => FleetIntel.Owned; }
        [JsonIgnore] public List<Fleet> ForeignFleets { get => FleetIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Fleet> AllFleets { get => FleetIntel.All; }
        [JsonIgnore] public Dictionary<Guid, Fleet> FleetsByGuid { get => FleetIntel.ItemsByGuid; }

        /// <summary>
        /// All map objects by their guid, for lookups
        /// </summary>
        [JsonIgnore] public Dictionary<Guid, MapObject> MapObjectsByGuid { get; set; } = new Dictionary<Guid, MapObject>();

        /// <summary>
        /// All battles by their guid, for lookups
        /// </summary>
        [JsonIgnore] public Dictionary<Guid, BattleRecord> BattlesByGuid { get; set; }

        [JsonIgnore] public Dictionary<Guid, BattlePlan> BattlePlansByGuid = new Dictionary<Guid, BattlePlan>();
        [JsonIgnore] public Dictionary<Guid, TransportPlan> TransportPlansByGuid = new Dictionary<Guid, TransportPlan>();

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

        #region Turn Actions

        /// <summary>
        /// Each player has a list of battle plans. Each fleet has a battle plan assigned
        /// Each player automatically has the Default battle plan
        /// </summary>
        public List<BattlePlan> BattlePlans { get; set; } = new List<BattlePlan>();
        public List<TransportPlan> TransportPlans { get; set; } = new List<TransportPlan>();
        public List<FleetComposition> FleetCompositions { get; set; } = new List<FleetComposition>();
        public List<CargoTransferOrder> CargoTransferOrders { get; set; } = new List<CargoTransferOrder>();
        public List<MergeFleetOrder> MergeFleetOrders { get; set; } = new List<MergeFleetOrder>();
        public List<SplitAllFleetOrder> SplitFleetOrders { get; set; } = new List<SplitAllFleetOrder>();

        [JsonProperty(ItemIsReference = true)]
        public List<FleetOrder> FleetOrders { get; set; } = new List<FleetOrder>();

        #endregion


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
        /// We use object references for players all over the place. Because of
        /// this, we "populate" our player object rather than load it into a new object
        /// I'm not super happy with this, but it's where the code is for now.
        /// Because of this population, we need to wipe out various lists the player
        /// object has. The serializer will re-add to them on load.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            // reset our various lists
            // the populate will add to them
            DesignIntel.Clear();
            PlanetIntel.Clear();
            FleetIntel.Clear();
            MineFieldIntel.Clear();
            MineralPacketIntel.Clear();
            SalvageIntel.Clear();
            WormholeIntel.Clear();
            MysteryTraderIntel.Clear();
            Battles.Clear();

            BattlePlans.Clear();
            TransportPlans.Clear();
            FleetCompositions.Clear();
            CargoTransferOrders.Clear();
            MergeFleetOrders.Clear();
            SplitFleetOrders.Clear();
            FleetOrders.Clear();
            DeletedDesigns.Clear();
            MergedFleets.Clear();
            Messages.Clear();

        }

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
            MineFieldIntel.SetupItemsByGuid();
            MineralPacketIntel.SetupItemsByGuid();
            SalvageIntel.SetupItemsByGuid();
            WormholeIntel.SetupItemsByGuid();
            MysteryTraderIntel.SetupItemsByGuid();
            BattlePlansByGuid = BattlePlans.ToLookup(plan => plan.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            TransportPlansByGuid = TransportPlans.ToLookup(plan => plan.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);
            BattlesByGuid = Battles.ToLookup(battle => battle.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);


            List<MapObject> mapObjects = new List<MapObject>();
            mapObjects.AddRange(PlanetIntel.All);
            mapObjects.AddRange(FleetIntel.All);
            mapObjects.AddRange(MineFieldIntel.All);
            mapObjects.AddRange(MineralPacketIntel.All);
            mapObjects.AddRange(SalvageIntel.All);
            mapObjects.AddRange(WormholeIntel.All);
            mapObjects.AddRange(MysteryTraderIntel.All);
            MapObjectsByLocation = mapObjects.ToLookup(mo => mo.Position).ToDictionary(lookup => lookup.Key, lookup => lookup.ToList());
            MapObjectsByGuid = mapObjects.ToLookup(mo => mo.Guid).ToDictionary(lookup => lookup.Key, lookup => lookup.ToArray()[0]);

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

            ComputeDesignsInUse();
        }

        internal void ComputeDesignsInUse()
        {
            // sum up all designs in use
            // get fleet designs
            var designsInUse = Fleets.SelectMany(f => f.Tokens).Select(token => token.Design).ToLookup(design => design).ToDictionary(lookup => lookup.Key, lookup => lookup.Count());
            // add in starbase designs
            var starbaseDesignsInUse = Planets.Where(planet => planet.HasStarbase).Select(planet => planet.Starbase.Design).ToLookup(design => design).ToDictionary(lookup => lookup.Key, lookup => lookup.Count());
            foreach (var starbaseDesign in starbaseDesignsInUse)
            {
                designsInUse.Add(starbaseDesign.Key, starbaseDesign.Value);
            }

            Designs.ForEach(design =>
            {
                if (designsInUse.TryGetValue(design, out int numInUse))
                {
                    design.NumInUse = numInUse;
                }
            });
        }

        #endregion

        /// <summary>
        /// Run all configured TurnProcessors for this player
        /// </summary>
        /// <param name="turnProcessorManager"></param>
        public void RunTurnProcessors(ITurnProcessorManager turnProcessorManager)
        {
            Settings.TurnProcessors.ForEach(processorName =>
            {
                var processor = turnProcessorManager.GetTurnProcessor(processorName);
                if (processor != null)
                {
                    processor.Process(this);
                }
            });
        }

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
            return Num == otherPlayer.Num;
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
            return Fleets.Select(f => f.Id).Max() + 1;
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
        /// <returns></returns>
        public TechPlanetaryScanner GetBestPlanetaryScanner()
        {
            return GetBestTech<TechPlanetaryScanner>(TechCategory.PlanetaryScanner);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <returns></returns>
        public TechDefense GetBestDefense()
        {
            return GetBestTech<TechDefense>(TechCategory.PlanetaryDefense);
        }

        /// <summary>
        /// Get the best terraform tech this player has for a terraform type
        /// </summary>
        /// <returns></returns>
        public TechTerraform GetBestTerraform(TerraformHabType habType)
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Terraforming).Where(t => t is TechTerraform tf && tf.HabType == habType).ToList();

            techs.Sort((t1, t2) => t2.Ranking.CompareTo(t1.Ranking));
            var tech = techs.Find(t => HasTech(t));

            return tech as TechTerraform;
        }

        /// <summary>
        /// Get the best engine this player has access to
        /// </summary>
        /// <returns></returns>
        public TechEngine GetBestEngine()
        {
            return GetBestTech<TechEngine>(TechCategory.Engine);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestScanner()
        {
            return GetBestTech<TechHullComponent>(TechCategory.Scanner);
        }

        /// <summary>
        /// Get the best shield this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestShield()
        {
            return GetBestTech<TechHullComponent>(TechCategory.Shield);
        }

        /// <summary>
        /// Get the best armor this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestArmor()
        {
            return GetBestTech<TechHullComponent>(TechCategory.Armor);
        }

        /// <summary>
        /// Get the best beam weapon this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestBeamWeapon()
        {
            return GetBestTech<TechHullComponent>(TechCategory.BeamWeapon);
        }

        /// <summary>
        /// Get the best torpedo this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestTorpedo()
        {
            return GetBestTech<TechHullComponent>(TechCategory.Torpedo);
        }

        /// <summary>
        /// Get the best bomb this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestBomb()
        {
            return GetBestTech<TechHullComponent>(TechCategory.Bomb);
        }

        /// <summary>
        /// Get the best mine robot this player has access to
        /// </summary>
        /// <param name="techStore"></param>
        /// <returns></returns>
        public TechHullComponent GetBestMineRobot()
        {
            return GetBestTech<TechHullComponent>(TechCategory.MineRobot);
        }

        /// <summary>
        /// Get the best fuel tank this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestFuelTank()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.FuelBonus > 0)
                .OrderByDescending(t => t is TechHullComponent hc ? hc.FuelBonus : 0);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best cargo pod this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestCargoPod()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.CargoBonus > 0)
                .OrderByDescending(t => t is TechHullComponent hc ? hc.FuelBonus : 0);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestMineLayer()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineLayer)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.MineFieldType != MineFieldType.SpeedBump)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestSpeedTrapLayer()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.MineLayer)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.MineFieldType == MineFieldType.SpeedBump)
                .OrderByDescending(t => t.Ranking);


            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestStargate()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Orbital)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.SafeRange > 0)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestMassDriver()
        {

            var techs = TechStore.GetTechsByCategory(TechCategory.Orbital)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.PacketSpeed > 0)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best mine layer this player has access to
        /// </summary>
        /// <returns></returns>
        public TechHullComponent GetBestColonizationModule()
        {
            var techs = TechStore.GetTechsByCategory(TechCategory.Mechanical)
                .Where(t => t is TechHullComponent hc && HasTech(hc) && hc.ColonizationModule)
                .OrderByDescending(t => t.Ranking);

            return techs.FirstOrDefault() as TechHullComponent;
        }

        /// <summary>
        /// Get the best tech by category
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="category"></param>
        /// <returns></returns>
        public T GetBestTech<T>(TechCategory category) where T : Tech
        {
            var techs = TechStore.GetTechsByCategory(category);
            var tech = techs
                .Where(t => HasTech(t))
                .OrderByDescending(t => t.Ranking).FirstOrDefault();
            return tech as T;
        }

        public ShipDesign GetDesign(string name)
        {
            return Designs.Find(d => d.Name == name);
        }

        public ShipDesign GetLatestDesign(ShipDesignPurpose purpose)
        {
            return Designs.Where(d => d.Purpose == purpose).OrderByDescending(d => d.Version).FirstOrDefault();
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
                FleetsByGuid.Remove(fleet.Guid);
            }
        }

        /// <summary>
        /// Go through each message and update the target to a value from our reports
        /// </summary>
        public void UpdateMessageTargets()
        {
            foreach (var message in Messages)
            {
                if (message.Target != null && MapObjectsByGuid.TryGetValue(message.Target.Guid, out var playerMapObject))
                {
                    message.Target = playerMapObject;
                }
                else
                {
                    // this could be because it's null, or because haven't scanned it, or the target was destroyed
                    message.Target = null;
                }
            }
        }

        #region Computed Properties

        /// <summary>
        /// Does this race discover a ShipDesign's components on scan?
        /// </summary>
        /// <value></value>
        public bool DiscoverDesignOnScan { get => Race.PRT == PRT.WM; }

        public Cost TerraformCost { get => Race.HasLRT(LRT.TT) ? Rules.TotalTerraformCost : Rules.TerraformCost; }

        /// <summary>
        /// PP races can fling packets 1 warp faster without decaying.
        /// </summary>
        public float GetPacketDecayRate(MineralPacket packet)
        {
            int overSafeWarp = packet.WarpFactor - packet.SafeWarpSpeed;

            if (Race.PRT == PRT.IT)
            {
                // IT is always count as being at least 1 over the safe warp
                overSafeWarp++;
            }

            // we only care about packets thrown up to 3 warp over the limit 
            overSafeWarp = Mathf.Clamp(packet.WarpFactor - packet.SafeWarpSpeed, 0, 3);

            var packetDecayRate = 0f;
            if (overSafeWarp > 0)
            {
                packetDecayRate = Rules.PacketDecayRate[overSafeWarp];
            }

            if (Race.PRT == PRT.PP)
            {
                // PP have half the decay rate
                packetDecayRate *= .5f;
            }

            return packetDecayRate;
        }

        /// <summary>
        /// The effectiveness of this player at receiving packets
        /// Races with the Interstellar trait are only 1/2 as effective at catching packets. To calculate the damage taken, divide receiverSpeed by two.
        /// </summary>
        /// <value></value>
        public float PacketReceiverFactor { get => Race.PRT == PRT.IT ? .5f : 1f; }

        /// <summary>
        /// Get the cost to construct a single or mixed mineral packet 
        /// </summary>
        public int PacketResourceCost
        {
            get
            {
                switch (Race.PRT)
                {
                    case PRT.PP:
                        return Rules.PacketResourceCostPP;
                    default:
                        return Rules.PacketResourceCost;
                }
            }
        }

        /// <summary>
        /// Get the premium this race pays for packets. PP races are perfectly efficient, but
        /// other races use minerals building the packet.
        /// </summary>
        public float PacketCostFactor
        {
            get
            {
                switch (Race.PRT)
                {
                    case PRT.PP:
                        return Rules.PacketMineralCostFactorPP;
                    case PRT.IT:
                        return Rules.PacketMineralCostFactorIT;
                    default:
                        return Rules.PacketMineralCostFactor;
                }
            }
        }

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int MineralsPerMixedMineralPacket
        {
            get
            {
                switch (Race.PRT)
                {
                    case PRT.PP:
                        return Rules.MineralsPerMixedMineralPacketPP;
                    default:
                        return Rules.MineralsPerMixedMineralPacket;
                }
            }
        }

        /// <summary>
        /// The number of minerals contained in each mixed mineral packet
        /// </summary>
        public int MineralsPerSingleMineralPacket
        {
            get
            {
                switch (Race.PRT)
                {
                    case PRT.PP:
                        return Rules.MineralsPerSingleMineralPacketPP;
                    default:
                        return Rules.MineralsPerSingleMineralPacket;
                }
            }
        }

        /// <summary>
        /// Does this player cloak ships with cargo without reducing the cloak percentage?
        /// </summary>
        public bool FreeCargoCloaking { get => Race.PRT == PRT.SS; }

        /// <summary>
        /// The player's built in cloaking percentage
        /// </summary>
        public int BuiltInCloaking { get => Race.PRT == PRT.SS ? Rules.BuiltInSSCloakUnits : 0; }

        /// <summary>
        /// Do this player's minefields act like scanners?
        /// </summary>
        /// <value></value>
        public bool MineFieldsAreScanners { get => Race.PRT == PRT.SD; }

        #endregion

        #region Equals Overload

        public override bool Equals(object obj) => this.Equals(obj as Player);

        public bool Equals(Player p)
        {
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (System.Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (Num == p.Num);
        }

        public override int GetHashCode() => (Num).GetHashCode();

        public static bool operator ==(Player lhs, Player rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Player lhs, Player rhs) => !(lhs == rhs);

        #endregion

    }
}
