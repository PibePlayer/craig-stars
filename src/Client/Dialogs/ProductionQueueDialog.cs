using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class ProductionQueueDialog : GameViewDialog
    {
        static CSLog log = LogProvider.GetLogger(typeof(ProductionQueueDialog));

        [Inject] protected PlanetService planetService;
        [Inject] protected PlayerService playerService;

        // the planet to use in this dialog
        public Planet Planet { get; set; } = new Planet();

        AvailablePlanetProductionQueueItems availableItems;
        QueuedPlanetProductionQueueItems queuedItems;

        Button addButton;
        Button removeButton;
        Button itemUpButton;
        Button itemDownButton;
        Button clearButton;
        Button prevButton;
        Button nextButton;
        CheckBox contributesOnlyLeftoverToResearchCheckbox;

        Button applyProductionPlanButton;
        Button editProductionPlanButton;
        OptionButton productionPlansOptionButton;

        CostGrid availableItemCostGrid;
        CostGrid queuedItemCostGrid;
        Label completionEstimateLabel;
        Label costOfQueuedLabel;

        int quantityModifier = 1;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();

            playerService = new PlayerService(GameInfo);

            queuedItems = (QueuedPlanetProductionQueueItems)FindNode("QueuedItems");
            availableItems = (AvailablePlanetProductionQueueItems)FindNode("AvailableItems");

            addButton = (Button)FindNode("AddButton");
            removeButton = (Button)FindNode("RemoveButton");
            itemUpButton = (Button)FindNode("ItemUpButton");
            itemDownButton = (Button)FindNode("ItemDownButton");
            clearButton = (Button)FindNode("ClearButton");
            prevButton = (Button)FindNode("PrevButton");
            nextButton = (Button)FindNode("NextButton");
            contributesOnlyLeftoverToResearchCheckbox = (CheckBox)FindNode("ContributesOnlyLeftoverToResearchCheckbox");

            applyProductionPlanButton = (Button)FindNode("ApplyProductionPlanButton");
            editProductionPlanButton = (Button)FindNode("EditProductionPlanButton");
            productionPlansOptionButton = (OptionButton)FindNode("ProductionPlansOptionButton");

            availableItemCostGrid = (CostGrid)FindNode("AvailableItemCostGrid");
            queuedItemCostGrid = (CostGrid)FindNode("QueuedItemCostGrid");
            completionEstimateLabel = (Label)FindNode("CompletionEstimateLabel");
            costOfQueuedLabel = (Label)FindNode("CostOfQueuedLabel");


            addButton.Connect("pressed", this, nameof(OnAddItem));
            clearButton.Connect("pressed", this, nameof(OnClear));
            removeButton.Connect("pressed", this, nameof(OnRemoveItem));
            itemUpButton.Connect("pressed", this, nameof(OnItemUp));
            itemDownButton.Connect("pressed", this, nameof(OnItemDown));
            contributesOnlyLeftoverToResearchCheckbox.Connect("pressed", this, nameof(OnContributesOnlyLeftoverToResearchCheckboxPressed));

            applyProductionPlanButton.Connect("pressed", this, nameof(OnApplyProductionPlanButtonPressed));
            editProductionPlanButton.Connect("pressed", this, nameof(OnEditProductionPlanButtonPressed));

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));

            availableItems.ItemSelectedEvent += OnSelectAvailableItem;
            availableItems.ItemActivatedEvent += OnAddItem;

            queuedItems.ItemSelectedEvent += OnSelectQueuedItem;

            EventManager.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                availableItems.ItemSelectedEvent -= OnSelectAvailableItem;
                availableItems.ItemActivatedEvent -= OnAddItem;
                queuedItems.ItemSelectedEvent -= OnSelectQueuedItem;
                EventManager.MapObjectCommandedEvent -= OnMapObjectCommanded;
            }
        }

        /// <summary>
        /// Update our planet queu
        /// </summary>
        void OnAboutToShow()
        {
            // select the top of the queue on startup
            queuedItems.SelectedItemIndex = 0;

            // this gets stuck sometimes. not sure why
            quantityModifier = 1;

            availableItems.Planet = Planet;
            queuedItems.Planet = Planet;

            productionPlansOptionButton.Clear();
            Me.ProductionPlans.Each((plan, index) =>
            {
                productionPlansOptionButton.AddItem(plan.Name);
            });

            productionPlansOptionButton.Select(0);

            contributesOnlyLeftoverToResearchCheckbox.Pressed = Planet.ContributesOnlyLeftoverToResearch;
            queuedItems.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
        }

        void OnPopupHide()
        {
            queuedItems.Clear();
            availableItems.Clear();
        }

        void OnMapObjectCommanded(MapObjectSprite mapObject)
        {
            // if the player is currently looking at the production queue and a new item comes up, reset ourselves to its
            // items
            if (IsVisibleInTree() && mapObject is PlanetSprite planet)
            {
                Planet = planet.Planet;

                OnAboutToShow();
            }
        }

        /// <summary>
        /// Update the time it takes to complete the selected item, or clear it if we have no item
        /// </summary>
        /// <param name="selectedQueueItem"></param>
        void UpdateCompletionEstimate(ProductionQueueItem selectedQueueItem = null)
        {
            if (selectedQueueItem != null)
            {
                ProductionQueueItem item = selectedQueueItem;
                queuedItemCostGrid.Visible = true;

                // figure out how much this queue item costs
                var cost = playerService.GetCostOfOne(Me, item) * item.Quantity;
                if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    cost = Planet.Starbase.GetUpgradeCost(item.Design);
                }
                queuedItemCostGrid.Cost = cost;
                costOfQueuedLabel.Text = $"Cost of {item.ShortName} x {item.Quantity}";

                completionEstimateLabel.Visible = true;
                completionEstimateLabel.Text = $"{(int)(item.percentComplete * 100)}% Done. Completion {item.yearsToBuildAll} year{(item.yearsToBuildAll > 1 ? "s" : "")}";
            }
            else
            {
                queuedItemCostGrid.Visible = false;
                completionEstimateLabel.Visible = false;
            }
        }

        #region Events 

        /// <summary>
        /// Set the quantity modifier for the dialog
        /// if the user holds shift, we multipy by 10, if they press control we multiply by 100
        /// both multiplies by 1000
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            quantityModifier = this.UpdateQuantityModifer(@event, quantityModifier);
        }

        /// <summary>
        /// When the ok button is pressed, save all these changes to the other players
        /// </summary>
        protected override void OnOk()
        {
            Save();
            Hide();
        }

        void Save()
        {
            if (Planet.ProductionQueue != null)
            {
                Planet.ProductionQueue.Items.Clear();
                Planet.ProductionQueue.Items.AddRange(queuedItems.Items);
            }
            else
            {
                log.Error($"ProductionQueue for planet {Planet.Name} is null");
            }
            Planet.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;

            EventManager.PublishProductionQueueChangedEvent(Planet);
        }

        async void OnApplyProductionPlanButtonPressed()
        {
            var plan = Me.ProductionPlans[productionPlansOptionButton.Selected];
            planetService.ApplyProductionPlan(queuedItems.Items, Me, plan);
            queuedItems.SelectedItemIndex = 0;
            await queuedItems.UpdateItems();
        }

        void OnEditProductionPlanButtonPressed()
        {
            EventManager.PublishProductionPlansDialogRequestedEvent();
        }

        void OnNextButtonPressed()
        {
            Save();
            EventManager.PublishCommandNextMapObjectEvent();
        }

        void OnPrevButtonPressed()
        {
            Save();
            EventManager.PublishCommandPrevMapObjectEvent();
        }

        void OnContributesOnlyLeftoverToResearchCheckboxPressed()
        {
            queuedItems.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
        }


        void OnSelectQueuedItem(ProductionQueueItem item)
        {
            UpdateCompletionEstimate(item);
        }

        void OnSelectAvailableItem(ProductionQueueItem item)
        {
            var cost = playerService.GetCostOfOne(Me, item);
            if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
            {
                cost = Planet.Starbase.GetUpgradeCost(item.Design);
            }
            availableItemCostGrid.Cost = cost;
        }

        void OnAddItem(ProductionQueueItem item)
        {
            if (item != null)
            {
                queuedItems.AddItem(item.Clone(), quantityModifier);
            }
        }

        void OnAddItem()
        {
            var item = availableItems.GetSelectedItem();
            if (item != null)
            {
                queuedItems.AddItem(item.Clone(), quantityModifier);
            }
        }

        void OnClear()
        {
            queuedItems.Items.Clear();
            var _ = queuedItems.UpdateItems();
        }

        void OnRemoveItem()
        {
            queuedItems.RemoveItem(quantityModifier);
        }

        void OnItemUp()
        {
            queuedItems.ItemUp();
        }

        void OnItemDown()
        {
            queuedItems.ItemDown();
        }

        #endregion
    }
}