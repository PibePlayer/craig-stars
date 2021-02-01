using Godot;
using System;

namespace CraigStars
{

    public class PlanetStarbaseTile : PlanetTile
    {
        Label title;
        Control statsContainer;

        Label dockCapacity;
        Label armor;
        Label shields;
        Label damage;
        Label massDriver;
        Label destination;
        Button setDestinationButton;

        public override void _Ready()
        {
            base._Ready();
            title = FindNode("Name") as Label;
            statsContainer = FindNode("StatsContainer") as Control;

            dockCapacity = FindNode("DockCapacity") as Label;
            armor = FindNode("Armor") as Label;
            shields = FindNode("Shields") as Label;
            damage = FindNode("Damage") as Label;
            massDriver = FindNode("MassDriver") as Label;
            destination = FindNode("Destination") as Label;
            setDestinationButton = FindNode("SetDestinationButton") as Button;
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                if (ActivePlanet.Planet.HasStarbase)
                {
                    title.Text = "Starbase";
                    statsContainer.Visible = true;

                    var starbase = ActivePlanet.Planet.Starbase;
                    dockCapacity.Text = starbase.DockCapacity == TechHull.UnlimitedSpaceDock ? "Unlimited" : $"{starbase.Aggregate.SpaceDock}kT";
                    armor.Text = $"{starbase.Aggregate.Armor}dp";
                    shields.Text = $"{starbase.Aggregate.Shield}dp";
                    damage.Text = starbase.Damage == 0 ? "none" : $"{starbase.Damage}dp";

                    // TODO: Fill out mass driver and stargate fields
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
