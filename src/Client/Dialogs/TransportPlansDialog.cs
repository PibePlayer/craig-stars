using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars.Client
{

    public class TransportPlansDialog : GameViewDialog
    {
        ItemList transportPlansItemList;
        TransportPlanDetail transportPlanDetail;
        Label detailPlanNameLabel;

        Button okButton;
        Button newButton;
        Button deleteButton;

        List<TransportPlan> transportPlans = new List<TransportPlan>();
        List<TransportPlan> deletedPlans = new List<TransportPlan>();
        TransportPlan selectedPlan;


        public override void _Ready()
        {
            base._Ready();
            transportPlansItemList = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerList/TransportPlansItemList");
            transportPlanDetail = GetNode<TransportPlanDetail>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/TransportPlanDetail");
            detailPlanNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/DetailPlanNameLabel");

            okButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer2/OKButton");
            newButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer/NewButton");
            deleteButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer/DeleteButton");

            transportPlansItemList.Connect("item_selected", this, nameof(OnTransportPlanSelected));

            okButton.Connect("pressed", this, nameof(OnOk));
            newButton.Connect("pressed", this, nameof(OnNewButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));


            // uncomment to test in scene
            // PlayersManager.Instance.SetupPlayers();
            // Me.TransportPlans.Add(new TransportPlan("Default"));
            // Me.TransportPlans.Add(new TransportPlan("Load Colonists")
            // {
            //     Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.LoadAll))
            // });
            // Me.TransportPlans.Add(new TransportPlan("Unload Colonists")
            // {
            //     Tasks = new WaypointTransportTasks(colonists: new WaypointTransportTask(WaypointTaskTransportAction.UnloadAll))
            // });
            // Show();
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        protected override void OnVisibilityChanged()
        {
            if (IsVisibleInTree())
            {
                deletedPlans.Clear();
                transportPlans.Clear();
                Me.TransportPlans.ForEach(plan => transportPlans.Add(plan.Clone()));
                UpdateTransportPlansItemList();
                OnTransportPlanSelected(0);
            }
        }

        /// <summary>
        /// Save the changes back to the user
        /// </summary>
        void OnOk()
        {
            foreach (var plan in transportPlans)
            {
                if (Me.TransportPlansByGuid.TryGetValue(plan.Guid, out var existingPlan))
                {
                    if (existingPlan != Me.TransportPlans[0])
                    {
                        // plan 0 is always
                        existingPlan.Name = plan.Name;
                    }
                    // update the existing plan
                    existingPlan.Tasks = plan.Tasks;
                }
                else
                {
                    // add a new plan
                    Me.TransportPlans.Add(plan);
                    Me.TransportPlansByGuid[plan.Guid] = plan;
                }
            }

            foreach (var plan in deletedPlans)
            {
                if (Me.TransportPlansByGuid.TryGetValue(plan.Guid, out var existingPlan) && existingPlan != Me.TransportPlans[0])
                {
                    Me.TransportPlans.Remove(existingPlan);
                    Me.TransportPlansByGuid.Remove(existingPlan.Guid);
                }
            }

            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();

            Hide();
        }

        void OnNewButtonPressed()
        {
            transportPlans.Add(new TransportPlan($"Battle Plan {transportPlans.Count + 1}"));
            UpdateTransportPlansItemList();
        }

        /// <summary>
        /// Delete the currently selected battle plan
        /// TODO: Warn about in use battle plans
        /// </summary>
        void OnDeleteButtonPressed()
        {
            if (selectedPlan != null && selectedPlan != transportPlans[0])
            {
                deletedPlans.Add(selectedPlan);
                transportPlans.Remove(selectedPlan);
                UpdateTransportPlansItemList();
                OnTransportPlanSelected(0);
            }
        }

        void UpdateTransportPlansItemList()
        {
            transportPlansItemList.Clear();
            transportPlans.Each((plan, index) =>
            {
                transportPlansItemList.AddItem(plan.Name);
                if (index == 0)
                {
                    transportPlansItemList.SetItemCustomBgColor(0, Colors.DarkBlue);
                }
            });
        }

        void OnTransportPlanSelected(int index)
        {
            deleteButton.Disabled = false;
            transportPlanDetail.ShowName = true;

            if (index >= 0 && index < transportPlans.Count)
            {
                if (index == 0)
                {
                    // The default can't be deleted or have its name changed
                    deleteButton.Disabled = true;
                    transportPlanDetail.ShowName = false;
                }
                selectedPlan = transportPlans[index];
                detailPlanNameLabel.Text = selectedPlan.Name;
                transportPlanDetail.Plan = selectedPlan;
            }
        }

    }
}