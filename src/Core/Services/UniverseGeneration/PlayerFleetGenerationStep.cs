using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Create fleets for each player based on their race settings and designs
    /// Note: This step will create all fleets in the Player's Designs, which are
    /// generated by the PlayerShipDesignsGenerationStep
    /// </summary>
    public class PlayerFleetGenerationStep : UniverseGenerationStep
    {
        private readonly ITechStore techStore;
        private readonly FleetSpecService fleetSpecService;

        public PlayerFleetGenerationStep(IProvider<Game> gameProvider, ITechStore techStore, FleetSpecService fleetSpecService) : base(gameProvider, UniverseGenerationState.Fleets)
        {
            this.techStore = techStore;
            this.fleetSpecService = fleetSpecService;
        }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                Game.Fleets.AddRange(GenerateFleets(player, player.Homeworld));
            });
        }

        internal List<Fleet> GenerateFleets(Player player, Planet homeworld)
        {
            var fleets = new List<Fleet>();
            var id = 1;

            foreach (StartingFleet startingFleet in player.Race.Spec.StartingFleets)
            {
                var hull = techStore.GetTechByName<TechHull>(startingFleet.HullName);
                var design = player.GetDesign(startingFleet.Name);
                fleets.Add(CreateFleet(design, player, id++, homeworld));
            }

            // add fleets to any extra planets, as defined in the PRTSpec
            var startingPlanets = player.Race.Spec.StartingPlanets;
            if (startingPlanets.Count > 0)
            {
                for (int i = 1; i < startingPlanets.Count; i++)
                {
                    var planet = player.Planets[i];
                    var startingPlanet = startingPlanets[i];
                    if (startingPlanet.StartingFleets != null)
                    {
                        foreach (StartingFleet startingFleet in startingPlanet.StartingFleets)
                        {
                            var hull = techStore.GetTechByName<TechHull>(startingFleet.HullName);
                            var design = player.GetDesign(startingFleet.Name);
                            fleets.Add(CreateFleet(design, player, id++, planet));
                        }
                    }
                }
            }

            return fleets;
        }

        internal Fleet CreateFleet(ShipDesign playerDesign, Player player, int id, Planet planet)
        {
            var design = Game.DesignsByGuid[playerDesign.Guid];
            var fleet = new Fleet()
            {
                Id = id
            };
            fleet.BaseName = $"{playerDesign.Name}";
            fleet.BattlePlan = player.BattlePlans[0];
            player.Stats.NumFleetsBuilt++;
            fleet.Tokens.Add(
                    new ShipToken()
                    {
                        Design = design,
                        Quantity = 1
                    }
                );
            fleet.Position = planet.Position;
            fleet.Orbiting = planet;
            fleet.Waypoints.Add(Waypoint.TargetWaypoint(fleet.Orbiting));
            planet.OrbitingFleets.Add(fleet);
            fleet.PlayerNum = player.Num;

            // spec all the design data
            fleetSpecService.ComputeFleetSpec(player, fleet);
            fleet.Fuel = fleet.Spec.FuelCapacity;

            return fleet;
        }
    }
}