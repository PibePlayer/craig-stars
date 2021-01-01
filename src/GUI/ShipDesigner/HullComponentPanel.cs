using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class HullComponentPanel : Panel
    {

        [Export]
        public HullSlotType Type
        {
            get => type;
            set
            {
                type = value;
                UpdateControls();
            }
        }
        HullSlotType type = HullSlotType.Mechanical;

        [Export(PropertyHint.Range, "1,512")]
        public int Quantity
        {
            get => quantity;
            set
            {
                quantity = value;
                UpdateControls();
            }
        }
        int quantity = 1;

        [Export]
        public bool Required
        {
            get => required;
            set
            {
                required = value;
                UpdateControls();
            }
        }
        bool required;

        public TechHullSlot TechHullSlot
        {
            get => techHullSlot; set
            {
                techHullSlot = value;
                if (techHullSlot != null)
                {
                    quantity = techHullSlot.Capacity;
                    type = techHullSlot.Type;
                    required = techHullSlot.Required;
                }
                UpdateControls();
            }
        }
        TechHullSlot techHullSlot;

        public ShipDesignSlot ShipDesignSlot
        {
            get => shipDesignSlot;
            set
            {
                shipDesignSlot = value;
                if (shipDesignSlot != null)
                {
                    quantity = shipDesignSlot.Quantity;
                }
                UpdateControls();
            }
        }
        ShipDesignSlot shipDesignSlot;

        Label quantityLabel;
        Label typeLabel;
        TextureRect hullComponentIcon;

        public override void _Ready()
        {
            quantityLabel = FindNode("QuantityLabel") as Label;
            typeLabel = FindNode("TypeLabel") as Label;
            hullComponentIcon = FindNode("HullComponentIcon") as TextureRect;

            UpdateControls();
        }

        public string TypeDescription
        {
            get
            {
                switch (Type)
                {
                    case HullSlotType.Mechanical:
                        return "Mech";
                    case HullSlotType.ShieldArmor:
                        return "Shield \nor\n Armor";
                    case HullSlotType.ScannerElectricalMechanical:
                        return "Scanner Elec Mech";
                    case HullSlotType.General:
                        return "General Purpose";
                    default:
                        return Type.ToString();
                }
            }
        }

        void UpdateControls()
        {
            if (quantityLabel != null)
            {
                if (ShipDesignSlot != null)
                {
                    hullComponentIcon.Visible = true;
                    var texture = TextureLoader.Instance.FindTechTexture(ShipDesignSlot.HullComponent.Name, ShipDesignSlot.HullComponent.Category);
                    hullComponentIcon.Texture = texture;
                    typeLabel.Visible = false;
                    quantityLabel.Text = $"{ShipDesignSlot.Quantity} of {Quantity}";
                }
                else
                {
                    hullComponentIcon.Visible = false;
                    typeLabel.Visible = true;
                    typeLabel.Text = $"{TypeDescription}";
                    if (required)
                    {
                        quantityLabel.Text = $"needs {Quantity}";
                    }
                    else
                    {
                        quantityLabel.Text = $"up to {Quantity}";
                    }
                }
            }
        }
    }

}