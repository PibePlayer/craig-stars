using Godot;
using log4net;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;
using CraigStars.Utils;

namespace CraigStars
{
    public class ProductionQueueDialog : WindowDialog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ProductionQueueDialog));

        // the planet to use in this dialog
        public Planet Planet { get; set; } = new Planet();

        public Player Me { get; set; }

        Tree queuedItemsTree;
        Tree availableItemsTree;
        TreeItem queuedItemsTreeRoot;
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

            availableItemsTree.SetColumnMinWidth(1, (int)availableItemsTree.GetFont("").GetStringSize("9999").x);

            Me = PlayersManager.Me;

            // add each design
            var availableItemIndex = 0;
            Me.Designs.ForEach(design =>
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, design: design), index: availableItemIndex++);
            });

            // add each type of item.
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Factory), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Mine), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Defense), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Alchemy), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactory), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMine), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefense), index: availableItemIndex++);
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoAlchemy), index: availableItemIndex++);

            if (Planet.ProductionQueue != null)
            {
                AddTopOfQueueItem();
                Planet.ProductionQueue.Items.Each((item, index) =>
                {
                    queuedItems.Add(item);
                    AddQueuedItem(item, index);
                });
            }

            contributesOnlyLeftoverToResearchCheckbox.Pressed = Planet.ContributesOnlyLeftoverToResearch;
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
            var treeItem = queuedItemsTree.CreateItem(queuedItemsTreeRoot);
            treeItem.SetText(0, Planet.ProductionQueue.Items.Count == 0 ? "-- Queue is Empty --" : "-- Top of the Queue --");
            treeItem.SetMetadata(0, -1);
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
                var cost = item.GetCostOfOne(RulesManager.Rules, Me.Race) * item.quantity;
                queuedItemCostGrid.Cost = cost;
                costOfQueuedLabel.Text = $"Cost of {item.ShortName} x {item.quantity}";

                // see how much has been allocated for the top item in the queue
                var allocatedSoFar = Planet.ProductionQueue.Allocated;

                // figure out how much each previous item in the list is costing us
                Cost previousItemCost = -allocatedSoFar;
                for (int i = 0; i < selectedQueuedItemIndex; i++)
                {
                    var previousItem = queuedItems[i];
                    previousItemCost += previousItem.GetCostOfOne(RulesManager.Rules, Me.Race) * previousItem.quantity;
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
            if (@event is InputEventKey key)
            {
                if (key.Pressed && key.Scancode == (uint)KeyList.Shift)
                {
                    quantityModifier *= 10;
                }
                else if (key.Pressed && key.Scancode == (uint)KeyList.Control)
                {
                    quantityModifier *= 100;
                }
                else
                {
                    quantityModifier = 1;
                }
            }
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
                availableItemCostGrid.Cost = item.Value.GetCostOfOne(RulesManager.Rules, Me.Race);
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
                    queuedItems.Insert(selectedQueuedItemIndex + 1, newItem);
                    log.Debug($"Inserted new item to at {selectedQueuedItemIndex + 1} - {newItem}");
                }
                else
                {
                    queuedItems.Add(newItem);
                    selectedQueuedItemIndex = queuedItems.Count - 1;
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