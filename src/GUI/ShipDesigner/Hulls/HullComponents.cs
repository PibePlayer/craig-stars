using Godot;
using System;
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
                if (ShipDesign != null)
                {
                    GD.Print($"Showing HullComponents for ShipDesign {ShipDesign.Name}");
                }
                this.GetAllNodesOfType<HullComponentPanel>().Each((hullComponentPanel, index) =>
                {
                    if (ShipDesign != null && index < ShipDesign.Slots.Count)
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
