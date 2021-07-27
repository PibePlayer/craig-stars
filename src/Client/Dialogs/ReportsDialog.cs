using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars.Client
{
    public class ReportsDialog : GameViewDialog
    {
        Button okButton;

        PlanetsReportTable planetsTable;
        FleetsReportTable fleetsTable;

        public override void _Ready()
        {
            base._Ready();
            okButton = FindNode("OKButton") as Button;
            planetsTable = GetNode<PlanetsReportTable>("MarginContainer/VBoxContainer/TabContainer/Planets/PlanetsTable");
            fleetsTable = GetNode<FleetsReportTable>("MarginContainer/VBoxContainer/TabContainer/Fleets/FleetsTable");

            okButton.Connect("pressed", this, nameof(OnOK));
        }

        void OnOK() => Hide();

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (Visible)
            {
                planetsTable.ShowAll = false;
                fleetsTable.ShowAll = false;
                planetsTable.ResetTableData();
                fleetsTable.ResetTableData();
            }
        }
    }
}