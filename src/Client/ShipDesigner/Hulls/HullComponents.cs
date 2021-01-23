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

        public override void _Ready()
        {
            UpdateControls();
        }

        void UpdateControls()
        {
            if (ShipDesign != null || Hull != null)
            {
                // get an array of hull component panels excluding some types
                var hullComponentPanels = this.GetAllNodesOfType<HullComponentPanel>()
                .Where(hcp => hcp.Type != HullSlotType.Cargo && hcp.Type != HullSlotType.SpaceDock)
                .ToArray();

                // assign ship design slots to each HullComponentPanel (other than space docs and cargo)
                ShipDesign?.Slots?.ForEach(slot =>
                {
                    if (slot.HullSlotIndex > 0 && slot.HullSlotIndex <= hullComponentPanels.Length)
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
