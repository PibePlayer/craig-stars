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

        /// <summary>
        /// This is called when a draggable item is drug over this HullComponentPanel.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="data">Whatever data is returned from GetDragData, in this case we are expecting a serialized DraggableTech</param>
        /// <returns></returns>
        public override bool CanDropData(Vector2 position, object data)
        {
            if (data is string json)
            {
                DraggableTech? draggableTech = Serializers.Deserialize<DraggableTech>(json);
                if (draggableTech != null)
                {
                    // Each HullComponentPanel only accepts certain techs, like engines or scanners. Make sure we can 
                    // drop this tech on our panel
                    if ((int)(draggableTech.Value.hullSlotType & Type) > 0)
                    {
                        // Yay! we allow this tech to be dropped!
                        log.Debug($"Trying to drop Draggable item {draggableTech.Value.name}");
                        return true;
                    }
                    else
                    {
                        // wrong type
                        return false;
                    }
                }
                else
                {
                    log.Error($"Failed to parse dropped json: {json}");
                }
            }
            // bummer, can't drop this data here
            log.Debug($"Can't drop data {data}");
            return false;
        }

        /// <summary>
        /// Once the user lets go of the mouse, drop the item on our panel
        /// </summary>
        /// <param name="position"></param>
        /// <param name="data"></param>
        public override void DropData(Vector2 position, object data)
        {
            // do another check to make sure we convert this godot array into a DraggableTech
            if (data is string json)
            {
                DraggableTech? draggableTech = Serializers.Deserialize<DraggableTech>(json);
                if (draggableTech != null)
                {
                    // The DraggableTech only has the name of a tech, nothing else. Get a full tech object from the TechStore
                    TechHullComponent hullComponent = TechStore.Instance.GetTechByName<TechHullComponent>(draggableTech.Value.name);
                    log.Info($"Dropped new HullComponent {hullComponent.Name} on {Index}");

                    // Tell any parent nodes subscribed to our AddHullComponentEvent that we have a new hull component dropped on us
                    AddHullComponentEvent?.Invoke(this, hullComponent);
                }
                else
                {
                    log.Error($"Unable to Deserialize json into a DraggableTech. {json}");
                }
            }
            else
            {
                // this should never be called, so log an error if we get here for some reason
                // CanDropItem should only return true for valid drop events.
                log.Error($"Tried to drop unknown item. {data}");
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