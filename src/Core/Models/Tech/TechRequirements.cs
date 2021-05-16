using System.Collections.Generic;

namespace CraigStars
{
    public class TechRequirements : TechLevel
    {
        public PRT PRTRequired { get; set; } = PRT.None;
        public PRT PRTDenied { get; set; } = PRT.None;
        public HashSet<LRT> LRTsRequired { get; set; } = new HashSet<LRT>();
        public HashSet<LRT> LRTsDenied { get; set; } = new HashSet<LRT>();

        public TechRequirements() { }

        /// <summary>
        /// Most of our techs only have one lrt that is required or denied, so this is a simpler consturctor for that
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="weapons"></param>
        /// <param name="propulsion"></param>
        /// <param name="construction"></param>
        /// <param name="electronics"></param>
        /// <param name="biotechnology"></param>
        /// <param name="prtRequired"></param>
        /// <param name="prtDenied"></param>
        /// <param name="lrtsRequired"></param>
        /// <param name="lrtsDenied"></param>
        /// <returns></returns>
        public TechRequirements(
            int energy = 0,
            int weapons = 0,
            int propulsion = 0,
            int construction = 0,
            int electronics = 0,
            int biotechnology = 0,
            PRT prtRequired = PRT.None,
            PRT prtDenied = PRT.None,
            LRT lrtsRequired = LRT.None,
            LRT lrtsDenied = LRT.None
            ) : base(energy, weapons, propulsion, construction, electronics, biotechnology)
        {
            PRTRequired = prtRequired;
            PRTDenied = prtDenied;
            if (lrtsRequired != LRT.None)
            {
                LRTsRequired.Add(lrtsRequired);
            }

            if (lrtsDenied != LRT.None)
            {
                LRTsDenied.Add(lrtsDenied);
            }
        }

        /// <summary>
        /// Get the difference between a requirement's levels and a player TechLevel
        /// </summary>
        /// <param name="requirements"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static TechLevel operator -(TechRequirements requirements, TechLevel level)
        {
            return new TechLevel(
                requirements.Energy - level.Energy,
                requirements.Weapons - level.Weapons,
                requirements.Propulsion - level.Propulsion,
                requirements.Construction - level.Construction,
                requirements.Electronics - level.Electronics,
                requirements.Biotechnology - level.Biotechnology
            );
        }

    }
}
