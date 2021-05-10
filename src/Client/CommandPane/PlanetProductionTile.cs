using Godot;
using System;

using CraigStars.Singletons;
using CraigStarsTable;

namespace CraigStars
{

    public class PlanetProductionTile : PlanetTile
    {
        CSTable productionQueue;
        CSConfirmDialog confirmDialog;
        Button changeButton;
        Button clearButton;
        Button routeButton;
        Label routeTo;

        public override void _Ready()
        {
            base._Ready();

            changeButton = (Button)FindNode("ChangeButton");
            clearButton = (Button)FindNode("ClearButton");
            routeButton = (Button)FindNode("RouteButton");
            routeTo = (Label)FindNode("RouteTo");
            productionQueue = GetNode<CSTable>("VBoxContainer/MarginContainer/ScrollContainer/ProductionQueue");
            confirmDialog = GetNode<CSConfirmDialog>("ConfirmDialog");

            changeButton.Connect("pressed", this, nameof(OnChangeButtonPressed));
            clearButton.Connect("pressed", this, nameof(OnClearButtonPressed));
            Signals.ProductionQueueChangedEvent += OnProductionQueueChanged;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.ProductionQueueChangedEvent -= OnProductionQueueChanged;
        }

        void OnProductionQueueChanged(Planet planet)
        {
            if (CommandedPlanet?.Planet == planet)
            {
                UpdateControls();
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            productionQueue.ClearTable();
            productionQueue.Data.Clear();
            productionQueue.Data.AddColumn("Item");
            productionQueue.Data.AddColumn("Quantity", align: Label.AlignEnum.Right);
            if (CommandedPlanet != null)
            {
                // populate the production queue
                CommandedPlanet.Planet.ProductionQueue?.Items.ForEach(item =>
                {
                    productionQueue.Data.AddRow(item.ShortName, item.Quantity);
                });

            }
            productionQueue.ResetTable();
        }

        void OnChangeButtonPressed()
        {
            if (CommandedPlanet != null)
            {
                Signals.PublishChangeProductionQueuePressedEvent(CommandedPlanet);
            }
        }

        void OnClearButtonPressed()
        {
            if (CommandedPlanet != null)
            {
                confirmDialog.Show($"Are you sure you want to clear the production queue at {CommandedPlanet.Planet.Name}?",
                () =>
                {
                    CommandedPlanet.Planet.ProductionQueue.Items.Clear();
                    UpdateControls();
                });

            }
        }

    }
}