using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class TechStore : Node
    {
        public List<Tech> Techs { get; set; } = new List<Tech>();
        public Dictionary<String, Tech> TechsByName { get; set; } = new Dictionary<String, Tech>();
        public Dictionary<TechCategory, List<Tech>> TechsByCategory { get; set; } = new Dictionary<TechCategory, List<Tech>>();

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static TechStore instance;
        public static TechStore Instance
        {
            get
            {
                return instance;
            }
        }

        public override void _Ready()
        {
            // From advice in the godot forums, this is probably a good idea.
            // It's possible that godot will use reflection to instantiate us twice
            instance = this;

            // for now, just use our statically defined techs. Eventually we might want to load these techs
            // from a file or something like that
            Techs.AddRange(CraigStars.Techs.AllTechs);
            Techs.ForEach(t => TechsByName[t.Name] = t);
            TechsByCategory = Techs.GroupBy(t => t.Category).ToDictionary(group => group.Key, group => group.ToList());
        }

    }
}