using System.Collections.Generic;

namespace CraigStars
{
    public class TechRequirements : TechLevel
    {
        public PRT PRTRequired { get; set; }
        public PRT PRTDenied { get; set; }
        public HashSet<LRT> LRTsRequired { get; set; } = new HashSet<LRT>();
        public HashSet<LRT> LRTsDenied { get; set; } = new HashSet<LRT>();
    }
}
