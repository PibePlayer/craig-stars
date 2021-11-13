using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// Discoverer for discovering fleets
    /// </summary>
    public class FleetDiscoverer : Discoverer<Fleet>
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetDiscoverer));

        private readonly ShipDesignDiscoverer designDiscoverer;

        public FleetDiscoverer(ShipDesignDiscoverer designDiscoverer)
        {
            this.designDiscoverer = designDiscoverer;
        }

        protected override List<Fleet> GetOwnedItemReports(Player player) => player.Fleets;
        protected override List<Fleet> GetForeignItemReports(Player player) => player.ForeignFleets;
        protected override Dictionary<Guid, Fleet> GetItemsByGuid(Player player) => player.FleetsByGuid;

        protected override Fleet CreateEmptyReport(Fleet item) => new Fleet()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            BaseName = item.BaseName,
            RaceName = item.RaceName,
            RacePluralName = item.RacePluralName,
            PlayerNum = item.PlayerNum,
            WarpSpeed = item.WarpSpeed,
            Heading = item.Heading,
            Mass = item.Mass
        };

        protected override void OnNewReportCreated(Player player, Fleet item, Fleet itemReport)
        {
            // update orbiting information
            if (item.Orbiting != null)
            {
                var orbitingPlanetReport = player.PlanetsByGuid[item.Orbiting.Guid];
                itemReport.Orbiting = orbitingPlanetReport;
                itemReport.Orbiting.OrbitingFleets.Add(itemReport);
            }
        }

        protected override void DiscoverForeign(Player player, Fleet item, Fleet itemReport, bool penScanned)
        {
            foreach (var token in item.Tokens)
            {
                if (!player.DesignsByGuid.TryGetValue(token.Design.Guid, out var existingDesign) || (existingDesign.Slots.Count == 0 && penScanned))
                {
                    designDiscoverer.Discover(player, token.Design);
                }
                itemReport.Tokens.Add(new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity, token.Damage, token.QuantityDamaged));
            }
        }

        protected override void DiscoverOwn(Player player, Fleet item, Fleet itemReport)
        {
            itemReport.RepeatOrders = item.RepeatOrders;
            itemReport.IdleTurns = item.IdleTurns;
            itemReport.Waypoints.Clear();
            // copy waypoints
            item.Waypoints.ForEach(wp =>
            {
                if (wp.Target is MapObject mapObject)
                {
                    if (player.MapObjectsByGuid.TryGetValue(mapObject.Guid, out var playerMapObject))
                    {
                        var target = playerMapObject;
                        var playerWaypoint = Waypoint.TargetWaypoint(target, wp.WarpFactor, wp.Task, wp.TransportTasks);
                        playerWaypoint.OriginalPosition = wp.OriginalPosition;

                        if (wp.OriginalTarget is MapObject originalTarget)
                        {
                            if (player.MapObjectsByGuid.TryGetValue(originalTarget.Guid, out var playerOriginalTarget))
                            {
                                playerWaypoint.OriginalTarget = playerOriginalTarget;
                            }
                            else
                            {
                                log.Error($"Game Fleet has OriginalTarget {originalTarget} but no matching mapObject in player.MapObjectsByGuid.");
                            }
                        }

                        itemReport.Waypoints.Add(playerWaypoint);
                    }
                    else
                    {
                        log.Error($"Game Fleet has target {mapObject} but no matching mapObject in player.MapObjectsByGuid.");
                    }
                }
                else
                {
                    // TODO: figure out targeting other items
                    // we might have to add waypoint reports as a separate
                    // step
                    itemReport.Waypoints.Add(Waypoint.PositionWaypoint(wp.Position, wp.WarpFactor, wp.Task, wp.TransportTasks));
                }
            });
            itemReport.Cargo = item.Cargo;
            itemReport.Fuel = item.Fuel;
            itemReport.BattlePlan = player.BattlePlansByGuid[item.BattlePlan.Guid];
            itemReport.Tokens.AddRange(item.Tokens.Select(token => new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity, token.Damage, token.QuantityDamaged)).ToList());

        }
    }
}