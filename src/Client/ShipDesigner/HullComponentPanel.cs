using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class HullComponentPanel : Panel
    {

        /// <summary>
        /// This is just for the ui to show the slot index when building hull designs
        /// </summary>
        /// <value></value>
        [Export]
        public int Index
        {
            get => index;
            set
            {
                index = value;
                UpdateControls();
            }
        }
        int index = 1;

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

        [Export]
        public bool Unlimited
        {
            get => unlimited;
            set
            {
                unlimited = value;
                UpdateControls();
            }
        }
        bool unlimited;

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
        Label indexLabel;
        TextureRect hullComponentIcon;

        public override void _Ready()
        {
            indexLabel = FindNode("IndexLabel") as Label;
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
                    case HullSlotType.SpaceDock:
                        return "Space Dock";
                    case HullSlotType.ShieldArmor:
                        return "Shield \nor\n Armor";
                    case HullSlotType.OrbitalElectrical:
                        return "Orbital \nor\n Electrical";
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
            if (Engine.EditorHint && indexLabel != null)
            {
                indexLabel.Visible = true;
                indexLabel.Text = $"Slot {Index}";
            }
            if (quantityLabel != null)
            {
                if (Type == HullSlotType.Cargo || Type == HullSlotType.SpaceDock)
                {
                    SelfModulate = new Color(1.5f, 1.5f, 1.5f);
                }
                else
                {
                    SelfModulate = Colors.White;
                }
                if (ShipDesignSlot != null)
                {
                    hullComponentIcon.Visible = true;
                    var texture = TextureLoader.Instance.FindTexture(ShipDesignSlot.HullComponent);
                    hullComponentIcon.Texture = texture;
                    typeLabel.Visible = false;
                    quantityLabel.Text = $"{ShipDesignSlot.Quantity} of {Quantity}";
                }
                else
                {
                    hullComponentIcon.Visible = false;
                    typeLabel.Visible = true;
                    typeLabel.Text = $"{TypeDescription}";
                    if (Required)
                    {
                        quantityLabel.Text = $"needs {Quantity}";
                    }
                    else if (Unlimited)
                    {
                        quantityLabel.Text = $"Unlimited";
                    }
                    else
                    {
                        if (Type == HullSlotType.Cargo || Type == HullSlotType.SpaceDock)
                        {
                            quantityLabel.Text = $"{Quantity}kT";
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

}