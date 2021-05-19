using Godot;

namespace CraigStars
{
    /// <summary>
    /// Helper methods for cloaking
    /// </summary>
    public static class CloakUtils
    {

        /// <summary>
        /// Compute cloak percent (as an int) based on total cloakUnits
        /// 
        /// https://wiki.starsautohost.org/wiki/Guts_of_Cloaking
        /// </summary>
        /// <param name="cloakUnits"></param>
        /// <returns></returns>
        public static int GetCloakPercentForCloakUnits(int cloakUnits)
        {
            if (cloakUnits <= 100)
            {
                return (int)(cloakUnits / 2 + .5f);
            }
            else
            {
                cloakUnits = cloakUnits - 100;
                if (cloakUnits <= 200)
                {
                    return 50 + cloakUnits / 8;
                }
                else
                {
                    cloakUnits = cloakUnits - 200;
                    if (cloakUnits < 312)
                    {
                        return 75 + cloakUnits / 24;
                    }
                    else
                    {
                        cloakUnits = cloakUnits - 312;
                        if (cloakUnits <= 512)
                        {
                            return 88 + cloakUnits / 64;
                        }
                        else if (cloakUnits < 768)
                        {
                            return 96;
                        }
                        else if (cloakUnits < 1000)
                        {
                            return 97;
                        }
                        else
                        {
                            return 99;
                        }
                    }
                }
            }
        }


    }
}