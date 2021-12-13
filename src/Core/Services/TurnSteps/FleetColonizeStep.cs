using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class FleetColonize0Step : FleetColonizeStep
    {
        public FleetColonize0Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, FleetSpecService fleetSpecService)
            : base(gameProvider, rulesProvider, planetService, fleetSpecService, TurnGenerationState.FleetColonize0Step) { }
    }

    public class FleetColonize1Step : FleetColonizeStep
    {
        public FleetColonize1Step(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, FleetSpecService fleetSpecService)
            : base(gameProvider, rulesProvider, planetService, fleetSpecService, TurnGenerationState.FleetColonize1Step) { }
    }

    /// <summary>
    /// TurnGenerationStep to transfer fleets from one player to another
    /// </summary>
    public abstract class FleetColonizeStep : AbstractFleetWaypointProcessStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(FleetColonizeStep));

        private readonly PlanetService planetService;
        private readonly FleetSpecService fleetSpecService;

        public FleetColonizeStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, PlanetService planetService, FleetSpecService fleetSpecService, TurnGenerationState state)
            : base(gameProvider, rulesProvider, state, WaypointTask.Colonize)
        {
            this.planetService = planetService;
            this.fleetSpecService = fleetSpecService;
        }

        /// <summary>
        /// Merge this fleet with another fleet
        /// </summary>
        /// <param name="task"></param>
        /// <returns>true (we message the player if the merge fails, but the waypoint is considered processed)</returns>
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            if (wp.Target is Planet planet)
            {
                if (planet.Owned)
                {
                    Message.ColonizeOwnedPlanet(player, fleet);
                }
                else if (!fleet.Spec.Colonizer)
                {
                    Message.ColonizeWithNoModule(player, fleet);
                }
                else if (fleet.Cargo.Colonists <= 0)
                {
                    Message.ColonizeWithNoColonists(player, fleet);
                }
                else
                {
                    // we own this planet now, yay!
                    planet.PlayerNum = fleet.PlayerNum;
                    planet.ProductionQueue = new ProductionQueue();
                    if (task.Player.ProductionPlans.Count > 0)
                    {
                        // apply the default production plan
                        planetService.ApplyProductionPlan(planet.ProductionQueue.Items, task.Player, task.Player.ProductionPlans[0]);
                    }
                    planet.Population = fleet.Cargo.Colonists * 100;
                    planet.Mines = player.Race.Spec.InnateMining ? planetService.GetInnateMines(planet, player) : 0;

                    fleet.Cargo = fleet.Cargo.WithColonists(0);

                    if (fleet.Spec.OrbitalConstructionModule)
                    {
                        // get the Starter Colony for this player
                        var design = Game.Designs.Where(design => design.PlayerNum == player.Num && design.Purpose == ShipDesignPurpose.StarterColony).FirstOrDefault();
                        if (design != null)
                        {
                            planet.Starbase = new Starbase()
                            {
                                Name = design.Name,
                                Orbiting = planet,
                                Position = planet.Position,
                                BattlePlan = player.BattlePlans[0],

                                Tokens = new List<ShipToken>() {
                                    new ShipToken(design, 1)
                                }
                            };
                            fleetSpecService.ComputeStarbaseSpec(player, planet.Starbase, true);
                        }
                        else
                        {
                            log.Error("Uh oh, we tried to colonize a planet with an Orbital Construction Module but we have no Starter Colony design.");
                        }
                    }

                    planet.Spec = planetService.ComputePlanetSpec(planet, player);

                    Message.PlanetColonized(player, planet);
                    ScrapFleet(task.Fleet, task.Waypoint, task.Player);
                }
            }
            else
            {
                Message.ColonizeNonPlanet(player, fleet);
            }

            return true;
        }

    }
}