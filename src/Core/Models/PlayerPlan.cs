using System;
using System.Collections.Generic;

namespace CraigStars
{

    /// <summary>
    /// All player plans have a name and a guid
    /// </summary>
    public abstract class PlayerPlan<T> where T : PlayerPlan<T>, new()
    {
        public PlayerPlan() { }
        public PlayerPlan(string name)
        {
            Name = name;
        }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public string Name { get; set; }

        public abstract T Clone();

    }
}