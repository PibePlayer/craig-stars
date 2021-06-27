using Godot;
using System;

using CraigStars.Singletons;

namespace CraigStars
{
    public class HullSummary : Control
    {
        // TODO: if we ever get more than 99 Hull sets, make this smarter.
        public static int MaxHullSets = 99;
        public event Action<ShipDesignSlot> SlotUpdatedEvent;
        public event Action<HullComponentPanel, TechHullComponent> SlotPressedEvent;

        [Export]
        public bool ShowIconSelector { get; set; } = false;

        [Export]
        public bool Editable { get; set; }

        public TechHull Hull { get; set; }
        public ShipDesign ShipDesign { get; set; }
        public ShipToken Token { get; set; }

        Label nameLabel;
        Label costTitleLabel;
        Label maxFuelLabel;
        Label maxFuelAmountLabel;
        Label armorAmountLabel;
        Label shieldsLabel;
        Label shieldsAmountLabel;
        Label ratingLabel;
        Label ratingAmountLabel;
        Label cloakJamLabel;
        Label cloakJamAmountLabel;
        Label initiativeMovesLabel;
        Label initiativeMovesAmountLabel;
        Label scannerRangeLabel;
        Label scannerRangeAmountLabel;
        Label purposeValueLabel;
        Label massLabel;
        Label damageLabel;
        Label damageAmountLabel;

        // icon
        Control iconButtonContainer;
        Button prevIconButton;
        Button nextIconButton;
        TextureRect icon;

        // used to set the Hull components drawings
        Control HullComponentsContainer;
        CostGrid costGrid;

        Control HullContainer;
        Control noHullContainer;



        public override void _Ready()
        {
            HullContainer = FindNode("HullContainer") as Control;
            noHullContainer = FindNode("NoHullContainer") as Control;
            nameLabel = FindNode("NameLabel") as Label;
            costTitleLabel = FindNode("CostTitleLabel") as Label;
            maxFuelAmountLabel = FindNode("MaxFuelAmountLabel") as Label;
            maxFuelLabel = FindNode("MaxFuelLabel") as Label;
            armorAmountLabel = FindNode("ArmorAmountLabel") as Label;
            shieldsLabel = FindNode("ShieldsLabel") as Label;
            shieldsAmountLabel = FindNode("ShieldsAmountLabel") as Label;
            ratingLabel = FindNode("RatingLabel") as Label;
            ratingAmountLabel = FindNode("RatingAmountLabel") as Label;
            cloakJamLabel = FindNode("CloakJamLabel") as Label;
            cloakJamAmountLabel = FindNode("CloakJamAmountLabel") as Label;
            initiativeMovesLabel = FindNode("InitiativeMovesLabel") as Label;
            initiativeMovesAmountLabel = FindNode("InitiativeMovesAmountLabel") as Label;
            scannerRangeLabel = FindNode("ScannerRangeLabel") as Label;
            scannerRangeAmountLabel = FindNode("ScannerRangeAmountLabel") as Label;
            purposeValueLabel = FindNode("PurposeValueLabel") as Label;
            damageLabel = FindNode("DamageLabel") as Label;
            damageAmountLabel = FindNode("DamageAmountLabel") as Label;
            massLabel = FindNode("MassLabel") as Label;
            icon = FindNode("Icon") as TextureRect;
            HullComponentsContainer = FindNode("HullComponentsContainer") as Control;
            costGrid = FindNode("CostGrid") as CostGrid;

            // icon controls
            iconButtonContainer = (Control)FindNode("IconButtonContainer");
            prevIconButton = FindNode("PrevIconButton") as Button;
            nextIconButton = FindNode("NextIconButton") as Button;

            icon.Connect("gui_input", this, nameof(OnIconGuiInput));
            prevIconButton.Connect("pressed", this, nameof(OnPrevIconButtonPressed));
            nextIconButton.Connect("pressed", this, nameof(OnNextIconButtonPressed));

            UpdateControls();
        }

        public void ResetHullComponents()
        {
            if (HullComponentsContainer != null)
            {
                foreach (Node child in HullComponentsContainer.GetChildren())
                {
                    if (child is HullComponents HullComponents)
                    {
                        HullComponents.SlotUpdatedEvent -= OnSlotUpdated;
                        HullComponents.SlotPressedEvent -= OnSlotPressed;
                    }
                    HullComponentsContainer.RemoveChild(child);
                    child.QueueFree();
                }
            }
            var HullNameWithoutSpaces = Hull.Name.Replace(" ", "").Replace("-", "");
            var scenePath = $"res://src/Client/ShipDesigner/Hulls/{HullNameWithoutSpaces}HullComponents.tscn";
            var HullComponentsScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (HullComponentsScene != null)
            {
                var HullComponents = HullComponentsScene.Instance() as HullComponents;

                HullComponents.Editable = Editable;
                HullComponents.Hull = Hull;
                HullComponents.ShipDesign = ShipDesign;
                HullComponents.SlotUpdatedEvent += OnSlotUpdated;
                HullComponents.SlotPressedEvent += OnSlotPressed;
                HullComponents.UpdateControls();
                HullComponentsContainer.AddChild(HullComponents);
            }
        }

        /// <summary>
        /// Propogate slot pressed events up to any listeners (so we can update costs or labels)
        /// </summary>
        /// <param name="HullComponentPanel"></param>
        /// <param name="HullComponent"></param>
        void OnSlotPressed(HullComponentPanel HullComponentPanel, TechHullComponent HullComponent)
        {
            SlotPressedEvent?.Invoke(HullComponentPanel, HullComponent);
        }

        /// <summary>
        /// Propogate slot update events up to any listeners
        /// </summary>
        /// <param name="slot"></param>
        void OnSlotUpdated(ShipDesignSlot slot)
        {
            ShipDesign.ComputeAggregate(PlayersManager.Me, true);
            SlotUpdatedEvent?.Invoke(slot);
        }

        void OnIconGuiInput(InputEvent @event)
        {
            if (Hull != null && @event.IsActionPressed("Hullcomponent_alternate_select"))
            {
                GetTree().SetInputAsHandled();

                TechSummaryPopup.Tech = Hull;
                TechSummaryPopup.ShowAtMouse();
            }
            else if (@event.IsActionReleased("Hullcomponent_alternate_select"))
            {
                TechSummaryPopup.Instance.Hide();
            }

        }

        /// <summary>
        /// Cycle backwards through Hull set images, returning the latest Hull set if we go before 0
        /// </summary>
        void OnPrevIconButtonPressed()
        {
            if (ShipDesign != null)
            {
                int HullSetIndex = ShipDesign.HullSetNumber;
                if (ShipDesign.HullSetNumber > 0)
                {
                    ShipDesign.HullSetNumber--;
                }
                else
                {
                    // find the last Hullset
                    for (int i = 0; i < 99; i++)
                    {
                        var texture = TextureLoader.Instance.FindTexture(Hull, i);
                        if (texture == null && i > 0)
                        {
                            ShipDesign.HullSetNumber = i - 1;
                            break;
                        }
                    }
                }
                UpdateControls();
            }
        }

        /// <summary>
        /// Cycle through Hull set images, resetting to 0 if we pass the last available Hull set image
        /// </summary>
        void OnNextIconButtonPressed()
        {
            if (ShipDesign != null)
            {
                int HullSetIndex = ShipDesign.HullSetNumber;
                var texture = TextureLoader.Instance.FindTexture(Hull, ShipDesign.HullSetNumber + 1);
                if (texture != null)
                {
                    ShipDesign.HullSetNumber++;
                }
                else
                {
                    ShipDesign.HullSetNumber = 0;
                }
                UpdateControls();
            }
        }

        internal void UpdateControls()
        {
            iconButtonContainer.Visible = ShowIconSelector;
            // make sure we have controls to update
            if (nameLabel != null && Hull != null)
            {
                HullContainer.Visible = true;
                noHullContainer.Visible = false;

                ResetHullComponents();

                if (Token?.Damage > 0)
                {
                    damageLabel.Visible = damageAmountLabel.Visible = true;
                    var damagePercent = ((float)Token.Damage * Token.QuantityDamaged) / (Token.Design.Armor * Token.Quantity);
                    damageAmountLabel.Text = $"{Token.QuantityDamaged}@{damagePercent * 100:#}%";
                }
                else
                {
                    damageLabel.Visible = damageAmountLabel.Visible = false;
                }

                if (ShipDesign != null)
                {
                    nameLabel.Text = ShipDesign.Name;
                    icon.Texture = TextureLoader.Instance.FindTexture(ShipDesign);

                    ShipDesign.ComputeAggregate(PlayersManager.Me);
                    costTitleLabel.Text = ShipDesign.Name.Empty() ? "Cost of design" : $"Cost of one {ShipDesign.Name}";
                    costGrid.Cost = ShipDesign.Aggregate.Cost;
                    if (Hull.Starbase)
                    {
                        maxFuelLabel.Visible = maxFuelAmountLabel.Visible = false;
                    }
                    else
                    {
                        maxFuelAmountLabel.Text = $"{ShipDesign.Aggregate.FuelCapacity}mg";
                        maxFuelLabel.Visible = maxFuelAmountLabel.Visible = true;
                    }
                    armorAmountLabel.Text = $"{ShipDesign.Aggregate.Armor}dp";
                    massLabel.Text = $"{ShipDesign.Aggregate.Mass}kT";
                    shieldsAmountLabel.Text = $"{(ShipDesign.Aggregate.Shield > 0 ? $"{ShipDesign.Aggregate.Shield}dp" : "none")}";
                    cloakJamAmountLabel.Text = $"{ShipDesign.Aggregate.CloakPercent}%/{0:.}%"; // TODO: jamming
                    initiativeMovesAmountLabel.Text = $"{ShipDesign.Aggregate.Initiative}/{ShipDesign.Aggregate.Movement}";
                    if (ShipDesign.Aggregate.Scanner)
                    {
                        scannerRangeLabel.Visible = scannerRangeAmountLabel.Visible = true;
                        scannerRangeAmountLabel.Text = $"{(ShipDesign.Aggregate.ScanRange >= 0 ? ShipDesign.Aggregate.ScanRange.ToString() : "")}/{(ShipDesign.Aggregate.ScanRangePen >= 0 ? ShipDesign.Aggregate.ScanRangePen.ToString() : "")}";
                    }
                    else
                    {
                        scannerRangeLabel.Visible = scannerRangeAmountLabel.Visible = false;
                    }

                    shieldsLabel.Visible = true;
                    shieldsAmountLabel.Visible = true;
                    ratingLabel.Visible = true; // TODO: support rating
                    ratingAmountLabel.Visible = true;
                    cloakJamLabel.Visible = true;
                    cloakJamAmountLabel.Visible = true;
                    initiativeMovesLabel.Visible = true;
                    initiativeMovesAmountLabel.Visible = true;
                    purposeValueLabel.Visible = true;
                    purposeValueLabel.Text = $"{ShipDesign.Purpose}";
                }
                else if (Hull != null)
                {
                    icon.Texture = TextureLoader.Instance.FindTexture(Hull);
                    nameLabel.Text = Hull.Name;
                    costTitleLabel.Text = $"Cost of one {Hull.Name}";
                    costGrid.Cost = Hull.Cost;
                    maxFuelAmountLabel.Text = $"{Hull.FuelCapacity}mg";
                    armorAmountLabel.Text = $"{Hull.Armor}dp";
                    massLabel.Text = $"{Hull.Mass}kT";

                    shieldsLabel.Visible = false;
                    shieldsAmountLabel.Visible = false;
                    ratingLabel.Visible = false;
                    ratingAmountLabel.Visible = false;
                    cloakJamLabel.Visible = false;
                    cloakJamAmountLabel.Visible = false;
                    initiativeMovesLabel.Visible = false;
                    initiativeMovesAmountLabel.Visible = false;
                    scannerRangeLabel.Visible = false;
                    scannerRangeAmountLabel.Visible = false;
                    purposeValueLabel.Visible = false;
                }
            }
            else
            {
                Hull = null;
                HullContainer.Visible = false;
                noHullContainer.Visible = true;
            }
        }

    }
}