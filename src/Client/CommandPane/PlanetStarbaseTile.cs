using Godot;
using System;

namespace CraigStars
{

    public class PlanetStarbaseTile : PlanetTile
    {
        Label title;
        Control statsContainer;
        Control statsGrid;

        Label dockCapacity;
        Label armor;
        Label shields;
        Label damage;
        Label stargate;
        Label massDriver;
        Label destination;
        Label destinationLabel;
        Button setDestinationButton;

        PopupPanel hullSummaryPopup;
        HullSummary hullSummary;

        public override void _Ready()
        {
            base._Ready();
            title = FindNode("Name") as Label;
            statsContainer = FindNode("StatsContainer") as Control;
            statsGrid = GetNode<Container>("VBoxContainer/StatsContainer/StatsGrid");

            dockCapacity = FindNode("DockCapacity") as Label;
            armor = FindNode("Armor") as Label;
            shields = FindNode("Shields") as Label;
            damage = FindNode("Damage") as Label;
            stargate = FindNode("Stargate") as Label;
            massDriver = FindNode("MassDriver") as Label;
            destination = FindNode("Destination") as Label;
            destinationLabel = FindNode("DestinationLabel") as Label;
            setDestinationButton = FindNode("SetDestinationButton") as Button;

            hullSummaryPopup = GetNode<PopupPanel>("HullSummaryPopup");
            hullSummary = GetNode<HullSummary>("HullSummaryPopup/HullSummary");

            statsGrid.Connect("gui_input", this, nameof(OnStatsGridGUIInput));
        }

        void OnStatsGridGUIInput(InputEvent @event)
        {
            if (CommandedPlanet != null && CommandedPlanet.Planet.HasStarbase && @event.IsActionPressed("viewport_select"))
            {
                GetTree().SetInputAsHandled();

                hullSummary.Hull = CommandedPlanet.Planet.Starbase.Design.Hull;
                hullSummary.ShipDesign = CommandedPlanet.Planet.Starbase.Design;
                hullSummaryPopup.PopupCentered();
            }
        }


        protected override void UpdateControls()
        {
            base.UpdateControls();
            destination.Visible = false;
            destinationLabel.Visible = false;
            setDestinationButton.Visible = false;
            if (CommandedPlanet != null)
            {
                if (CommandedPlanet.Planet.HasStarbase)
                {
                    var starbase = CommandedPlanet.Planet.Starbase;
                    title.Text = $"{starbase.Name} v{starbase.Tokens[0].Design.Version}";
                    statsContainer.Visible = true;

                    dockCapacity.Text = starbase.DockCapacity == TechHull.UnlimitedSpaceDock ? "Unlimited" : $"{starbase.Aggregate.SpaceDock}kT";
                    armor.Text = $"{starbase.Aggregate.Armor}dp";
                    shields.Text = $"{starbase.Aggregate.Shield}dp";
                    damage.Text = starbase.Damage == 0 ? "none" : $"{starbase.Damage}dp";

                    if (starbase.Aggregate.HasGate)
                    {
                        var safeHullMass = $"{(starbase.Aggregate.SafeHullMass == TechHullComponent.InfinteGate ? "any" : $"{starbase.Aggregate.SafeHullMass}")}";
                        var safeRange = $"{(starbase.Aggregate.SafeRange == TechHullComponent.InfinteGate ? "any" : $"{starbase.Aggregate.SafeRange}")}";
                        stargate.Text = $"{safeHullMass}/{safeRange}";
                    }
                    if (starbase.Aggregate.HasMassDriver)
                    {
                        destination.Visible = true;
                        destinationLabel.Visible = true;
                        setDestinationButton.Visible = true;
                        massDriver.Text = $"Warp {starbase.Aggregate.PacketSpeed}";
                        if (starbase.MassDriverTarget != null)
                        {
                            destination.Text = starbase.MassDriverTarget.Name;
                        }
                        else
                        {
                            destination.Text = "none";
                        }
                    }
                }
                else
                {
                    title.Text = "No Starbase";
                    statsContainer.Visible = false;
                }

            }
        }
    }
}
