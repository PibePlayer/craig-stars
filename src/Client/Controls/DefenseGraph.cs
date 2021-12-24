using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{

    public class DefenseGraph : Graph
    {
        static CSLog log = LogProvider.GetLogger(typeof(DefenseGraph));

        protected Player Me { get => PlayersManager.Me; }
        protected PublicGameInfo GameInfo { get => PlayersManager.GameInfo; }

        [Inject] PlanetService planetService;

        public TechDefense Defense { get; set; } = Techs.MissileBattery;

        public DefenseGraph()
        {
            XAxisLabels = Enumerable.Range(0, 6).Select(x => new AxisLabel((x * 20).ToString(), x * 20, Colors.White)).ToList();

            for (float yAxisValue = 0; yAxisValue <= 100; yAxisValue += 20)
            {
                YAxisLabels.Add(new AxisLabel($"{yAxisValue}%", yAxisValue, Colors.White));
            }
        }

        public override void _Ready()
        {
            this.ResolveDependencies();

            // for running the scene locally
            // PlayersManager.Me = new();
            // PlayersManager.GameInfo = new();
            base._Ready();
        }

        protected override List<Vector2> GetPoints()
        {
            var points = new List<Vector2>();

            if (Me != null)
            {
                var planet = new Planet()
                {
                    Hab = Me.Race.HabCenter,
                };
                planet.Population = planet.Spec.MaxPopulation;

                for (int numDefenses = 0; numDefenses <= 100; numDefenses += 20)
                {
                    planet.Defenses = numDefenses;
                    float defenseCoverage = planetService.GetDefenseCoverage(planet, Me, Defense);
                    float defenseCoverageSmart = planetService.GetDefenseCoverageSmart(planet, Me, Defense);

                    // each "box" in our graph has a unit of one. We have 6 boxes (0, 20, 40, 60, 80, 100%) so
                    // multiply by 6. 
                    points.Add(new Vector2(numDefenses, defenseCoverage * 6));

                    // TODO: add smartDefenseCoverage points
                }
            }

            return points;
        }

    }
}