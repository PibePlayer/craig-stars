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

        public event Action<ShipDesignSlot> SlotUpdatedEvent;

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
        int quantityModifier = 1;

        public override void _Ready()
        {
            UpdateControls();
        }

        public override void _ExitTree()
        {
        }

        /// <summary>
        /// Set the quantity modifier for the dialog
        /// if the user holds shift, we multipy by 10, if they press control we multiply by 100
        /// both multiplies by 1000
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey key)
            {
                if (key.Pressed && key.Scancode == (uint)KeyList.Shift)
                {
                    quantityModifier *= 10;
                }
                else if (key.Pressed && key.Scancode == (uint)KeyList.Control)
                {
                    quantityModifier *= 100;
                }
                else
                {
                    quantityModifier = 1;
                }
            }
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
                    slot = new ShipDesignSlot()
                    {
                        HullComponent = hullComponent,
                        HullSlotIndex = hullComponentPanel.Index,
                        Quantity = Mathf.Clamp(quantityModifier, 1, hullComponentPanel.TechHullSlot.Capacity)
                    };
                    ShipDesign.Slots.Add(slot);
                }
                else if (slot.HullComponent != hullComponent)
                {
                    slot.HullComponent = hullComponent;
                    slot.Quantity = Mathf.Clamp(quantityModifier, 1, hullComponentPanel.TechHullSlot.Capacity);
                }
                else
                {
                    // add a slot
                    slot.Quantity = Mathf.Clamp(slot.Quantity + quantityModifier, 1, hullComponentPanel.TechHullSlot.Capacity);
                }
                hullComponentPanel.ShipDesignSlot = slot;
                SlotUpdatedEvent?.Invoke(slot);
                UpdateControls();
            }
        }

        void UpdateControls()
        {
            // make sure we clear out any events
            UnsubscribeHullComponentEvents();

            if (Hull != null)
            {
                // get an array of hull component panels excluding some types
                hullComponentPanels = this.GetAllNodesOfType<HullComponentPanel>()
                .Where(hcp => hcp.Type != HullSlotType.Cargo && hcp.Type != HullSlotType.SpaceDock)
                .ToList();

                // subscribe to events for these new hull  components
                SubscribeHullComponentEvents();

                // assign each slot a TechHullSlot from the Hull
                Hull.Slots.Each((slot, index) =>
                {
                    var hullComponentPanel = hullComponentPanels[index];
                    hullComponentPanel.TechHullSlot = slot;
                });

                // assign ship design slots to each HullComponentPanel (other than space docs and cargo)
                ShipDesign?.Slots?.ForEach(slot =>
                {
                    if (slot.HullSlotIndex > 0 && slot.HullSlotIndex <= hullComponentPanels.Count)
                    {
                        var hullComponentPanel = hullComponentPanels[slot.HullSlotIndex - 1];
                        hullComponentPanel.ShipDesignSlot = slot;
                    }
                });
            }
        }

    }
}
