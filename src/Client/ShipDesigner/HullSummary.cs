using Godot;
using System;

using CraigStars.Singletons;

namespace CraigStars
{
    public class HullSummary : Control
    {
        // TODO: if we ever get more than 99 hull sets, make this smarter.
        public static int MaxHullSets = 99;
        public event Action<ShipDesignSlot> SlotUpdatedEvent;
        public event Action<HullComponentPanel, TechHullComponent> SlotPressedEvent;

        [Export]
        public bool ShowIconSelector { get; set; } = false;

        [Export]
        public bool Editable { get; set; }

        public TechHull Hull
        {
            get => hull;
            set
            {
                hull = value;
                UpdateControls();
            }
        }
        TechHull hull;

        public ShipDesign ShipDesign
        {
            get => shipDesign;
            set
            {
                shipDesign = value;
                UpdateControls();
            }
        }
        ShipDesign shipDesign;

        public ShipToken Token
        {
            get => token;
            set
            {
                token = value;
                UpdateControls();
            }
        }
        ShipToken token;

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

        // used to set the hull components drawings
        Control hullComponentsContainer;
        CostGrid costGrid;

        Control hullContainer;
        Control noHullContainer;



        public override void _Ready()
        {
            hullContainer = FindNode("HullContainer") as Control;
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
            hullComponentsContainer = FindNode("HullComponentsContainer") as Control;
            costGrid = FindNode("CostGrid") as CostGrid;

            // icon controls
            iconButtonContainer = (Control)FindNode("IconButtonContainer");
            prevIconButton = FindNode("PrevIconButton") as Button;
            nextIconButton = FindNode("NextIconButton") as Button;

            prevIconButton.Connect("pressed", this, nameof(OnPrevIconButtonPressed));
            nextIconButton.Connect("pressed", this, nameof(OnNextIconButtonPressed));

            UpdateControls();
        }

        public void ResetHullComponents()
        {
            if (hullComponentsContainer != null)
            {
                foreach (Node child in hullComponentsContainer.GetChildren())
                {
                    if (child is HullComponents hullComponents)
                    {
                        hullComponents.SlotUpdatedEvent -= OnSlotUpdated;
                        hullComponents.SlotPressedEvent -= OnSlotPressed;
                    }
                    hullComponentsContainer.RemoveChild(child);
                    child.QueueFree();
                }
            }
            var hullNameWithoutSpaces = Hull.Name.Replace(" ", "").Replace("-", "");
            var scenePath = $"res://src/Client/ShipDesigner/Hulls/{hullNameWithoutSpaces}HullComponents.tscn";
            var hullComponentsScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (hullComponentsScene != null)
            {
                var hullComponents = hullComponentsScene.Instance() as HullComponents;

                hullComponents.Hull = Hull;
                hullComponents.ShipDesign = ShipDesign;
                hullComponents.Editable = Editable;
                hullComponents.SlotUpdatedEvent += OnSlotUpdated;
                hullComponents.SlotPressedEvent += OnSlotPressed;
                hullComponentsContainer.AddChild(hullComponents);
            }
        }

        /// <summary>
        /// Propogate slot pressed events up to any listeners (so we can update costs or labels)
        /// </summary>
        /// <param name="hullComponentPanel"></param>
        /// <param name="hullComponent"></param>
        void OnSlotPressed(HullComponentPanel hullComponentPanel, TechHullComponent hullComponent)
        {
            SlotPressedEvent?.Invoke(hullComponentPanel, hullComponent);
        }

        /// <summary>
        /// Propogate slot update events up to any listeners
        /// </summary>
        /// <param name="slot"></param>
        void OnSlotUpdated(ShipDesignSlot slot)
        {
            ShipDesign.ComputeAggregate(PlayersManager.Me, true);
            UpdateControls();
            SlotUpdatedEvent?.Invoke(slot);
        }

        /// <summary>
        /// Cycle backwards through hull set images, returning the latest hull set if we go before 0
        /// </summary>
        void OnPrevIconButtonPressed()
        {
            if (ShipDesign != null)
            {
                int hullSetIndex = ShipDesign.HullSetNumber;
                if (ShipDesign.HullSetNumber > 0)
                {
                    ShipDesign.HullSetNumber--;
                }
                else
                {
                    // find the last hullset
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
        /// Cycle through hull set images, resetting to 0 if we pass the last available hull set image
        /// </summary>
        void OnNextIconButtonPressed()
        {
            if (ShipDesign != null)
            {
                int hullSetIndex = ShipDesign.HullSetNumber;
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

        void UpdateControls()
        {
            iconButtonContainer.Visible = ShowIconSelector;
            // make sure we have controls to update
            if (nameLabel != null && Hull != null)
            {
                hullContainer.Visible = true;
                noHullContainer.Visible = false;

                ResetHullComponents();

                if (token?.Damage > 0)
                {
                    damageLabel.Visible = damageAmountLabel.Visible = true;
                    var damagePercent = ((float)Token.Damage * Token.QuantityDamaged) / (token.Design.Armor * Token.Quantity);
                    damageAmountLabel.Text = $"{token.QuantityDamaged}@{damagePercent * 100:#}%";
                }
                else
                {
                    damageLabel.Visible = damageAmountLabel.Visible = false;
                }

                if (shipDesign != null)
                {
                    nameLabel.Text = shipDesign.Name;
                    icon.Texture = TextureLoader.Instance.FindTexture(shipDesign);

                    shipDesign.ComputeAggregate(PlayersManager.Me);
                    costTitleLabel.Text = shipDesign.Name.Empty() ? "Cost of design" : $"Cost of one {shipDesign.Name}";
                    costGrid.Cost = shipDesign.Aggregate.Cost;
                    if (Hull.Starbase)
                    {
                        maxFuelLabel.Visible = maxFuelAmountLabel.Visible = false;
                    }
                    else
                    {
                        maxFuelAmountLabel.Text = $"{shipDesign.Aggregate.FuelCapacity}mg";
                        maxFuelLabel.Visible = maxFuelAmountLabel.Visible = true;
                    }
                    armorAmountLabel.Text = $"{shipDesign.Aggregate.Armor}dp";
                    massLabel.Text = $"{shipDesign.Aggregate.Mass}kT";
                    shieldsAmountLabel.Text = $"{(shipDesign.Aggregate.Shield > 0 ? $"{shipDesign.Aggregate.Shield}dp" : "none")}";
                    cloakJamAmountLabel.Text = $"{shipDesign.Aggregate.CloakPercent}%/{0:.}%"; // TODO: jamming
                    initiativeMovesAmountLabel.Text = $"{shipDesign.Aggregate.Initiative}/{shipDesign.Aggregate.Movement}";
                    if (shipDesign.Aggregate.Scanner)
                    {
                        scannerRangeLabel.Visible = scannerRangeAmountLabel.Visible = true;
                        scannerRangeAmountLabel.Text = $"{(shipDesign.Aggregate.ScanRange >= 0 ? shipDesign.Aggregate.ScanRange.ToString() : "")}/{(shipDesign.Aggregate.ScanRangePen >= 0 ? shipDesign.Aggregate.ScanRangePen.ToString() : "")}";
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
                    purposeValueLabel.Text = $"{shipDesign.Purpose}";
                }
                else if (hull != null)
                {
                    icon.Texture = TextureLoader.Instance.FindTexture(hull);
                    nameLabel.Text = hull.Name;
                    costTitleLabel.Text = $"Cost of one {hull.Name}";
                    costGrid.Cost = hull.Cost;
                    maxFuelAmountLabel.Text = $"{hull.FuelCapacity}mg";
                    armorAmountLabel.Text = $"{hull.Armor}dp";
                    massLabel.Text = $"{hull.Mass}kT";

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
                hull = null;
                hullContainer.Visible = false;
                noHullContainer.Visible = true;
            }
        }

    }
}