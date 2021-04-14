using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class BattlePlansDialog : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }

        ItemList battlePlansItemList;
        LineEdit nameLineEdit;
        Label detailPlanNameLabel;
        OptionButton primaryTargetOptionButton;
        OptionButton secondaryTargetOptionButton;
        OptionButton tacticOptionButton;
        OptionButton attackWhoOptionButton;
        CheckBox dumpCargoCheckBox;

        Button okButton;
        Button newButton;
        Button deleteButton;

        List<BattlePlan> battlePlans = new List<BattlePlan>();
        List<BattlePlan> deletedPlans = new List<BattlePlan>();
        BattlePlan selectedPlan;

        public override void _Ready()
        {

            battlePlansItemList = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerList/BattlePlansItemList");
            detailPlanNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/DetailPlanNameLabel");
            nameLineEdit = GetNode<LineEdit>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/GridContainer/NameLineEdit");
            primaryTargetOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/GridContainer/PrimaryTargetOptionButton");
            secondaryTargetOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/GridContainer/SecondaryTargetOptionButton");
            tacticOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/GridContainer/TacticOptionButton");
            attackWhoOptionButton = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/GridContainer/AttackWhoOptionButton");
            dumpCargoCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerDetail/DumpCargoCheckBox");

            okButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer2/OKButton");
            newButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer/NewButton");
            deleteButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/HBoxContainer/DeleteButton");

            primaryTargetOptionButton.PopulateOptionButton<BattleTargetType>((type) => EnumUtils.GetLabelForBattleTargetType(type));
            secondaryTargetOptionButton.PopulateOptionButton<BattleTargetType>((type) => EnumUtils.GetLabelForBattleTargetType(type));
            tacticOptionButton.PopulateOptionButton<BattleTactic>((tactic) => EnumUtils.GetLabelForBattleTactic(tactic));
            attackWhoOptionButton.PopulateOptionButton<BattleAttackWho>((attackWho) => EnumUtils.GetLabelForBattleAttackWho(attackWho));

            battlePlansItemList.Connect("item_selected", this, nameof(OnBattlePlanSelected));

            primaryTargetOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            secondaryTargetOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            tacticOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            attackWhoOptionButton.Connect("item_selected", this, nameof(OnOptionButtonItemSelected));
            nameLineEdit.Connect("text_changed", this, nameof(OnNameLineEditTextChanged));
            dumpCargoCheckBox.Connect("toggled", this, nameof(OnDumpCargoCheckBoxToggled));

            okButton.Connect("pressed", this, nameof(OnOk));
            newButton.Connect("pressed", this, nameof(OnNewButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            // uncomment to test in scene
            // PlayersManager.Instance.SetupPlayers();
            // Me.BattlePlans.Add(new BattlePlan("Default"));
            // Me.BattlePlans.Add(new BattlePlan("Sniper")
            // {
            //     AttackWho = BattleAttackWho.Everyone,
            //     PrimaryTarget = BattleTargetType.FuelTransports,
            //     SecondaryTarget = BattleTargetType.Freighters,
            //     Tactic = BattleTactic.DisengageIfChallenged,
            // });
            // Show();
        }

        /// <summary>
        /// When the dialog becomes visible, update the controls for this player
        /// </summary>
        void OnVisibilityChanged()
        {
            if (Visible)
            {
                deletedPlans.Clear();
                battlePlans.Clear();
                Me.BattlePlans.ForEach(plan => battlePlans.Add(plan.Clone()));
                UpdateBattlePlansItemList();
                OnBattlePlanSelected(0);
            }
        }

        /// <summary>
        /// Save the changes back to the user
        /// </summary>
        void OnOk()
        {
            foreach (var plan in battlePlans)
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

            Hide();
        }

        void OnNewButtonPressed()
        {
            battlePlans.Add(new BattlePlan($"Battle Plan {battlePlans.Count + 1}"));
            UpdateBattlePlansItemList();
        }

        /// <summary>
        /// Delete the currently selected battle plan
        /// TODO: Warn about in use battle plans
        /// </summary>
        void OnDeleteButtonPressed()
        {
            if (selectedPlan != null && selectedPlan != battlePlans[0])
            {
                deletedPlans.Add(selectedPlan);
                battlePlans.Remove(selectedPlan);
                UpdateBattlePlansItemList();
                OnBattlePlanSelected(0);
            }
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

        void OnNameLineEditTextChanged(string newText)
        {
            if (selectedPlan != null && selectedPlan != battlePlans[0])
            {
                selectedPlan.Name = newText;
            }
        }

        void OnDumpCargoCheckBoxToggled(bool toggled)
        {
            if (selectedPlan != null)
            {
                selectedPlan.DumpCargo = toggled;
            }
        }

        void UpdateBattlePlansItemList()
        {
            battlePlansItemList.Clear();
            battlePlans.Each((plan, index) =>
            {
                battlePlansItemList.AddItem(plan.Name);
                if (index == 0)
                {
                    battlePlansItemList.SetItemCustomBgColor(0, Colors.DarkBlue);
                }
            });
        }

        void OnBattlePlanSelected(int index)
        {
            deleteButton.Disabled = true;

            if (index >= 0 && index < battlePlans.Count)
            {
                if (index != 0)
                {
                    deleteButton.Disabled = false;
                }
                selectedPlan = battlePlans[index];
                detailPlanNameLabel.Text = selectedPlan.Name;
                nameLineEdit.Text = selectedPlan.Name;
                primaryTargetOptionButton.Select((int)selectedPlan.PrimaryTarget);
                secondaryTargetOptionButton.Select((int)selectedPlan.SecondaryTarget);
                tacticOptionButton.Select((int)selectedPlan.Tactic);
                attackWhoOptionButton.Select((int)selectedPlan.AttackWho);
                dumpCargoCheckBox.Pressed = selectedPlan.DumpCargo;
            }
        }



    }
}