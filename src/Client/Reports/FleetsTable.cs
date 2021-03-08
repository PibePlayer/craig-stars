using CraigStars.Singletons;
using Godot;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetsTable : Table<Fleet>
    {
        ILog log = LogManager.GetLogger(typeof(PlanetsTable));

        public override void _Ready()
        {
            Columns.Clear();
            Columns.AddRange(new List<string>() {
            "Name",
            "Location",
            "Destination",
            "ETA",
            "Task",
            "Fuel",
            "Cargo",
            "Mass",
        });
            base._Ready();
        }

        /// <summary>
        /// Create column data for a fleet
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override ColumnData[] CreateColumnData(Fleet item)
        {
            var player = PlayersManager.Me;
            var race = player.Race;

            var wp0 = item.Waypoints[0];
            var destination = "--";
            var etaText = "--";
            var eta = double.MaxValue;
            var task = "(no task here)";

            if (item.Waypoints.Count > 1)
            {
                var nextWaypoint = item.Waypoints[1];
                destination = nextWaypoint.TargetName;
                eta = Math.Ceiling(wp0.GetTimeToWaypoint(nextWaypoint));
                etaText = $"{eta}y";
                task = nextWaypoint.Task.ToString();
            }

            return new ColumnData[] {
                new ColumnData(item.Name, guid: item.Guid),
                new ColumnData(wp0.TargetName),
                new ColumnData(destination),
                new ColumnData(etaText, eta),
                new ColumnData(task),
                new ColumnData($"{item.Fuel}mg", item.Fuel),
                new ColumnData($"i: {item.Cargo.Ironium} b: {item.Cargo.Boranium} g: {item.Cargo.Germanium} c: {item.Cargo.Colonists}", item.Cargo.Total),
                new ColumnData($"{item.Aggregate.Mass}kT", item.Aggregate.Mass),
            };
        }

        protected override IEnumerable<Fleet> GetItems()
        {
            return PlayersManager.Me.Fleets.Where(f => f.Player == PlayersManager.Me);
        }

        protected override void ItemSelected(TreeItem row, int col)
        {
            Guid guid = Guid.Parse(row.GetMetadata(0).ToString());
            if (PlayersManager.Me.FleetsByGuid.TryGetValue(guid, out var fleet))
            {
                Signals.PublishCommandMapObjectEvent(fleet);
            }
            else
            {
                log.Error($"Tried to command map object with unknown guid: {guid}");
            }
        }
    }
}