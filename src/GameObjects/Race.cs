using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class Race
    {
        public string Name { get; set; } = "Humanoid";
        public string PluralName { get; set; } = "Humanoids";
        public PRT PRT { get; set; } = PRT.JoaT;
        public HashSet<LRT> LRTs { get; set; } = new HashSet<LRT>();

        public Hab HabLow { get => habLow; set { habLow = value; habCenter = null; } }
        Hab habLow = new Hab(15, 15, 15);

        public Hab HabHigh { get => habHigh; set { habHigh = value; habCenter = null; } }
        Hab habHigh = new Hab(85, 85, 85);

        public Hab HabCenter
        {
            get
            {
                if (habCenter == null)
                {
                    habCenter = new Hab((HabHigh.Grav - HabLow.Grav) / 2 + HabLow.Grav, (HabHigh.Temp - HabLow.Temp) / 2 + HabLow.Temp, (HabHigh.Rad - HabLow.Rad) / 2 + HabLow.Rad);
                }
                return habCenter;
            }
        }
        Hab habCenter = null;

        public Hab HabWidth
        {
            get
            {
                if (habWidth == null)
                {
                    habWidth = new Hab((HabHigh.Grav - HabLow.Grav) / 2, (HabHigh.Temp - HabLow.Temp), (HabHigh.Rad - HabLow.Rad) / 2);
                }
                return habWidth;
            }
        }
        Hab habWidth = null;

        public int GrowthRate { get; set; } = 15;
        public int ColonistsPerResource { get; set; } = 1000;
        public int FactoryOutput { get; set; } = 10;
        public int FactoryCost { get; set; } = 10;
        public int NumFactories { get; set; } = 10;
        public bool FactoriesCostLess { get; set; } = false;
        public int MineOutput { get; set; } = 10;
        public int MineCost { get; set; } = 5;
        public int NumMines { get; set; } = 10;
        public bool TechsStartHigh { get; set; } = false;
        public bool ImmuneGrav { get; set; } = false;
        public bool ImmuneTemp { get; set; } = false;
        public bool ImmuneRad { get; set; } = false;

        public bool HasLRT(LRT lrt)
        {
            return LRTs.Contains(lrt);
        }

        /// <summary>
        /// Return whether this race is immune to a specific hab, by index
        /// </summary>
        /// <param name="index">The index of the hab, 0 == gravity, 1 == temp, 2 == radiation</param>
        /// <returns>Whether this race is immune to the specific hab type.</returns>
        public bool IsImmune(int index)
        {
            switch (index)
            {
                case 0:
                    return ImmuneGrav;
                case 1:
                    return ImmuneTemp;
                case 2:
                    return ImmuneRad;
                default:
                    throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}");
            }

        }

        /// <summary>
        /// Get the habitability of this race for a given planet's hab value
        /// </summary>
        /// <param name="planetHab">The Hab value for a planet.</param>
        /// <returns>The habiability of this race to that planet, with 100 being the best</returns>
        public int GetPlanetHabitability(Hab planetHab)
        {
            long planetValuePoints = 0, redValue = 0, ideality = 10000;
            int habValue, habCenter, habUpper, habLower, fromIdeal, habRadius, poorPlanetMod, habRed, tmp;
            for (var habType = 0; habType < 3; habType++)
            {
                habValue = planetHab[habType];
                habCenter = HabCenter[habType];
                habLower = HabLow[habType];
                habUpper = HabHigh[habType];

                if (IsImmune(habType))
                    planetValuePoints += 10000;
                else
                {
                    if (habLower <= habValue && habUpper >= habValue)
                    {
                        /* green planet */
                        fromIdeal = Math.Abs(habValue - habCenter) * 100;
                        if (habCenter > habValue)
                        {
                            habRadius = habCenter - habLower;
                            fromIdeal /= habRadius;
                            tmp = habCenter - habValue;
                        }
                        else
                        {
                            habRadius = habUpper - habCenter;
                            fromIdeal /= habRadius;
                            tmp = habValue - habCenter;
                        }
                        poorPlanetMod = ((tmp) * 2) - habRadius;
                        fromIdeal = 100 - fromIdeal;
                        planetValuePoints += fromIdeal * fromIdeal;
                        if (poorPlanetMod > 0)
                        {
                            ideality *= habRadius * 2 - poorPlanetMod;
                            ideality /= habRadius * 2;
                        }
                    }
                    else
                    {
                        /* red planet */
                        if (habLower <= habValue)
                            habRed = habValue - habUpper;
                        else
                            habRed = habLower - habValue;

                        if (habRed > 15)
                            habRed = 15;

                        redValue += habRed;
                    }
                }
            }

            if (redValue != 0)
            {
                return (int)-redValue;
            }

            planetValuePoints = (long)(Math.Sqrt((double)planetValuePoints / 3.0) + 0.9);
            planetValuePoints = planetValuePoints * ideality / 10000;

            return (int)planetValuePoints;
        }
    }
}