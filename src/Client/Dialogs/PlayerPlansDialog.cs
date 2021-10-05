using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Client
{
    /// <summary>
    /// A generic list of plans like transport, battle, or production plans
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PlayerPlansDialog<T> : GameViewDialog where T : PlayerPlan<T>, new()
    {
        /// <summary>
        /// Override to return the player's source plans, i.e. Me.BattlePlans or Me.TransportPlans
        /// </summary>
        /// <value></value>
        protected abstract List<T> SourcePlans { get; }

        /// <summary>
        /// Override to update fields when a new plan is selected
        /// </summary>
        protected abstract void OnPlanSelected(T newPlan, T previousPlan);

        protected ItemList itemList;
        protected LineEdit nameLineEdit;
        Label detailPlanNameLabel;

        Button newButton;
        Button deleteButton;

        protected List<T> plans = new();
        protected List<T> deletedPlans = new();
        protected T selectedPlan;

        public override void _Ready()
        {
            base._Ready();

            itemList = GetNode<ItemList>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerList/ItemList");
            detailPlanNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/DetailPlanNameLabel");
            nameLineEdit = GetNode<LineEdit>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/GridContainer/NameLineEdit");

            newButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/PlansButtonsContainer/NewButton");
            deleteButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/PlansButtonsContainer/DeleteButton");

            itemList.Connect("item_selected", this, nameof(OnPlanSelected));
            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));

            newButton.Connect("pressed", this, nameof(OnNewButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

        }


        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (IsVisibleInTree())
            {
                deletedPlans.Clear();
                plans.Clear();
                SourcePlans.ForEach(plan => plans.Add(plan.Clone()));
                UpdateBattlePlansItemList();
                OnPlanSelected(0);
            }
        }

        void OnNewButtonPressed()
        {
            // dirty string manipulation...
            // TODO: fix this for localization
            plans.Add(new T() { Name = $"{typeof(T).Name.Replace("Plan", " Plan")} {plans.Count + 1}" });
            UpdateBattlePlansItemList();
        }

        /// <summary>
        /// Delete the currently selected battle plan
        /// TODO: Warn about in use battle plans
        /// </summary>
        void OnDeleteButtonPressed()
        {
            if (selectedPlan != null && selectedPlan != plans[0])
            {
                deletedPlans.Add(selectedPlan);
                plans.Remove(selectedPlan);
                UpdateBattlePlansItemList();
                OnPlanSelected(0);
            }
        }

        protected void OnNameLineEditTextChanged(string newText)
        {
            if (selectedPlan != null && selectedPlan != plans[0])
            {
                selectedPlan.Name = newText;
                itemList.SetItemText(itemList.GetSelectedItems()[0], newText);
            }
        }

        void UpdateBattlePlansItemList()
        {
            itemList.Clear();
            plans.Each((plan, index) =>
            {
                itemList.AddItem(plan.Name);
                if (index == 0)
                {
                    itemList.SetItemCustomBgColor(0, Colors.DarkBlue);
                }
            });
        }

        void OnPlanSelected(int index)
        {
            deleteButton.Disabled = true;

            if (index >= 0 && index < plans.Count)
            {
                if (index != 0)
                {
                    deleteButton.Disabled = false;
                }
                var newPlan = plans[index];
                var previousPlan = selectedPlan;
                selectedPlan = newPlan;
                detailPlanNameLabel.Text = selectedPlan.Name;
                nameLineEdit.Text = selectedPlan.Name;
                nameLineEdit.Editable = index != 0;
                
                OnPlanSelected(newPlan, previousPlan);
            }
        }


    }
}