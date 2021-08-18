using Godot;
using System;

using CraigStars.Singletons;
using CraigStarsTable;

namespace CraigStars.Client
{

    public class PlanetProductionTile : PlanetTile
    {
        QueuedPlanetProductionQueueItems productionQueue;
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
            productionQueue = GetNode<QueuedPlanetProductionQueueItems>("VBoxContainer/Controls/MarginContainer/ProductionQueue");

            changeButton.Connect("pressed", this, nameof(OnChangeButtonPressed));
            clearButton.Connect("pressed", this, nameof(OnClearButtonPressed));
            EventManager.ProductionQueueChangedEvent += OnProductionQueueChanged;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            EventManager.ProductionQueueChangedEvent -= OnProductionQueueChanged;
        }

        void OnProductionQueueChanged(Planet planet)
        {
            if (CommandedPlanet?.Planet == planet)
            {
                UpdateControls();
            }
        }

        void OnChangeButtonPressed()
        {
            if (CommandedPlanet != null)
            {
                EventManager.PublishProductionQueueDialogRequestedEvent(CommandedPlanet);
            }
        }

        void OnClearButtonPressed()
        {
            if (CommandedPlanet != null)
            {
                CSConfirmDialog.Show($"Are you sure you want to clear the production queue at {CommandedPlanet.Planet.Name}?",
                () =>
                {
                    CommandedPlanet.Planet.ProductionQueue.Items.Clear();
                    UpdateControls();
                });

            }
        }

        protected override void UpdateControls()
        {
            productionQueue.ShowTopOfQueue = CommandedPlanet?.Planet?.ProductionQueue?.Items.Count == 0;
            productionQueue.Planet = CommandedPlanet?.Planet;
            base.UpdateControls();
        }

    }
}