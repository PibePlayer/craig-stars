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

        void UpdateControls()
        {
            if (ActivePlanet != null)
            {
                ironium.Text = $"{ActivePlanet.Cargo.Ironium}kT";
                boranium.Text = $"{ActivePlanet.Cargo.Boranium}kT";
                germaninum.Text = $"{ActivePlanet.Cargo.Germanium}kT";

                mines.Text = $"{ActivePlanet.Mines} of {ActivePlanet.MaxMines}";
                factories.Text = $"{ActivePlanet.Factories} of {ActivePlanet.MaxFactories}";
            }
        }

    }
}
