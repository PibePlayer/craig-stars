
namespace CraigStars
{
    public class FleetScrapperService
    {
        private readonly IRulesProvider rulesProvider;
        protected Rules Rules => rulesProvider.Rules;

        public FleetScrapperService(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
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
        /// <param name="player"></param>
        /// <param name="fleet"></param>
        /// <param name="planet"></param>
        public void ScrapFleet(Player player, Fleet fleet, Planet planet)
        {
            // create a new cargo instance out of our fleet cost
            Mineral scrappedMinerals = fleet.Spec.Cost;

            float scrapMineralFactor = Rules.ScrapMineralAmount;
            float scrapResourceFactor = Rules.ScrapResourceAmount;
            int extraResources = 0;
            int planetResources = 0;

            if (planet != null && planet.OwnedBy(player))
            {
                planetResources = planet.Spec.ResourcesPerYear;
                // UR races get resources when scrapping 
                if (planet.HasStarbase)
                {
                    // scrapping over a planet we own with a starbase, calculate bonus minerals and resources
                    scrapMineralFactor += player.Race.Spec.ScrapMineralOffsetStarbase;
                    scrapResourceFactor += player.Race.Spec.ScrapResourcesOffsetStarbase;
                }
                else
                {
                    // scrapping over a planet we own without a starbase, calculate bonus minerals and resources
                    scrapMineralFactor += player.Race.Spec.ScrapMineralOffset;
                    scrapResourceFactor += player.Race.Spec.ScrapResourcesOffset;
                }
            }

            // figure out much cargo and resources we get
            scrappedMinerals *= scrapMineralFactor;

            if (scrapResourceFactor > 0)
            {
                // Formula for calculating resources: (Current planet production * Extra resources)/(Current planet production + Extra Resources)
                extraResources = (int)(fleet.Spec.Cost.Resources * scrapResourceFactor + .5f);
                extraResources = (int)((planetResources * extraResources) / (planetResources + extraResources));
            }

            // add in any mineral cargo the fleet was holding
            scrappedMinerals += fleet.Cargo;
            fleet.Scrapped = true;

            if (planet != null)
            {
                planet.Cargo += scrappedMinerals;
                if (planet.OwnedBy(player))
                {
                    planet.BonusResources += extraResources;
                }
            }
            else
            {
                var salvage = new Salvage()
                {
                    PlayerNum = fleet.PlayerNum,
                    Name = "Salvage",
                    Position = fleet.Position,
                    Cargo = scrappedMinerals
                };
                EventManager.PublishMapObjectCreatedEvent(salvage);
            }

            // delete the fleet
            EventManager.PublishMapObjectDeletedEvent(fleet);
        }
    }
}