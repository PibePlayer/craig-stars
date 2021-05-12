using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
{

    public class PlanetStarbaseTile : PlanetTile
    {
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
            statsGrid = GetNode<Container>("StatsGrid");

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

            setDestinationButton.Connect("pressed", this, nameof(OnSetDestinationButtonPressed));

            statsGrid.Connect("gui_input", this, nameof(OnStatsGridGUIInput));

            Signals.PacketDestinationChangedEvent += OnPacketDestinationChanged;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            Signals.PacketDestinationChangedEvent -= OnPacketDestinationChanged;
        }

        void OnPacketDestinationChanged(Planet planet, Planet target)
        {
            if (planet == CommandedPlanet.Planet)
            {
                UpdateControls();
            }
        }

        void OnSetDestinationButtonPressed()
        {
            Signals.PublishPacketDestinationToggleEvent();
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
                    UpdateTitle($"{starbase.Name} v{starbase.Tokens[0].Design.Version}");
                    Visible = true;

                    dockCapacity.Text = starbase.DockCapacity == TechHull.UnlimitedSpaceDock ? "Unlimited" : $"{starbase.Aggregate.SpaceDock}kT";
                    armor.Text = $"{starbase.Aggregate.Armor}dp";
                    shields.Text = $"{starbase.Aggregate.Shield}dp";
                    damage.Text = starbase.Damage == 0 ? "none" : $"{starbase.Damage}dp";

                    if (starbase.Aggregate.HasStargate)
                    {
                        var gate = starbase.Aggregate.Stargate;
                        var safeHullMass = $"{(gate.SafeHullMass == TechHullComponent.InfinteGate ? "any" : $"{gate.SafeHullMass}kT")}";
                        var safeRange = $"{(gate.SafeRange == TechHullComponent.InfinteGate ? "any" : $"{gate.SafeRange} l.y.")}";
                        stargate.Text = $"{safeHullMass}/{safeRange}";
                    }
                    if (starbase.Aggregate.HasMassDriver)
                    {
                        destination.Visible = true;
                        destinationLabel.Visible = true;
                        setDestinationButton.Visible = true;
                        massDriver.Text = $"Warp {starbase.Aggregate.MassDriver.PacketSpeed}";
                        if (CommandedPlanet.Planet.PacketTarget != null)
                        {
                            destination.Text = CommandedPlanet.Planet.PacketTarget.Name;
                        }
                        else
                        {
                            destination.Text = "none";
                        }
                    }
                }
                else
                {
                    UpdateTitle("No Starbase");
                    Visible = false;
                }

            }
        }
    }
}
