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
        static CSLog log = LogProvider.GetLogger(typeof(PlanetDiscoverer));
        private readonly PlanetService planetService;
        private readonly ShipDesignDiscoverer designDiscoverer;
        private readonly FleetSpecService fleetSpecService;
        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public PlanetDiscoverer(PlanetService planetService, ShipDesignDiscoverer designDiscoverer, FleetSpecService fleetSpecService, IRulesProvider rulesProvider)
        {
            this.planetService = planetService;
            this.designDiscoverer = designDiscoverer;
            this.fleetSpecService = fleetSpecService;
            this.rulesProvider = rulesProvider;
        }

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

        /// <summary>
        /// This special PlanetDiscoverer function is for discovering surface minerals through remote mining
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <param name="itemReport"></param>
        public void DiscoverCargo(Player player, Planet item)
        {
            if (player.PlanetsByGuid.TryGetValue(item.Guid, out var itemReport))
            {
                itemReport.CargoDiscovered = true;
                itemReport.Cargo = new Cargo(
                    item.Cargo.Ironium,
                    item.Cargo.Boranium,
                    item.Cargo.Germanium,
                    itemReport.Cargo.Colonists // we can't discover colonists this way, just use whatever we already know
                );
            }
        }

        protected override void DiscoverForeign(Player player, Planet item, Planet itemReport, bool penScanned)
        {
            if (penScanned)
            {
                var reportAge = itemReport.ReportAge;
                // our scanned population is ± 20%
                var randomPopulationError = Rules.Random.NextDouble() * (Rules.PopulationScannerError - (-Rules.PopulationScannerError)) - Rules.PopulationScannerError;

                // if we remote mine a planet, we discover its surface minerals, otherwise we don't know
                // this is done in a separate discover step
                if (!itemReport.CargoDiscovered)
                {
                    itemReport.Cargo = Cargo.Empty;
                }
                itemReport.Population = (int)(item.Population * (1 - randomPopulationError));
                itemReport.MineralConcentration = item.MineralConcentration;
                itemReport.Hab = item.Hab;
                itemReport.TerraformedAmount = item.TerraformedAmount;
                itemReport.BaseHab = item.BaseHab;
                itemReport.TerraformedAmount = item.TerraformedAmount;
                itemReport.ReportAge = 0;
                itemReport.PlayerNum = item.PlayerNum;
                itemReport.RaceName = item.RaceName;
                itemReport.RacePluralName = item.RacePluralName;

                // discover this starbase
                if (item.Starbase != null)
                {
                    itemReport.Starbase = new Starbase()
                    {
                        Position = item.Starbase.Position,
                        Guid = item.Starbase.Guid,
                        Id = item.Starbase.Id,
                        Name = item.Starbase.Name,
                        RaceName = item.RaceName,
                        RacePluralName = item.RacePluralName,
                        PlayerNum = item.Starbase.PlayerNum,
                    };

                    foreach (var token in item.Starbase.Tokens)
                    {
                        if (!player.DesignsByGuid.ContainsKey(token.Design.Guid))
                        {
                            designDiscoverer.Discover(player, token.Design);
                        }
                        itemReport.Starbase.Tokens.Add(new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity));
                    }

                }

                if (reportAge == MapObject.Unexplored)
                {
                    itemReport.Spec = planetService.ComputePlanetSpec(itemReport, player);
                    Message.PlanetDiscovered(player, itemReport, itemReport.Spec.TerraformAmount);
                }
            }
        }

        protected override void DiscoverOwn(Player player, Planet item, Planet itemReport)
        {
            if (itemReport.PlayerNum != item.PlayerNum)
            {
                // this planet wasn't owned by us last turn, switch which list it belongs to
                player.ForeignPlanets.Remove(itemReport);
                player.Planets.Add(itemReport);
            }

            itemReport.PlayerNum = item.PlayerNum;
            itemReport.Cargo = item.Cargo;
            itemReport.Population = item.Population;
            itemReport.MineralConcentration = item.MineralConcentration;
            itemReport.Hab = item.Hab;
            itemReport.BaseHab = item.BaseHab;
            itemReport.TerraformedAmount = item.TerraformedAmount;
            itemReport.ReportAge = 0;
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
            itemReport.ProductionQueue.LeftoverResources = item.ProductionQueue.LeftoverResources;
            itemReport.ProductionQueue.Items.Clear();
            itemReport.ProductionQueue.Items.AddRange(item.ProductionQueue.Items.Select(queueItem => queueItem.Clone()).Select(queueItem =>
            {
                if (queueItem.Design != null)
                {
                    if (player.DesignsByGuid.TryGetValue(queueItem.Design.Guid, out var playerDesign))
                    {
                        // use the Game design, not the player one
                        queueItem.Design = playerDesign;
                    }
                    else
                    {
                        log.Error($"Player ProductionQueueItem has unknown design: {player} - {playerDesign.Name}");
                    }
                }
                return queueItem;
            }));

            if (item.HasStarbase)
            {
                if (!player.DesignsByGuid.ContainsKey(item.Starbase.Design.Guid))
                {
                    designDiscoverer.Discover(player, item.Starbase.Design);
                }

                itemReport.Starbase = new Starbase()
                {
                    Guid = item.Starbase.Guid,
                    Position = item.Starbase.Position,
                    Name = item.Starbase.Name,
                    RaceName = player.Race.Name,
                    RacePluralName = player.Race.PluralName,
                    PlayerNum = player.Num,
                    BattlePlan = player.BattlePlansByGuid[item.Starbase.BattlePlan.Guid]
                };

                itemReport.Starbase.Tokens.Add(new ShipToken(player.DesignsByGuid[item.Starbase.Design.Guid], 1));
                fleetSpecService.ComputeStarbaseSpec(player, itemReport.Starbase, recompute: true);
            }
        }
    }
}