using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using CraigStars.Utils;
using CraigStars.Singletons;

namespace CraigStars
{

    public class HullComponents : Control
    {
        public TechHull Hull
        {
            get => hull;
            set
            {
                hull = value;
                if (hull != null)
                {
                    UpdateControls();
                }
            }
        }
        TechHull hull;

        public ShipDesign ShipDesign
        {
            get => shipDesign;
            set
            {
                shipDesign = value;
                if (shipDesign != null)
                {
                    UpdateControls();
                }
            }
        }
        ShipDesign shipDesign;

        List<HullComponentPanel> hullComponentPanels;

        public override void _Ready()
        {
            UpdateControls();
        }

        public override void _ExitTree()
        {
        }

        void UnsubscribeHullComponentEvents()
        {
            hullComponentPanels?.ForEach(hc => hc.AddHullComponentEvent -= OnAddHullComponent);
        }

        void SubscribeHullComponentEvents()
        {
            hullComponentPanels?.ForEach(hc => hc.AddHullComponentEvent += OnAddHullComponent);
        }

        void OnAddHullComponent(HullComponentPanel hullComponentPanel, TechHullComponent hullComponent)
        {
            if (ShipDesign != null && Hull != null)
            {
                var slot = ShipDesign.Slots.Find(s => s.HullSlotIndex == hullComponentPanel.Index);
                if (slot == null)
                {
                    ShipDesign.Slots.Add(new ShipDesignSlot()
                    {
                        HullComponent = hullComponent,
                        HullSlotIndex = hullComponentPanel.Index,
                        Quantity = 1
                    });
                }
                else
                {
                    // add a slot
                    slot.Quantity = Mathf.Clamp(slot.Quantity + 1, 0, hullComponentPanel.TechHullSlot.Capacity);
                }
                hullComponentPanel.ShipDesignSlot = slot;
                UpdateControls();
            }
        }

        void UpdateControls()
        {
            // make sure we clear out any events
            UnsubscribeHullComponentEvents();

            if (ShipDesign != null || Hull != null)
            {
                // get an array of hull component panels excluding some types
                hullComponentPanels = this.GetAllNodesOfType<HullComponentPanel>()
                .Where(hcp => hcp.Type != HullSlotType.Cargo && hcp.Type != HullSlotType.SpaceDock)
                .ToList();

                // subscribe to events for these new hull  components
                SubscribeHullComponentEvents();

                // assign ship design slots to each HullComponentPanel (other than space docs and cargo)
                ShipDesign?.Slots?.ForEach(slot =>
                {
                    if (slot.HullSlotIndex > 0 && slot.HullSlotIndex <= hullComponentPanels.Count)
                    {
                        var hullComponentPanel = hullComponentPanels[slot.HullSlotIndex - 1];
                        hullComponentPanel.ShipDesignSlot = slot;
                        if (Hull != null && slot.HullSlotIndex < Hull.Slots.Count)
                        {
                            hullComponentPanel.TechHullSlot = Hull.Slots[slot.HullSlotIndex];
                        }
                    }
                });
            }
        }

    }
}
