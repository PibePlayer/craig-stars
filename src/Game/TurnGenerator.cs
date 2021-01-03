using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;

namespace CraigStars
{
    public class TurnGenerator
    {
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

        PlanetProducer planetProducer = new PlanetProducer();

        /// <summary>
        /// Generate a turn
        /// 
        /// Stars! Order of Events
        /// <c>
        ///     Scrapping fleets (w/possible tech gain) 
        ///     Waypoint 0 unload tasks 
        ///     Waypoint 0 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 0 load tasks 
        ///     Other Waypoint 0 tasks * 
        ///     MT moves 
        ///     In-space packets move and decay 
        ///     PP packets (de)terraform 
        ///     Packets cause damage 
        ///     Wormhole entry points jiggle 
        ///     Fleets move (run out of fuel, hit minefields (fields reduce as they are hit), stargate, wormhole travel) 
        ///     Inner Strength colonists grow in fleets 
        ///     Mass Packets still in space and Salvage decay 
        ///     Wormhole exit points jiggle 
        ///     Wormhole endpoints degrade/jump 
        ///     SD Minefields detonate (possibly damaging again fleet that hit minefield during movement) 
        ///     Mining 
        ///     Production (incl. research, packet launch, fleet/starbase construction) 
        ///     SS Spy bonus obtained 
        ///     Population grows/dies 
        ///     Packets that just launched and reach their destination cause damage 
        ///     Random events (comet strikes, etc.) 
        ///     Fleet battles (w/possible tech gain) 
        ///     Meet MT 
        ///     Bombing 
        ///     Waypoint 1 unload tasks 
        ///     Waypoint 1 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 1 load tasks 
        ///     Mine Laying 
        ///     Fleet Transfer 
        ///     CA Instaforming 
        ///     Mine sweeping 
        ///     Starbase and fleet repair 
        ///     Remote Terraforming
        /// </c>
        /// </summary>
        /// <param name="game"></param>
        public void GenerateTurn(Game game, TechStore techStore)
        {
            game.Year++;
            game.Players.ForEach(p => p.Messages.Clear());

            var ownedPlanets = game.Planets.Where(p => p.Player != null).ToList();

            MoveFleets(game.Fleets);
            Mine(game.UniverseSettings, ownedPlanets);
            Produce(game.UniverseSettings, game.Planets);
            Grow(game.UniverseSettings, game.Planets);

        }

        /// <summary>
        /// Make sure each player has up to date information about the world
        /// </summary>
        /// <param name="game"></param>
        /// <param name="techStore"></param>
        public void UpdatePlayerReports(Game game, TechStore techStore)
        {
            game.Fleets.ForEach(f => f.ComputeAggregate());
            Scan(game.Year, techStore, game.Players, game.Planets, game.Fleets, game.MineralPackets, game.MineFields);
            UpdatePlayers(game.UniverseSettings, techStore, game.Players);
        }

        // move fleets
        internal void MoveFleets(List<Fleet> fleets)
        {
            foreach (var fleet in fleets)
            {
                fleet.Move();
                // TODO: Scrap fleets
            }

        }

        void Mine(UniverseSettings settings, List<Planet> planets)
        {
            planets.ForEach(p =>
            {
                p.Cargo.Add(p.GetMineralOutput());
                p.MineYears.Add(p.Mines);
                int mineralDecayFactor = settings.MineralDecayFactor;
                int minMineralConcentration = p.Homeworld ? settings.MinHomeworldMineralConcentration : settings.MinMineralConcentration;
                ReduceMineralConcentration(p, mineralDecayFactor, minMineralConcentration);
            });
        }

        /// <summary>
        /// Reduce the mineral concentrations of a planet after mining.
        /// </summary>
        /// <param name="planet">The planet to reduce mineral concentrations for</param>
        /// <param name="mineralDecayFactor">The factor of decay</param>
        /// <param name="minMineralConcentration"></param>
        void ReduceMineralConcentration(Planet planet, int mineralDecayFactor, int minMineralConcentration)
        {
            for (int i = 0; i < 3; i++)
            {
                int conc = planet.MineralConcentration[i];
                int minesPer = mineralDecayFactor / conc / conc;
                int mineYears = planet.MineYears[i];
                if (mineYears > minesPer)
                {
                    conc -= mineYears / minesPer;
                    if (conc < minMineralConcentration)
                    {
                        conc = minMineralConcentration;
                    }
                    mineYears %= minesPer;

                    planet.MineYears[i] = mineYears;
                    planet.MineralConcentration[i] = conc;
                }
            }
        }

        void Produce(UniverseSettings settings, List<Planet> planets)
        {
            var inhabitedPlanets = planets.Where(p => p.Player != null).ToList();
            inhabitedPlanets.ForEach(p =>
            {
                planetProducer.Build(settings, p);
            });

            Research(settings, inhabitedPlanets);
        }

        /// <summary>
        /// Apply research resources from each planet to research
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="inhabitedPlanets">A filtered list of inhabited planets</param>
        void Research(UniverseSettings settings, List<Planet> inhabitedPlanets)
        {
            var planetsByPlayer = inhabitedPlanets.GroupBy(p => p.Player);
            foreach (var playerPlanets in planetsByPlayer)
            {
                // figure out how many resoruces each planet has
                var resourcesToSpend = 0;
                foreach (var planet in playerPlanets)
                {
                    resourcesToSpend += playerPlanets.Sum(p => p.ResourcesPerYearResearch);
                }

                // research for this player
                playerPlanets.Key.ResearchNextLevel(settings, resourcesToSpend);
            }
        }

        /// <summary>
        /// Grow populations on planets
        /// </summary>
        void Grow(UniverseSettings settings, List<Planet> planets)
        {
            planets.ForEach(p => p.Population += p.GetGrowthAmount());
        }

        /// <summary>
        /// For each player, discover new planets, fleets, minefields, packets, etc
        /// </summary>
        /// <param name="players"></param>
        /// <param name="planets"></param>
        /// <param name="fleets"></param>
        /// <param name="mineralPackets"></param>
        /// <param name="mineFields"></param>
        void Scan(int year, TechStore techStore, List<Player> players, List<Planet> planets, List<Fleet> fleets, List<MineralPacket> mineralPackets, List<MineField> mineFields)
        {
            foreach (var player in players)
            {
                // clear out old fleets
                // we rebuild this list each turn
                player.Fleets.Clear();
                player.Planets.ForEach(p => p.OrbitingFleets.Clear());

                // increase the report age. We'll reset it to 0
                // if we scan it
                player.Planets.Where(p => p.ReportAge != Planet.Unexplored).Select(p => p.ReportAge++);

                // build a list of scanners for this player
                var scanners = new List<Scanner>();
                var planetaryScanner = player.GetBestPlanetaryScanner(techStore);

                foreach (var planet in planets.Where(p => p.Player == player && p.Scanner))
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
                foreach (var fleet in fleets.Where(f => f.Player == player && f.Aggregate.Scanner && (f.Orbiting == null || f.Orbiting.Player != player)))
                {
                    scanners.Add(new Scanner(fleet.Position, fleet.Aggregate.ScanRange, fleet.Aggregate.ScanRangePen));
                }

                // go through each planet and update its report if
                // we scanned it
                foreach (var planet in planets)
                {
                    // we own this planet, update the report
                    if (planet.Player == player)
                    {
                        player.UpdateReport(planet);
                        player.UpdatePlayerPlanet(planet);
                        continue;
                    }

                    foreach (var scanner in scanners)
                    {
                        if (scanner.RangePen >= scanner.Position.DistanceSquaredTo(planet.Position))
                        {
                            player.UpdateReport(planet);
                        }
                    }
                }

                foreach (var fleet in fleets)
                {
                    // we own this fleet, update the report
                    if (fleet.Player == player)
                    {
                        player.AddFleetReport(fleet, true);
                        continue;
                    }

                    foreach (var scanner in scanners)
                    {
                        // if we pen scanned this, update the report
                        if (scanner.RangePen >= scanner.Position.DistanceSquaredTo(fleet.Position))
                        {
                            // update the fleet report with pen scanners
                            player.AddFleetReport(fleet, true);
                            continue;
                        }

                        // if we aren't orbiting a planet, we can be seen with regular scanners
                        if (fleet.Orbiting == null && scanner.Range >= scanner.Position.DistanceSquaredTo(fleet.Position))
                        {
                            player.AddFleetReport(fleet, false);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// After a turn is generated, update some data on each player (like their current best planetary scanner)
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="techStore"></param>
        /// <param name="players"></param>
        void UpdatePlayers(UniverseSettings settings, TechStore techStore, List<Player> players)
        {
            players.ForEach(p =>
            {
                p.PlanetaryScanner = p.GetBestPlanetaryScanner(techStore);
                p.Fleets.ForEach(f => f.ComputeAggregate());
            });
        }
    }
}