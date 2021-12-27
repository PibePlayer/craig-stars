using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using static CraigStars.Utils.Utils;


namespace CraigStars
{
    /// <summary>
    /// For each player, discover new planets, fleets, minefields, packets, etc
    /// </summary>
    public class PlayerScanStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerScanStep));

        private readonly IRulesProvider rulesProvider;
        private readonly PlayerIntelDiscoverer playerIntelDiscoverer;
        private readonly PlayerTechService playerTechService;
        private readonly FleetSpecService fleetSpecService;

        Rules Rules { get => rulesProvider.Rules; }

        public PlayerScanStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlayerIntelDiscoverer playerIntel, PlayerTechService playerTechService, FleetSpecService fleetSpecService) : base(gameProvider, TurnGenerationState.PlayerScanStep)
        {
            this.rulesProvider = rulesProvider;
            this.playerIntelDiscoverer = playerIntel;
            this.playerTechService = playerTechService;
            this.fleetSpecService = fleetSpecService;
        }

        /// <summary>
        /// Helper class for sorting scanners
        /// </summary>
        public class Scanner
        {
            public Vector2 Position { get; set; }
            public Vector2 PreviousPosition { get; set; }
            public int Range { get; set; }
            public int RangePen { get; set; }

            /// <summary>
            /// the amount this scanner reduces enemy cloaking, as a percent
            /// i.e. if it reduces enemy cloaking by 81%, this would be .81
            /// </summary>
            /// <value></value>
            public float CloakReduction { get; set; }

            public Scanner(Vector2 position, int range = 0, int rangePen = 0, float cloadReduction = 0)
            {
                Position = position;
                PreviousPosition = position;
                Range = range;
                RangePen = rangePen;
                CloakReduction = cloadReduction;
            }

            public Scanner(Vector2 position, Vector2 previousPosition, int range = 0, int rangePen = 0, float cloadReduction = 0)
            {
                Position = position;
                PreviousPosition = previousPosition;
                Range = range;
                RangePen = rangePen;
                CloakReduction = cloadReduction;
            }

        }


        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            log.Debug("Before we scan, make sure all of our specs are up to date");
            Game.Fleets.ForEach(fleet => fleetSpecService.ComputeFleetSpec(Game.Players[fleet.PlayerNum], fleet));
            foreach (var planet in ownedPlanets.Where(p => p.HasStarbase))
            {
                fleetSpecService.ComputeFleetSpec(Game.Players[planet.PlayerNum], planet.Starbase);
            }
        }

        public override void Process()
        {
            var scanTasks = new List<Task>();
            foreach (var player in Game.Players)
            {
                // TODO: this is eventually slowing way down
                // I think I might be hitting some threadpool starvation issues
                // scanTasks.Add(Task.Run(() =>
                // {
                // try
                // {
                Scan(player);
                // }
                // catch (Exception e)
                // {
                //     log.Error($"Encountered error during PlayerScanStep for {player}.", e);
                // }
                // }));
            }

            // Task.WaitAll(scanTasks.ToArray());
        }

        internal void Scan(Player player)
        {
            // clear out old fleets
            // we rebuild this list each turn
            log.Debug($"{Game.Year}: {player} Clearing Transient Reports");
            playerIntelDiscoverer.ClearTransientReports(player);

            foreach (var planet in player.AllPlanets)
            {
                planet.OrbitingFleets.Clear();
                if (planet.ReportAge != MapObject.Unexplored)
                {
                    // increase the report age. We'll reset it to 0
                    // if we scan it
                    planet.ReportAge++;
                }
            }

            // build a list of scanners for this player
            var scanners = new List<Scanner>();

            log.Debug($"{Game.Year}: {player} Building List of Planet Scanners");
            foreach (var planet in Game.Planets.Where(p => p.PlayerNum == player.Num && p.Scanner))
            {
                // find the best scanner at this location, whether fleet or planet
                var scanner = new Scanner(planet.Position, (int)(planet.Spec.ScanRange), planet.Spec.ScanRangePen);
                foreach (var fleet in planet.OrbitingFleets.Where(f => f.PlayerNum == player.Num && f.Spec.Scanner))
                {
                    scanner.Range = Math.Max(scanner.Range, fleet.Spec.ScanRange);
                    scanner.RangePen = Math.Max(scanner.RangePen, fleet.Spec.ScanRangePen);
                    scanner.CloakReduction = Math.Max(scanner.CloakReduction, fleet.Spec.ReduceCloaking);
                }
                // square these ranges, because we are using
                // the faster DistanceSquaredTo method to compare distances
                scanner.Range *= scanner.Range;
                scanner.RangePen *= scanner.RangePen;
                scanners.Add(scanner);
            }

            // find all our fleets that are out and about
            log.Debug($"{Game.Year}: {player} Building List of Fleet Scanners");
            foreach (var fleet in Game.Fleets.Where(f => f.PlayerNum == player.Num && f.Spec.Scanner))
            {
                scanners.Add(new Scanner(fleet.Position, fleet.PreviousPosition.GetValueOrDefault(fleet.Position), fleet.Spec.ScanRange * fleet.Spec.ScanRange, fleet.Spec.ScanRangePen * fleet.Spec.ScanRangePen, fleet.Spec.ReduceCloaking));
            }

            // PP players have built in scanners on packets
            if (player.Race.Spec.PacketBuiltInScanner)
            {
                foreach (var packet in Game.MineralPackets.Where(p => p.OwnedBy(player)))
                {
                    // add packets as pen scanners
                    scanners.Add(new Scanner(packet.Position, 0, packet.WarpFactor * packet.WarpFactor * packet.WarpFactor * packet.WarpFactor));
                }
            }

            // Space demolition minefields act as scanners
            if (player.Race.Spec.MineFieldsAreScanners)
            {
                foreach (var mineField in Game.MineFields.Where(mf => mf.PlayerNum == player.Num))
                {
                    scanners.Add(new Scanner(mineField.Position, (int)mineField.Radius, 0));
                }
            }

            // discover our own designs
            log.Debug($"{Game.Year}: {player} Discovering player designs (from {Game.Designs.Count} total designs)");
            foreach (var design in Game.Designs.Where(d => d.PlayerNum == player.Num))
            {
                playerIntelDiscoverer.Discover(player, design, true);
            }

            // go through each planet and update its report if
            // we scanned it
            log.Debug($"{Game.Year}: {player} Scanning {Game.Planets.Count} planets.");
            foreach (var planet in Game.Planets)
            {
                // we own this planet, update the report
                if (planet.PlayerNum == player.Num)
                {
                    playerIntelDiscoverer.Discover(player, planet, true);
                    continue;
                }

                foreach (var scanner in scanners)
                {
                    if (Rules.FleetsScanWhileMoving && scanner.PreviousPosition != scanner.Position)
                    {
                        // this scanner moved, so make sure if we travelled past this planet it is still scanned
                        // we do this by checking the closest point the planet came to the line the fleet moved on
                        // if the distance from that point to the planet is less than that scan range, than it was in our
                        // scanners as we went by
                        var closestPointToScanCapsule = GetClosestPointToSegment2D(planet.Position, scanner.PreviousPosition, scanner.Position);
                        if (scanner.RangePen >= closestPointToScanCapsule.DistanceSquaredTo(planet.Position))
                        {
                            playerIntelDiscoverer.Discover(player, planet, true);
                            break;
                        }
                    }
                    else if (scanner.RangePen >= scanner.Position.DistanceSquaredTo(planet.Position))
                    {
                        playerIntelDiscoverer.Discover(player, planet, true);
                        break;
                    }
                }
            }

            ScanFleets(player, scanners);

            log.Debug($"{Game.Year}: {player} Scanning {Game.MineFields.Count} minefields.");
            foreach (var mineField in player.AllMineFields)
            {
                if (mineField.ReportAge != MapObject.Unexplored)
                {
                    // increase the report age. We'll reset it to 0
                    // if we scan it
                    mineField.ReportAge++;
                }
            }

            // salvage is only scanned by normal scanners
            foreach (var mineField in Game.MineFields)
            {
                if (mineField.PlayerNum == player.Num)
                {
                    // discover our own minefields
                    playerIntelDiscoverer.Discover(player, mineField, true);
                    continue;
                }

                // minefields are cloaked if we haven't discovered them before
                var cloakFactor = player.MineFieldsByGuid.ContainsKey(mineField.Guid) ? 1f : 1 - (Game.Rules.MineFieldCloak / 100f);

                foreach (var scanner in scanners)
                {
                    if (cloakFactor != 1)
                    {
                        // calculate cloak reduction for tachyon detectors if this minefield is cloaked
                        cloakFactor = 1 - (1 - cloakFactor) * scanner.CloakReduction;
                    }
                    var distance = scanner.Position.DistanceSquaredTo(mineField.Position);

                    // multiple the scanRange by the cloak factor, i.e. if we are 75% cloaked, our scanner range is effectively 25% of what it normally is
                    if (scanner.RangePen * (cloakFactor * scanner.CloakReduction) >= distance - (mineField.Radius * mineField.Radius))
                    {
                        // update the fleet report with pen scanners
                        playerIntelDiscoverer.Discover(player, mineField, true);
                        break;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range * (cloakFactor - scanner.CloakReduction) >= distance - (mineField.Radius * mineField.Radius))
                    {
                        playerIntelDiscoverer.Discover(player, mineField, false);
                        break;
                    }
                }
            }

            // salvage is only scanned by normal scanners
            foreach (var salvage in Game.Salvage)
            {
                foreach (var scanner in scanners)
                {
                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range >= scanner.Position.DistanceSquaredTo(salvage.Position))
                    {
                        playerIntelDiscoverer.Discover(player, salvage, false);
                        break;
                    }
                }
            }

            // salvage is only scanned by normal scanners
            foreach (var packet in Game.MineralPackets)
            {
                if (packet.PlayerNum == player.Num)
                {
                    playerIntelDiscoverer.Discover(player, packet, true);
                    continue;
                }

                // PP races detect all packets in flight
                if (player.Race.Spec.DetectAllPackets)
                {
                    playerIntelDiscoverer.Discover(player, packet, false);
                    continue;
                }

                foreach (var scanner in scanners)
                {
                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range >= scanner.Position.DistanceSquaredTo(packet.Position))
                    {
                        playerIntelDiscoverer.Discover(player, packet, false);
                        break;
                    }
                }
            }

            // increment the report age of wormholes
            foreach (var wormhole in player.Wormholes)
            {
                if (wormhole.ReportAge != MapObject.Unexplored)
                {
                    // increase the report age. We'll reset it to 0
                    // if we scan it
                    wormhole.ReportAge++;
                }
            }

            // wormholes
            foreach (var wormhole in Game.Wormholes)
            {
                // wormholes are cloaked if we haven't discovered them before
                var cloakFactor = player.WormholesByGuid.ContainsKey(wormhole.Guid) ? 1f : 1 - (Game.Rules.WormholeCloak / 100f);
                foreach (var scanner in scanners)
                {
                    if (cloakFactor != 1)
                    {
                        // calculate cloak reduction for tachyon detectors if this minefield is cloaked
                        cloakFactor = 1 - (1 - cloakFactor) * scanner.CloakReduction;
                    }

                    // we only care about regular scanners for wormholes
                    if (scanner.Range * cloakFactor >= scanner.Position.DistanceSquaredTo(wormhole.Position))
                    {
                        playerIntelDiscoverer.Discover(player, wormhole, false);
                        break;
                    }
                }
            }

            // mysteryTraders
            foreach (var mysterytrader in Game.MysteryTraders)
            {
                foreach (var scanner in scanners)
                {
                    // we only careabout regular scanners for mysteryTraders
                    if (scanner.Range >= scanner.Position.DistanceSquaredTo(mysterytrader.Position))
                    {
                        playerIntelDiscoverer.Discover(player, mysterytrader, false);
                        break;
                    }
                }
            }

            if (player.Race.Spec.CanDetectStargatePlanets)
            {
                DiscoverStargatesInRange(player);
            }

        }

        /// <summary>
        /// Scan all fleets in the game and discover them
        /// </summary>
        /// <param name="player"></param>
        /// <param name="scanners"></param>
        internal void ScanFleets(Player player, List<Scanner> scanners)
        {
            log.Debug($"{Game.Year}: {player} {scanners.Count} scanners scanning {Game.Fleets.Count} fleets.");
            List<Fleet> fleetsToDiscover = new List<Fleet>();
            foreach (var fleet in Game.Fleets)
            {
                // we own this fleet, update the report
                if (fleet.PlayerNum == player.Num)
                {
                    playerIntelDiscoverer.Discover(player, fleet, true);
                    continue;
                }

                // only scan this once. If we pen scan it, we break out of the loop
                // and go to the next fleet
                foreach (var scanner in scanners)
                {
                    var cloakFactor = 1 - (fleet.Spec.CloakPercent * scanner.CloakReduction / 100f);
                    var distance = scanner.Position.DistanceSquaredTo(fleet.Position);
                    // if we pen scanned this, update the report
                    if (scanner.RangePen * cloakFactor >= distance)
                    {
                        // update the fleet report with pen scanners
                        fleetsToDiscover.Add(fleet);
                        break;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (fleet.Orbiting == null && scanner.Range * cloakFactor >= distance)
                    {
                        fleetsToDiscover.Add(fleet);
                        break;
                    }
                }

            }
            log.Debug($"{Game.Year}: {player} Discovering {fleetsToDiscover.Count} foreign fleets.");
            fleetsToDiscover.ForEach(fleet => playerIntelDiscoverer.Discover(player, fleet, player.Race.Spec.DiscoverDesignOnScan));
        }

        /// <summary>
        /// We can detect other races with stargates, discover them.
        /// </summary>
        /// <param name="player"></param>
        void DiscoverStargatesInRange(Player player)
        {
            var playerStargatePlanets = OwnedPlanets.Where(p => p.OwnedBy(player) && p.HasStargate).ToList();
            foreach (var planet in OwnedPlanets.Where(p => !p.OwnedBy(player) && p.HasStargate))
            {
                foreach (var playerStargatePlanet in playerStargatePlanets)
                {
                    var range = playerStargatePlanet.Starbase.Spec.Stargate.SafeRange * playerStargatePlanet.Starbase.Spec.Stargate.SafeRange;
                    if (planet.Position.DistanceSquaredTo(playerStargatePlanet.Position) <= range)
                    {
                        playerIntelDiscoverer.Discover(player, planet, true);
                        // go on to the next planet
                        break;
                    }
                }
            }
        }


    }
}