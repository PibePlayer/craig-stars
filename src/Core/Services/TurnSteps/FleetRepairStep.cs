using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetRepairStep : TurnGenerationStep
    {
        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public FleetRepairStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider) : base(gameProvider, TurnGenerationState.FleetRepairStep)
        {
            this.rulesProvider = rulesProvider;
        }

        public override void Process()
        {
            foreach (var fleet in Game.Fleets.Where(fleet => fleet.Damage > 0))
            {
                RepairFleet(fleet, Game.Players[fleet.PlayerNum], fleet.Orbiting);
            }

            foreach (var planet in Game.OwnedPlanets.Where(planet => planet.HasStarbase && planet.Starbase.Damage > 0))
            {
                RepairStarbase(planet.Starbase, Game.Players[planet.PlayerNum]);
            }
        }

        /// <summary>
        /// Repair a fleet. This changes based on where the fleet is
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="player"></param>
        /// <param name="orbiting"></param>
        internal void RepairFleet(Fleet fleet, Player player, Planet orbiting)
        {
            RepairRate rate = RepairRate.Moving;
            if (fleet.Waypoints.Count == 1)
            {
                if (orbiting != null)
                {
                    if (fleet.Aggregate.Bomber && player.IsEnemy(orbiting.PlayerNum))
                    {
                        // no repairs while bombing
                        rate = RepairRate.None;
                    }
                    else
                    {
                        if (orbiting.OwnedBy(player))
                        {
                            rate = RepairRate.OrbitingOwnPlanet;
                        }
                        else
                        {
                            rate = RepairRate.Orbiting;
                        }
                    }
                }
                else
                {
                    rate = RepairRate.Stopped;
                }
            }

            var repairRate = Rules.RepairRates[rate];
            if (repairRate > 0)
            {
                // apply any bonuses for the fleet
                repairRate += fleet.Aggregate.RepairBonus;
                if (rate == RepairRate.OrbitingOwnPlanet && orbiting.HasStarbase)
                {
                    // apply any bonuses for the starbase if we own this planet and it has a starbase
                    repairRate += orbiting.Starbase.Aggregate.RepairBonus;
                }

                // IS races double repair
                var repairAmount = fleet.Aggregate.Armor * repairRate * player.Race.Spec.RepairFactor;

                // Remove damage from this fleet by its armor * repairRate
                fleet.Damage = Math.Max(0, (int)(fleet.Damage - repairAmount));
            }
        }


        /// <summary>
        /// Repair a starbase 
        /// </summary>
        /// <param name="starbase"></param>
        /// <param name="player"></param>
        internal void RepairStarbase(Starbase starbase, Player player)
        {
            var repairRate = Rules.RepairRates[RepairRate.Starbase];
            
            // IS races repair starbases 1.5x
            var repairAmount = starbase.Aggregate.Armor * repairRate * player.Race.Spec.StarbaseRepairFactor;

            // Remove damage from this fleet by its armor * repairRate
            starbase.Damage = Math.Max(0, (int)(starbase.Damage - repairAmount));
        }

    }
}