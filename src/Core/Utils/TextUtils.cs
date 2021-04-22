using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Utils
{
    public static class TextUtils
    {

        public static string GetDescriptionForPRT(PRT prt)
        {
            switch (prt)
            {
                case PRT.HE:
                    return prt.ToString();
                case PRT.SS:
                    return prt.ToString();
                case PRT.WM:
                    return prt.ToString();
                case PRT.CA:
                    return prt.ToString();
                case PRT.IS:
                    return prt.ToString();
                case PRT.SD:
                    return prt.ToString();
                case PRT.PP:
                    return prt.ToString();
                case PRT.IT:
                    return prt.ToString();
                case PRT.AR:
                    return prt.ToString();
                case PRT.JoaT:
                    return @"Your race does not specialize in a single area. You start the game with Tech 3 in all areas 
and an assortment of ships. Your Scout, Destroyer, and Frigate hulls have a built-in penetrating scanner 
which grows more powerful as your Electronics tech increases. Your maximum planetary population is 20% 
greater than other races.".Replace("\n", "");
                default:
                    return prt.ToString();
            }
        }

        public static string GetDescriptionForLRT(LRT lrt)
        {
            switch (lrt)
            {
                case LRT.IFE:
                    return @"This gives you the Fuel Mizer and Galaxy Scoop engines and increases your starting 
Propulsion tech by 1 level. All engines use 15% less fuel.".Replace("\n", "");
                case LRT.TT:
                    return lrt.ToString();
                case LRT.ARM:
                    return lrt.ToString();
                case LRT.ISB:
                    return lrt.ToString();
                case LRT.GR:
                    return lrt.ToString();
                case LRT.UR:
                    return lrt.ToString();
                case LRT.NRSE:
                    return lrt.ToString();
                case LRT.OBRM:
                    return lrt.ToString();
                case LRT.NAS:
                    return lrt.ToString();
                case LRT.LSP:
                    return lrt.ToString();
                case LRT.BET:
                    return lrt.ToString();
                case LRT.RS:
                    return lrt.ToString();
                case LRT.MA:
                    return lrt.ToString();
                case LRT.CE:
                    return lrt.ToString();
                default:
                    return lrt.ToString();
            }
        }

        /// <summary>
        /// Get a position string like (21.32, 256.2) for printing coords
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static string GetPositionString(Vector2 position)
        {
            return $"({position.x:.##}, {position.y:.##})";
        }

        public static string GetGravString(int grav)
        {
            int result, tmp = Math.Abs(grav - 50);
            if (tmp <= 25)
                result = (tmp + 25) * 4;
            else
                result = tmp * 24 - 400;
            if (grav < 50)
                result = 10000 / result;

            double value = result / 100 + (result % 100 / 100.0);
            return $"{value:0.00}g";
        }

        public static string GetTempString(int temp)
        {
            int result;
            result = (temp - 50) * 4;

            return $"{result}°C";
        }

        public static string GetRadString(int rad)
        {
            return rad + "mR";
        }

    }
}

