using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering planets
    /// </summary>
    public class PlanetDiscoverer : Discoverer<Planet>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlanetDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<Planet> GetOwnedItemReports(Player player) => player.Planets;
        protected override List<Planet> GetForeignItemReports(Player player) => player.ForeignPlanets;
        protected override Dictionary<Guid, Planet> GetItemsByGuid(Player player) => player.PlanetsByGuid;

        protected override Planet CreateEmptyReport(Planet item) => new Planet()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            ReportAge = MapObject.Unexplored,
        };

        protected override void DiscoverForeign(Player player, Planet item, Planet itemReport, bool penScanned)
        {
            if (penScanned)
            {
                var reportAge = itemReport.ReportAge;
                // our scanned population is ± 20%
                var randomPopulationError = player.Rules.Random.NextDouble() * (player.Rules.PopulationScannerError - (-player.Rules.PopulationScannerError)) - player.Rules.PopulationScannerError;

                itemReport.Cargo = Cargo.Empty;
                itemReport.Population = (int)(item.Population * (1 - randomPopulationError));
                itemReport.MineralConcentration = item.MineralConcentration;
                itemReport.Hab = item.Hab;
                itemReport.ReportAge = 0;
                itemReport.Owner = item.Owner;

                // discover this starbase
                if (item.Starbase != null)
                {
                    itemReport.Starbase = new Starbase()
                    {
                        Position = item.Starbase.Position,
                        Guid = item.Starbase.Guid,
                        Id = item.Starbase.Id,
                        Name = item.Starbase.Name,
                        RaceName = item.Starbase.Player.Race.Name,
                        RacePluralName = item.Starbase.Player.Race.PluralName,
                        Owner = item.Starbase.Owner,
                    };

                    foreach (var token in item.Starbase.Tokens)
                    {
                        if (!player.DesignsByGuid.ContainsKey(token.Design.Guid))
                        {
                            designDiscoverer.Discover(player, token.Design, penScanned);
                        }
                        itemReport.Starbase.Tokens.Add(new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity));
                    }

                }

                if (reportAge == MapObject.Unexplored)
                {
                    Message.PlanetDiscovered(player, itemReport);
                }
            }
        }

        protected override void DiscoverOwn(Player player, Planet item, Planet itemReport)
        {
            if (itemReport.Player != item.Player)
            {
                // this planet wasn't owned by us last turn, switch which list it belongs to
                player.ForeignPlanets.Remove(itemReport);
                player.Planets.Add(itemReport);
            }

            itemReport.Player = item.Player;
            itemReport.Cargo = item.Cargo;
            itemReport.Population = item.Population;
            itemReport.MineralConcentration = item.MineralConcentration;
            itemReport.Hab = item.Hab;
            itemReport.ReportAge = 0;
            itemReport.Owner = item.Owner;
            itemReport.MineYears = item.MineYears;
            itemReport.Mines = item.Mines;
            itemReport.Factories = item.Factories;
            itemReport.Defenses = item.Defenses;
            itemReport.Scanner = item.Scanner;
            itemReport.Homeworld = item.Homeworld;
            itemReport.ContributesOnlyLeftoverToResearch = item.ContributesOnlyLeftoverToResearch;

            if (itemReport.ProductionQueue == null)
            {
                itemReport.ProductionQueue = new ProductionQueue();
            }
            itemReport.ProductionQueue.Allocated = item.ProductionQueue.Allocated;
            itemReport.ProductionQueue.LeftoverResources = item.ProductionQueue.LeftoverResources;
            itemReport.ProductionQueue.Items.Clear();
            itemReport.ProductionQueue.Items.AddRange(item.ProductionQueue.Items.Select(planet =>
            {
                if (planet.Design != null)
                {
                    if (player.DesignsByGuid.TryGetValue(planet.Design.Guid, out var playerDesign))
                    {
                        // use the Game design, not the player one
                        planet.Design = playerDesign;
                    }
                    else
                    {
                        log.Error($"Player ProductionQueueItem has unknown design: {player} - {playerDesign.Name}");
                    }
                }
                return planet;
            }));

            if (item.HasStarbase)
            {
                itemReport.Starbase = new Starbase()
                {
                    Guid = item.Starbase.Guid,
                    Position = item.Starbase.Position,
                    Name = item.Starbase.Name,
                    RaceName = item.Starbase.Player.Race.Name,
                    RacePluralName = item.Starbase.Player.Race.PluralName,
                    Player = player,
                    BattlePlan = player.BattlePlansByGuid[item.Starbase.BattlePlan.Guid]
                };
                itemReport.Starbase.Tokens.AddRange(item.Starbase.Tokens);
                itemReport.Starbase.ComputeAggregate();
            }
        }
    }
}