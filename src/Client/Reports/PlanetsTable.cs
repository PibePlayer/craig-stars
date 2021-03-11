using CraigStars;
using CraigStars.Singletons;
using Godot;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class PlanetsTable : Table<Planet>
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
            "Starbase",
            "Population",
            "Cap (%)",
            "Value (%)",
            "Production",
            "Mine",
            "Factory",
            "Defense",
            });

            // maybe we need a column header class with a "hidden" field?
            if (ShowAll)
            {
                Columns.Insert(1, "Owner");
            }
        }

        /// <summary>
        /// Create column data for a planet
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override ColumnData[] CreateColumnData(Planet item)
        {
            var player = PlayersManager.Me;
            var race = player.Race;

            var explored = item.Explored;
            var owned = item.OwnedBy(player);

            var owner = "(unexplored)";
            var capacity = 0.0;
            var habitability = int.MinValue;
            var habitabilityText = "(unexplored)";
            var foreign = false;
            if (explored)
            {
                if (item.Owner != null)
                {
                    owner = $"{item.Owner.RacePluralName}";
                    if (!owned)
                    {
                        // has an owner, but not owned by us
                        foreign = true;
                    }
                }
                else
                {
                    owner = "--";
                }
                capacity = item.Population / (float)item.GetMaxPopulation(race, player.Rules);
                habitability = race.GetPlanetHabitability(item.Hab.Value);
                habitabilityText = $"{habitability:.#}%";
            }
            var production = "--";
            if (item.ProductionQueue?.Items.Count > 0)
            {
                production = $"{item.ProductionQueue.Items[0].ShortName}";
            }

            return new ColumnData[] {
                new ColumnData(item.Name, guid: item.Guid),
                new ColumnData(owner, hidden: !ShowAll),
                new ColumnData($"{(item.Starbase != null ? item.Starbase.Name : "--")}"),
                new ColumnData($"{(foreign ? "±" : "")}{item.Population}", item.Population),
                new ColumnData($"{capacity*100:.#}%", capacity),
                new ColumnData(habitabilityText, habitability),
                new ColumnData(production),
                (explored && owned) ? new ColumnData(item.Mines) : new ColumnData("--", -1),
                (explored && owned) ? new ColumnData(item.Factories) : new ColumnData("--", -1),
                (explored && owned) ? new ColumnData(item.Defenses) : new ColumnData("--", -1),
            };

        }

        protected override IEnumerable<Planet> GetItems()
        {
            if (ShowAll)
            {
                return PlayersManager.Me.AllPlanets;
            }
            else
            {
                return PlayersManager.Me.Planets;
            }
        }

        protected override void ItemSelected(TreeItem row, int col)
        {
            Guid guid = Guid.Parse(row.GetMetadata(0).ToString());
            if (PlayersManager.Me.PlanetsByGuid.TryGetValue(guid, out var planet))
            {
                if (planet.OwnedBy(PlayersManager.Me))
                {
                    Signals.PublishCommandMapObjectEvent(planet);
                }
                else
                {
                    Signals.PublishSelectMapObjectEvent(planet);
                }
            }
            else
            {
                log.Error($"Tried to command map object with unknown guid: {guid}");
            }
        }

    }
}