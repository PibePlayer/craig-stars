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
                // assign ship design slots to each HullComponentPanel (other than space docs and cargo)
                this.GetAllNodesOfType<HullComponentPanel>()
                .Where(hcp => hcp.Type != HullSlotType.Cargo && hcp.Type != HullSlotType.SpaceDock)
                .Each((hullComponentPanel, index) =>
                {
                    if (index < ShipDesign.Slots.Count)
                    {
                        var slot = ShipDesign.Slots[index];
                        hullComponentPanel.ShipDesignSlot = slot;
                    }
                    if (Hull != null && index < Hull.Slots.Count)
                    {
                        hullComponentPanel.TechHullSlot = Hull.Slots[index];
                    }
                });
            }
        }

    }
}
