using CraigStars.Singletons;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class MergeFleetsDialog : WindowDialog
    {
        public FleetSprite SourceFleet { get; set; }
        public Player Me { get => PlayersManager.Me; }

        Button okButton;
        Button cancelButton;
        Button selectAllButton;
        Button unselectAllButton;

        ItemList fleetsItemList;


        public override void _Ready()
        {
            okButton = (Button)FindNode("OKButton");
            cancelButton = (Button)FindNode("CancelButton");
            selectAllButton = (Button)FindNode("SelectAllButton");
            unselectAllButton = (Button)FindNode("UnselectAllButton");

            fleetsItemList = (ItemList)FindNode("FleetsItemList");

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
            okButton.Connect("pressed", this, nameof(OnOK));
            cancelButton.Connect("pressed", this, nameof(OnCancel));
            selectAllButton.Connect("pressed", this, nameof(OnSelectAll));
            unselectAllButton.Connect("pressed", this, nameof(OnUnselectAll));
        }

        void OnVisibilityChanged()
        {
            fleetsItemList.Clear();
            if (SourceFleet != null)
            {
                foreach (var fleet in SourceFleet.OtherFleets.Where(f => f.OwnedByMe))
                {
                    fleetsItemList.AddItem(fleet.Fleet.Name);
                }

                WindowTitle = $"Merge Fleets into {SourceFleet.Fleet.Name}";
            }
        }

        void OnOK()
        {
            var fleetsToMerge = new List<Fleet>();
            var fleetSpritesToMerge = new List<FleetSprite>();
            for (int i = 0; i < fleetsItemList.Items.Count; i++)
            {
                if (fleetsItemList.IsSelected(i))
                {
                    fleetsToMerge.Add(SourceFleet.OtherFleets[i].Fleet);
                    fleetSpritesToMerge.Add(SourceFleet.OtherFleets[i]);
                }
            }

            if (fleetsToMerge.Count > 0)
            {
                var order = new MergeFleetOrder()
                {
                    Source = SourceFleet.Fleet,
                    MergingFleets = fleetsToMerge
                };
                Me.MergeFleetOrders.Add(order);
                Me.FleetOrders.Add(order);

                // merge the fleet on the client
                SourceFleet.Fleet.Merge(order);
                fleetsToMerge.ForEach(f => { Me.MergedFleets.Add(f); Me.Fleets.Remove(f); });
                fleetSpritesToMerge.ForEach(f => Signals.PublishFleetDeletedEvent(f));
            }

            Hide();
        }

        void OnCancel()
        {
            Hide();
        }

        void OnSelectAll()
        {
            for (int i = 0; i < fleetsItemList.Items.Count; i++)
            {
                fleetsItemList.Select(i);
            }
        }

        void OnUnselectAll()
        {
            for (int i = 0; i < fleetsItemList.Items.Count; i++)
            {
                fleetsItemList.Select(i, false);
            }
        }

    }
}