using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class PlanetsReportTable : ReportTable<Planet>
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlanetsReportTable));

        PlanetService planetService = new();

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
                "Starbase",
                "Population",
                "Cap (%)",
                "Value (%)",
                "Production",
                "Mine",
                "Factory",
                "Defense",
                new Column("Surface Minerals")
                {
                    CellProvider = (col, cell, row) =>
                    {
                        var cellControl = CSResourceLoader.GetPackedScene("MineralsCell.tscn").Instance<MineralsCell>();
                        cellControl.Column = col;
                        cellControl.Cell = cell;
                        cellControl.Row = row;
                        return cellControl;
                    }
                }
            );

        }

        /// <summary>
        /// Create column data for a planet
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override List<Cell> CreateCellsForItem(Planet item)
        {
            var player = PlayersManager.Me;
            var race = player.Race;

            var explored = item.Explored;
            var owned = item.OwnedBy(player);

            var owner = "(unexplored)";
            var ownerColor = Colors.Gray;
            var capacity = 0.0;
            var habitability = int.MinValue;
            var habitabilityText = "(unexplored)";
            var habColor = Colors.White;
            var foreign = false;
            if (explored)
            {
                if (item.Owned)
                {
                    owner = $"{item.RacePluralName}";
                    if (!owned)
                    {
                        // has an owner, but not owned by us
                        foreign = true;
                        ownerColor = GameInfo.Players[item.PlayerNum].Color;
                    }
                    else
                    {
                        ownerColor = Colors.White;
                    }
                }
                else
                {
                    owner = "--";
                }
                capacity = planetService.GetPopulationDensity(item, player, RulesManager.Rules);
                habitability = race.GetPlanetHabitability(item.Hab.Value);
                habitabilityText = $"{habitability:.#}%";
                habColor = habitability > 0 ? Colors.Green : Colors.Red;

            }
            var production = "--";
            if (item.ProductionQueue?.Items.Count > 0)
            {
                production = $"{item.ProductionQueue.Items[0].ShortName}";
            }


            return new List<Cell>() {
                new Cell(item.Name),
                new Cell(owner),
                new Cell($"{(item.Starbase != null ? item.Starbase.Name : "--")}"),
                new Cell($"{(foreign ? "±" : "")}{item.Population}", item.Population),
                new Cell($"{capacity*100:.#}%", capacity),
                new Cell(habitabilityText, habitability, color: habColor),
                new Cell(production),
                (explored && owned) ? new Cell(item.Mines) : new Cell("--", -1),
                (explored && owned) ? new Cell(item.Factories) : new Cell("--", -1),
                (explored && owned) ? new Cell(item.Defenses) : new Cell("--", -1),
                new Cell("Surface Minerals", item.Cargo.Total, metadata: item.Cargo)
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

        protected override void ItemSelected(Planet item, Cell cell)
        {
            if (item.OwnedBy(PlayersManager.Me))
            {
                EventManager.PublishCommandMapObjectEvent(item);
            }
            else
            {
                EventManager.PublishSelectMapObjectEvent(item);
            }
        }

    }
}