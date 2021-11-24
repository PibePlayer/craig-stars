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
        MinesTooltip minesTooltip;
        FactoriesTooltip factoriesTooltip;

        public override void _Ready()
        {
            ironium = FindNode("Ironium") as Label;
            boranium = FindNode("Boranium") as Label;
            germanium = FindNode("Germanium") as Label;

            mines = FindNode("Mines") as Label;
            factories = FindNode("Factories") as Label;

            mineralTooltip = GetNode<MineralTooltip>("VBoxContainer/Controls/CanvasLayer/MineralTooltip");
            minesTooltip = GetNode<MinesTooltip>("VBoxContainer/Controls/CanvasLayer/MinesTooltip");
            factoriesTooltip = GetNode<FactoriesTooltip>("VBoxContainer/Controls/CanvasLayer/FactoriesTooltip");

            ironium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Ironium });
            boranium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Boranium });
            germanium.Connect("gui_input", this, nameof(OnMineralGuiInput), new Godot.Collections.Array() { MineralType.Germanium });
            mines.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { minesTooltip });
            factories.Connect("gui_input", this, nameof(OnTooltipGuiInput), new Godot.Collections.Array() { factoriesTooltip });

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

                mines.Text = $"{CommandedPlanet.Planet.Mines} of {CommandedPlanet.Planet.Spec.MaxMines}";
                factories.Text = $"{CommandedPlanet.Planet.Factories} of {CommandedPlanet.Planet.Spec.MaxFactories}";
            }
        }

        void OnMineralGuiInput(InputEvent @event, MineralType type)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                mineralTooltip.ShowAtMouse(CommandedPlanet?.Planet, type);
            }
            else if (@event.IsActionReleased("ui_select"))
            {
                mineralTooltip.Hide();
            }
        }

        void OnTooltipGuiInput(InputEvent @event, CSTooltip tooltip)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                tooltip.ShowAtMouse(CommandedPlanet?.Planet);
            }
            else if (@event.IsActionReleased("ui_select"))
            {
                tooltip.Hide();
            }
        }

    }
}
