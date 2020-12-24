using Godot;
using System;

public class PlanetStatus : MarginContainer
{
    Label population;

    Planet selectedPlanet;

    public override void _Ready()
    {
        population = FindNode("Population") as Label;

        Signals.MapObjectSelectedEvent += OnMapObjectSelected;
        Signals.TurnPassedEvent += OnTurnPassed;
    }

    void OnTurnPassed(int year)
    {
        UpdateLabels(selectedPlanet);
    }

    void OnMapObjectSelected(MapObject obj)
    {
        if (obj is Planet planet)
        {
            selectedPlanet = planet;
            UpdateLabels(selectedPlanet);
        }
    }

    void UpdateLabels(Planet planet)
    {
        if (planet != null)
        {
            population.Text = $"{planet.Population:n0}";
        }
    }
}
