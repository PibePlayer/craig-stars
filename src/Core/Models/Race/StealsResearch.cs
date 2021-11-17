using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{

    public record StealsResearch(
        float Energy = 0f,
        float Weapons = 0f,
        float Propulsion = 0f,
        float Construction = 0f,
        float Electronics = 0f,
        float Biotechnology = 0f
    )
    {
        public float this[TechField index]
        {
            get
            {
                switch (index)
                {
                    case TechField.Energy:
                        return Energy;
                    case TechField.Weapons:
                        return Weapons;
                    case TechField.Propulsion:
                        return Propulsion;
                    case TechField.Construction:
                        return Construction;
                    case TechField.Electronics:
                        return Electronics;
                    case TechField.Biotechnology:
                        return Biotechnology;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }
    }
}