using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class BattlePlansDialog : PlayerPlansDialog<BattlePlan>
    {
        OptionButton primaryTargetOptionButton;
        OptionButton secondaryTargetOptionButton;
        OptionButton tacticOptionButton;
        OptionButton attackWhoOptionButton;
        CheckBox dumpCargoCheckBox;

        public override void _Ready()
        {
            // uncomment to test in scene
            // PlayersManager.Me = new Player();
            // Me.BattlePlans.Add(new BattlePlan("Default"));
            // Me.BattlePlans.Add(new BattlePlan("Sniper")
            // {
            //     AttackWho = BattleAttackWho.Everyone,
            //     PrimaryTarget = BattleTargetType.FuelTransports,
            //     SecondaryTarget = BattleTargetType.Freighters,
            //     Tactic = BattleTactic.DisengageIfChallenged,
            // });
            // CallDeferred(nameof(TestShow));

            base._Ready();
            primaryTargetOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/GridContainer/PrimaryTargetOptionButton");
            secondaryTargetOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/GridContainer/SecondaryTargetOptionButton");
            tacticOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/GridContainer/TacticOptionButton");
            attackWhoOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/GridContainer/AttackWhoOptionButton");
            dumpCargoCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ContentContainer/HBoxContainer/VBoxContainerDetail/DumpCargoCheckBox");

            // populate option buttons
            primaryTargetOptionButton.PopulateOptionButton<BattleTargetType>((type) => EnumUtils.GetLabelForBattleTargetType(type));
            secondaryTargetOptionButton.PopulateOptionButton<BattleTargetType>((type) => EnumUtils.GetLabelForBattleTargetType(type));
            tacticOptionButton.PopulateOptionButton<BattleTactic>((tactic) => EnumUtils.GetLabelForBattleTactic(tactic));
            attackWhoOptionButton.PopulateOptionButton<BattleAttackWho>((attackWho) => EnumUtils.GetLabelForBattleAttackWho(attackWho));

            // connect up option button events
            primaryTargetOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            secondaryTargetOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            tacticOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            attackWhoOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            dumpCargoCheckBox.Connect("toggled", this, nameof(OnDumpCargoCheckBoxToggled));

        }

        // void TestShow()
        // {
        //     Show();
        // }

        protected override List<BattlePlan> SourcePlans { get => Me.BattlePlans; }

        protected override void OnPlanSelected(BattlePlan newPlan, BattlePlan previousPlan)
        {
            primaryTargetOptionButton.Select((int)newPlan.PrimaryTarget);
            secondaryTargetOptionButton.Select((int)newPlan.SecondaryTarget);
            tacticOptionButton.Select((int)newPlan.Tactic);
            attackWhoOptionButton.Select((int)newPlan.AttackWho);
            dumpCargoCheckBox.Pressed = newPlan.DumpCargo;
        }

        void OnOptionButtonItemSelected(int index)
        {
            if (selectedPlan != null)
            {
                selectedPlan.PrimaryTarget = (BattleTargetType)primaryTargetOptionButton.Selected;
                selectedPlan.SecondaryTarget = (BattleTargetType)secondaryTargetOptionButton.Selected;
                selectedPlan.Tactic = (BattleTactic)tacticOptionButton.Selected;
                selectedPlan.AttackWho = (BattleAttackWho)attackWhoOptionButton.Selected;
            }
        }


        /// <summary>
        /// Save the changes back to the user
        /// </summary>
        protected override void OnOk()
        {
            foreach (var plan in plans)
            {
                if (Me.BattlePlansByGuid.TryGetValue(plan.Guid, out var existingPlan))
                {
                    if (existingPlan != Me.BattlePlans[0])
                    {
                        // plan 0 is always
                        existingPlan.Name = plan.Name;
                    }
                    // update the existing plan
                    existingPlan.PrimaryTarget = plan.PrimaryTarget;
                    existingPlan.SecondaryTarget = plan.SecondaryTarget;
                    existingPlan.Tactic = plan.Tactic;
                    existingPlan.AttackWho = plan.AttackWho;
                    existingPlan.DumpCargo = plan.DumpCargo;
                }
                else
                {
                    // add a new plan
                    Me.BattlePlans.Add(plan);
                    Me.BattlePlansByGuid[plan.Guid] = plan;
                }
            }

            foreach (var plan in deletedPlans)
            {
                if (Me.BattlePlansByGuid.TryGetValue(plan.Guid, out var existingPlan) && existingPlan != Me.BattlePlans[0])
                {
                    Me.BattlePlans.Remove(existingPlan);
                    Me.BattlePlansByGuid.Remove(existingPlan.Guid);
                    Me.Fleets.ForEach(fleet =>
                    {
                        // assign to the default if we delete this plan
                        if (fleet.BattlePlan == existingPlan)
                        {
                            fleet.BattlePlan = Me.BattlePlans[0];
                        }
                    });
                }
            }

            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();

            Hide();
        }

        void OnDumpCargoCheckBoxToggled(bool toggled)
        {
            if (selectedPlan != null)
            {
                selectedPlan.DumpCargo = toggled;
            }
        }
    }
}