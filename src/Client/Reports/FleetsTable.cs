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

        public bool ShowAll { get; set; }

        public override void _Ready()
        {
            ResetColumns();
            base._Ready();
        }

        protected override void OnShowOwnedPressed() { ShowAll = false; ResetColumns(); UpdateItems(); }
        protected override void OnShowAllPressed() { ShowAll = true; ResetColumns(); UpdateItems(); }

        void ResetColumns()
        {
            Columns.Clear();
            Columns.AddRange(new List<string>() {
            "Name",
            "Id",
            "Location",
            "Destination",
            "ETA",
            "Task",
            "Fuel",
            "Cargo",
            "Mass",
        });
            // maybe we need a column header class with a "hidden" field?
            if (ShowAll)
            {
                Columns.Insert(1, "Owner");
            }
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

            var owner = "--";
            var location = $"Space: ({item.Position.x:.##}, {item.Position.y:.##})"; ;
            var destination = "--";
            var etaText = "--";
            var eta = double.MaxValue;
            var task = "(no task here)";

            if (item.Owner != null)
            {
                owner = $"{item.Owner.RacePluralName}";
            }
            else
            {
                owner = "--";
            }

            if (item.Waypoints?.Count > 1)
            {
                var wp0 = item.Waypoints[0];
                location = wp0.TargetName;
                var nextWaypoint = item.Waypoints[1];
                destination = nextWaypoint.TargetName;
                eta = Math.Ceiling(wp0.GetTimeToWaypoint(nextWaypoint));
                etaText = $"{eta}y";
                task = nextWaypoint.Task.ToString();
            }

            return new ColumnData[] {
                new ColumnData(item.Name, guid: item.Guid),
                new ColumnData(item.Id),
                new ColumnData(owner, hidden: !ShowAll),
                new ColumnData(location),
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
            if (ShowAll)
            {
                return PlayersManager.Me.AllFleets;
            }
            else
            {
                return PlayersManager.Me.Fleets;
            }
        }

        protected override void ItemSelected(TreeItem row, int col)
        {
            Guid guid = Guid.Parse(row.GetMetadata(0).ToString());
            if (PlayersManager.Me.FleetsByGuid.TryGetValue(guid, out var fleet))
            {
                if (fleet.OwnedBy(PlayersManager.Me))
                {
                    Signals.PublishCommandMapObjectEvent(fleet);
                }
                else
                {
                    Signals.PublishSelectMapObjectEvent(fleet);
                }
            }
            else
            {
                log.Error($"Tried to command map object with unknown guid: {guid}");
            }
        }
    }
}