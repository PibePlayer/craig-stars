using CraigStars.Singletons;
using Godot;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetsTable : ReportTable<Fleet>
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetsTable));

        public bool ShowAll { get; set; }

        public Column ownerColumn;

        protected override void OnShowOwnedPressed() { ShowAll = false; ownerColumn.Hidden = !ShowAll; base.OnShowOwnedPressed(); }
        protected override void OnShowAllPressed() { ShowAll = true; ownerColumn.Hidden = !ShowAll; base.OnShowAllPressed(); }


        protected override void AddColumns()
        {
            ownerColumn = new Column("Owner", hidden: !ShowAll);
            table.Data.AddColumns(
            "Name",
            ownerColumn,
            "Id",
            "Location",
            "Destination",
            "ETA",
            "Task",
            "Fuel",
            new Column("Cargo", scene: "res://src/Client/Controls/Table/CargoCell.tscn"),
            "Mass"
            );
        }

        /// <summary>
        /// Create column data for a fleet
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override List<Cell> CreateCellsForItem(Fleet item)
        {
            var player = PlayersManager.Me;
            var race = player.Race;

            var owner = "--";
            var ownerColor = Colors.Gray;
            var location = $"Space: ({item.Position.x:.##}, {item.Position.y:.##})"; ;
            var destination = "--";
            var etaText = "--";
            var eta = double.MaxValue;
            var task = "(no task here)";

            if (item.Owner != null)
            {
                owner = $"{item.Owner.RacePluralName}";
                if (item.OwnedBy(PlayersManager.Me))
                {
                    ownerColor = Colors.White;
                }
                else
                {
                    ownerColor = item.Owner.Color;
                }
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

            return new List<Cell>() {
                new Cell(item.Name),
                new Cell(owner, color: ownerColor),
                new Cell(item.Id),
                new Cell(location),
                new Cell(destination),
                new Cell(etaText, eta),
                new Cell(task),
                new Cell($"{item.Fuel}mg", item.Fuel),
                new Cell("Cargo", item.Cargo.Total, metadata: item.Cargo),
                new Cell($"{item.Aggregate.Mass}kT", item.Aggregate.Mass),
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

        protected override void ItemSelected(Fleet item, Cell cell)
        {
            if (item.OwnedBy(PlayersManager.Me))
            {
                Signals.PublishCommandMapObjectEvent(item);
            }
            else
            {
                Signals.PublishSelectMapObjectEvent(item);
            }
        }
    }
}