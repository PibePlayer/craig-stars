using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetMineralsOnHandTile : PlanetTile
    {
        Label ironium;
        Label boranium;
        Label germanium;

        Label mines;
        Label factories;

        public override void _Ready()
        {
            ironium = FindNode("Ironium") as Label;
            boranium = FindNode("Boranium") as Label;
            germanium = FindNode("Germanium") as Label;

            mines = FindNode("Mines") as Label;
            factories = FindNode("Factories") as Label;

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

    }
}
