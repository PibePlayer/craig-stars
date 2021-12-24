using System;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class ReportsDialog : GameViewDialog
    {
        PlanetsReportTable planetsTable;
        FleetsReportTable fleetsTable;

        public override void _Ready()
        {
            base._Ready();
            planetsTable = GetNode<PlanetsReportTable>("MarginContainer/VBoxContainer/ContentContainer/TabContainer/Planets/PlanetsTable");
            fleetsTable = GetNode<FleetsReportTable>("MarginContainer/VBoxContainer/ContentContainer/TabContainer/Fleets/FleetsTable");

        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (IsVisibleInTree())
            {
                planetsTable.ShowAll = false;
                fleetsTable.ShowAll = false;
                planetsTable.ResetTableData();
                fleetsTable.ResetTableData();
            }
        }
    }
}