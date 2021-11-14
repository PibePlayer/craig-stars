using System;
using System.Linq;
using System.Collections.Generic;
using static CraigStars.Utils.Utils;

namespace CraigStars
{

    /// <summary>
    /// Invade planets
    /// 
    /// From starswiki:
    /// 
    /// (attackers) = (attackers)*(1 - .75*(defense coverage))
    /// (attack bonus) = 1.1*(1 + 0.5*(is attacker WM?)) 
    /// (defense bonus) = 1 + (is defender IS?)
    /// IF (attackers)*(attack bonus) > (defenders)*(defense bonus)
    ///     (owner) = (attacker race) (pop) = (attackers) - (defenders)*(defense bonus)/(attack bonus)
    /// ELSE (owner) = (defender race) (pop) = (defenders) - (attackers)*(attack bonus)/(defense bonus)
    /// </summary>
    public class InvasionProcessor
    {
        private readonly PlayerService playerService;
        private readonly PlanetService planetService;

        public InvasionProcessor(PlayerService playerService, PlanetService planetService)
        {
            this.playerService = playerService;
            this.planetService = planetService;
        }

        /// <summary>
        /// An attacker is invading a planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="attacker"></param>
        /// <param name="fleet">The invading fleet</param>
        /// <param name="colonistsDropped"></param>
        public void InvadePlanet(Planet planet, Player defender, Player attacker, Fleet fleet, int colonistsDropped)
        {
            if (!planet.Owned || planet.Population == 0)
            {
                // can't invade uninhabited planets
                Message.InvadeEmptyPlanet(attacker, fleet, planet);
                return;
            }

            // figure out how many attackers are stopped by defenses
            int attackers = (int)(colonistsDropped * (1 - planetService.GetDefenseCoverage(planet, defender) * attacker.Rules.InvasionDefenseCoverageFactor));
            int defenders = planet.Population;

            // determine bonuses for warmongers and inner strength
            float attackBonus = playerService.GetInvasionAttackBonus(attacker.Race);
            float defenseBonus = playerService.GetInvasionDefendBonus(defender.Race);

            if (attackers * attackBonus > defenders * defenseBonus)
            {
                var remainingAttackers = RoundToNearest(attackers - defenders * defenseBonus / attackBonus);

                // if we have a last-person-standing, they instantly repopulate. :)
                if (remainingAttackers == 0)
                {
                    remainingAttackers = 100;
                }

                var attackersKilled = colonistsDropped - remainingAttackers;

                // notify each player of the invasion                
                Message.PlanetInvaded(defender, planet, fleet, attackersKilled, planet.Population);
                Message.PlanetInvaded(attacker, planet, fleet, attackersKilled, planet.Population);

                // take over the planet.
                // empty this planet
                planet.PlayerNum = attacker.Num;
                planet.RaceName = attacker.RaceName;
                planet.RacePluralName = attacker.RacePluralName;
                planet.Starbase = null;
                planet.Scanner = false;
                planet.Defenses = 0; // defenses are destroyed during invasion
                planet.ProductionQueue = new ProductionQueue();
                planet.Population = remainingAttackers;

            }
            else
            {
                var remainingDefenders = RoundToNearest(defenders - (attackers * attackBonus) / defenseBonus);

                // if we have a last-person-standing, they instantly repopulate. :)
                if (remainingDefenders == 0)
                {
                    remainingDefenders = 100;
                }
                int defendersKilled = planet.Population - remainingDefenders;

                // notify each player of the invasion                
                Message.PlanetInvaded(defender, planet, fleet, colonistsDropped, defendersKilled);
                Message.PlanetInvaded(attacker, planet, fleet, colonistsDropped, defendersKilled);

                // reduce the population to however many colonists remain
                planet.Population = remainingDefenders;
            }
        }

    }
}