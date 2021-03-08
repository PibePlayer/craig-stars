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

        public override void _Ready()
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
            base._Ready();
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

            var capacity = item.Population / (float)item.GetMaxPopulation(race, player.Rules);
            var habitability = race.GetPlanetHabitability(item.Hab.Value);
            var production = "--";
            if (item?.ProductionQueue.Items.Count > 0)
            {
                production = $"{item.ProductionQueue.Items[0].ShortName}";
            }

            return new ColumnData[] {
                new ColumnData(item.Name, guid: item.Guid),
                new ColumnData($"{(item.Starbase != null ? item.Starbase.Name : "--")}"),
                new ColumnData($"{item.Population}", item.Population),
                new ColumnData($"{capacity*100:.#}%", capacity),
                new ColumnData($"{habitability:.#}%", habitability),
                new ColumnData(production),
                new ColumnData(item.Mines),
                new ColumnData(item.Factories),
                new ColumnData(item.Defenses),
            };
        }

        protected override IEnumerable<Planet> GetItems()
        {
            return PlayersManager.Me.Planets.Where(p => p.Player == PlayersManager.Me);
        }

        protected override void ItemSelected(TreeItem row, int col)
        {
            Guid guid = Guid.Parse(row.GetMetadata(0).ToString());
            if (PlayersManager.Me.PlanetsByGuid.TryGetValue(guid, out var planet))
            {
                Signals.PublishCommandMapObjectEvent(planet);
            }
            else
            {
                log.Error($"Tried to command map object with unknown guid: {guid}");
            }
        }

    }
}