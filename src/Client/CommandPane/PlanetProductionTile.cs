using Godot;
using System;

using CraigStars.Singletons;

namespace CraigStars
{

    public class PlanetProductionTile : PlanetTile
    {
        GridContainer productionQueueContainer;
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
            productionQueueContainer = GetNode<GridContainer>("VBoxContainer/MarginContainer/Panel/MarginContainer/ScrollContainer/ProductionQueueContainer");
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
            foreach (Node node in productionQueueContainer.GetChildren())
            {
                productionQueueContainer.RemoveChild(node);
                node.QueueFree();
            }
            if (CommandedPlanet != null)
            {
                // populate the production queue
                CommandedPlanet.Planet.ProductionQueue?.Items.ForEach(item =>
                {
                    var nameLabel = new Label() { Text = item.ShortName };
                    nameLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                    productionQueueContainer.AddChild(nameLabel);
                    productionQueueContainer.AddChild(new Label() { Text = $"{item.quantity}", Align = Label.AlignEnum.Right });
                });
            }
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