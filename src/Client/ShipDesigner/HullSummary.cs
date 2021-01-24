using Godot;
using System;

using CraigStars.Singletons;

namespace CraigStars
{
    public class HullSummary : Control
    {
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

        Label nameLabel;
        Label costTitleLabel;
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
        Label massLabel;
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
            massLabel = FindNode("MassLabel") as Label;
            icon = FindNode("Icon") as TextureRect;
            hullComponentsContainer = FindNode("HullComponentsContainer") as Control;
            costGrid = FindNode("CostGrid") as CostGrid;

            UpdateControls();
        }

        void UpdateControls()
        {
            // make sure we have controls to update
            if (nameLabel != null && Hull != null)
            {
                hullContainer.Visible = true;
                noHullContainer.Visible = false;
                if (hullComponentsContainer != null)
                {
                    foreach (Node child in hullComponentsContainer.GetChildren())
                    {
                        hullComponentsContainer.RemoveChild(child);
                        child.QueueFree();
                    }
                }
                var hullNameWithoutSpaces = Hull.Name.Replace(" ", "");
                var scenePath = $"res://src/Client/ShipDesigner/Hulls/{hullNameWithoutSpaces}HullComponents.tscn";
                var hullComponentsScene = ResourceLoader.Load<PackedScene>(scenePath);
                if (hullComponentsScene != null)
                {
                    var hullComponents = hullComponentsScene.Instance() as HullComponents;

                    hullComponents.Hull = Hull;
                    hullComponents.ShipDesign = ShipDesign;
                    hullComponentsContainer.AddChild(hullComponents);
                }

                if (shipDesign != null)
                {
                    nameLabel.Text = shipDesign.Name;
                    icon.Texture = TextureLoader.Instance.FindTexture(shipDesign);

                    shipDesign.ComputeAggregate(PlayersManager.Instance.Me, SettingsManager.Settings);
                    costTitleLabel.Text = $"Cost of one {shipDesign.Name}";
                    costGrid.Cost = shipDesign.Aggregate.Cost;
                    maxFuelAmountLabel.Text = $"{shipDesign.Aggregate.FuelCapacity}mg";
                    armorAmountLabel.Text = $"{shipDesign.Aggregate.Armor}dp";
                    massLabel.Text = $"{shipDesign.Aggregate.Mass}kT";
                    shieldsAmountLabel.Text = $"{(shipDesign.Aggregate.Shield > 0 ? shipDesign.Aggregate.Shield.ToString() : "")}dp";
                    cloakJamAmountLabel.Text = $"0/0"; // TODO: support cloak
                    initiativeMovesAmountLabel.Text = $"0/1"; // TODO: support moves
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
                    ratingLabel.Visible = true;
                    ratingAmountLabel.Visible = true;
                    cloakJamLabel.Visible = true;
                    cloakJamAmountLabel.Visible = true;
                    initiativeMovesLabel.Visible = true;
                    initiativeMovesAmountLabel.Visible = true;
                    scannerRangeLabel.Visible = true;
                    scannerRangeAmountLabel.Visible = true;
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
                }
            }
            else
            {
                hull = null;
                hullContainer.Visible = false;
                noHullContainer.Visible = true;
            }
        }

        void Blah()
        {
            GD.Print("blah");
        }
    }
}