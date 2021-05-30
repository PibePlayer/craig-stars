using System;
using CraigStars.Singletons;
using Godot;

namespace CraigStars.Client
{
    public class PlanetMineralsOnHandTile : PlanetTile
    {
        Label ironium;
        Label boranium;
        Label germanium;

        Label mines;
        Label factories;

        MineralTooltip mineralTooltip;

        public override void _Ready()
        {
            ironium = FindNode("Ironium") as Label;
            boranium = FindNode("Boranium") as Label;
            germanium = FindNode("Germanium") as Label;

            mines = FindNode("Mines") as Label;
            factories = FindNode("Factories") as Label;

            mineralTooltip = GetNode<MineralTooltip>("MineralTooltip");

            ironium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Ironium });
            boranium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Boranium });
            germanium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Germanium });

            base._Ready();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (CommandedPlanet != null)
            {
                ironium.Text = $"{CommandedPlanet.Planet.Cargo.Ironium}kT";
                boranium.Text = $"{CommandedPlanet.Planet.Cargo.Boranium}kT";
                germanium.Text = $"{CommandedPlanet.Planet.Cargo.Germanium}kT";

                mines.Text = $"{CommandedPlanet.Planet.Mines} of {CommandedPlanet.Planet.MaxMines}";
                factories.Text = $"{CommandedPlanet.Planet.Factories} of {CommandedPlanet.Planet.MaxFactories}";
            }
        }

        void OnMineralGuiInput(InputEvent @event, MineralType type)
        {
            if (@event.IsActionPressed("viewport_alternate_select"))
            {
                mineralTooltip.Type = type;
                mineralTooltip.Planet = CommandedPlanet?.Planet;
                mineralTooltip.SetGlobalPosition(GetGlobalMousePosition());
                mineralTooltip.OnAboutToShow();
                mineralTooltip.Show();
            }
            else if (@event.IsActionReleased("viewport_alternate_select"))
            {
                mineralTooltip.Hide();
            }
        }

    }
}
