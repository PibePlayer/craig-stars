using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    /// <summary>
    /// This service manages damaging fleets hit/detonated by minefields
    /// </summary>
    public class MineFieldDamager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MineFieldDamager));

        /// <summary>
        /// Hit a minefield, taking damage
        /// https://wiki.starsautohost.org/wiki/Guts_of_Minefields
        /// 
        /// </summary>
        /// <param name="fleet"></param>
        public void TakeMineFieldDamage(Fleet fleet, MineField mineField, MineFieldStats stats, bool detonating = false)
        {
            // if any ship has ramscoops, everyone takes more damage
            var hasRamScoop = fleet.Tokens.Any(token => token.Design.Aggregate.Engine.FreeSpeed > 1);
            var minDamage = hasRamScoop ? stats.MinDamagePerFleetRS : stats.MinDamagePerFleet;
            var damagePerEngine = hasRamScoop ? stats.DamagePerEngineRS : stats.DamagePerEngine;

            var totalDamage = 0;
            var shipsDestroyed = 0;

            // only calculate damage if we are a damaging minefield
            if (minDamage > 0)
            {
                if (fleet.Aggregate.TotalShips <= 5)
                {
                    int firstDesignNumEngines = 0;
                    // we apply the min damage here, but it's based on number of engines
                    foreach (var token in fleet.Tokens)
                    {
                        var design = token.Design;

                        // the first token takes the min damage for the fleet * the number of engines
                        if (firstDesignNumEngines == 0)
                        {
                            firstDesignNumEngines = design.Aggregate.NumEngines;
                            var tokenDamage = firstDesignNumEngines * minDamage;
                            totalDamage += tokenDamage;
                            var result = token.ApplyMineDamage(tokenDamage);
                            shipsDestroyed += result.shipsDestroyed;
                        }
                        else
                        {
                            // We applied the minimum to the first token
                            // we only damage additional tokens if they have more engines
                            if (design.Aggregate.NumEngines > firstDesignNumEngines)
                            {
                                // i.e. if the next design has 4 engines instead of 2, we apply
                                // 125 * 2 or 250 damage 
                                var tokenDamage = damagePerEngine * (design.Aggregate.NumEngines - firstDesignNumEngines) * token.Quantity;
                                totalDamage += tokenDamage;
                                var result = token.ApplyMineDamage(tokenDamage);
                                shipsDestroyed += result.shipsDestroyed;
                            }
                        }
                    }
                }
                else
                {
                    // we have more than 5 ships, so min damage doesn't apply. Each ship
                    // takes 
                    foreach (var token in fleet.Tokens)
                    {
                        var design = token.Design;
                        var tokenDamage = damagePerEngine * (design.Aggregate.NumEngines) * token.Quantity;
                        totalDamage += tokenDamage;
                        var result = token.ApplyMineDamage(tokenDamage);
                        shipsDestroyed += result.shipsDestroyed;
                    }
                }
            }

            // send messages to players
            Message.FleetHitMineField(fleet.Player, fleet, mineField, totalDamage, shipsDestroyed);
            Message.FleetHitMineField(mineField.Player, fleet, mineField, totalDamage, shipsDestroyed);

            // remove any complete destroyed tokens.
            fleet.Tokens = fleet.Tokens.Where(token => token.Quantity > 0).ToList();
            if (fleet.Tokens.Count == 0)
            {
                EventManager.PublishMapObjectDeletedEvent(fleet);
            }

        }

        /// <summary>
        /// When a minefield is collided with, reduce it's number of mines
        /// </summary>
        /// <param name="mineField"></param>
        public void ReduceMineFieldOnImpact(MineField mineField)
        {
            long numMines = mineField.NumMines;
            if (numMines <= 10)
            {
                numMines = 0;
                EventManager.PublishMapObjectDeletedEvent(mineField);
            }
            else if (numMines <= 200)
            {
                numMines -= 10;
            }
            else if (numMines <= 1000)
            {
                numMines = (long)(numMines * 0.95);
            }
            else if (numMines <= 5000)
            {
                numMines -= 50;
            }
            else
            {
                numMines = (long)(numMines * 0.95);
            }
        }
    }
}

