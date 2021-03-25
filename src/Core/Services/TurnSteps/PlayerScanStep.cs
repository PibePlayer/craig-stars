using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var player in Game.Players)
            {
                Scan(player);
            }
        }

        internal void Scan(Player player)
        {
            // clear out old fleets
            // we rebuild this list each turn
            playerIntel.ClearFleetReports(player);

            foreach (var planet in player.AllPlanets)
            {
                planet.OrbitingFleets.Clear();
                if (planet.ReportAge != Planet.Unexplored)
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

                foreach (var scanner in scanners)
                {
                    // if we pen scanned this, update the report
                    if (scanner.RangePen >= scanner.Position.DistanceSquaredTo(fleet.Position))
                    {
                        // update the fleet report with pen scanners
                        playerIntel.Discover(player, fleet, true);
                        continue;
                    }

                    // if we aren't orbiting a planet, we can be seen with regular scanners
                    if (fleet.Orbiting == null && scanner.Range >= scanner.Position.DistanceSquaredTo(fleet.Position))
                    {
                        playerIntel.Discover(player, fleet, false);
                    }
                }
            }
        }

    }
}