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
        private static readonly ILog log = LogManager.GetLogger(typeof(FleetDiscoverer));

        ShipDesignDiscoverer designDiscoverer = new ShipDesignDiscoverer();

        protected override List<Fleet> GetOwnedItemReports(Player player) => player.Fleets;
        protected override List<Fleet> GetForeignItemReports(Player player) => player.ForeignFleets;
        protected override Dictionary<Guid, Fleet> GetItemsByGuid(Player player) => player.FleetsByGuid;

        protected override Fleet CreateEmptyReport(Fleet item) => new Fleet()
        {
            Position = item.Position,
            Guid = item.Guid,
            Id = item.Id,
            Name = item.Name,
            RaceName = item.Player.Race.Name,
            RacePluralName = item.Player.Race.PluralName,
            Owner = item.Owner,
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
                    designDiscoverer.Discover(player, token.Design, penScanned);
                }
                itemReport.Tokens.Add(new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity));
            }
        }

        protected override void DiscoverOwn(Player player, Fleet item, Fleet itemReport)
        {
            itemReport.Player = player;
            itemReport.Waypoints.Clear();
            // copy waypoints
            item.Waypoints.ForEach(wp =>
            {
                if (wp.Target is Planet)
                {
                    var targetedPlanetReport = player.PlanetsByGuid[wp.Target.Guid];
                    itemReport.Waypoints.Add(Waypoint.TargetWaypoint(targetedPlanetReport, wp.WarpFactor, wp.Task, wp.TransportTasks));
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
            itemReport.Tokens.AddRange(item.Tokens.Select(token => new ShipToken(player.DesignsByGuid[token.Design.Guid], token.Quantity)).ToList());

        }
    }
}