using Godot;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using CraigStars.Singletons;
using System;

namespace CraigStars
{
    /// <summary>
    /// A starbase is just a fleet with one token
    /// </summary>
    public class Starbase : Fleet
    {
        public int DockCapacity { get => Tokens?[0]?.Design?.Hull?.SpaceDock ?? 0; }
    }
}
