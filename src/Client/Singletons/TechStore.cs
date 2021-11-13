using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Singletons
{
    /// <summary>
    /// We may want game specific tech stores. For now we
    /// just use the StaticTechStore until we load them from disk at some other point
    /// </summary>
    public class TechStore : Node, ITechStore
    {
        private static ITechStore techStore = StaticTechStore.Instance;

        public IEnumerable<Tech> Techs => techStore.Techs;

        public IEnumerable<TechHullComponent> HullComponents => techStore.HullComponents;

        public IEnumerable<TechHull> Hulls => techStore.Hulls;

        public IEnumerable<TechHull> ShipHulls => techStore.ShipHulls;

        public IEnumerable<TechHull> StarbaseHulls => techStore.StarbaseHulls;

        public IEnumerable<TechCategory> Categories => techStore.Categories;

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static TechStore instance;
        public static ITechStore Instance
        {
            get
            {
                return instance;
            }
        }

        public override void _EnterTree()
        {
            // From advice in the godot forums, this is probably a good idea.
            // It's possible that godot will use reflection to instantiate us twice
            instance = this;
        }


        public IEnumerable<TechCategory> GetCategoriesForTechs(IEnumerable<Tech> techs)
        {
            return techStore.GetCategoriesForTechs(techs);
        }

        public T GetTechByName<T>(string name) where T : Tech
        {
            return techStore.GetTechByName<T>(name);
        }

        public IEnumerable<Tech> GetTechsByCategory(TechCategory category)
        {
            return techStore.GetTechsByCategory(category);
        }
    }
}