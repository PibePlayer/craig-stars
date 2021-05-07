using CraigStars.Singletons;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class ReportsDialog : GameViewDialog
    {
        Button okButton;

        PlanetsTable planetsTable;
        FleetsTable fleetsTable;

        public override void _Ready()
        {
            base._Ready();
            okButton = FindNode("OKButton") as Button;
            planetsTable = GetNode<PlanetsTable>("MarginContainer/VBoxContainer/TabContainer/Planets/PlanetsTable");
            fleetsTable = GetNode<FleetsTable>("MarginContainer/VBoxContainer/TabContainer/Fleets/FleetsTable");

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