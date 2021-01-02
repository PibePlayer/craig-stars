using System;
using System.Linq;

namespace CraigStars
{
    public class TechLevel
    {
        public int Energy { get; set; }
        public int Weapons { get; set; }
        public int Propulsion { get; set; }
        public int Construction { get; set; }
        public int Electronics { get; set; }
        public int Biotechnology { get; set; }

        public TechLevel() { }
        public TechLevel(int energy = 0, int weapons = 0, int propulsion = 0, int construction = 0, int electronics = 0, int biotechnology = 0)
        {
            Energy = energy;
            Weapons = weapons;
            Propulsion = propulsion;
            Construction = construction;
            Electronics = electronics;
            Biotechnology = biotechnology;
        }

        public int this[TechField index]
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
            set
            {
                switch (index)
                {
                    case TechField.Energy:
                        Energy = value;
                        break;
                    case TechField.Weapons:
                        Weapons = value;
                        break;
                    case TechField.Propulsion:
                        Propulsion = value;
                        break;
                    case TechField.Construction:
                        Construction = value;
                        break;
                    case TechField.Electronics:
                        Electronics = value;
                        break;
                    case TechField.Biotechnology:
                        Biotechnology = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }
            }
        }

        /// <summary>
        /// Determine if this TechLevel meets a given tech requirements
        /// </summary>
        /// <param name="requirements"></param>
        /// <returns></returns>
        public bool HasRequiredLevels(TechRequirements requirements)
        {
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                if (this[field] < requirements[field])
                {
                    return false;
                }
            }
            return true;

        }

        /// <summary>
        /// Get the lowest level field
        /// </summary>
        /// <returns></returns>
        public TechField Lowest()
        {
            var lowestField = TechField.Energy;
            var lowest = 0;
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                var level = this[field];
                if (level < lowest)
                {
                    lowestField = field;
                    lowest = level;
                }
            }

            return lowestField;
        }

        /// <summary>
        /// Sum up the total number of tech levels in this list.
        /// This is used for determining research costs
        /// </summary>
        /// <returns></returns>
        public int Sum()
        {
            var total = 0;
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                total += this[field];
            }
            return total;
        }
    }
}
