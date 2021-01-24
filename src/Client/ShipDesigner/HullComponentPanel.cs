using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using log4net;
using System;

namespace CraigStars
{
    [Tool]
    public class HullComponentPanel : Panel
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HullComponentPanel));

        public event Action<HullComponentPanel, TechHullComponent> AddHullComponentEvent;

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

            if (!Engine.EditorHint)
            {
                indexLabel.Visible = false;
            }
            UpdateControls();
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            if (data is Godot.Collections.Array arrayData)
            {
                DraggableTech draggableTech = GodotSerializers.FromArray(arrayData);
                if ((int)(draggableTech.hullSlotType & Type) > 0)
                {
                    // we can drop this
                    log.Info($"Trying to drop Draggable item {draggableTech.name}");
                    return true;
                }
            }
            else
            {
                log.Info($"Can't drop data {data}");
            }
            return false;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (data is Godot.Collections.Array arrayData)
            {
                DraggableTech draggableTech = GodotSerializers.FromArray(arrayData);
                TechHullComponent hullComponent = TechStore.Instance.GetTechByName<TechHullComponent>(draggableTech.name);
                log.Info($"Dropped new HullComponent {hullComponent.Name} on {Index}");
                AddHullComponentEvent?.Invoke(this, hullComponent);
            }
            else
            {
                log.Error("Tried to drop unknown item.");
            }
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
                    SelfModulate = new Color(2f, 2f, 2f);
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