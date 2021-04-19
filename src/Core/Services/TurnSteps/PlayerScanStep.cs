using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using Godot;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// For each player, discover new planets, fleets, minefields, packets, etc
    /// </summary>
    public class PlayerScanStep : Step
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlayerScanStep));
        public PlayerScanStep(Game game, TurnGeneratorState state) : base(game, state) { }

        /// <summary>
        /// Helper class for sorting scanners
        /// </summary>
        public class Scanner
        {
            public Vector2 Position { get; set; }
            public int Range { get; set; }
            public int RangePen { get; set; }
            public Scanner(Vector2 position, int range = 0, int rangePen = 0)
            {
                Position = position;
                Range = range;
                RangePen = rangePen;
            }
        }

        PlayerIntel playerIntel = new PlayerIntel();

        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            log.Debug("Before we scan, make sure all of our aggregates are up to date");
            Game.Fleets.ForEach(f => f.ComputeAggregate());
            Game.Planets.ForEach(p => p.Starbase?.ComputeAggregate());
        }

        public override void Process()
        {
            var scanTasks = new List<Task>();
            foreach (var player in Game.Players)
            {
                // TODO: this is crashing when generating a bunch of turns...
                // scanTasks.Add(Task.Factory.StartNew(() =>
                // {
                try
                {
                    Scan(player);
                }
                catch (Exception e)
                {
                    log.Error($"Encountered error during PlayerScanStep for {player}.", e);
                }
                // }));
            }

            // Task.WaitAll(scanTasks.ToArray());
        }

        internal void Scan(Player player)
        {
            // clear out old fleets
            // we rebuild this list each turn
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

            foreach (var planet in Game.Planets.Where(p => p.Player == player && p.Scanner))
            {
                // find the best scanner at this location, whether fleet or planet
                var scanner = new Scanner(planet.Position, planetaryScanner.ScanRange, planetaryScanner.ScanRangePen);
                foreach (var fleet in planet.OrbitingFleets.Where(f => f.Player == player && f.Aggregate.Scanner))
                {
                    scanner.Range = Math.Max(scanner.Range, fleet.Aggregate.ScanRange);
                    scanner.RangePen = Math.Max(scanner.RangePen, fleet.Aggregate.ScanRangePen);
                }
                // square these ranges, because we are using
                // the faster DistanceSquaredTo method to compare distances
                scanner.Range *= scanner.Range;
                scanner.RangePen *= scanner.RangePen;
                scanners.Add(scanner);
            }

            // find all our fleets that are out and about
            foreach (var fleet in Game.Fleets.Where(f => f.Player == player && f.Aggregate.Scanner && (f.Orbiting == null || f.Orbiting.Player != player)))
            {
                scanners.Add(new Scanner(fleet.Position, fleet.Aggregate.ScanRange * fleet.Aggregate.ScanRange, fleet.Aggregate.ScanRangePen * fleet.Aggregate.ScanRangePen));
            }

            // Space demolition minefields act as scanners
            if (player.Race.PRT == PRT.SD)
            {
                foreach (var mineField in Game.MineFields.Where(mf => mf.Player == player))
                {
                    scanners.Add(new Scanner(mineField.Position, mineField.Radius, 0));
                }
            }

            // discover our own designs
            foreach (var design in Game.Designs.Where(d => d.Player == player))
            {
                playerIntel.Discover(player, design, true);
            }

            // go through each planet and update its report if
            // we scanned it
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
                bool scanned = false;
                foreach (var scanner in scanners)
                {
                    var distance = scanner.Position.DistanceSquaredTo(fleet.Position);
                    // if we pen scanned this, update the report
                    if (!scanned && scanner.RangePen >= distance)
                    {
                        // update the fleet report with pen scanners
                        playerIntel.Discover(player, fleet, true);
                        scanned = true;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (fleet.Orbiting == null && scanner.Range >= distance)
                    {
                        playerIntel.Discover(player, fleet, false);
                        break;
                    }
                }
            }

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
                var cloakFactor = player.MineFieldsByGuid.ContainsKey(mineField.Guid) ? 1f : 1 - Game.Rules.MineFieldCloak;

                foreach (var scanner in scanners)
                {
                    var distance = scanner.Position.DistanceSquaredTo(mineField.Position);

                    // multiple the scanRange by the cloak factor, i.e. if we are 75% cloaked, our scanner range is effectively 25% of what it normally is
                    if (scanner.RangePen * cloakFactor >= distance - (mineField.Radius * mineField.Radius))
                    {
                        // update the fleet report with pen scanners
                        playerIntel.Discover(player, mineField, true);
                        break;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (scanner.Range * cloakFactor >= distance - (mineField.Radius * mineField.Radius))
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
                var cloakFactor = player.WormholesByGuid.ContainsKey(wormhole.Guid) ? 1f : 1 - Game.Rules.WormholeCloak;
                foreach (var scanner in scanners)
                {
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

    }
}