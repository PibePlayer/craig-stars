using Godot;
using System;
using CraigStars.Singletons;
using System.Collections.Generic;
using CraigStars.Utils;

namespace CraigStars
{
    public class FleetCompositionTile : FleetTile
    {
        Tree tokensTree;
        Label estimatedRange;
        Label percentCloaked;

        Button splitButton;
        Button splitAllButton;
        Button mergeButton;
        OptionButton battlePlanOptionButton;

        public override void _Ready()
        {
            base._Ready();
            tokensTree = (Tree)FindNode("Tokens");
            estimatedRange = (Label)FindNode("EstimatedRange");
            percentCloaked = (Label)FindNode("PercentCloaked");

            splitButton = (Button)FindNode("SplitButton");
            splitAllButton = (Button)FindNode("SplitAllButton");
            mergeButton = (Button)FindNode("MergeButton");

            battlePlanOptionButton = (OptionButton)FindNode("BattlePlanOptionButton");

            splitButton.Connect("pressed", this, nameof(OnSplitButtonPressed));
            splitAllButton.Connect("pressed", this, nameof(OnSplitAllButtonPressed));
            mergeButton.Connect("pressed", this, nameof(OnMergeButtonPressed));
            battlePlanOptionButton.Connect("item_selected", this, nameof(OnBattlePlanOptionButtonItemSelected));
        }

        protected override void OnFleetsCreated(List<Fleet> fleets)
        {
            base.OnFleetsCreated(fleets);
            AddItemsToTree();
        }

        protected override void OnFleetDeleted(FleetSprite fleet)
        {
            base.OnFleetDeleted(fleet);
            AddItemsToTree();
        }

        void OnBattlePlanOptionButtonItemSelected(int index)
        {
            if (ActiveFleet != null && index >= 0 && index < Me?.BattlePlans?.Count)
            {
                var plan = Me.BattlePlans[index];
                ActiveFleet.Fleet.BattlePlan = plan;
            }
        }

        void OnSplitButtonPressed()
        {

        }

        void OnSplitAllButtonPressed()
        {
            var order = new SplitAllFleetOrder() { Source = ActiveFleet.Fleet };
            Me.SplitFleetOrders.Add(order);
            Me.FleetOrders.Add(order);

            // Execute this fleet order and then add it to our player's
            // list of fleets so they can be commanded.
            // TODO: this is a bit fragile. If we add these fleets to the player's
            // Fleets in the Fleet.Split, we end up adding the fleets to the player's list twice.
            // This Fleet.Split should probably be handled differently...
            var fleets = ActiveFleet.Fleet.Split(order);
            fleets.ForEach(newFleet =>
            {
                Me.FleetsByGuid[newFleet.Guid] = newFleet;
                Me.Fleets.Add(newFleet);
            });

            Signals.PublishFleetsCreatedEvent(fleets);
        }

        void OnMergeButtonPressed()
        {
            Signals.PublishMergeFleetsDialogRequestedEvent(ActiveFleet);
        }

        void AddItemsToTree()
        {
            tokensTree.Clear();
            var root = tokensTree.CreateItem();
            foreach (var token in ActiveFleet.Fleet.Tokens)
            {
                var item = tokensTree.CreateItem(root);
                item.SetText(0, token.Design.Name);
                item.SetText(1, $"{token.Quantity}");
                item.SetTextAlign(1, TreeItem.TextAlign.Right);
            }
        }

        protected override void OnNewActiveFleet()
        {
            base.OnNewActiveFleet();
            if (ActiveFleet == null)
            {
                tokensTree.Clear();
            }
            else
            {
                AddItemsToTree();
            }
            UpdateControls();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActiveFleet != null)
            {
                int selectedBattlePlanIndex = -1;
                battlePlanOptionButton.Clear();
                Me.BattlePlans.Each((plan, index) =>
                {
                    battlePlanOptionButton.AddItem(plan.Name);
                    if (plan == ActiveFleet.Fleet.BattlePlan)
                    {
                        selectedBattlePlanIndex = index;
                    }
                });

                if (selectedBattlePlanIndex != -1)
                {
                    battlePlanOptionButton.Select(selectedBattlePlanIndex);
                }

                estimatedRange.Text = $"{ActiveFleet.Fleet.GetEstimatedRange()} l.y.";
                if (ActiveFleet.Fleet.Aggregate.CloakPercent == 0)
                {
                    percentCloaked.Text = "None";
                }
                else
                {
                    percentCloaked.Text = $"{ActiveFleet.Fleet.Aggregate.CloakPercent:.#}%";
                }

                // enable/disable buttons
                if (ActiveFleet.Fleet.Tokens.Count == 1 && ActiveFleet.Fleet.Tokens[0].Quantity == 1)
                {
                    splitAllButton.Disabled = splitButton.Disabled = true;
                }
                else
                {
                    splitAllButton.Disabled = splitButton.Disabled = false;
                }

                if (ActiveFleet.Fleet.OtherFleets.Count == 0)
                {
                    mergeButton.Disabled = true;
                }
                else
                {
                    mergeButton.Disabled = false;
                }
            }
        }

    }
}