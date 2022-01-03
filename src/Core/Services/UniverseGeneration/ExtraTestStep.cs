using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// This is a test step for adding any code for changing the universe
    /// </summary>
    public class ExtraTestStep : UniverseGenerationStep
    {
        private readonly ITechStore techStore;
        private readonly FleetSpecService fleetSpecService;
        private readonly PlanetService planetService;

        public ExtraTestStep(IProvider<Game> gameProvider, ITechStore techStore, FleetSpecService fleetSpecService, PlanetService planetService) : base(gameProvider, UniverseGenerationState.Fleets)
        {
            this.techStore = techStore;
            this.fleetSpecService = fleetSpecService;
            this.planetService = planetService;
        }

        public override void Process()
        {
            AddStealCargoTest();
        }

        void AddStealCargoTest()
        {
            if (Game.Players.Count > 1)
            {
                var planet1 = Game.Planets.Find(planet => !planet.Owned);
                var player1 = Game.Players[0];
                var player2 = Game.Players[1];

                // take over this planet
                planet1.PlayerNum = player2.Num;
                planet1.RaceName = player2.Race.Name;
                planet1.RacePluralName = player2.Race.PluralName;
                planet1.Cargo = new Cargo(100, 200, 300, 400);
                planet1.ProductionQueue = new ProductionQueue();
                planet1.Spec = planetService.ComputePlanetSpec(planet1, player2);

                // get another one
                var planet2 = Game.Planets.Find(planet => !planet.Owned);
                planet2.PlayerNum = player2.Num;
                planet2.RaceName = player2.Race.Name;
                planet2.RacePluralName = player2.Race.PluralName;
                planet2.Cargo = new Cargo(100, 200, 300, 400);
                planet2.ProductionQueue = new ProductionQueue();
                planet2.Spec = planetService.ComputePlanetSpec(planet2, player2);

                var stealerDesign = ShipDesigns.RobberBaroner.Clone(player1);
                Game.Designs.Add(stealerDesign);
                var stealerFleet = new Fleet()
                {
                    BaseName = "Pick Pocketer",
                    PlayerNum = player1.Num,
                    RaceName = player1.Race.Name,
                    RacePluralName = player1.Race.PluralName,
                    Position = planet1.Position,
                    Orbiting = planet1,
                    BattlePlan = player1.BattlePlans[0],
                    Waypoints = new List<Waypoint>() { Waypoint.TargetWaypoint(planet1) },
                    Tokens = new List<ShipToken>() { new ShipToken(stealerDesign, 1) }
                };
                player1.Stats.NumFleetsBuilt++;
                fleetSpecService.ComputeDesignSpec(player1, stealerDesign);
                fleetSpecService.ComputeFleetSpec(player1, stealerFleet);
                Game.AddMapObject(stealerFleet);

                var markDesign = ShipDesigns.Teamster.Clone(player2);
                Game.Designs.Add(markDesign);
                var markFleet = new Fleet()
                {
                    BaseName = "Mark",
                    PlayerNum = player2.Num,
                    RaceName = player2.Race.Name,
                    RacePluralName = player2.Race.PluralName,
                    Position = planet1.Position,
                    Orbiting = planet1,
                    BattlePlan = player2.BattlePlans[0],
                    Waypoints = new List<Waypoint>() { Waypoint.TargetWaypoint(planet1) },
                    Tokens = new List<ShipToken>() { new ShipToken(markDesign, 1) },
                    Cargo = new Cargo(10, 20, 30, 40),
                };
                player2.Stats.NumFleetsBuilt++;
                fleetSpecService.ComputeDesignSpec(player2, markDesign);
                fleetSpecService.ComputeFleetSpec(player2, markFleet);
                Game.AddMapObject(markFleet);

                // make a freighter with a nice pen scanner that can't steal
                var nonStealerDesign = ShipDesigns.Teamster.Clone(player1);
                nonStealerDesign.Slots[1].HullComponent = Techs.ChameleonScanner;
                Game.Designs.Add(nonStealerDesign);
                var nonStealerFleet = new Fleet()
                {
                    BaseName = "No-Can-Steal",
                    PlayerNum = player1.Num,
                    RaceName = player1.Race.Name,
                    RacePluralName = player1.Race.PluralName,
                    Position = planet2.Position,
                    Orbiting = planet2,
                    BattlePlan = player1.BattlePlans[0],
                    Waypoints = new List<Waypoint>() { Waypoint.TargetWaypoint(planet2) },
                    Tokens = new List<ShipToken>() { new ShipToken(nonStealerDesign, 1) }
                };
                player1.Stats.NumFleetsBuilt++;
                fleetSpecService.ComputeDesignSpec(player1, nonStealerDesign);
                fleetSpecService.ComputeFleetSpec(player1, nonStealerFleet);
                Game.AddMapObject(nonStealerFleet);

            }
        }
    }
}