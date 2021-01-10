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
        ProductionQueueItem selectedQueueItem;

        int quantityModifier = 1;

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
            contributesToResearchCheckbox = FindNode("ContributesOnlyLeftoverToResearchCheckbox") as CheckBox;

            availableItemCostGrid = FindNode("AvailableItemCostGrid") as CostGrid;
            queuedItemCostGrid = FindNode("QueuedItemCostGrid") as CostGrid;

            queuedItemsTree.SetColumnExpand(0, true);
            queuedItemsTree.SetColumnExpand(1, false);
            queuedItemsTree.SetColumnMinWidth(1, 60);

            availableItemsTree.SetColumnExpand(0, true);
            availableItemsTree.Connect("item_activated", this, nameof(OnAddItem));
            addButton.Connect("pressed", this, nameof(OnAddItem));
            clearButton.Connect("pressed", this, nameof(OnClear));
            removeButton.Connect("pressed", this, nameof(OnRemoveItem));
            okButton.Connect("pressed", this, nameof(OnOk));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
        }

        void OnPopupHide()
        {
            availableItems.Clear();
            queuedItems.Clear();
            availableItemsTree.Clear();
            queuedItemsTree.Clear();
            selectedQueueItem = null;

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
                AddQueuedItem(item.Clone());
            });

            contributesToResearchCheckbox.Pressed = Planet.ContributesOnlyLeftoverToResearch;

            availableItemsTree.Connect("item_selected", this, nameof(OnAvailableItemSelected));
            queuedItemsTree.Connect("item_selected", this, nameof(OnQueuedItemSelected));
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
            selectedQueueItem = item;
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
            item.Select(0);
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
            Planet.ProductionQueue.Items.Clear();
            Planet.ProductionQueue.Items.AddRange(queuedItems);
            Planet.ContributesOnlyLeftoverToResearch = contributesToResearchCheckbox.Pressed;

            Signals.PublishProductionQueueChangedEvent(Planet);
            Hide();
        }

        void OnAddItem()
        {
            var index = (int)availableItemsTree.GetSelected().GetMetadata(0);
            var item = availableItems[index];

            if (selectedQueueItem?.Type == item.Type)
            {
                selectedQueueItem.Quantity += quantityModifier;
                queuedItemsTree.GetSelected().SetText(1, selectedQueueItem.Quantity.ToString());
            }
            else
            {
                var queuedItem = new ProductionQueueItem(item.Type, quantityModifier, item.Design);
                AddQueuedItem(queuedItem);
            }
        }

        void OnClear()
        {
            selectedQueueItem = null;
            queuedItems.Clear();
            queuedItemsTree.Clear();
            queuedItemsTreeRoot = queuedItemsTree.CreateItem();
        }

        void OnRemoveItem()
        {
            if (queuedItemsTree.GetSelected() != null)
            {
                // Reduce the quantity or remove the selected item.
                var index = (int)queuedItemsTree.GetSelected().GetMetadata(0);
                var item = queuedItems[index];

                item.Quantity -= quantityModifier;
                if (item.Quantity <= 0)
                {
                    queuedItemsTree.GetSelected().Free();
                    selectedQueueItem = null;
                    queuedItems.RemoveAt(index);
                    var queuedItem = queuedItemsTreeRoot.GetNext();
                    int newIndex = 0;
                    while (queuedItem != null)
                    {
                        queuedItem.SetMetadata(0, newIndex++);
                    }
                }
                else
                {
                    queuedItemsTree.GetSelected().SetText(1, item.Quantity.ToString());
                }
                // force a UI update: https://github.com/godotengine/godot/issues/38787
                queuedItemsTree.HideRoot = queuedItemsTree.HideRoot;
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
            selectedQueueItem = item;
            queuedItemCostGrid.Cost = item.GetCostOfOne(SettingsManager.Settings, Me.Race) * item.Quantity;
        }

        #endregion
    }
}