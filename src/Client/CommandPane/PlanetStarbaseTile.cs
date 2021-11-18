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

        public override void _Ready()
        {
            base._Ready();
            statsGrid = GetNode<Container>("VBoxContainer/Controls/StatsGrid");

            dockCapacity = FindNode("DockCapacity") as Label;
            armor = FindNode("Armor") as Label;
            shields = FindNode("Shields") as Label;
            damage = FindNode("Damage") as Label;
            stargate = FindNode("Stargate") as Label;
            massDriver = FindNode("MassDriver") as Label;
            destination = FindNode("Destination") as Label;
            destinationLabel = FindNode("DestinationLabel") as Label;
            setDestinationButton = FindNode("SetDestinationButton") as Button;

            setDestinationButton.Connect("pressed", this, nameof(OnSetDestinationButtonPressed));

            statsGrid.Connect("gui_input", this, nameof(OnStatsGridGUIInput));

            EventManager.PacketDestinationChangedEvent += OnPacketDestinationChanged;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.PacketDestinationChangedEvent -= OnPacketDestinationChanged;
            }
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
            EventManager.PublishPacketDestinationToggleEvent();
        }

        void OnStatsGridGUIInput(InputEvent @event)
        {
            if (CommandedPlanet != null && CommandedPlanet.Planet.HasStarbase && @event.IsActionPressed("viewport_select"))
            {
                GetTree().SetInputAsHandled();

                HullSummaryPopup.Instance.Hull = CommandedPlanet.Planet.Starbase.Design.Hull;
                HullSummaryPopup.Instance.ShipDesign = CommandedPlanet.Planet.Starbase.Design;
                HullSummaryPopup.ShowAtMouse();
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

                    dockCapacity.Text = starbase.DockCapacity == TechHull.UnlimitedSpaceDock ? "Unlimited" : $"{starbase.Spec.SpaceDock}kT";
                    armor.Text = $"{starbase.Spec.Armor}dp";
                    shields.Text = $"{starbase.Spec.Shield}dp";
                    damage.Text = starbase.Damage == 0 ? "none" : $"{starbase.Damage}dp";

                    if (starbase.Spec.HasStargate)
                    {
                        var gate = starbase.Spec.Stargate;
                        var safeHullMass = $"{(gate.SafeHullMass == TechHullComponent.InfinteGate ? "any" : $"{gate.SafeHullMass}kT")}";
                        var safeRange = $"{(gate.SafeRange == TechHullComponent.InfinteGate ? "any" : $"{gate.SafeRange} l.y.")}";
                        stargate.Text = $"{safeHullMass}/{safeRange}";
                    }
                    if (starbase.Spec.HasMassDriver)
                    {
                        destination.Visible = true;
                        destinationLabel.Visible = true;
                        setDestinationButton.Visible = true;
                        massDriver.Text = $"Warp {starbase.Spec.SafePacketSpeed}";
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
