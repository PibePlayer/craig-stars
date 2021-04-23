using System;
using System.Collections.Generic;

namespace CraigStars
{
    public interface ITechStore
    {
        IEnumerable<Tech> Techs { get; }

        /// <summary>
        /// A list of Hull Components
        /// </summary>
        /// <value></value>
        IEnumerable<TechHullComponent> HullComponents { get; }

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        IEnumerable<TechHull> Hulls { get; }

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        IEnumerable<TechHull> ShipHulls { get; }

        /// <summary>
        /// A list of Starbase Hulls
        /// </summary>
        /// <value></value>
        IEnumerable<TechHull> StarbaseHulls { get; }

        IEnumerable<TechCategory> Categories { get; }
        T GetTechByName<T>(string name) where T : Tech;
        IEnumerable<Tech> GetTechsByCategory(TechCategory category);

        /// <summary>
        /// Get a list of categories, sorted, for a set of techs.
        /// </summary>
        /// <param name="techs">The techs to get categories for</param>
        /// <returns>A list of categories, sorted by name</returns>
        IEnumerable<TechCategory> GetCategoriesForTechs(IEnumerable<Tech> techs);
        IEnumerable<Tech> GetAvailableTechs(Player player);

    }
}