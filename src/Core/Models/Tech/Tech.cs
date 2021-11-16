using System;

namespace CraigStars
{
    public abstract class Tech
    {
        public string Name { get; set; }
        public Cost Cost { get; set; } = new Cost();
        public TechRequirements Requirements { get; set; } = new TechRequirements();
        public int Ranking { get; set; }
        public TechCategory Category { get; set; }

        public Tech()
        {

        }

        public Tech(string name, Cost cost, TechRequirements requirements, int ranking, TechCategory category)
        {
            Name = name;
            Cost = cost;
            Requirements = requirements;
            Ranking = ranking;
            Category = category;
        }

        public override string ToString()
        {
            return $"{GetType()} - {Name}";
        }

        /// <summary>
        /// Techs become cheaper to build as your tech levels increase
        /// For every level you achieve beyond the minimum, the cost drops by 4% (5% for bleeding edge tech)
        /// </summary>
        /// <param name="tech"></param>
        /// <returns></returns>
        public Cost GetPlayerCost(Player player)
        {
            Cost cost = Cost;

            // figure out miniaturization
            // this is 4% per level above the required tech we have.
            // We count the smallest diff, i.e. if you have
            // tech level 10 energy, 12 bio and the tech costs 9 energy, 4 bio
            // the smallest level difference you have is 1 energy level (not 8 bio levels)

            var levelDiff = new TechLevel(
                Requirements.Energy <= 0 ? -1 : player.TechLevels.Energy - Requirements.Energy,
                Requirements.Weapons <= 0 ? -1 : player.TechLevels.Weapons - Requirements.Weapons,
                Requirements.Propulsion <= 0 ? -1 : player.TechLevels.Propulsion - Requirements.Propulsion,
                Requirements.Construction <= 0 ? -1 : player.TechLevels.Construction - Requirements.Construction,
                Requirements.Electronics <= 0 ? -1 : player.TechLevels.Electronics - Requirements.Electronics,
                Requirements.Biotechnology <= 0 ? -1 : player.TechLevels.Biotechnology - Requirements.Biotechnology
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
            // if we are at level 26, a beginner tech would cost (26 * .04)
            // return cost * (1 - Math.Min(.75, .04 * lowestRequiredDiff));
        }
    }
}
