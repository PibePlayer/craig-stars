using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class StaticTechStore : ITechStore
    {
        public IEnumerable<Tech> Techs { get => techs; }
        List<Tech> techs = new List<Tech>();

        /// <summary>
        /// A list of Hull Components
        /// </summary>
        /// <value></value>
        public IEnumerable<TechHullComponent> HullComponents
        {
            get
            {
                if (hullComponents == null || hullComponents.Count == 0)
                {
                    hullComponents = Techs.Where(tech => tech is TechHullComponent).Cast<TechHullComponent>().ToList();
                }
                return hullComponents;
            }
        }
        List<TechHullComponent> hullComponents;

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        public IEnumerable<TechHull> Hulls
        {
            get
            {
                if (hulls == null || hulls.Count == 0)
                {
                    hulls = Techs.Where(tech => tech is TechHull).Cast<TechHull>().ToList();
                }
                return hulls;
            }
        }
        List<TechHull> hulls;

        /// <summary>
        /// A list of Ship Hulls
        /// </summary>
        /// <value></value>
        public IEnumerable<TechHull> ShipHulls
        {
            get
            {
                if (shipHulls == null || shipHulls.Count == 0)
                {
                    shipHulls = Techs.Where(tech => tech is TechHull && tech.Category == TechCategory.ShipHull).Cast<TechHull>().ToList();
                }
                return shipHulls;
            }
        }
        List<TechHull> shipHulls;

        /// <summary>
        /// A list of Starbase Hulls
        /// </summary>
        /// <value></value>
        public IEnumerable<TechHull> StarbaseHulls
        {
            get
            {
                if (starbaseHulls == null || starbaseHulls.Count == 0)
                {
                    starbaseHulls = Techs.Where(tech => tech is TechHull && tech.Category == TechCategory.StarbaseHull).Cast<TechHull>().ToList();
                }
                return starbaseHulls;
            }
        }
        List<TechHull> starbaseHulls;

        Dictionary<String, Tech> TechsByName { get; set; } = new Dictionary<String, Tech>();
        Dictionary<TechCategory, List<Tech>> TechsByCategory { get; set; } = new Dictionary<TechCategory, List<Tech>>();
        public IEnumerable<TechCategory> Categories
        {
            get
            {
                if (categories == null || categories.Count == 0)
                {
                    categories = GetCategoriesForTechs(Techs).ToList();
                }
                return categories;
            }
        }
        List<TechCategory> categories;

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static StaticTechStore instance = new StaticTechStore();
        public static ITechStore Instance
        {
            get
            {
                return instance;
            }
        }

        StaticTechStore()
        {
            // From advice in the godot forums, this is probably a good idea.
            // It's possible that godot will use reflection to instantiate us twice
            instance = this;

            // for now, just use our statically defined techs. Eventually we might want to load these techs
            // from a file or something like that
            techs.AddRange(CraigStars.Techs.AllTechs);
            techs.ForEach(t => TechsByName[t.Name] = t);
            TechsByCategory = Techs.ToLookup(lookup => lookup.Category).ToDictionary(Lookup => Lookup.Key, lookup => lookup.OrderBy(t => t.Ranking).ToList());
        }

        public IEnumerable<Tech> GetTechsByCategory(TechCategory category)
        {
            TechsByCategory.TryGetValue(category, out var techs);
            return techs;
        }

        /// <summary>
        /// Get a tech from the tech store, by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetTechByName<T>(string name) where T : Tech
        {
            TechsByName.TryGetValue(name, out var tech);
            return tech as T;
        }

        /// <summary>
        /// Get a list of all techs available to the player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public IEnumerable<Tech> GetAvailableTechs(Player player)
        {
            return Techs.Where(tech => player.HasTech(tech)).ToList();
        }

        /// <summary>
        /// Get a list of categories, sorted, for a set of techs.
        /// </summary>
        /// <param name="techs">The techs to get categories for</param>
        /// <returns>A list of categories, sorted by name</returns>
        public IEnumerable<TechCategory> GetCategoriesForTechs(IEnumerable<Tech> techs)
        {
            var categories = techs.Select(tech => tech.Category).Distinct().ToList();
            categories.Sort();
            return categories;
        }
    }
}