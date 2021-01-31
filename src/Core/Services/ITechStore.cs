using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface ITechStore
    {
        List<Tech> Techs { get; }

        /// <summary>
        /// A list of Hull Components
        /// </summary>
        /// <value></value>
        List<TechHullComponent> HullComponents { get; }

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        List<TechHull> Hulls { get; }

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        List<TechHull> ShipHulls { get; }

        /// <summary>
        /// A list of Starbase Hulls
        /// </summary>
        /// <value></value>
        List<TechHull> StarbaseHulls { get; }

        List<TechCategory> Categories { get; }
        T GetTechByName<T>(string name) where T : Tech;
        List<Tech> GetTechsByCategory(TechCategory category);

        /// <summary>
        /// Get a list of categories, sorted, for a set of techs.
        /// </summary>
        /// <param name="techs">The techs to get categories for</param>
        /// <returns>A list of categories, sorted by name</returns>
        List<TechCategory> GetCategoriesForTechs(List<Tech> techs);
        List<Tech> GetAvailableTechs(Player player);

    }
}