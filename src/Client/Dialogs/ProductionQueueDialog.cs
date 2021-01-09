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
        Button clearButton;
        Button prevButton;
        Button nextButton;
        Button okButton;
        CheckBox contributesToResearchCheckbox;

        CostGrid availableItemCostGrid;
        CostGrid queuedItemCostGrid;

        List<ProductionQueueItem> availableItems = new List<ProductionQueueItem>();
        List<ProductionQueueItem> queuedItems = new List<ProductionQueueItem>();

        public override void _Ready()
        {
            queuedItemsTree = FindNode("QueuedItemsTree") as Tree;
            availableItemsTree = FindNode("AvailableItemsTree") as Tree;

            addButton = FindNode("AddButton") as Button;
            removeButton = FindNode("RemoveButton") as Button;
            clearButton = FindNode("ClearButton") as Button;
            prevButton = FindNode("PrevButton") as Button;
            nextButton = FindNode("NextButton") as Button;
            okButton = FindNode("OKButton") as Button;
            contributesToResearchCheckbox = FindNode("ContributesToResearchCheckbox") as CheckBox;

            availableItemCostGrid = FindNode("AvailableItemCostGrid") as CostGrid;
            queuedItemCostGrid = FindNode("QueuedItemCostGrid") as CostGrid;

            queuedItemsTree.SetColumnExpand(0, true);
            queuedItemsTree.SetColumnExpand(1, false);
            queuedItemsTree.SetColumnMinWidth(1, 60);

            availableItemsTree.SetColumnExpand(0, true);
            Connect("about_to_show", this, nameof(AboutToShow));
        }

        /// <summary>
        /// Update our planet queu
        /// </summary>
        void AboutToShow()
        {
            availableItemsTree.Clear();
            queuedItemsTree.Clear();
            availableItemsTreeRoot = availableItemsTree.CreateItem();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();

            Me = PlayersManager.Instance.Me;

            // add each design
            Me.Designs.ForEach(design =>
            {
                AddAvailableItem(new ProductionQueueItem(QueueItemType.ShipToken, 1, design));
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

            Planet.ProductionQueue.Items.ForEach(item =>
            {
                AddQueuedItem(item);
            });

            availableItemsTree.Connect("item_selected", this, nameof(OnAvailableItemSelected));
            queuedItemsTree.Connect("item_selected", this, nameof(OnQueuedItemSelected));
            // PopupCentered();
        }

        void AddAvailableItem(ProductionQueueItem item)
        {
            var index = availableItems.Count;
            availableItems.Add(item);
            AddTreeItem(availableItemsTree, availableItemsTreeRoot, item.FullName, index);
        }

        void AddQueuedItem(ProductionQueueItem item)
        {
            var index = queuedItems.Count;
            queuedItems.Add(item);
            AddTreeItem(queuedItemsTree, queuedItemsTreeRoot, item.FullName, index, item.Quantity);
        }

        void AddTreeItem(Tree tree, TreeItem root, String text, int index, int quantity = 0)
        {
            var item = tree.CreateItem(root);
            item.SetText(0, text);
            item.SetMetadata(0, index);
            if (quantity != 0)
            {
                item.SetText(1, $"{quantity}");
                item.SetTextAlign(1, TreeItem.TextAlign.Right);
            }
        }

        void OnAvailableItemSelected()
        {
            var index = (int)availableItemsTree.GetSelected().GetMetadata(0);
            var item = availableItems[index];

            availableItemCostGrid.Cost = item.GetCostOfOne(SettingsManager.Settings, Me.Race);
        }

        void OnQueuedItemSelected()
        {
            var index = (int)queuedItemsTree.GetSelected().GetMetadata(0);
            var item = queuedItems[index];
            queuedItemCostGrid.Cost = item.GetCostOfOne(SettingsManager.Settings, Me.Race) * item.Quantity;
        }
    }
}