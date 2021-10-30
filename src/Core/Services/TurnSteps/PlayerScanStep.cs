using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// For each player, discover new planets, fleets, minefields, packets, etc
    /// </summary>
    public class PlayerScanStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerScanStep));
        public PlayerScanStep(Game game) : base(game, TurnGenerationState.Scan) { }

        /// <summary>
        /// Helper class for sorting scanners
        /// </summary>
        public class Scanner
        {
            public Vector2 Position { get; set; }
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
                Range = range;
                RangePen = rangePen;
                CloakReduction = cloadReduction;
            }
        }

        PlayerIntel playerIntel = new PlayerIntel();

        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            log.Debug("Before we scan, make sure all of our aggregates are up to date");
            Game.Fleets.ForEach(f => f.ComputeAggregate());
            foreach (var planet in ownedPlanets)
            {
                planet.Starbase?.ComputeAggregate();
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
            playerIntel.ClearTransientReports(player);

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
            var planetaryScanner = player.GetBestPlanetaryScanner();

            log.Debug($"{Game.Year}: {player} Building List of Planet Scanners");
            foreach (var planet in Game.Planets.Where(p => p.Player == player && p.Scanner))
            {
                // find the best scanner at this location, whether fleet or planet
                var scanner = new Scanner(planet.Position, planetaryScanner.ScanRange, planetaryScanner.ScanRangePen);
                foreach (var fleet in planet.OrbitingFleets.Where(f => f.Player == player && f.Aggregate.Scanner))
                {
                    scanner.Range = Math.Max(scanner.Range, fleet.Aggregate.ScanRange);
                    scanner.RangePen = Math.Max(scanner.RangePen, fleet.Aggregate.ScanRangePen);
                    scanner.CloakReduction = Math.Max(scanner.CloakReduction, fleet.Aggregate.ReduceCloaking);
                }
                // square these ranges, because we are using
                // the faster DistanceSquaredTo method to compare distances
                scanner.Range *= scanner.Range;
                scanner.RangePen *= scanner.RangePen;
                scanners.Add(scanner);
            }

            // find all our fleets that are out and about
            log.Debug($"{Game.Year}: {player} Building List of Fleet Scanners");
            foreach (var fleet in Game.Fleets.Where(f => f.Player == player && f.Aggregate.Scanner && (f.Orbiting == null || f.Orbiting.Player != player)))
            {
                scanners.Add(new Scanner(fleet.Position, fleet.Aggregate.ScanRange * fleet.Aggregate.ScanRange, fleet.Aggregate.ScanRangePen * fleet.Aggregate.ScanRangePen, fleet.Aggregate.ReduceCloaking));
            }

            // Space demolition minefields act as scanners
            if (player.MineFieldsAreScanners)
            {
                foreach (var mineField in Game.MineFields.Where(mf => mf.Player == player))
                {
                    scanners.Add(new Scanner(mineField.Position, (int)mineField.Radius, 0));
                }
            }

            // discover our own designs
            log.Debug($"{Game.Year}: {player} Discovering player designs (from {Game.Designs.Count} total designs)");
            foreach (var design in Game.Designs.Where(d => d.Player == player))
            {
                playerIntel.Discover(player, design, true);
            }

            // go through each planet and update its report if
            // we scanned it
            log.Debug($"{Game.Year}: {player} Scanning {Game.Planets.Count} planets.");
            foreach (var planet in Game.Planets)
            {
                // we own this planet, update the report
                if (planet.Player == player)
                {
                    playerIntel.Discover(player, planet, true);
                    continue;
                }

                foreach (var scanner in scanners)
                {
                    if (scanner.RangePen >= scanner.Position.DistanceSquaredTo(planet.Position))
                    {
                        playerIntel.Discover(player, planet, true);
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
                if (mineField.Player == player)
                {
                    // discover our own minefields
                    playerIntel.Discover(player, mineField, true);
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
                        playerIntel.Discover(player, mineField, true);
                        break;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range * (cloakFactor - scanner.CloakReduction) >= distance - (mineField.Radius * mineField.Radius))
                    {
                        playerIntel.Discover(player, mineField, false);
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
                        playerIntel.Discover(player, salvage, false);
                        break;
                    }
                }
            }

            // salvage is only scanned by normal scanners
            foreach (var packet in Game.MineralPackets)
            {
                if (packet.Player == player)
                {
                    playerIntel.Discover(player, packet, true);
                    continue;
                }

                foreach (var scanner in scanners)
                {
                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range >= scanner.Position.DistanceSquaredTo(packet.Position))
                    {
                        playerIntel.Discover(player, packet, false);
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
                        playerIntel.Discover(player, wormhole, false);
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
                        playerIntel.Discover(player, mysterytrader, false);
                        break;
                    }
                }
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
                if (fleet.Player == player)
                {
                    playerIntel.Discover(player, fleet, true);
                    continue;
                }

                // only scan this once. If we pen scan it, we break out of the loop
                // and go to the next fleet
                foreach (var scanner in scanners)
                {
                    var cloakFactor = 1 - (fleet.Aggregate.CloakPercent * scanner.CloakReduction / 100f);
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
            fleetsToDiscover.ForEach(fleet => playerIntel.Discover(player, fleet, player.DiscoverDesignOnScan));
        }

    }
}