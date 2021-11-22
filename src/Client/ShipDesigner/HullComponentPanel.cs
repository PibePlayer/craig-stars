using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars.Client
{
    [Tool]
    public class HullComponentPanel : Panel
    {
        static CSLog log = LogProvider.GetLogger(typeof(HullComponentPanel));

        public event Action<HullComponentPanel, TechHullComponent> AddHullComponentEvent;
        public event Action<HullComponentPanel, TechHullComponent> RemoveHullComponentEvent;
        public event Action<HullComponentPanel, TechHullComponent> DroppedEvent;
        public event Action<HullComponentPanel, TechHullComponent> PressedEvent;

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

        [Export(PropertyHint.Range, "1,65000")]
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

        [Export]
        public bool Editable { get; set; }

        [Export]
        public bool Selected
        {
            get => selectedPanel != null && selectedPanel.Visible;
            set
            {
                if (selectedPanel != null)
                {
                    selectedPanel.Visible = value;
                }
            }
        }

        public TechHullSlot TechHullSlot
        {
            get => techHullSlot;
            set
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
                UpdateControls();
            }
        }
        ShipDesignSlot shipDesignSlot;

        Label quantityLabel;
        Label typeLabel;
        Label indexLabel;
        TextureRect hullComponentIcon;
        Panel selectedPanel;

        public override void _Ready()
        {
            indexLabel = FindNode("IndexLabel") as Label;
            quantityLabel = FindNode("QuantityLabel") as Label;
            typeLabel = FindNode("TypeLabel") as Label;
            hullComponentIcon = FindNode("HullComponentIcon") as TextureRect;
            selectedPanel = FindNode("SelectedPanel") as Panel;

            if (!Engine.EditorHint)
            {
                indexLabel.Visible = false;
            }

            Connect("gui_input", this, nameof(OnGuiInput));

            UpdateControls();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (Editable && Selected && @event.IsActionPressed("delete"))
            {
                RemoveHullComponent();
                GetTree()?.SetInputAsHandled();
            }
        }

        void OnGuiInput(InputEvent @event)
        {
            if (ShipDesignSlot != null && @event.IsActionPressed("hullcomponent_select"))
            {
                PressedEvent?.Invoke(this, ShipDesignSlot?.HullComponent);
                GetTree().SetInputAsHandled();
            }
            else if (ShipDesignSlot != null && ShipDesignSlot.HullComponent != null && @event.IsActionPressed("hullcomponent_alternate_select"))
            {
                GetTree().SetInputAsHandled();

                TechSummaryPopup.Tech = ShipDesignSlot.HullComponent;
                TechSummaryPopup.ShowAtMouse();
            }
            else if (@event.IsActionReleased("hullcomponent_alternate_select"))
            {
                TechSummaryPopup.Instance.Hide();
            }

        }

        public void RemoveHullComponent()
        {
            if (ShipDesignSlot != null)
            {
                RemoveHullComponentEvent?.Invoke(this, ShipDesignSlot.HullComponent);
                ShipDesignSlot = null;
                Selected = false;
                UpdateControls();
            }
        }

        /// <summary>
        /// Override GetDragData to allow the selected TreeItem to be drag and dropped
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override object GetDragData(Vector2 position)
        {
            if (!Editable || ShipDesignSlot == null)
            {
                // can't drag
                return null;
            }
            // We create a new Control with a TextureRect in it. The Control is just a container
            // so we can position the TextureRect to be centered on the mouse.
            var control = new Control();
            var preview = new TextureRect()
            {
                // use whatever icon is set on the selected tree item
                Texture = hullComponentIcon.Texture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            };
            // offset the preview texture to be centered
            preview.RectPosition = new Vector2(-preview.Texture.GetWidth() / 2, -preview.Texture.GetHeight() / 2);
            control.AddChild(preview);

            // show our user the icon of the tree item they are dragging
            SetDragPreview(control);

            control.Connect("tree_exiting", this, nameof(OnThisComponentDropped));

            // allow this ship design slot to be dragged
            log.Debug($"Dragging {Index}: {ShipDesignSlot.HullComponent.Name}");
            return Serializers.Serialize<TechHullComponent>(ShipDesignSlot.HullComponent);
        }

        /// <summary>
        /// This is called when this hull component is dropped after being drug somewhere
        /// </summary>
        void OnThisComponentDropped()
        {
            log.Debug($"Dropped {Index}: {ShipDesignSlot.HullComponent.Name}");
            RemoveHullComponent();
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
                        // log.Debug($"Trying to drop Draggable item {draggableTech.Value.name}");
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
            // log.Debug($"Can't drop data {data}");
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
                    case HullSlotType.MineLayer:
                        return "Mine\nLayer";
                    case HullSlotType.Mechanical:
                        return "Mech";
                    case HullSlotType.SpaceDock:
                        return "Space Dock";
                    case HullSlotType.ShieldArmor:
                        return "Shield\nor\nArmor";
                    case HullSlotType.ShieldElectricalMechanical:
                        return "Shield\nElect\nMech";
                    case HullSlotType.OrbitalElectrical:
                        return "Orbital\nor\nElectrical";
                    case HullSlotType.WeaponShield:
                        return "Weapon\nor\nShield";
                    case HullSlotType.ScannerElectricalMechanical:
                        return "Scanner\nElec\nMech";
                    case HullSlotType.ArmorScannerElectricalMechanical:
                        return "Armor\nScanner\nElec/Mech";
                    case HullSlotType.MineElectricalMechanical:
                        return "Mine\nElec\nMech";
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
                // the slot index is useful to see when designing hull designs in the editor, but we don't
                // want it during actual gameplay
                indexLabel.Visible = true;
                indexLabel.Text = $"Slot {Index}";
            }
            if (quantityLabel != null)
            {
                if (Required && (ShipDesignSlot == null || Quantity > ShipDesignSlot.Quantity))
                {
                    quantityLabel.AddColorOverride("font_color", Colors.Red);
                }
                else
                {
                    quantityLabel.AddColorOverride("font_color", Colors.Black);
                }

                if (Type == HullSlotType.Cargo || Type == HullSlotType.SpaceDock)
                {
                    SelfModulate = new Color(2f, 2f, 2f);
                }
                else
                {
                    SelfModulate = Colors.White;
                }
                if (ShipDesignSlot != null && ShipDesignSlot.HullComponent != null)
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