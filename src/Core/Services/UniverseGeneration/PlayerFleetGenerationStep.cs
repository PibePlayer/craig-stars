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
        private readonly PlayerService playerService;

        public PlayerFleetGenerationStep(IProvider<Game> gameProvider, ITechStore techStore, PlayerService playerService) : base(gameProvider, UniverseGenerationState.Fleets)
        {
            this.techStore = techStore;
            this.playerService = playerService;
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

            foreach (StartingFleet startingFleet in playerService.GetStartingFleets(player.Race))
            {
                var hull = techStore.GetTechByName<TechHull>(startingFleet.HullName);
                var design = player.GetDesign(startingFleet.Name);
                fleets.Add(CreateFleet(design, player, id++, homeworld));
            }

            // add fleets to any extra planets, as defined in the PRTSpec
            var startingPlanets = playerService.GetStartingPlanets(player.Race);
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
            fleet.Name = $"{fleet.BaseName} #{fleet.Id}";
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

            // aggregate all the design data
            fleet.ComputeAggregate(player);
            fleet.Fuel = fleet.Aggregate.FuelCapacity;

            return fleet;
        }
    }
}