namespace CraigStars
{
    public class TechDefense : Tech
    {

        public float DefenseCoverage { get; set; }

        public TechDefense()
        {
        }

        public TechDefense(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {

        }

    }

}
