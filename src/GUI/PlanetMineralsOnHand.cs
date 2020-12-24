using Godot;
using System;

public class PlanetMineralsOnHand : MarginContainer
{
    Label ironium;
    Label boranium;
    Label germaninum;

    Label mines;
    Label factories;

    Planet selectedPlanet;

    public override void _Ready()
    {
        ironium = FindNode("Ironium") as Label;
        boranium = FindNode("Boranium") as Label;
        germaninum = FindNode("Germanium") as Label;

        mines = FindNode("Mines") as Label;
        factories = FindNode("Factories") as Label;

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
            ironium.Text = $"{planet.Cargo.Ironium}kT";
            boranium.Text = $"{planet.Cargo.Boranium}kT";
            germaninum.Text = $"{planet.Cargo.Germanium}kT";

            mines.Text = $"{planet.Mines} of {planet.MaxMines}";
            factories.Text = $"{planet.Factories} of {planet.MaxFactories}";
        }
    }

}
