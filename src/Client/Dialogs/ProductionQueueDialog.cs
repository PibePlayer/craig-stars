using Godot;
using log4net;
using System;
using System.Collections.Generic;

using CraigStars.Singletons;

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

            availableItemCostGrid = FindNode("AvailableItemCostGrid") as CostGrid;
            queuedItemCostGrid = FindNode("QueuedItemCostGrid") as CostGrid;
            completionEstimateLabel = FindNode("CompletionEstimateLabel") as Label;

            queuedItemsTree.SetColumnExpand(0, true);
            queuedItemsTree.SetColumnExpand(1, false);
            queuedItemsTree.SetColumnMinWidth(1, 60);

            availableItemsTree.SetColumnExpand(0, true);
            availableItemsTree.Connect("item_activated", this, nameof(OnAddItem));
            addButton.Connect("pressed", this, nameof(OnAddItem));
            clearButton.Connect("pressed", this, nameof(OnClear));
            removeButton.Connect("pressed", this, nameof(OnRemoveItem));
            itemUpButton.Connect("pressed", this, nameof(OnItemUp));
            itemDownButton.Connect("pressed", this, nameof(OnItemDown));
            okButton.Connect("pressed", this, nameof(OnOk));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
        }

        void OnPopupHide()
        {
            availableItemsTree.Clear();
            queuedItemsTree.Clear();

            availableItemsTree.Disconnect("item_selected", this, nameof(OnAvailableItemSelected));
            queuedItemsTree.Disconnect("item_selected", this, nameof(OnQueuedItemSelected));
        }

        /// <summary>
        /// Update our planet queu
        /// </summary>
        void OnAboutToShow()
        {
            availableItemsTree.Clear();
            queuedItemsTree.Clear();
            availableItemsTreeRoot = availableItemsTree.CreateItem();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();

            Me = PlayersManager.Me;

            // add each design
            Me.Designs.ForEach(design =>
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, design: design));
            });

            // add each type of item.
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Factory));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Mine));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Defense));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.Alchemy));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoFactory));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoMine));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoDefense));
            AddAvailableItem(new ProductionQueueItem(QueueItemType.AutoAlchemy));

            if (Planet.ProductionQueue != null)
            {
                AddTopOfQueueItem();
                Planet.ProductionQueue.Items.ForEach(item =>
                {
                    AddQueuedItem(item);
                });
            }

            contributesOnlyLeftoverToResearchCheckbox.Pressed = Planet.ContributesOnlyLeftoverToResearch;

            availableItemsTree.Connect("item_selected", this, nameof(OnAvailableItemSelected));
            queuedItemsTree.Connect("item_selected", this, nameof(OnQueuedItemSelected));
        }

        void AddAvailableItem(ProductionQueueItem item)
        {
            AddTreeItem(availableItemsTree, availableItemsTreeRoot, item.FullName, item);
        }

        void AddQueuedItem(ProductionQueueItem item)
        {
            AddTreeItem(queuedItemsTree, queuedItemsTreeRoot, item.FullName, item);
        }

        void AddTreeItem(Tree tree, TreeItem root, string text, ProductionQueueItem item)
        {
            var treeItem = tree.CreateItem(root);
            treeItem.SetText(0, text);
            treeItem.SetMetadata(0, Serializers.Serialize(item, PlayersManager.Instance.Players, TechStore.Instance));
            tree.SetColumnMinWidth(1, (int)tree.GetFont("").GetStringSize("1000").x);

            if (item.quantity != 0)
            {
                treeItem.SetText(1, $"{item.quantity}");
                treeItem.SetTextAlign(1, TreeItem.TextAlign.Right);
            }
            treeItem.Select(0);
        }

        void AddTopOfQueueItem()
        {
            var treeItem = queuedItemsTree.CreateItem(queuedItemsTreeRoot);
            treeItem.SetText(0, Planet.ProductionQueue.Items.Count == 0 ? "-- Queue is Empty --" : "-- Top of the Queue --");
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
                completionEstimateLabel.Visible = false;

                // figure out how much this queue item costs
                var cost = item.GetCostOfOne(RulesManager.Rules, Me.Race) * item.quantity;
                queuedItemCostGrid.Cost = cost;

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

                var allocatedSoFar = Planet.ProductionQueue.Allocated;
                var availableCost = Planet.Cargo.ToCost(yearlyResources);
                var yearlyMinerals = Planet.MineralOutput;

                log.Debug($"Cost: {cost} Allocated: {allocatedSoFar}, available: {availableCost}, yearlyMinerals: {yearlyMinerals}");

                var remaining = cost - allocatedSoFar - availableCost - yearlyMinerals;

                // TODO: figure this algorithm out...
                log.Debug($"After year passed: {remaining}");
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
            if (Planet.ProductionQueue != null)
            {
                Planet.ProductionQueue.Items.Clear();
                var child = queuedItemsTreeRoot.GetChildren();
                while (child != null)
                {
                    if (Serializers.Deserialize<ProductionQueueItem>(child.GetMetadata(0).ToString(), PlayersManager.Instance.Players, TechStore.Instance) is ProductionQueueItem item)
                    {
                        Planet.ProductionQueue.Items.Add(item);
                    }
                    child = child.GetNext();
                }
            }
            else
            {
                log.Error($"ProductionQueue for planet {Planet.Name} is null");
            }
            Planet.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;

            Signals.PublishProductionQueueChangedEvent(Planet);
            Hide();
        }

        void OnAddItem()
        {
            var itemToAdd = Serializers.Deserialize<ProductionQueueItem>(availableItemsTree.GetSelected()?.GetMetadata(0).ToString(), PlayersManager.Instance.Players, TechStore.Instance);
            var selectedQueueItem = Serializers.Deserialize<ProductionQueueItem>(queuedItemsTree.GetSelected()?.GetMetadata(0).ToString(), PlayersManager.Instance.Players, TechStore.Instance);

            if (itemToAdd != null && selectedQueueItem != null && selectedQueueItem?.type == itemToAdd?.type)
            {
                var selectedItem = selectedQueueItem.Value;
                selectedItem.quantity += quantityModifier;
                queuedItemsTree.GetSelected().SetText(1, selectedItem.quantity.ToString());
                queuedItemsTree.GetSelected().SetMetadata(0, Serializers.Serialize(selectedItem, PlayersManager.Instance.Players, TechStore.Instance));
            }
            else if (itemToAdd != null)
            {
                var queuedItem = new ProductionQueueItem(itemToAdd.Value.type, quantityModifier, itemToAdd.Value.Design);
                AddQueuedItem(queuedItem);
            }
        }

        void OnClear()
        {
            queuedItemsTree.Clear();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();
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
                    queuedItemsTree.GetSelected().Free();
                }
                else
                {
                    // save the item back to our list and upate the tree
                    queuedItemsTree.GetSelected().SetText(1, item.quantity.ToString());
                    queuedItemsTree.GetSelected().SetMetadata(0, Serializers.Serialize(item, PlayersManager.Instance.Players, TechStore.Instance));
                }
                // force a UI update: https://github.com/godotengine/godot/issues/38787
                queuedItemsTree.HideRoot = queuedItemsTree.HideRoot;
            }
        }

        void OnItemUp()
        {
        }

        void OnItemDown()
        {

        }

        void OnAvailableItemSelected()
        {
            var item = GetSelectedAvailableItem();

            if (item != null)
            {
                availableItemCostGrid.Cost = item.Value.GetCostOfOne(RulesManager.Rules, Me.Race);
            }
        }

        void OnQueuedItemSelected()
        {
            UpdateCompletionEstimate(GetSelectedQueueItem());
        }

        ProductionQueueItem? GetSelectedQueueItem()
        {
            var selectedItem = queuedItemsTree.GetSelected();
            if (selectedItem != null && selectedItem.GetMetadata(0) != null)
            {
                return Serializers.Deserialize<ProductionQueueItem>(selectedItem.GetMetadata(0).ToString(), PlayersManager.Instance.Players, TechStore.Instance);
            }
            return null;
        }

        ProductionQueueItem? GetSelectedAvailableItem()
        {
            var selectedItem = availableItemsTree.GetSelected();
            if (selectedItem != null)
            {
                return Serializers.Deserialize<ProductionQueueItem>(selectedItem.GetMetadata(0).ToString(), PlayersManager.Instance.Players, TechStore.Instance);
            }
            return null;
        }

        #endregion
    }
}