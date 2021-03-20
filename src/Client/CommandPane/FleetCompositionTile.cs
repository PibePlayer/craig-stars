using Godot;
using System;
using CraigStars.Singletons;

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

        public override void _Ready()
        {
            base._Ready();
            tokensTree = (Tree)FindNode("Tokens");
            estimatedRange = (Label)FindNode("EstimatedRange");
            percentCloaked = (Label)FindNode("PercentCloaked");

            splitButton = (Button)FindNode("SplitButton");
            splitAllButton = (Button)FindNode("SplitAllButton");
            mergeButton = (Button)FindNode("MergeButton");

            splitButton.Connect("pressed", this, nameof(OnSplitButtonPressed));
            splitAllButton.Connect("pressed", this, nameof(OnSplitAllButtonPressed));
            mergeButton.Connect("pressed", this, nameof(OnMergeButtonPressed));
        }

        public override void _ExitTree()
        {
            base._ExitTree();
        }

        void OnSplitButtonPressed()
        {

        }

        void OnSplitAllButtonPressed()
        {

        }

        void OnMergeButtonPressed()
        {

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