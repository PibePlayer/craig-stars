using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CraigStars.UniverseGeneration;
using Godot;
using Newtonsoft.Json;
using SimpleInjector;

namespace CraigStars
{
    /// <summary>
    /// The Game manages generating a universe, submitting turns, and storing the source of
    /// truth for all planets, fleets, designs, etc
    /// </summary>
    public class Game : IRulesProvider
    {
        static CSLog log = LogProvider.GetLogger(typeof(Game));

        #region PublicGameInfo

        /// <summary>
        /// The game gives each player a copy of the PublicGameInfo. The properties
        /// in the game that match up with PublicGameInfo properties are just forwarded
        /// </summary>
        [JsonIgnore] public PublicGameInfo GameInfo { get; set; } = new PublicGameInfo();
        public Guid Guid { get => GameInfo.Guid; set => GameInfo.Guid = value; }
        public string Name { get => GameInfo.Name; set => GameInfo.Name = value; }
        public Rules Rules { get => GameInfo.Rules; private set => GameInfo.Rules = value; }
        public VictoryConditions VictoryConditions { get => GameInfo.VictoryConditions; set => GameInfo.VictoryConditions = value; }
        public bool VictorDeclared { get => GameInfo.VictorDeclared; set => GameInfo.VictorDeclared = value; }
        public int Year { get => GameInfo.Year; set => GameInfo.Year = value; }
        public GameMode Mode { get => GameInfo.Mode; set => GameInfo.Mode = value; }
        public GameState Lifecycle { get => GameInfo.State; set => GameInfo.State = value; }
        public GameStartMode StartMode { get => GameInfo.StartMode; set => GameInfo.StartMode = value; }
        public Size Size { get => GameInfo.Size; set => GameInfo.Size = value; }
        public Density Density { get => GameInfo.Density; set => GameInfo.Density = value; }
        [JsonIgnore] public int YearsPassed { get => GameInfo.YearsPassed; }
        public Boolean AllPlayersSubmitted() => GameInfo.AllPlayersSubmitted();

        #endregion

        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))]
        public List<Player> Players { get; set; } = new List<Player>();

        /// <summary>
        /// This is an array of player orders. If null, it means the player
        /// hasn't submitted the order yet
        /// </summary>
        /// <typeparam name="ShipDesign"></typeparam>
        /// <returns></returns>
        [JsonIgnore] public PlayerOrders[] PlayerOrders { get; set; }

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
        [JsonIgnore] public IEnumerable<Planet> OwnedPlanets { get => Planets.Where(p => p.Owned); }
        [JsonIgnore] public Dictionary<Vector2, List<MapObject>> MapObjectsByLocation = new Dictionary<Vector2, List<MapObject>>();

        #endregion

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            GameInfo.Players.Clear();
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // init this as an empty array
            PlayerOrders = new PlayerOrders[Players.Count];

            // Update the Game dictionaries used for lookups, like PlanetsByGuid, FleetsByGuid, etc.
            UpdateInternalDictionaries();

            foreach (var fleet in Fleets)
            {
                // update what planet we are orbiting
                if (MapObjectsByLocation.TryGetValue(fleet.Position, out var mapObjectsAtLocation))
                {
                    var orbiting = mapObjectsAtLocation.FirstOrDefault(mo => mo is Planet) as Planet;
                    fleet.Orbiting = orbiting;
                }
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

            foreach (var planet in OwnedPlanets)
            {
                planet.PacketTarget = planet.PacketTargetGuid.HasValue ? MapObjectsByGuid[planet.PacketTargetGuid.Value] : null;
                planet.RouteTarget = planet.RouteTargetGuid.HasValue ? MapObjectsByGuid[planet.RouteTargetGuid.Value] : null;
                foreach (var item in planet.ProductionQueue.Items.Where(item => item.DesignGuid.HasValue))
                {
                    item.Design = DesignsByGuid[item.DesignGuid.Value];
                }
            }

            foreach (var order in PlayerOrders.Where(order => order != null))
            {
                var player = Players[order.PlayerNum];
                foreach (var planetOrder in order.PlanetProductionOrders)
                {
                    foreach (var item in planetOrder.Items.Where(item => item.DesignGuid.HasValue))
                    {
                        item.Design = player.DesignsByGuid[item.DesignGuid.Value];
                    }
                }
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

        public void Init(List<Player> players, Rules rules)
        {
            Players.Clear();
            Players.AddRange(players);
            GameInfo.Players.Clear();
            GameInfo.Players.AddRange(Players.Cast<PublicPlayerInfo>());

            // init this as an empty array
            PlayerOrders = new PlayerOrders[Players.Count];

            Rules = rules;
        }

        /// <summary>
        /// To reference objects between player knowledge and the server's data, we build Dictionaries by Guid
        /// </summary>
        internal void UpdateInternalDictionaries()
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

            UpdateMapObjectsByLocation();
        }

        void AddMapObject<T>(T mapObject, List<T> items, Dictionary<Guid, T> itemsByGuid) where T : MapObject
        {
            items.Add(mapObject);
            itemsByGuid[mapObject.Guid] = mapObject;
            MapObjectsByGuid[mapObject.Guid] = mapObject;

            if (!MapObjectsByLocation.TryGetValue(mapObject.Position, out var mapObjectsAtLocation))
            {
                mapObjectsAtLocation = new List<MapObject>();
                MapObjectsByLocation[mapObject.Position] = mapObjectsAtLocation;
            }
            mapObjectsAtLocation.Add(mapObject);

            if (mapObject is ICargoHolder cargoHolder)
            {
                CargoHoldersByGuid[cargoHolder.Guid] = cargoHolder;
            }

            log.Debug($"Created new {typeof(T)} {mapObject.Name} - {mapObject.Guid}");
        }

        public void AddMapObject(MapObject mapObject)
        {
            if (mapObject is Planet planet)
            {
                AddMapObject(planet, Planets, PlanetsByGuid);
            }
            else if (mapObject is Fleet fleet)
            {
                AddMapObject(fleet, Fleets, FleetsByGuid);
            }
            else if (mapObject is MineField mineField)
            {
                AddMapObject(mineField, MineFields, MineFieldsByGuid);
            }
            else if (mapObject is MineralPacket mineralPacket)
            {
                AddMapObject(mineralPacket, MineralPackets, MineralPacketsByGuid);
            }
            else if (mapObject is Salvage salvage)
            {
                AddMapObject(salvage, Salvage, SalvageByGuid);
            }
            else if (mapObject is Wormhole wormhole)
            {
                AddMapObject(wormhole, Wormholes, WormholesByGuid);
            }
            else if (mapObject is MysteryTrader mysteryTrader)
            {
                AddMapObject(mysteryTrader, MysteryTraders, MysteryTradersByGuid);
            }
        }

        public void MoveMapObject(MapObject mapObject, Vector2 originalPosition, Vector2 newPosition)
        {
            if (MapObjectsByLocation.TryGetValue(originalPosition, out var mapObjectsAtOriginalLocation))
            {
                mapObjectsAtOriginalLocation.Remove(mapObject);
            }
            else
            {
                log.Warn($"Tried to move {mapObject} from {originalPosition} to {newPosition}, but it wasn't found at {originalPosition}");
            }

            if (!MapObjectsByLocation.TryGetValue(newPosition, out var mapObjectsAtLocation))
            {
                mapObjectsAtLocation = new List<MapObject>();
                MapObjectsByLocation[newPosition] = mapObjectsAtLocation;
            }
            mapObjectsAtLocation.Add(mapObject);
            mapObject.Position = newPosition;
        }

        void RemoveMapObject<T>(T mapObject, List<T> items, Dictionary<Guid, T> itemsByGuid) where T : MapObject
        {
            items.Remove(mapObject);
            itemsByGuid.Remove(mapObject.Guid);
            MapObjectsByGuid.Remove(mapObject.Guid);
            MapObjectsByLocation[mapObject.Position].Remove(mapObject);

            if (mapObject is ICargoHolder cargoHolder)
            {
                CargoHoldersByGuid.Remove(cargoHolder.Guid);
            }

            log.Debug($"Deleted {typeof(T)} {mapObject.Name} - {mapObject.Guid}");
        }

        public void RemoveMapObject(MapObject mapObject)
        {

            if (mapObject is Fleet fleet)
            {
                RemoveMapObject(fleet, Fleets, FleetsByGuid);
            }
            else if (mapObject is MineField mineField)
            {
                RemoveMapObject(mineField, MineFields, MineFieldsByGuid);
            }
            else if (mapObject is Salvage salvage)
            {
                RemoveMapObject(salvage, Salvage, SalvageByGuid);
            }
            else if (mapObject is MineralPacket packet)
            {
                RemoveMapObject(packet, MineralPackets, MineralPacketsByGuid);
            }
            else if (mapObject is Wormhole wormhole)
            {
                RemoveMapObject(wormhole, Wormholes, WormholesByGuid);
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
                RemoveMapObject(mysteryTrader, MysteryTraders, MysteryTradersByGuid);
            }
        }

    }
}
