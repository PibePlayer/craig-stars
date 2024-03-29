using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using CraigStars.Utils;
using Godot;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Player : PublicPlayerInfo
    {
        static CSLog log = LogProvider.GetLogger(typeof(Player));

        /// <summary>
        /// Each player starts with an API token that is used to identify it to the server
        /// </summary>
        /// <returns></returns>
        public string Token { get; set; } = System.Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        /// <summary>
        /// true if this player has made changes and needs to save
        /// </summary>
        /// <value></value>
        [JsonIgnore] public bool Dirty { get; set; }

        public Race Race { get; set; } = new();
        public int DefaultHullSet { get; set; } = 0;
        public PlayerStats Stats { get; set; } = new();
        public PlayerScore Score { get; set; } = new();
        public List<PlayerScore> ScoreHistory { get; set; } = new();

        #region Diplomacy

        public List<PlayerRelationship> PlayerRelations { get; set; } = new();
        public List<PlayerInfo> PlayerInfoIntel { get; set; } = new();

        #endregion

        #region Research

        /// <summary>
        /// Each player has a certain level of each tech.
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevels { get; set; } = new();

        /// <summary>
        /// The amount spent on each tech level
        /// </summary>
        /// <returns></returns>
        public TechLevel TechLevelsSpent { get; set; } = new();

        /// <summary>
        /// The percentage of resources to spend on research
        /// </summary>
        /// <value></value>
        [DefaultValue(15)]
        public int ResearchAmount { get; set; } = 15;
        public int ResearchSpentLastYear { get; set; } = 0;
        public TechField Researching { get; set; } = TechField.Energy;
        [DefaultValue(NextResearchField.LowestField)]
        public NextResearchField NextResearchField { get; set; } = NextResearchField.LowestField;
        [JsonIgnore] public int LeftoverResources { get; set; }

        #endregion

        #region Plans

        /// <summary>
        /// Each player has a list of battle plans. Each fleet has a battle plan assigned
        /// Each player automatically has the Default battle plan
        /// </summary>
        [JsonProperty(ItemIsReference = true)]
        public List<BattlePlan> BattlePlans { get; set; } = new() { new BattlePlan("Default") };
        public List<TransportPlan> TransportPlans { get; set; } = new();
        public List<ProductionPlan> ProductionPlans { get; set; } = new();

        #endregion

        #region Intel

        public Intel<ShipDesign> DesignIntel { get; set; } = new();
        public Intel<Planet> PlanetIntel { get; set; } = new();
        public Intel<MineField> MineFieldIntel { get; set; } = new();
        public Intel<MineralPacket> MineralPacketIntel { get; set; } = new();
        public Intel<Salvage> SalvageIntel { get; set; } = new();
        public Intel<Wormhole> WormholeIntel { get; set; } = new();
        public Intel<MysteryTrader> MysteryTraderIntel { get; set; } = new();
        public Intel<Fleet> FleetIntel { get; set; } = new();
        public List<BattleRecord> Battles { get; set; } = new();

        #endregion

        #region Designs

        [JsonIgnore] public List<ShipDesign> Designs { get => DesignIntel.Owned; }
        [JsonIgnore] public List<ShipDesign> ForeignDesigns { get => DesignIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<ShipDesign> AllDesigns { get => DesignIntel.All; }
        [JsonIgnore] public Dictionary<Guid, ShipDesign> DesignsByGuid { get => DesignIntel.ItemsByGuid; }

        #endregion

        #region Universe Data

        public PlayerUISettings UISettings { get; set; } = new();
        public PlayerSettings Settings { get; set; } = new();

        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new();

        [JsonIgnore] public List<Planet> Planets { get => PlanetIntel.Owned; }
        [JsonIgnore] public List<Planet> ForeignPlanets { get => PlanetIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Planet> AllPlanets { get => PlanetIntel.All; }
        [JsonIgnore] public Dictionary<Guid, Planet> PlanetsByGuid { get => PlanetIntel.ItemsByGuid; }

        [JsonIgnore] public List<Salvage> Salvage { get => SalvageIntel.Owned; }
        [JsonIgnore] public List<Salvage> ForeignSalvage { get => SalvageIntel.Foriegn; }
        [JsonIgnore] public IEnumerable<Salvage> AllSalvage { get => SalvageIntel.All; }
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

        [JsonIgnore]
        public IEnumerable<MapObject> AllMapObjects
        {
            get => AllPlanets
                .Cast<MapObject>()
                .Concat(AllSalvage)
                .Concat(AllMineFields)
                .Concat(AllMineralPackets)
                .Concat(AllFleets)
                .Concat(WormholeIntel.All)
                .Concat(MysteryTraderIntel.All);
        }


        /// <summary>
        /// All map objects by their guid, for lookups
        /// </summary>
        [JsonIgnore] public Dictionary<Guid, MapObject> MapObjectsByGuid { get; set; } = new();

        /// <summary>
        /// All battles by their guid, for lookups
        /// </summary>
        [JsonIgnore] public Dictionary<Guid, BattleRecord> BattlesByGuid { get; set; } = new();
        [JsonIgnore] public Dictionary<Guid, BattlePlan> BattlePlansByGuid { get; set; } = new();
        [JsonIgnore] public Dictionary<Guid, TransportPlan> TransportPlansByGuid { get; set; } = new();
        [JsonIgnore] public Dictionary<Guid, ProductionPlan> ProductionPlansByGuid { get; set; } = new();

        /// <summary>
        /// FleetCompositions
        /// </summary>
        [JsonIgnore] public Dictionary<Guid, FleetComposition> FleetCompositionsByGuid = new();
        [JsonIgnore] public Dictionary<FleetCompositionType, FleetComposition> FleetCompositionsByType = new();

        public List<Message> Messages { get; set; } = new();
        [JsonIgnore] public IEnumerable<Message> FilteredMessages { get => Messages.Where(m => !UISettings.MessageTypeFilter.Contains(m.Type)); }

        #region Turn Actions

        public List<FleetComposition> FleetCompositions { get; set; } = new List<FleetComposition>();
        public List<CargoTransferOrder> CargoTransferOrders { get; set; } = new List<CargoTransferOrder>();
        public List<MergeFleetOrder> MergeFleetOrders { get; set; } = new List<MergeFleetOrder>();
        public List<SplitAllFleetOrder> SplitFleetOrders { get; set; } = new List<SplitAllFleetOrder>();

        [JsonProperty(ItemIsReference = true)]
        public List<ImmediateFleetOrder> ImmediateFleetOrders { get; set; } = new();

        /// <summary>
        /// Get the orders for this player for turn submittal
        /// </summary>
        /// <returns></returns>
        public PlayerOrders GetOrders()
        {
            return new PlayerOrders()
            {
                PlayerNum = Num,
                Token = Token,
                UISettings = UISettings,
                Settings = Settings,
                PlayerRelations = new(PlayerRelations),
                Research = new PlayerResearchOrder()
                {
                    ResearchAmount = ResearchAmount,
                    Researching = Researching,
                    NextResearchField = NextResearchField,
                },

                FleetOrders = Fleets.Select(fleet => fleet.GetOrders()).ToList(),
                PlanetProductionOrders = Planets.Select(planet => planet.GetOrders()).ToList(),
                ShipDesigns = new(Designs),
                BattlePlans = new(BattlePlans),
                TransportPlans = new(TransportPlans),
                ProductionPlans = new(ProductionPlans),
                FleetCompositions = new(FleetCompositions),
                ImmedateFleetOrders = new(ImmediateFleetOrders),
            };
        }

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
        /// When a player is deserialized, we have to calculate all the computed values
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            SetupMapObjectMappings();

            foreach (var fleet in AllFleets)
            {
                // update what planet we are orbiting
                if (MapObjectsByLocation.TryGetValue(fleet.Position, out var mapObjectsAtLocation))
                {
                    var orbiting = mapObjectsAtLocation.FirstOrDefault(mo => mo is Planet) as Planet;
                    fleet.Orbiting = orbiting;
                }

                if (fleet.OwnedBy(Num))
                {
                    foreach (var waypoint in fleet.Waypoints)
                    {
                        if (waypoint.TargetGuid.HasValue && MapObjectsByGuid.TryGetValue(waypoint.TargetGuid.Value, out var target))
                        {
                            waypoint.Target = target;
                        }
                        if (waypoint.OriginalTargetGuid.HasValue && MapObjectsByGuid.TryGetValue(waypoint.OriginalTargetGuid.Value, out var origintalTarget))
                        {
                            waypoint.OriginalTarget = origintalTarget;
                        }
                    }
                }
            }

            foreach (var planet in Planets)
            {
                planet.PacketTarget = planet.PacketTargetGuid.HasValue ? MapObjectsByGuid[planet.PacketTargetGuid.Value] : null;
                planet.RouteTarget = planet.RouteTargetGuid.HasValue ? MapObjectsByGuid[planet.RouteTargetGuid.Value] : null;
                foreach (var item in planet.ProductionQueue.Items.Where(item => item.DesignGuid.HasValue))
                {
                    item.Design = DesignsByGuid[item.DesignGuid.Value];
                }
            }

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
            BattlesByGuid = Battles.ToGuidDictionary(battle => battle.Guid);

            SetupPlanMappings();

            List<MapObject> mapObjects = new List<MapObject>();
            mapObjects.AddRange(PlanetIntel.All);
            mapObjects.AddRange(FleetIntel.All);
            mapObjects.AddRange(MineFieldIntel.All);
            mapObjects.AddRange(MineralPacketIntel.All);
            mapObjects.AddRange(SalvageIntel.All);
            mapObjects.AddRange(WormholeIntel.All);
            mapObjects.AddRange(MysteryTraderIntel.All);
            MapObjectsByGuid = mapObjects.ToGuidDictionary(mo => mo.Guid);
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

        public void SetupPlanMappings()
        {
            BattlePlansByGuid = BattlePlans.ToGuidDictionary(_ => _.Guid);
            TransportPlansByGuid = TransportPlans.ToGuidDictionary(_ => _.Guid);
            ProductionPlansByGuid = ProductionPlans.ToGuidDictionary(_ => _.Guid);
            FleetCompositionsByGuid = FleetCompositions.ToGuidDictionary(_ => _.Guid);
            FleetCompositionsByType = FleetCompositions.ToSingleDictionary(fc => fc.Type);
        }

        #endregion

        /// <summary>
        /// Return true if this other player is our enemy
        /// </summary>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public bool IsEnemy(int playerNum)
        {
            // either they are explicit enemies or not friends
            return playerNum != Num && playerNum >= 0 && playerNum < PlayerRelations.Count
                && PlayerRelations[playerNum].Relation == PlayerRelation.Enemy || (!IsFriend(playerNum) && !IsNeutral(playerNum));
        }

        /// <summary>
        /// Are we neutral to this player
        /// </summary>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public bool IsNeutral(int playerNum)
        {
            return playerNum >= 0 && playerNum < PlayerRelations.Count
                && PlayerRelations[playerNum].Relation == PlayerRelation.Neutral;
        }

        /// <summary>
        /// Are we friends?
        /// </summary>
        /// <param name="playerNum"></param>
        /// <returns></returns>
        public bool IsFriend(int playerNum)
        {
            return Num == playerNum || playerNum >= 0 && playerNum < PlayerRelations.Count
                && PlayerRelations[playerNum].Relation == PlayerRelation.Friend;
        }

        /// <summary>
        /// Delete this design and also delete any tokens that use the design
        /// </summary>
        /// <param name="design"></param>
        public void DeleteDesign(ShipDesign design)
        {
            var designIndex = Designs.FindIndex(d => d == design);

            Designs[designIndex].Status = ShipDesign.DesignStatus.Deleted;

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
        public long GetNextFleetId()
        {
            return Fleets.Select(f => f.Id).Max() + 1;
        }

        public ShipDesign GetDesign(string name)
        {
            return Designs.Find(d => !d.Deleted && d.Name == name);
        }

        public ShipDesign GetLatestDesign(ShipDesignPurpose purpose)
        {
            return Designs
                .Where(d => !d.Deleted && d.Purpose == purpose)
                .OrderByDescending(d => d.Version)
                .FirstOrDefault();
        }
    }
}
