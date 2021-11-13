using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{
    public class MergeFleetsDialog : GameViewDialog
    {
        [Inject] protected FleetService fleetService;

        public FleetSprite SourceFleet { get; set; }

        Button selectAllButton;
        Button unselectAllButton;

        ItemList fleetsItemList;


        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            selectAllButton = (Button)FindNode("SelectAllButton");
            unselectAllButton = (Button)FindNode("UnselectAllButton");

            fleetsItemList = (ItemList)FindNode("FleetsItemList");

            selectAllButton.Connect("pressed", this, nameof(OnSelectAll));
            unselectAllButton.Connect("pressed", this, nameof(OnUnselectAll));
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
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

        protected override void OnOk()
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
                fleetService.Merge(SourceFleet.Fleet, Me, order);
                fleetsToMerge.ForEach(f => { Me.MergedFleets.Add(f); Me.Fleets.Remove(f); });
                fleetSpritesToMerge.ForEach(f => EventManager.PublishFleetDeletedEvent(f.Fleet));
            }

            Hide();
        }

        void OnSelectAll()
        {
            for (int i = 0; i < fleetsItemList.Items.Count; i++)
            {
                fleetsItemList.Select(i, false);
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