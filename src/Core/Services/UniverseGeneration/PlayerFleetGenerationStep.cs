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
            fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.LongRangeScount.Name), player, 1, homeworld));
            fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.SantaMaria.Name), player, 1, homeworld));
            switch (player.Race.PRT)
            {
                case PRT.SD:
                    fleets.Add(CreateFleet(player.GetLatestDesign(ShipDesignPurpose.DamageMineLayer), player, 1, homeworld));
                    fleets.Add(CreateFleet(player.GetLatestDesign(ShipDesignPurpose.SpeedMineLayer), player, 1, homeworld));
                    break;
                case PRT.JoaT:
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.ArmoredProbe.Name), player, 2, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.Teamster.Name), player, 4, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.CottonPicker.Name), player, 5, homeworld));
                    fleets.Add(CreateFleet(player.GetDesign(ShipDesigns.StalwartDefender.Name), player, 6, homeworld));
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