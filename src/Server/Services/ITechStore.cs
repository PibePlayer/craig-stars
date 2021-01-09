using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface ITechStore
    {
        List<Tech> Techs { get; }
        T GetTechByName<T>(string name) where T : Tech;
        List<Tech> GetTechsByCategory(TechCategory category);
    }
}