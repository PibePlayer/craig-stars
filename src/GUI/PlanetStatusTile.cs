using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    public class PlanetStatusTile : PlanetTile
    {
        Label population;

        public override void _Ready()
        {
            base._Ready();
            population = FindNode("Population") as Label;
        }

        protected override void UpdateControls()
        {
            base.UpdateControls();
            if (ActivePlanet != null)
            {
                population.Text = $"{ActivePlanet.Population:n0}";
            }
        }
    }
}
