namespace CraigStars
{
    public abstract class Tech
    {
        public string Name { get; set; }
        public Cost Cost { get; set; } = new Cost();
        public TechRequirements Requirements { get; set; } = new TechRequirements();
        public int Ranking { get; set; }
        public TechCategory Category { get; set; }
    }
}
