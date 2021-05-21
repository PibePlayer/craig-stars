namespace CraigStars
{
    public class TechTerraform : Tech
    {

        public int Ability { get; set; }
        public TerraformHabType HabType { get; set; }

        public TechTerraform()
        {
        }

        public TechTerraform(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

    }

}
