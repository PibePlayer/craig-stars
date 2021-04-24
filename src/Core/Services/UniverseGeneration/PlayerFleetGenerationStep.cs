using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Create fleets for each player based on their race settings
    /// </summary>
    public class PlayerFleetGenerationStep : UniverseGenerationStep
    {
        public PlayerFleetGenerationStep(Game game) : base(game, UniverseGenerationState.Fleets) { }

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
            foreach(var design in player.Designs.Where(d => !d.Hull.Starbase)) {
                fleets.Add(CreateFleet(design, player, 1, homeworld));
            }
            switch (player.Race.PRT)
            {
                // races with second planets get a scout
                case PRT.PP:
                case PRT.IT:
                    if (player.Planets.Count > 1)
                    {
                        // add a scout to the second world
                        fleets.Add(CreateFleet(player.GetLatestDesign(ShipDesignPurpose.Scout), player, 4, player.Planets[1]));
                    }
                    break;
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
            fleet.Name = $"{playerDesign.Name} #{fleet.Id}";
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
            fleet.Player = player;

            // aggregate all the design data
            fleet.ComputeAggregate();
            fleet.Fuel = fleet.Aggregate.FuelCapacity;

            return fleet;
        }
    }
}