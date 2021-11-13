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
Morph hull will be available only to your race.".Replace("\n", "");
                case PRT.SS:
                    return @"You can sneak through enemy territory and execute stunning surprise
attacks. You are given top-drawer cloaks and all your ships have 75%
cloaking built in. Cargo does not decrease your cloaking abilities. The
Stealth Bomber and Rogue are at your disposal, as are a scanner, shield
and armor with stealthy properties. Two scanners which allow you to
steal minerals from enemy fleets and planets are also available. You
may safely travel through mine fields at one warp speed faster than the
limits.".Replace("\n", "");
                case PRT.WM:
                    return @"You rule the battle field. Your colonists attack better, your ships are
faster in battle, and you build weapons 25% cheaper than other
races.You start the game with a knowledge of Tech 6 weapons and Tech
1 in enerqy and propulsion. Unfortunately, your race doesn't understand
the necessity of building any but the most basic planetary defenses and
no mine fields.".Replace("\n", "");
                case PRT.CA:
                    return @"You are an expert at fiddling with planetary environments. You start the
game with Tech 6 in Biotechnology and a ship capable of terraforming
planets from orbit. You can arm your ships with bombs that unterraform
enemy worlds. Terraforming costs you nothing and planets you leave
revert to their original environments. Planets you own have up to a 10%
chance of permanently improving an environment variable by 1% per
vear.".Replace("\n", "");
                case PRT.IS:
                    return @"You are strong and hard to defeat. Your colonists repel attacks better,
your ships heal faster, you have special battle devices that protect your
ships and can lay Speed Trap mine fields. You have a device that acts as
both a shield and armor. Your peace-loving people refuse to build Smart
Bombs. Planetary defenses cost you 40% less, though weapons cost
you 25% more. Your colonists are able to reproduce while being
transported by your fleets.".Replace("\n", "");
                case PRT.SD:
                    return @"You are an expert in laying mine fields. You have a vast array of mine
types at your disposal and two unique hull design which are made for
mine dispersal. Your mine fields act as scanners and you have the ability
to remote detonate your own Standard mine fields. You may safely
travel two warp speeds faster than the stated limits through enemy
mine fields. You start the game with 2 mine laying ships and Tech 2 in
Propulsion and BioTech.".Replace("\n", "");
                case PRT.PP:
                    return @"Your race excels at accelerating mineral packets to distant planets. You
start with a Warp 5 accelerator at your home starbase and Tech 4 Energy.
You will eventually be able to fling packets at the mind numbing speed
of Warp 13. You can fling smaller packets and all of your packets have
penetrating scanners embedded in them. You will start the game
owning a second planet some distance away if the universe size isn't
tiny. Packets you fling that aren't fully caught have a chance of
terraforming the target planet.".Replace("\n", "");
                case PRT.IT:
                    return @"Your race excels in building stargates. You start with Tech 5 in
Propulsion and Construction. You start the game with a second planet if
the universe size isn't tiny. Both planets have stargates. Eventually you
may build stargates which have unlimited capabilities. Stargates cost
you 25% less to build. Your race can automatically scan any enemy planet
with a stargate which is in range of one of your stargates. Exceeding the
safety limits of stargates is less likely to kill your ships.".Replace("\n", "");
                case PRT.AR:
                    return @"Your race developed in an alternate plane. Your people cannot survive
on planets and live in orbit on your starbases, which are 20% cheaper to
build. You cannot build planetary installations, but your people have an
intrinsic ability to mine and scan for enemy fleets. You can remote mine
your own worlds. If a starbase is destroyed, all your colonists orbiting
that world are killed. Your population maximums are determined by the
type of starbase you have. You will eventually be able to build the Death
Star.".Replace("\n", "");
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

