using Godot;
using System;

using CraigStars.Singletons;

namespace CraigStars
{

    public class PlanetProductionTile : PlanetTile
    {
        ItemList productionQueueItemList;
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
            productionQueueItemList = (ItemList)FindNode("ProductionQueueItemList");

            changeButton.Connect("pressed", this, nameof(OnChangeButtonPressed));
            Signals.ProductionQueueChangedEvent += OnProductionQueueChanged;
        }

        public override void _ExitTree()
        {
            Signals.ProductionQueueChangedEvent -= OnProductionQueueChanged;
        }

        void OnProductionQueueChanged(Planet planet)
        {
            if (ActivePlanet?.Planet == planet)
            {
                UpdateControls();
            }
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                // populate the production queue
                productionQueueItemList.Clear();
                ActivePlanet.Planet.ProductionQueue?.Items.ForEach(item =>
                {
                    productionQueueItemList.AddItem(item.ShortName);
                    productionQueueItemList.AddItem($"{item.quantity}", selectable: false);
                });
            }
        }

        void OnChangeButtonPressed()
        {
            if (ActivePlanet != null)
            {
                Signals.PublishChangeProductionQueuePressedEvent(ActivePlanet);
            }
        }

    }
}