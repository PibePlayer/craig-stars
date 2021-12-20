
using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class PlayerResearchOrder
    {
        public int ResearchAmount { get; set; }
        public TechField Researching { get; set; }
        public NextResearchField NextResearchField { get; set; }
    }
}