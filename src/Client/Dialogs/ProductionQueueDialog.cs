using Godot;
using log4net;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class ProductionQueueDialog : GameViewDialog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProductionQueueDialog));

        // the planet to use in this dialog
        public Planet Planet { get; set; } = new Planet();

        Tree queuedItemsTree;
        Tree availableItemsTree;
        TreeItem queuedItemsTreeRoot;
        TreeItem topOfQueueItem;
        TreeItem availableItemsTreeRoot;

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

        int selectedAvailableItemIndex = -1;
        int selectedQueuedItemIndex = -1;
        List<ProductionQueueItem> availableItems = new List<ProductionQueueItem>();
        List<ProductionQueueItem> queuedItems = new List<ProductionQueueItem>();

        int quantityModifier = 1;

        public override void _Ready()
        {
            base._Ready();

            queuedItemsTree = (Tree)FindNode("QueuedItemsTree");
            availableItemsTree = (Tree)FindNode("AvailableItemsTree");

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

            queuedItemsTree.Columns = 2;
            queuedItemsTree.SetColumnExpand(0, true);
            queuedItemsTree.SetColumnExpand(1, false);
            queuedItemsTree.SetColumnMinWidth(1, 60);

            availableItemsTree.SetColumnExpand(0, true);
            availableItemsTree.Connect("item_activated", this, nameof(OnAddItem));
            queuedItemsTree.Connect("item_selected", this, nameof(OnSelectQueuedItem));
            availableItemsTree.Connect("item_selected", this, nameof(OnSelectAvailableItem));

            addButton.Connect("pressed", this, nameof(OnAddItem));
            clearButton.Connect("pressed", this, nameof(OnClear));
            removeButton.Connect("pressed", this, nameof(OnRemoveItem));
            itemUpButton.Connect("pressed", this, nameof(OnItemUp));
            itemDownButton.Connect("pressed", this, nameof(OnItemDown));

            nextButton.Connect("pressed", this, nameof(OnNextButtonPressed));
            prevButton.Connect("pressed", this, nameof(OnPrevButtonPressed));
            okButton.Connect("pressed", this, nameof(OnOk));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));

            Signals.MapObjectCommandedEvent += OnMapObjectCommanded;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectCommandedEvent -= OnMapObjectCommanded;
        }

        /// <summary>
        /// Update our planet queu
        /// </summary>
        void OnAboutToShow()
        {
            queuedItems.Clear();
            availableItems.Clear();
            availableItemsTree.Clear();
            queuedItemsTree.Clear();
            availableItemsTreeRoot = availableItemsTree.CreateItem();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();

            // add each design
            var availableItemIndex = 0;
            if (Planet.HasStarbase && Planet.Starbase.DockCapacity > 0)
            {
                Me.Designs.ForEach(design =>
                {
                    if (Planet.CanBuild(Me, design.Aggregate.Mass))
                    {
                        if (design.Hull.Starbase)
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.Starbase, design: design), index: availableItemIndex++);
                        }
                        else
                        {
                            AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, design: design), index: availableItemIndex++);
                        }
                    }
                });

                if (Planet.HasMassDriver)
                {
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.IroniumMineralPacket), index: availableItemIndex++);
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.BoraniumMineralPacket), index: availableItemIndex++);
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.GermaniumMineralPacket), index: availableItemIndex++);
                    AddAvailableItem(new ProductionQueueItem(QueueItemType.MixedMineralPacket), index: availableItemIndex++);
                }
            }

            if (availableItemsTree.Columns > 1)
            {
                availableItemsTree.SetColumnMinWidth(1, (int)availableItemsTree.GetFont("").GetStringSize("9999").x);
            }

            // add each type of item.
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Factory), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Mine), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Defenses), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.MineralAlchemy), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactories), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMines), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefenses), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralAlchemy), index: availableItemIndex++);
            if (Planet.HasMassDriver)
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMineralPacket), index: availableItemIndex++);
            }

            if (Planet.ProductionQueue != null)
            {
                AddTopOfQueueItem();
                Planet.ProductionQueue.Items.Each((item, index) =>
                {
                    queuedItems.Add(item);
                    AddQueuedItem(item, index);
                });
            }

            // select the top of the queue on startup
            topOfQueueItem.Select(0);

            contributesOnlyLeftoverToResearchCheckbox.Pressed = Planet.ContributesOnlyLeftoverToResearch;
        }

        void OnPopupHide()
        {
            availableItemsTree.Clear();
            queuedItemsTree.Clear();
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

        void AddAvailableItem(ProductionQueueItem item, int index)
        {
            availableItems.Add(item);
            AddTreeItem(availableItemsTree, availableItemsTreeRoot, item.FullName, item, index);
        }

        void AddQueuedItem(ProductionQueueItem item, int index)
        {
            AddTreeItem(queuedItemsTree, queuedItemsTreeRoot, item.FullName, item, index);
        }

        void UpdateQueuedItems()
        {
            queuedItemsTree.Clear();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();
            AddTopOfQueueItem();
            queuedItems.Each((item, index) =>
            {
                AddQueuedItem(item, index);
            });

            // re-select our selected item or select the top
            if (selectedQueuedItemIndex >= 0 && selectedQueuedItemIndex < queuedItems.Count)
            {
                var item = queuedItemsTreeRoot.GetChildren().GetNext();
                int i;
                for (i = 0; i < selectedQueuedItemIndex; i++)
                {
                    item = item.GetNext();
                }

                log.Debug("selecting queued item " + i);
                item.Select(0);
            }
            else
            {
                // select the top of queue
                log.Debug("selecting top of queue");
                queuedItemsTreeRoot.GetChildren().Select(0);
            }
        }

        void AddTreeItem(Tree tree, TreeItem root, string text, ProductionQueueItem item, int index = 0)
        {
            var treeItem = tree.CreateItem(root);
            treeItem.SetText(0, text);
            treeItem.SetMetadata(0, index);

            if (item.quantity != 0)
            {
                treeItem.SetText(1, $"{item.quantity}");
                treeItem.SetTextAlign(1, TreeItem.TextAlign.Right);
            }
        }

        void AddTopOfQueueItem()
        {
            topOfQueueItem = queuedItemsTree.CreateItem(queuedItemsTreeRoot);
            topOfQueueItem.SetText(0, Planet.ProductionQueue.Items.Count == 0 ? "-- Queue is Empty --" : "-- Top of the Queue --");
            topOfQueueItem.SetMetadata(0, -1);
        }

        /// <summary>
        /// Update the time it takes to complete the selected item, or clear it if we have no item
        /// </summary>
        /// <param name="selectedQueueItem"></param>
        void UpdateCompletionEstimate(ProductionQueueItem? selectedQueueItem = null)
        {
            if (selectedQueueItem != null)
            {
                ProductionQueueItem item = selectedQueueItem.Value;
                queuedItemCostGrid.Visible = true;

                // figure out how much this queue item costs
                var cost = item.GetCostOfOne(RulesManager.Rules, Me) * item.quantity;
                if (item.type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    cost = Planet.Starbase.GetUpgradeCost(item.Design);
                }
                queuedItemCostGrid.Cost = cost;
                costOfQueuedLabel.Text = $"Cost of {item.ShortName} x {item.quantity}";

                // see how much has been allocated for the top item in the queue
                var allocatedSoFar = Planet.ProductionQueue.Allocated;

                // figure out how much each previous item in the list is costing us
                Cost previousItemCost = -allocatedSoFar;
                for (int i = 0; i < selectedQueuedItemIndex; i++)
                {
                    var previousItem = queuedItems[i];
                    previousItemCost += previousItem.GetCostOfOne(RulesManager.Rules, Me) * previousItem.quantity;
                }


                // figure out how many resources we have per year
                int yearlyResources = 0;
                if (contributesOnlyLeftoverToResearchCheckbox.Pressed)
                {
                    yearlyResources = Planet.ResourcesPerYear;
                }
                else
                {
                    yearlyResources = Planet.ResourcesPerYearAvailable;
                }

                // this is how man resources and minerals our planet produces each year
                var yearlyAvailableCost = new Cost(Planet.MineralOutput, yearlyResources);

                // Get the total cost of this item plus any previous items in the queue
                // and subtract what we have on hand (that will be applied this year)
                Cost remainingCost = cost + previousItemCost - Planet.Cargo.ToCost();

                // If we have a bunch of leftover minerals because our planet is full, 0 those out
                remainingCost = new Cost(
                    Math.Max(0, remainingCost.Ironium),
                    Math.Max(0, remainingCost.Boranium),
                    Math.Max(0, remainingCost.Germanium),
                    Math.Max(0, remainingCost.Resources)
                );

                float percentComplete = selectedQueuedItemIndex == 0 && allocatedSoFar != Cost.Zero ? Mathf.Clamp(allocatedSoFar / cost, 0, 1) : 0;
                int numYearsToBuild = remainingCost == Cost.Zero ? 1 : (int)Math.Ceiling(Mathf.Clamp(remainingCost / yearlyAvailableCost, 1, int.MaxValue));

                completionEstimateLabel.Visible = true;
                completionEstimateLabel.Text = $"{(int)(percentComplete * 100)}% Done. Completion {numYearsToBuild} year{(numYearsToBuild > 1 ? "s" : "")}";
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
                Planet.ProductionQueue.Items.AddRange(queuedItems);
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


        void OnSelectQueuedItem()
        {
            selectedQueuedItemIndex = (int)queuedItemsTree.GetSelected().GetMetadata(0);
            var item = GetSelectedQueueItem();
            UpdateCompletionEstimate(item);
            if (item.HasValue)
            {
                log.Debug($"Selected item {selectedQueuedItemIndex} - {(item.HasValue ? item.Value.ToString() : "(empty)")}");
            }
        }

        void OnSelectAvailableItem()
        {
            selectedAvailableItemIndex = (int)availableItemsTree.GetSelected().GetMetadata(0);
            var item = GetSelectedAvailableItem();

            if (item != null)
            {
                var cost = item.Value.GetCostOfOne(RulesManager.Rules, Me);
                if (item.Value.type == QueueItemType.Starbase && Planet.HasStarbase)
                {
                    cost = Planet.Starbase.GetUpgradeCost(item.Value.Design);
                }
                availableItemCostGrid.Cost = cost;
            }
        }

        void OnAddItem()
        {
            var itemToAdd = GetSelectedAvailableItem();
            var selectedQueueItem = GetSelectedQueueItem();
            if (itemToAdd.HasValue && selectedQueueItem.HasValue
                && selectedQueueItem.Value.type == itemToAdd.Value.type &&
                selectedQueueItem.Value.Design == itemToAdd.Value.Design)
            {
                var newItem = selectedQueueItem.Value;
                newItem.quantity = newItem.quantity + quantityModifier;
                queuedItems[selectedQueuedItemIndex] = newItem;
                UpdateQueuedItems();
            }
            else if (itemToAdd != null)
            {
                var newItem = itemToAdd.Value;
                newItem.quantity = quantityModifier;
                if (selectedQueueItem.HasValue)
                {
                    // add below the selected item
                    if (queuedItems.Count > selectedQueuedItemIndex + 1)
                    {
                        var nextItem = queuedItems[selectedQueuedItemIndex + 1];
                        if (nextItem.type == newItem.type && nextItem.Design == newItem.Design)
                        {
                            // increase quantity
                            nextItem.quantity += quantityModifier;
                            queuedItems[selectedQueuedItemIndex + 1] = nextItem;
                        }
                        else
                        {
                            queuedItems.Insert(selectedQueuedItemIndex + 1, newItem);
                            log.Debug($"Inserted new item to at {selectedQueuedItemIndex + 1} - {newItem}");
                        }
                    }
                    else
                    {
                        queuedItems.Insert(selectedQueuedItemIndex + 1, newItem);
                        log.Debug($"Inserted new item to at {selectedQueuedItemIndex + 1} - {newItem}");
                    }
                }
                else
                {
                    queuedItems.Insert(0, newItem);
                    selectedQueuedItemIndex = 0;
                    log.Debug($"Added new item to at {selectedQueuedItemIndex} - {newItem}");
                }
                UpdateQueuedItems();
            }
        }

        void OnClear()
        {
            queuedItems.Clear();
            queuedItemsTree.Clear();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();
            AddTopOfQueueItem();
        }

        void OnRemoveItem()
        {
            var selectedQueueItem = GetSelectedQueueItem();
            if (selectedQueueItem != null)
            {
                var item = selectedQueueItem.Value;
                item.quantity -= quantityModifier;
                if (item.quantity <= 0)
                {
                    queuedItems.RemoveAt(selectedQueuedItemIndex);
                    if (selectedQueuedItemIndex > queuedItems.Count && queuedItems.Count > 0)
                    {
                        // if we removed the last item, set our index to the next last item
                        selectedQueuedItemIndex = queuedItems.Count;
                    }
                    else
                    {
                        selectedQueuedItemIndex = -1;
                    }
                }
                else
                {
                    queuedItems[selectedQueuedItemIndex] = item;
                }
                UpdateQueuedItems();
            }
        }

        void OnItemUp()
        {
            if (queuedItemsTree.GetSelected() != null)
            {
                if (selectedQueuedItemIndex > 0 && selectedQueuedItemIndex < queuedItems.Count)
                {
                    // swap items and redraw the tree
                    var selectedItem = queuedItems[selectedQueuedItemIndex];
                    var previousItem = queuedItems[selectedQueuedItemIndex - 1];
                    queuedItems[selectedQueuedItemIndex] = previousItem;
                    queuedItems[selectedQueuedItemIndex - 1] = selectedItem;
                    selectedQueuedItemIndex--;
                    UpdateQueuedItems();
                }
            }
        }

        void OnItemDown()
        {
            if (queuedItemsTree.GetSelected() != null)
            {
                if (selectedQueuedItemIndex >= 0 && selectedQueuedItemIndex < queuedItems.Count - 1)
                {
                    // swap items and redraw the tree
                    var selectedItem = queuedItems[selectedQueuedItemIndex];
                    var nextItem = queuedItems[selectedQueuedItemIndex + 1];
                    queuedItems[selectedQueuedItemIndex] = nextItem;
                    queuedItems[selectedQueuedItemIndex + 1] = selectedItem;
                    selectedQueuedItemIndex++;
                    UpdateQueuedItems();
                }
            }
        }

        ProductionQueueItem? GetSelectedQueueItem()
        {
            if (selectedQueuedItemIndex >= 0 && selectedQueuedItemIndex < queuedItems.Count)
            {
                return queuedItems[selectedQueuedItemIndex];
            }
            return null;
        }

        ProductionQueueItem? GetSelectedAvailableItem()
        {
            if (selectedAvailableItemIndex >= 0 && selectedAvailableItemIndex < availableItems.Count)
            {
                return availableItems[selectedAvailableItemIndex];
            }
            return null;
        }

        #endregion
    }
}