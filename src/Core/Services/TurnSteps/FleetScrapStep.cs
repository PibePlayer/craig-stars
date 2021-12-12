using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public class FleetScrapStep : AbstractFleetWaypointProcessStep
    {
        public FleetScrapStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider)
            : base(gameProvider, rulesProvider, TurnGenerationState.FleetScrapStep, WaypointTask.ScrapFleet)
        {
        }

        /// <summary>
        /// Scrap this fleet, adding resources to the waypoint
        /// from the stars wiki:
        /// After battle, 1/3 of the mineral cost of the destroyed ships is left as salvage. If the battle took place in orbit, these minerals are deposited on the planet below.
        /// In deep space, each type of mineral decays 10%, or 10kT per year, whichever is higher. Salvage deposited on planets does not decay.
        /// Scrapping: (from help file)
        /// 
        /// A ship scrapped at a starbase deposits 80% of the original minerals on the planet, or 90% of the minerals and 70% of the resources if the LRT 'Ultimate Recycling' is selected.
        /// A ship scrapped at a planet with no starbase leaves 33% of the original minerals on the planet, or 45% of the minerals if the LRT Ultimate Recycling is selected.
        /// Wih UR the resources recovered is:
        /// (resources the ship costs * resources on the planet)/(resources the ship cost + resources on the planet)
        /// The maximum recoverable resources occurs when the cost of the scrapped ship equals the resources produced at the planet where it is scrapped.
        /// 
        /// A ship scrapped in space leaves no minerals behind.
        /// When a ship design is deleted, all such ships vanish leaving nothing behind. (moral: scrap before you delete!)
        /// </summary>
        /// <param name="wp">The waypoint pointing to the target planet to colonize</param>    
        protected override bool ProcessTask(FleetWaypointProcessTask task)
        {
            var (fleet, wp, player) = task;

            ScrapFleet(fleet, wp, player);

            // this task is complete
            return true;
        }
    }
}