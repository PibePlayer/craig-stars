using Godot;
using log4net;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using CraigStars.Utils;
using CraigStarsTable;

namespace CraigStars
{
    public class ProductionQueueDialog : GameViewDialog
    {
        static CSLog log = LogProvider.GetLogger(typeof(ProductionQueueDialog));

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
        Button okButton;
        CheckBox contributesOnlyLeftoverToResearchCheckbox;

        CostGrid availableItemCostGrid;
        CostGrid queuedItemCostGrid;
        Label completionEstimateLabel;
        Label costOfQueuedLabel;

        int quantityModifier = 1;

        public override void _Ready()
        {
            base._Ready();

            queuedItems = (QueuedPlanetProductionQueueItems)FindNode("QueuedItems");
            availableItems = (AvailablePlanetProductionQueueItems)FindNode("AvailableItems");

            addButton = (Button)FindNode("AddButton");
            removeButton = (Button)FindNode("RemoveButton");
            itemUpButton = (Button)FindNode("ItemUpButton");
            itemDownButton = (Button)FindNode("ItemDownButton");
            clearButton = (Button)FindNode("ClearButton");
            prevButton = (Button)FindNode("PrevButton");
            nextButton = (Button)FindNode("NextButton");
            okButton = (Button)FindNode("OKButton");
            contributesOnlyLeftoverToResearchCheckbox = (CheckBox)FindNode("ContributesOnlyLeftoverToResearchCheckbox");

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

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            okButton.Connect("pressed", this, nameof(OnOk));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));

            availableItems.RowSelectedEvent += OnSelectAvailableItem;
            availableItems.RowActivatedEvent += OnAddItem;

            queuedItems.RowSelectedEvent += OnSelectQueuedItem;

            Signals.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _ExitTree()
        {
            availableItems.RowSelectedEvent -= OnSelectAvailableItem;
            availableItems.RowActivatedEvent -= OnAddItem;
            queuedItems.RowSelectedEvent -= OnSelectQueuedItem;
            Signals.MapObjectCommandedEvent -= OnMapObjectCommanded;
        }

        /// <summary>
        /// Update our planet queu
        /// </summary>
        void OnAboutToShow()
        {
            // select the top of the queue on startup
            queuedItems.SelectedItemIndex = 0;

            availableItems.Planet = Planet;
            queuedItems.Planet = Planet;

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
            if (Visible && mapObject is PlanetSprite planet)
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
                var cost = item.GetCostOfOne(RulesManager.Rules, Me) * item.Quantity;
                if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    cost = Planet.Starbase.GetUpgradeCost(item.Design);
                }
                queuedItemCostGrid.Cost = cost;
                costOfQueuedLabel.Text = $"Cost of {item.ShortName} x {item.Quantity}";

                completionEstimateLabel.Visible = true;
                completionEstimateLabel.Text = $"{(int)(item.percentComplete * 100)}% Done. Completion {item.yearsToBuild} year{(item.yearsToBuild > 1 ? "s" : "")}";
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
        void OnOk()
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

            Signals.PublishProductionQueueChangedEvent(Planet);
        }

        void OnNextButtonPressed()
        {
            Save();
            Signals.PublishActiveNextMapObjectEvent();
        }

        void OnPrevButtonPressed()
        {
            Save();
            Signals.PublishActivePrevMapObjectEvent();
        }

        void OnContributesOnlyLeftoverToResearchCheckboxPressed()
        {
            queuedItems.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
        }


        void OnSelectQueuedItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item)
        {
            UpdateCompletionEstimate(item);
        }

        void OnSelectAvailableItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item)
        {
            var cost = item.GetCostOfOne(RulesManager.Rules, Me);
            if (item.Type == QueueItemType.Starbase && Planet.HasStarbase)
            {
                cost = Planet.Starbase.GetUpgradeCost(item.Design);
            }
            availableItemCostGrid.Cost = cost;
        }

        void OnAddItem(int rowIndex, int colIndex, Cell cell, ProductionQueueItem item)
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
            queuedItems.UpdateItems();
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