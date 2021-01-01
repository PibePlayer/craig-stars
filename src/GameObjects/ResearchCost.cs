using System;

namespace CraigStars
{
    /// <summary>
    /// The Research Cost for each tech field for a race
    /// </summary>
    public class ResearchCost
    {

        public ResearchCostLevel Energy { get; set; } = ResearchCostLevel.Standard;
        public ResearchCostLevel Weapons { get; set; } = ResearchCostLevel.Standard;
        public ResearchCostLevel Propulsion { get; set; } = ResearchCostLevel.Standard;
        public ResearchCostLevel Construction { get; set; } = ResearchCostLevel.Standard;
        public ResearchCostLevel Electronics { get; set; } = ResearchCostLevel.Standard;
        public ResearchCostLevel Biotechnology { get; set; } = ResearchCostLevel.Standard;

        public ResearchCost()
        {
        }

        public ResearchCostLevel this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Energy;
                    case 1:
                        return Weapons;
                    case 2:
                        return Propulsion;
                    case 3:
                        return Construction;
                    case 4:
                        return Electronics;
                    case 5:
                        return Biotechnology;
                    default:
                        throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
                }

            }
        }

        public ResearchCostLevel this[TechField field]
        {
            get
            {
                switch (field)
                {
                    case TechField.Biotechnology:
                        return Biotechnology;
                    case TechField.Construction:
                        return Construction;
                    case TechField.Electronics:
                        return Electronics;
                    case TechField.Energy:
                        return Energy;
                    case TechField.Propulsion:
                        return Propulsion;
                    case TechField.Weapons:
                        return Weapons;
                    default:
                        throw new IndexOutOfRangeException($"Index {field} out of range for {this.GetType().ToString()}");
                }
            }
        }
    }
}