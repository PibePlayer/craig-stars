using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetMineralsOnHandTile : PlanetTile
    {
        Label ironium;
        Label boranium;
        Label germaninum;

        Label mines;
        Label factories;

        public override void _Ready()
        {
            ironium = FindNode("Ironium") as Label;
            boranium = FindNode("Boranium") as Label;
            germaninum = FindNode("Germanium") as Label;

            mines = FindNode("Mines") as Label;
            factories = FindNode("Factories") as Label;

            base._Ready();
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                ironium.Text = $"{ActivePlanet.Planet.Cargo.Ironium}kT";
                boranium.Text = $"{ActivePlanet.Planet.Cargo.Boranium}kT";
                germaninum.Text = $"{ActivePlanet.Planet.Cargo.Germanium}kT";

                mines.Text = $"{ActivePlanet.Planet.Mines} of {ActivePlanet.Planet.MaxMines}";
                factories.Text = $"{ActivePlanet.Planet.Factories} of {ActivePlanet.Planet.MaxFactories}";
            }
        }

    }
}
