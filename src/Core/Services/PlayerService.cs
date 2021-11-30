using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class PlayerService
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayerService));

        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public PlayerService(IRulesProvider rulesProvider)
        {
            this.rulesProvider = rulesProvider;
        }

        #region Production

        /// <summary>
        /// Get the cost of a single item in this ProductionQueueItem
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="race"></param>
        /// <returns></returns>
        public Cost GetCostOfOne(Player player, ProductionQueueItem item)
        {
            if (player.Race.Spec.Costs.TryGetValue(item.Type, out var cost))
            {
                return cost;
            }
            else if (item.Type == QueueItemType.ShipToken || item.Type == QueueItemType.Starbase)
            {
                // ship designs have their own cost
                return item.Design.Spec.Cost;
            }
            else
            {
                throw new ArgumentException($"Cannot find player cost for item type: {item.Type}");
            }
        }

        /// <summary>
        /// Get the player's cost for this tech accounting for miniaturization and racial specs
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tech"></param>
        /// <returns></returns>
        public Cost GetTechCost(Player player, Tech tech)
        {
            Cost cost = tech.Cost;

            // see if this race has any discounts for this category
            float factor = player.Race.Spec.TechCostOffset.TryGetValue(tech.Category, out factor) ? 1 + factor : 1f;

            cost *= factor;

            // figure out miniaturization
            // this is 4% per level above the required tech we have.
            // We count the smallest diff, i.e. if you have
            // tech level 10 energy, 12 bio and the tech costs 9 energy, 4 bio
            // the smallest level difference you have is 1 energy level (not 8 bio levels)

            var levelDiff = new TechLevel(
                tech.Requirements.Energy <= 0 ? -1 : player.TechLevels.Energy - tech.Requirements.Energy,
                tech.Requirements.Weapons <= 0 ? -1 : player.TechLevels.Weapons - tech.Requirements.Weapons,
                tech.Requirements.Propulsion <= 0 ? -1 : player.TechLevels.Propulsion - tech.Requirements.Propulsion,
                tech.Requirements.Construction <= 0 ? -1 : player.TechLevels.Construction - tech.Requirements.Construction,
                tech.Requirements.Electronics <= 0 ? -1 : player.TechLevels.Electronics - tech.Requirements.Electronics,
                tech.Requirements.Biotechnology <= 0 ? -1 : player.TechLevels.Biotechnology - tech.Requirements.Biotechnology
            );

            // From the diff between the player level and the requirements, find the lowest difference
            // i.e. 1 energey level in the example above
            int numTechLevelsAboveRequired = int.MaxValue;
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                var fieldDiff = levelDiff[field];
                if (fieldDiff != -1 && fieldDiff <= numTechLevelsAboveRequired)
                {
                    numTechLevelsAboveRequired = fieldDiff;
                }
            }

            // for starter techs, they are all 0 requirements, so just use our lowest field
            if (numTechLevelsAboveRequired == int.MaxValue)
            {
                numTechLevelsAboveRequired = player.TechLevels.Min();
            }

            // As we learn techs, they get cheaper. We start off with full priced techs, but every additional level of research we learn makes
            // techs cost a little less, maxing out at some discount (i.e. 75% or 80% for races with BET)
            var miniaturization = Math.Min(player.Race.Spec.MiniaturizationMax, player.Race.Spec.MiniaturizationPerLevel * numTechLevelsAboveRequired);
            // New techs cost BET races 2x
            // new techs will have 0 for miniaturization.
            double miniaturizationFactor = (numTechLevelsAboveRequired == 0 ? player.Race.Spec.NewTechCostFactor : 1) - miniaturization;

            return new Cost(
                (int)Math.Ceiling(cost.Ironium * miniaturizationFactor),
                (int)Math.Ceiling(cost.Boranium * miniaturizationFactor),
                (int)Math.Ceiling(cost.Germanium * miniaturizationFactor),
                (int)Math.Ceiling(cost.Resources * miniaturizationFactor)
            );
        }

        #endregion
    }

}