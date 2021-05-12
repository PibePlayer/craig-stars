using Godot;
using System;
using CraigStars.Singletons;
using System.Collections.Generic;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class FleetCompositionTile : FleetTile
    {
        FleetCompositionTileTokens tokens;
        Label estimatedRange;
        Label percentCloaked;

        Button splitButton;
        Button splitAllButton;
        Button mergeButton;
        OptionButton battlePlanOptionButton;

        public override void _Ready()
        {
            base._Ready();
            tokens = GetNode<FleetCompositionTileTokens>("Tokens");
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

        void OnBattlePlanOptionButtonItemSelected(int index)
        {
            if (CommandedFleet != null && index >= 0 && index < Me?.BattlePlans?.Count)
            {
                var plan = Me.BattlePlans[index];
                CommandedFleet.Fleet.BattlePlan = plan;
            }
        }

        void OnSplitButtonPressed()
        {

        }

        void OnSplitAllButtonPressed()
        {
            var order = new SplitAllFleetOrder() { Source = CommandedFleet.Fleet };
            Me.SplitFleetOrders.Add(order);
            Me.FleetOrders.Add(order);

            // Execute this fleet order and then add it to our player's
            // list of fleets so they can be commanded.
            // TODO: this is a bit fragile. If we add these fleets to the player's
            // Fleets in the Fleet.Split, we end up adding the fleets to the player's list twice.
            // This Fleet.Split should probably be handled differently...
            var fleets = CommandedFleet.Fleet.Split(order);
            fleets.ForEach(newFleet =>
            {
                Me.FleetsByGuid[newFleet.Guid] = newFleet;
                Me.Fleets.Add(newFleet);
            });

            Signals.PublishFleetsCreatedEvent(fleets);
        }

        void OnMergeButtonPressed()
        {
            Signals.PublishMergeFleetsDialogRequestedEvent(CommandedFleet);
        }

        protected override void OnNewCommandedFleet()
        {
            base.OnNewCommandedFleet();
            UpdateControls();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedFleet != null)
            {
                int selectedBattlePlanIndex = -1;
                battlePlanOptionButton.Clear();
                Me.BattlePlans.Each((plan, index) =>
                {
                    battlePlanOptionButton.AddItem(plan.Name);
                    if (plan == CommandedFleet.Fleet.BattlePlan)
                    {
                        selectedBattlePlanIndex = index;
                    }
                });

                if (selectedBattlePlanIndex != -1)
                {
                    battlePlanOptionButton.Select(selectedBattlePlanIndex);
                }

                estimatedRange.Text = $"{CommandedFleet.Fleet.GetEstimatedRange()} l.y.";
                if (CommandedFleet.Fleet.Aggregate.CloakPercent == 0)
                {
                    percentCloaked.Text = "None";
                }
                else
                {
                    percentCloaked.Text = $"{CommandedFleet.Fleet.Aggregate.CloakPercent:.#}%";
                }

                // enable/disable buttons
                if (CommandedFleet.Fleet.Tokens.Count == 1 && CommandedFleet.Fleet.Tokens[0].Quantity == 1)
                {
                    splitAllButton.Disabled = splitButton.Disabled = true;
                }
                else
                {
                    splitAllButton.Disabled = splitButton.Disabled = false;
                }

                if (CommandedFleet.Fleet.OtherFleets.Count == 0)
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