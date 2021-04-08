using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class BattlePlansDialog : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }

        ItemList battlePlansItemList;
        LineEdit nameLineEdit;
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

            okButton.Connect("pressed", this, nameof(OnOk));
            newButton.Connect("pressed", this, nameof(OnNewButtonPressed));
            deleteButton.Connect("pressed", this, nameof(OnDeleteButtonPressed));

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            PlayersManager.Instance.SetupPlayers();
            Me.BattlePlans.Add(new BattlePlan("Default"));
            Me.BattlePlans.Add(new BattlePlan("Sniper")
            {
                AttackWho = BattleAttackWho.Everyone,
                PrimaryTarget = BattleTargetType.FuelTransports,
                SecondaryTarget = BattleTargetType.Freighters,
                Tactic = BattleTactic.DisengageIfChallenged,
            });
            Show();
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
                }
            }

            Hide();
        }

        void OnNewButtonPressed()
        {
            battlePlans.Add(new BattlePlan($"Battle Plan {battlePlans.Count + 1}"));
            UpdateBattlePlansItemList();
        }

        void OnDeleteButtonPressed()
        {
            if (selectedPlan != null && selectedPlan != battlePlans[0])
            {
                deletedPlans.Add(selectedPlan);
                battlePlans.Remove(selectedPlan);
                UpdateBattlePlansItemList();
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