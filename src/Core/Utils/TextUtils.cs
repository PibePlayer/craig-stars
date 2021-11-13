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
                    return @"You must expand to survive. You are given a small and cheap colony hull
and an engine which travels at Warp 6 using no fuel. Your race will grow
at twice the growth rate you select in step four; however, the maximum
population for a given planet is cut in half. The completely flexible Meta
Morph hull will be available only to your race.".Replace("\n", "");;
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
                    return @"This gives you the Fuel Mizer and Galaxy Scoop engines and increases your starting Propulsion tech by 1 level. All engines use 15% less fuel.";
                case LRT.TT:
                    return @"You begin the game with the ability to adjust each of a planet’s environment attributes by up to 3% in either direction. Throughout the game, additional terraforming technologies not available to other players will be achievable, up to 30% terraforming. Total Terraforming requires 30% fewer resources.";
                case LRT.ARM:
                    return @"Gives you three additional mining hulls and two new robots. You will start the game with two Midget Miners.";
                case LRT.ISB:
                    return @"Gives you two new starbase designs. The Space Dock hull allows you to build starbases which can in turn build small to medium ships. The Ultra-Station is much larger than a standard Starbase. Your starbases are automatically cloaked by 20%. Starbases will cost you 20% less to build.";
                case LRT.GR:
                    return @"Your race takes a holistic approach to research. Only half of the resources dedicated to research will be applied to the current field of research. 15% of the total will be applied to each of the fields.";
                case LRT.UR:
                    return @"When you scrap a fleet at a starbase, you recover 90% of the minerals and 70% of the resources used to produce the fleet. Scrapping at a planet gives you 45% of the minerals and 35% of the resources.";
                case LRT.NRSE:
                    return @"You will not be able to build the Radiating Hydro-Ram Scoop, Sub-Galactic Fuel Scoop, Trans-Galactic Fuel Scoop, Trans-Galactic Super Scoop, Trans- Galactic Mizer Scoop or the Galaxy Scoop. You will be able to build the Interspace-10 engine, which can travel warp 10 without taking damage.";
                case LRT.OBRM:
                    return @"No Robo-Miner, Robo-Maxi-Miner or Robo-Super-Miner robots.";
                case LRT.NAS:
                    return @"You will not have any standard scanners that can scan planets from a distance and see fleets hiding behind planets. All ranges for conventional scanners are doubled.";
                case LRT.LSP:
                    return @"Instead of 25000 people, you start with 17500 (30% fewer). It takes a long time to overcome a lower starting population - it helps to have a high growth rate, but even then it can be painful.";
                case LRT.BET:
                    return @"New technologies initially cost twice as much to build. As soon as you exceed all of the tech requirements by one level, the cost drops back to normal. Miniaturization, the lowering of production costs, occurs at 5% per level, up to 80%. Without this trait miniaturization occurs at 4% and tops out at 75%.";
                case LRT.RS:
                    return @"All shields are 40% stronger than the listed rating. Shields regenerate at 10% of the maximum strength after every round of battle. All armors are at 50% of their rated strength.";
                case LRT.MA:
                    return @"You will be able to turn resources into minerals more efficiently. One instance of mineral alchemy will use 25 resources to produce one kT of each mineral. Without this trait it takes 100 resources to produce one kT of each mineral.";
                case LRT.CE:
                    return @"Engines cost 50% less to build, your ship engines aren’t entirely reliable. When attempting to travel at speeds above warp 6, there is a 10% chance the engines will refuse to engage.";
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

