using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static CraigStars.Utils.Utils;

[assembly: InternalsVisibleTo("Core.Tests")]
namespace CraigStars
{
    /// <summary>
    /// Bombers orbiting enemy planets will Bomb planets
    /// ============================================================================
    /// Algorithms:
    /// Normalpopkills = sum[bomb_kill_perc(n)*#(n)] * (1-Def(pop))
    /// Minkills = sum[bomb_kill_min(n)*#(n)] * (1-Def(pop))
    ///
    /// 10 Cherry and 5 M-70 bombing vs 100 Neutron Defs (97.92%) 
    ///
    /// The calculations are, population kill:
    ///
    /// a    0.025 * 10  0.25        10 Cherry bombs
    /// b    0.012 * 5   0.06        5 M-70 bombs
    /// c    a + b       0.31        Total kill factor
    /// d    1 - 0.97    0.0208      1 - defense factor for 100 neutron defences
    /// e    c * d           0.006448    Total kill factor
    /// f    pop * c         64.48       Total colonists killed
    ///
    /// Minimum kill:
    ///
    /// a 10*300 + 5*300  4500   
    /// b 1 - 0.97        0.0208   1 - defense factor for 100 neutron defences
    /// c a *b            156      Total minimum kill
    /// ============================================================================    
    /// </summary>
    public class FleetBombStep : TurnGenerationStep
    {
        private readonly PlanetService planetService;

        public FleetBombStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.FleetBombStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            OwnedPlanets.ForEach(p =>
            {
                BombPlanet(p);
                if (p.Population == 0)
                {
                    EventManager.PublishPlanetPopulationEmptiedEvent(p);
                }
            });
        }

        internal void BombPlanet(Planet planet)
        {
            if (!planet.Owned || planet.Population == 0)
            {
                // can't bomb uninhabited planets
                return;
            }

            // get a list of all players orbiting the planet

            // find any enemy bombers orbiting this planet
            var enemyBombers = planet.OrbitingFleets.Where(fleet => fleet.Spec.Bomber && Game.Players[fleet.PlayerNum].IsEnemy(planet.PlayerNum));

            var orbitingPlayers = enemyBombers.Select(fleet => fleet.PlayerNum).ToHashSet();

            foreach (var player in orbitingPlayers)
            {
                BombPlanet(planet, Game.Players[planet.PlayerNum], Game.Players[player], enemyBombers.Where(fleet => fleet.PlayerNum == player));
                // stop bombing if everyone is dead
                if (planet.Population == 0)
                {
                    break;
                }
            }

            if (planet.Population > 0)
            {
                foreach (var player in orbitingPlayers)
                {
                    SmartBombPlanet(planet, Game.Players[planet.PlayerNum], Game.Players[player], enemyBombers.Where(fleet => fleet.PlayerNum == player));
                    // stop bombing if everyone is dead
                    if (planet.Population == 0)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Bomb a planet with normal bombs
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="attacker"></param>
        /// <param name="bombers"></param>
        void BombPlanet(Planet planet, Player defender, Player attacker, IEnumerable<Fleet> bombers)
        {

            // do all normal bombs
            foreach (var fleet in bombers)
            {
                if (fleet.Spec.Bombs.Count > 0)
                {
                    // figure out the killRate and minKill for this fleet's bombs
                    var defenseCoverage = planetService.GetDefenseCoverage(planet, defender);
                    var killRateColonistsKilled = RoundToNearest(GetColonistsKilled(planet.Population, defenseCoverage, fleet.Spec.Bombs));
                    var minColonistsKilled = RoundToNearest(GetMinColonistsKilled(planet.Population, defenseCoverage, fleet.Spec.Bombs));

                    var killed = Math.Max(killRateColonistsKilled, minColonistsKilled);
                    var leftoverPopulation = Math.Max(0, planet.Population - killed);
                    var actualKilled = planet.Population - leftoverPopulation;
                    planet.Population = leftoverPopulation;

                    // apply this against mines/factories and defenses proportionally
                    var structuresDestroyed = GetStructuresDestroyed(defenseCoverage, fleet.Spec.Bombs);
                    var totalStructures = planet.Mines + planet.Factories + planet.Defenses;
                    var leftoverMines = 0;
                    var leftoverFactories = 0;
                    var leftoverDefenses = 0;
                    if (totalStructures > 0)
                    {
                        leftoverMines = (int)Math.Max(0, planet.Mines - structuresDestroyed * ((float)planet.Mines / totalStructures));
                        leftoverFactories = (int)Math.Max(0, planet.Factories - structuresDestroyed * ((float)planet.Factories / totalStructures));
                        leftoverDefenses = (int)Math.Max(0, planet.Defenses - structuresDestroyed * ((float)planet.Defenses / totalStructures));
                    }

                    // make sure we only count stuctures that were actually destroyed
                    var minesDestroyed = planet.Mines - leftoverMines;
                    var factoriesDestroyed = planet.Factories - leftoverFactories;
                    var defensesDestroyed = planet.Defenses - leftoverDefenses;

                    planet.Mines = leftoverMines;
                    planet.Factories = leftoverFactories;
                    planet.Defenses = leftoverDefenses;

                    // let each player know a bombing happened
                    Message.PlanetBombed(attacker, planet, fleet, actualKilled, minesDestroyed, factoriesDestroyed, defensesDestroyed);
                    Message.PlanetBombed(defender, planet, fleet, actualKilled, minesDestroyed, factoriesDestroyed, defensesDestroyed);
                }
            }

        }

        /// <summary>
        /// Have a player smartbomb a planet
        /// </summary>
        /// <param name="planet"></param>
        /// <param name="attacker"></param>
        /// <param name="bombers"></param>
        void SmartBombPlanet(Planet planet, Player defender, Player attacker, IEnumerable<Fleet> bombers)
        {
            var smartDefenseCoverage = planetService.GetDefenseCoverageSmart(planet, defender, Game.Rules);
            // now do all smart bombs
            foreach (var fleet in bombers)
            {
                if (fleet.Spec.SmartBombs.Count > 0)
                {
                    // figure out the killRate and minKill for this fleet's bombs
                    var smartKilled = RoundToNearest(GetColonistsKilledWithSmartBombs(planet.Population, smartDefenseCoverage, fleet.Spec.SmartBombs));

                    var leftoverPopulation = Math.Max(0, planet.Population - smartKilled);
                    var actualKilled = planet.Population - leftoverPopulation;
                    planet.Population = leftoverPopulation;

                    // let each player know a bombing happened
                    Message.PlanetSmartBombed(attacker, planet, fleet, actualKilled);
                    Message.PlanetSmartBombed(defender, planet, fleet, actualKilled);
                }
            }

        }

        /// <summary>
        /// Get colonists killed using the KillRate of a bomb
        /// </summary>
        /// <param name="population"></param>
        /// <param name="defenseCoverage"></param>
        /// <param name="bombs"></param>
        /// <returns></returns>
        internal float GetColonistsKilled(int population, float defenseCoverage, List<Bomb> bombs)
        {
            // calculate the killRate for all these bombs
            var killRate = 0f;
            bombs.ForEach(bomb =>
            {
                killRate += bomb.KillRate * bomb.Quantity;
            });

            return GetColonistsKilled(population, defenseCoverage, killRate);
        }

        // Normal bombs versus buildings.

        //   Destroy_Build = sum[destroy_build_type(n)*#(n)] * (1-Def(build))

        // e.g. 10 Cherry + 5 M70 vs 100 Neutron Defs

        //                 = sum[10*10; 5*6] * (1-(97.92%/2)) 
        //                 = sum[100; 30] * (1-(48.96%)) 
        //                 = 130 * (1- 0.4896) 
        //                 = 130 * 0.5104 
        //                 = ~66 Buildings will be destroyed.

        // Building kills are allotted proportionately to each building type on 
        // the planet.  For example, a planet with 1000 installations (of all 
        // three types combined) taking 400 building kills will lose 40% of each 
        // of its factories, mines, and defenses.  If there had been 350 mines, 
        // 550 factories, and 100 defenses, the losses would be 140 mines, 220 
        // factories, and 40 defenses.

        // Normal bombs versus buildings.

        //   Destroy_Build = sum[destroy_build_type(n)*#(n)] * (1-Def(build))

        // e.g. 10 Cherry + 5 M70 vs 100 Neutron Defs

        //                 = sum[10*10; 5*6] * (1-(97.92%/2)) 
        //                 = sum[100; 30] * (1-(48.96%)) 
        //                 = 130 * (1- 0.4896) 
        //                 = 130 * 0.5104 
        //                 = ~66 Buildings will be destroyed.

        // Building kills are allotted proportionately to each building type on 
        // the planet.  For example, a planet with 1000 installations (of all 
        // three types combined) taking 400 building kills will lose 40% of each 
        // of its factories, mines, and defenses.  If there had been 350 mines, 
        // 550 factories, and 100 defenses, the losses would be 140 mines, 220 
        // factories, and 40 defenses.

        /// <summary>
        /// Get colonists killed using the KillRate of a bomb
        /// </summary>
        /// <param name="population"></param>
        /// <param name="defenseCoverage"></param>
        /// <param name="bombs"></param>
        /// <returns></returns>
        internal float GetStructuresDestroyed(float defenseCoverage, List<Bomb> bombs)
        {
            // calculate the killRate for all these bombs
            var structuresDestroyed = 0f;
            bombs.ForEach(bomb =>
            {
                structuresDestroyed += bomb.StructureDestroyRate * 10.0f * bomb.Quantity;
            });

            // this will destroy some number of structures that are allocated proportionally
            // among mines, factories and defenses
            // NOTE: defense coverage is halved for structures
            return structuresDestroyed * (1 - defenseCoverage * .5f);
        }


        /// <summary>
        /// Get the minimum number of colonists killed
        /// </summary>
        /// <param name="population"></param>
        /// <param name="defenseCoverage"></param>
        /// <param name="bombs"></param>
        /// <returns></returns>
        internal float GetMinColonistsKilled(int population, float defenseCoverage, List<Bomb> bombs)
        {
            // calculate the killRate for all these bombs
            var minKilled = 0;
            bombs.ForEach(bomb =>
            {
                minKilled += bomb.MinKillRate * bomb.Quantity;
            });

            return minKilled * (1 - defenseCoverage);
        }

        /// <summary>
        /// Get the number of colonists killed by a single bomb group (or calculated bomb group)
        /// </summary>
        /// <param name="population"></param>
        /// <param name="defenseCoverage"></param>
        /// <param name="killRate"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        internal float GetColonistsKilled(int population, float defenseCoverage, float killRate, int quantity = 1)
        {
            var killed = killRate / 100.0f * quantity * (1 - defenseCoverage) * population;

            return killed;
        }

        /// <summary>
        /// Get the number of colonists killed by smart bombs
        /// ============================================================================
        /// Each smart bomb type has a specific pop-kill percentage.  The values 
        /// given by _ONE_ bomb are summarized here:
        ///
        /// Smart              1.3% 
        /// Neutron            2.2% 
        /// Enriched Neutron   3.5% 
        /// Peerless           5.0% 
        /// Annihilator        7.0%
        ///
        /// Smart bombs do *not* add linearly; instead, they use this formula:
        ///
        ///   Pop_kill(smart) = (1-Def(smart))(1 - multiply[ (1 - kill_perc(n)^#n) ])
        ///
        /// Where "multiply[x(n)]" is the math "big-pi" operator, which means 
        /// multiply all the terms together, i.e.: 
        ///   multiply[x(n)] = x(n1)*x(n2)*x(n3)... *x(ni)
        ///
        /// e.g. 10 Annihilators + 5 neutron vs. 100 Neutron-Defs(Def(smart)=85.24%)
        ///
        ///                   = (1-85.24%) * (1 -  multiply[((1-7%)^10); ((1-2.2%)^5)]) 
        ///                   = (1-0.8524) * (1 -  ((1-0.07)^10) * ((1-0.022)^5)) 
        ///                   = 0.1476 * (1 - (0.93^10) * (0.978^5)) 
        ///                   = 0.1476 * (1 - 0.484 * 0.895) 
        ///                   = 0.1476 * 0.56682 
        ///                   = 0.0837 
        ///                   = 8.37% of planetary pop will be killed.
        /// ============================================================================
        /// </summary>
        /// <param name="population"></param>
        /// <param name="defenseCoverageSmart"></param>
        /// <param name="bombs"></param>
        /// <returns></returns>
        internal float GetColonistsKilledWithSmartBombs(int population, float defenseCoverageSmart, List<Bomb> bombs)
        {
            var smartKillRate = 0.0;
            bombs.ForEach(bomb =>
            {
                if (smartKillRate == 0)
                {
                    smartKillRate = Math.Pow(1 - bomb.KillRate / 100f, bomb.Quantity);
                }
                else
                {
                    smartKillRate *= Math.Pow(1 - bomb.KillRate / 100f, bomb.Quantity);
                }
            });

            if (smartKillRate != 0)
            {
                var percentKilled = (1 - defenseCoverageSmart) * (1 - smartKillRate);

                return (float)(population * percentKilled);
            }
            return 0;
        }
    }
}