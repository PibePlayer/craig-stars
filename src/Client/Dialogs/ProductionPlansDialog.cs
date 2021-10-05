using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class ProductionPlansDialog : PlayerPlansDialog<ProductionPlan>
    {
        protected override List<ProductionPlan> SourcePlans => Me.ProductionPlans;

        AvailableProductionPlanItems availableItems;
        ProductionPlanItems queuedItems;

        Button addButton;
        Button removeButton;
        Button itemUpButton;
        Button itemDownButton;
        Button clearButton;
        Button prevButton;
        Button nextButton;
        CheckBox contributesOnlyLeftoverToResearchCheckbox;

        int quantityModifier = 1;

        public override void _Ready()
        {
            base._Ready();

            availableItems = (AvailableProductionPlanItems)FindNode("AvailableProductionPlanItems");
            queuedItems = (ProductionPlanItems)FindNode("ProductionPlanItems");

            addButton = (Button)FindNode("AddButton");
            removeButton = (Button)FindNode("RemoveButton");
            itemUpButton = (Button)FindNode("ItemUpButton");
            itemDownButton = (Button)FindNode("ItemDownButton");
            clearButton = (Button)FindNode("ClearButton");
            prevButton = (Button)FindNode("PrevButton");
            nextButton = (Button)FindNode("NextButton");
            contributesOnlyLeftoverToResearchCheckbox = (CheckBox)FindNode("ContributesOnlyLeftoverToResearchCheckbox");

            addButton.Connect("pressed", this, nameof(OnAddItem));
            clearButton.Connect("pressed", this, nameof(OnClear));
            removeButton.Connect("pressed", this, nameof(OnRemoveItem));
            itemUpButton.Connect("pressed", this, nameof(OnItemUp));
            itemDownButton.Connect("pressed", this, nameof(OnItemDown));
            contributesOnlyLeftoverToResearchCheckbox.Connect("pressed", this, nameof(OnContributesOnlyLeftoverToResearchCheckboxPressed));

            availableItems.ItemActivatedEvent += OnAddItem;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                availableItems.ItemActivatedEvent -= OnAddItem;
            }
        }

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
        /// Save the changes back to the user
        /// </summary>
        protected override void OnOk()
        {
            // update the selected plan
            if (selectedPlan != null)
            {
                selectedPlan.Items = queuedItems.Items.Select(item => item.Clone()).ToList();
                selectedPlan.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
            }

            foreach (var plan in plans)
            {
                if (Me.ProductionPlansByGuid.TryGetValue(plan.Guid, out var existingPlan))
                {
                    // plan 0 is always default
                    if (existingPlan != Me.ProductionPlans[0])
                    {
                        existingPlan.Name = plan.Name;
                    }
                    // update the existing plan
                    existingPlan.Items = plan.Items.Select(item => item.Clone()).ToList();
                    existingPlan.ContributesOnlyLeftoverToResearch = plan.ContributesOnlyLeftoverToResearch;
                }
                else
                {
                    // add a new plan
                    Me.ProductionPlans.Add(plan);
                    Me.ProductionPlansByGuid[plan.Guid] = plan;
                }
            }

            foreach (var plan in deletedPlans)
            {
                if (Me.ProductionPlansByGuid.TryGetValue(plan.Guid, out var existingPlan) && existingPlan != Me.ProductionPlans[0])
                {
                    Me.ProductionPlans.Remove(existingPlan);
                    Me.ProductionPlansByGuid.Remove(existingPlan.Guid);
                }
            }

            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();

            Hide();
        }


        void OnContributesOnlyLeftoverToResearchCheckboxPressed()
        {
            selectedPlan.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
        }

        protected override void OnPlanSelected(ProductionPlan newPlan, ProductionPlan previousPlan)
        {
            if (previousPlan != null)
            {
                // save state to the previous plan
                previousPlan.Items = queuedItems.Items.Select(item => item.Clone()).ToList();
                previousPlan.ContributesOnlyLeftoverToResearch = contributesOnlyLeftoverToResearchCheckbox.Pressed;
            }
            // update the ui with the new plan
            queuedItems.Items = newPlan.Items.Select(item => item.Clone()).ToList();
            contributesOnlyLeftoverToResearchCheckbox.Pressed = newPlan.ContributesOnlyLeftoverToResearch;

            var _ = queuedItems.UpdateItems();
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
    }
}