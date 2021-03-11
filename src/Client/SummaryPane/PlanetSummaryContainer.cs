using Godot;
using System;
using CraigStars.Singletons;

namespace CraigStars
{
    public class PlanetSummaryContainer : VBoxContainer
    {
        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        Player Me { get => PlayersManager.Me; }

        public PlanetSprite Planet
        {
            get => planet;
            set
            {
                planet = value;
                UpdateControls();
            }
        }
        PlanetSprite planet;

        Label valueLabel;
        Label populationLabel;
        Label reportAgeLabel;
        Label ownerLabel;

        HabBar gravHabBar;
        HabBar tempHabBar;
        HabBar radHabBar;

        MineralBar ironiumMineralBar;
        MineralBar boraniumMineralBar;
        MineralBar germaniumMineralBar;

        public override void _Ready()
        {
            valueLabel = (Label)FindNode("Value");
            reportAgeLabel = (Label)FindNode("ReportAge");
            populationLabel = (Label)FindNode("Population");
            ownerLabel = (Label)FindNode("Owner");

            gravHabBar = (HabBar)FindNode("GravHabBar");
            tempHabBar = (HabBar)FindNode("TempHabBar");
            radHabBar = (HabBar)FindNode("RadHabBar");

            ironiumMineralBar = (MineralBar)FindNode("IroniumMineralBar");
            boraniumMineralBar = (MineralBar)FindNode("BoraniumMineralBar");
            germaniumMineralBar = (MineralBar)FindNode("GermaniumMineralBar");

            Connect("visibility_changed", this, nameof(OnVisible));
            Signals.MapObjectSelectedEvent += OnMapObjectSelected;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.MapObjectSelectedEvent -= OnMapObjectSelected;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void OnVisible()
        {
            var race = Me.Race;
            gravHabBar.Low = race.HabLow.grav;
            tempHabBar.Low = race.HabLow.temp;
            radHabBar.Low = race.HabLow.rad;

            gravHabBar.High = race.HabHigh.grav;
            tempHabBar.High = race.HabHigh.temp;
            radHabBar.High = race.HabHigh.rad;
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            Planet = mapObject as PlanetSprite;
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            UpdateControls();
        }

        void UpdateControls()
        {
            if (Planet != null)
            {
                var race = Me.Race;
                var planet = Planet.Planet;
                if (planet.Explored && planet.Hab is Hab hab)
                {
                    int habValue = race.GetPlanetHabitability(hab);
                    valueLabel.Text = $"{habValue}%";
                    // TODO: Add terraforming
                    if (habValue > 0)
                    {
                        valueLabel.Modulate = GUIColors.HabitablePlanetTextColor;
                    }
                    else
                    {
                        valueLabel.Modulate = GUIColors.UninhabitablePlanetTextColor;
                    }

                    if (planet.ReportAge == 0)
                    {
                        reportAgeLabel.Text = "Report is current";
                    }
                    else if (planet.ReportAge == 1)
                    {
                        reportAgeLabel.Text = "Report 1 year old";
                    }
                    else
                    {
                        reportAgeLabel.Text = $"Report {planet.ReportAge} years old";
                    }

                    if (planet.Population > 0)
                    {
                        if (planet.OwnedBy(Me))
                        {
                            populationLabel.Text = $"{planet.Population:n0}";
                        }
                        else
                        {
                            populationLabel.Text = $"±{planet.Population:n0}";
                        }
                    }
                    else
                    {
                        populationLabel.Text = "uninhabited";
                    }

                    if (planet.OwnedBy(Me))
                    {
                        ownerLabel.Text = "";
                    }
                    else if (planet.Owner != null)
                    {
                        ownerLabel.Text = planet.Owner.RacePluralName;
                        ownerLabel.Modulate = planet.Owner.Color;
                    }

                    gravHabBar.HabValue = hab.grav;
                    tempHabBar.HabValue = hab.temp;
                    radHabBar.HabValue = hab.rad;

                    ironiumMineralBar.Concentration = planet.MineralConcentration.Ironium;
                    boraniumMineralBar.Concentration = planet.MineralConcentration.Boranium;
                    germaniumMineralBar.Concentration = planet.MineralConcentration.Germanium;

                    ironiumMineralBar.Surface = planet.Cargo.Ironium;
                    boraniumMineralBar.Surface = planet.Cargo.Boranium;
                    germaniumMineralBar.Surface = planet.Cargo.Germanium;
                }
            }
        }
    }
}