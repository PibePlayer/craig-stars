using System;
using System.Collections.Generic;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{

    public class TransportPlansDialog : PlayerPlansDialog<TransportPlan>
    {
        TransportPlanDetail transportPlanDetail;

        protected override List<TransportPlan> SourcePlans => Me.TransportPlans;

        public override void _Ready()
        {
            base._Ready();
            transportPlanDetail = GetNode<TransportPlanDetail>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/TransportPlanDetail");
            transportPlanDetail.NameChangedEvent += OnNameChanged;

            // uncomment to test in scene
            // PlayersManager.Me = new Player();
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

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                transportPlanDetail.NameChangedEvent -= OnNameChanged;
            }
        }

        private void OnNameChanged(string newText)
        {
            // simulate a regular name change event
            OnNameLineEditTextChanged(newText);
        }

        /// <summary>
        /// Save the changes back to the user
        /// </summary>
        protected override void OnOk()
        {
            foreach (var plan in plans)
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


        protected override void OnPlanSelected(TransportPlan newPlan, TransportPlan previousPlan)
        {
            transportPlanDetail.Plan = newPlan;
            // the default plan cannot change names
            transportPlanDetail.NameEditable = newPlan.Guid != Me.TransportPlans[0].Guid;
        }
    }
}