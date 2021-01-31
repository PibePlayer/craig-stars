using System;

namespace CraigStars
{
    public class TechPlanetaryScanner : Tech
    {

        public int ScanRange { get; set; }
        public int ScanRangePen { get; set; }

        public TechPlanetaryScanner()
        {
        }

        public TechPlanetaryScanner(string name, Cost cost, TechRequirements techRequirements, int ranking, TechCategory category) : base(name, cost, techRequirements, ranking, category)
        {
        }

    }
}