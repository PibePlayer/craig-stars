using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class TurnGenerator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TurnGenerator));

        public event Action<TurnGeneratorState> TurnGeneratorAdvancedEvent;

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


        Game Game { get; }
        PlanetProducer planetProducer = new PlanetProducer();
        List<Planet> ownedPlanets = new List<Planet>();

        public TurnGenerator(Game game)
        {
            Game = game;
        }


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
        public void GenerateTurn()
        {
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Scrapping);
            Game.Year++;

            log.Debug("Resetting players");
            // reset the players for a new turn
            Game.Players.ForEach(p =>
            {
                p.SubmittedTurn = false;
                p.Messages.Clear();
                p.ComputeAggregates();
            });

            Game.Fleets.ForEach(f =>
            {
                f.Tokens.ForEach(t => t.Design.ComputeAggregate(f.Player));
                f.ComputeAggregate();
            });

            ownedPlanets = Game.Planets.Where(p => p.Player != null).ToList();

            log.Debug("Processing player immmediate cargo transfers");
            ProcessCargoTransfers();

            // regular turn stuff
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Waypoint0);
            log.Debug("Processing fleet waypoints");
            ProcessWaypoints();
            log.Debug("Moving fleets");
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.MoveFleets);
            MoveFleets();
            log.Debug("Mining");
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Mining);
            Mine();
            log.Debug("Producing");
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Production);
            Produce();
            log.Debug("Growing Planets");
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Grow);
            Grow();

        }

        /// <summary>
        /// Make sure each player has up to date information about the world
        /// </summary>
        public void UpdatePlayerReports()
        {
            log.Debug("Computing fleet aggregates for new turn");
            Game.Fleets.ForEach(f => f.ComputeAggregate());
            Game.Planets.ForEach(p => p.Starbase?.ComputeAggregate());
            log.Debug("Scanning");
            Scan();
            log.Debug("Updating player reports");
            UpdatePlayers();
        }

        void ProcessCargoTransfers()
        {
            Game.Players.ForEach(player =>
            {
                player.CargoTransferOrders.ForEach(cargoTransfer =>
                {
                    if (Game.CargoHoldersByGuid.TryGetValue(cargoTransfer.Source.Guid, out var source) &&
                    Game.CargoHoldersByGuid.TryGetValue(cargoTransfer.Dest.Guid, out var dest))
                    {
                        // make sure our source can lose the cargo
                        var result = source.AttemptTransfer(cargoTransfer.Transfer);
                        if (result)
                        {
                            // make sure our dest can take the cargo
                            result = dest.AttemptTransfer(-cargoTransfer.Transfer);
                            if (!result)
                            {
                                // revert the source changes
                                source.Cargo -= cargoTransfer.Transfer;
                                log.Error($"Failed to transfer {cargoTransfer.Transfer} from {source.Name} to {dest.Name}. {dest.Name} rejected cargo.");
                            }
                        }
                        else
                        {
                            log.Error($"Failed to transfer {cargoTransfer.Transfer} from {source.Name} to {dest.Name}. {source.Name} rejected cargo.");
                        }
                    }
                });
            });
        }

        void ProcessWaypoints()
        {
            // process all WP0 tasks
            Game.Fleets.ForEach(f => f.ProcessTask());

            // remove any fleets that were scrapped
            Game.Fleets.RemoveAll(f => f.Scrapped);
        }

        // move fleets
        internal void MoveFleets()
        {
            foreach (var fleet in Game.Fleets)
            {
                fleet.Move();
            }

            // remove any fleets that were scrapped at the end
            Game.Fleets.RemoveAll(f => f.Scrapped);
        }

        void Mine()
        {
            ownedPlanets.ForEach(p =>
            {
                p.Cargo += p.MineralOutput;
                p.MineYears += p.Mines;
                int mineralDecayFactor = Game.Rules.MineralDecayFactor;
                int minMineralConcentration = p.Homeworld ? Game.Rules.MinHomeworldMineralConcentration : Game.Rules.MinMineralConcentration;
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
            int[] planetMineYears = planet.MineYears;
            int[] planetMineralConcentration = planet.MineralConcentration;
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

                    planetMineYears[i] = mineYears;
                    planetMineralConcentration[i] = conc;
                }
            }
            planet.MineYears = planetMineYears;
            planet.MineralConcentration = planetMineralConcentration;
        }

        void Produce()
        {
            ownedPlanets.ForEach(p =>
            {
                planetProducer.Build(p);
            });

            Research();
        }

        /// <summary>
        /// Apply research resources from each planet to research
        /// </summary>
        void Research()
        {
            var planetsByPlayer = ownedPlanets.GroupBy(p => p.Player);
            foreach (var playerPlanets in planetsByPlayer)
            {
                // figure out how many resoruces each planet has
                var resourcesToSpend = 0;
                foreach (var planet in playerPlanets)
                {
                    resourcesToSpend += playerPlanets.Sum(p => p.ResourcesPerYearResearch);
                }

                // research for this player
                playerPlanets.Key.ResearchNextLevel(resourcesToSpend);
            }
        }

        /// <summary>
        /// Grow populations on planets
        /// </summary>
        void Grow()
        {
            ownedPlanets.ForEach(p => p.Population += p.GrowthAmount);
        }

        /// <summary>
        /// For each player, discover new planets, fleets, minefields, packets, etc
        /// </summary>
        void Scan()
        {
            foreach (var player in Game.Players)
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

                // go through each planet and update its report if
                // we scanned it
                foreach (var planet in Game.Planets)
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

                foreach (var fleet in Game.Fleets)
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
        void UpdatePlayers()
        {
            Game.Players.ForEach(p =>
            {
                p.Year = Game.Year;
                p.PlanetaryScanner = p.GetBestPlanetaryScanner();
                p.Fleets.ForEach(f => f.ComputeAggregate());
                p.SetupMapObjectMappings();
                p.UpdateMessageTargets();
            });
        }

        /// <summary>
        /// Run through all the turn processors for each player
        /// </summary>
        public void RunTurnProcessors()
        {
            List<TurnProcessor> processors = new List<TurnProcessor>() {
                new ScoutTurnProcessor(),
                new ColonyTurnProcessor(),
                new PlanetProductionTurnProcessor()
            };
            Game.Players.ForEach(player =>
            {
                // TODO: make turn processors configurable
                if (player.AIControlled || true)
                {
                    processors.ForEach(processor => processor.Process(Game.Year, player));
                }
            });

            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Finished);
        }

        #region Event Publishers

        public void PublishTurnGeneratorAdvancedEvent(TurnGeneratorState state)
        {
            TurnGeneratorAdvancedEvent?.Invoke(state);
        }

        #endregion
    }
}