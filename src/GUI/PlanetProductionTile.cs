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

            changeButton = FindNode("ChangeButton") as Button;
            clearButton = FindNode("ClearButton") as Button;
            routeButton = FindNode("RouteButton") as Button;
            routeTo = FindNode("RouteTo") as Label;
            productionQueueItemList = FindNode("ProductionQueueItemList") as ItemList;

            changeButton.Connect("pressed", this, nameof(OnChangeButtonPressed));
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                // populate the production queue
                productionQueueItemList.Clear();
                ActivePlanet.ProductionQueue.Items.ForEach(item =>
                {
                    productionQueueItemList.AddItem(item.ShortName);
                    productionQueueItemList.AddItem($"{item.Quantity}");
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