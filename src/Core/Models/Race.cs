using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    public class Race
    {
        public string Name { get; set; } = "Humanoid";
        public string PluralName { get; set; } = "Humanoids";
        public SpendLeftoverPointsOn SpendLeftoverPointsOn { get; set; } = SpendLeftoverPointsOn.SurfaceMinerals;
        public PRT PRT { get; set; } = PRT.JoaT;
        public HashSet<LRT> LRTs { get; set; } = new HashSet<LRT>();

        public Hab HabLow { get => habLow; set { habLow = value; habCenter = null; } }
        Hab habLow = new Hab(15, 15, 15);

        public Hab HabHigh { get => habHigh; set { habHigh = value; habCenter = null; } }
        Hab habHigh = new Hab(85, 85, 85);

        [JsonIgnore]
        public Hab HabCenter
        {
            get
            {
                if (habCenter == null)
                {
                    habCenter = new Hab((HabHigh.grav - HabLow.grav) / 2 + HabLow.grav, (HabHigh.temp - HabLow.temp) / 2 + HabLow.temp, (HabHigh.rad - HabLow.rad) / 2 + HabLow.rad);
                }
                return habCenter.Value;
            }
        }
        Hab? habCenter = null;

        [JsonIgnore]
        public Hab HabWidth
        {
            get
            {
                if (habWidth == null)
                {
                    habWidth = new Hab((HabHigh.grav - HabLow.grav) / 2, (HabHigh.temp - HabLow.temp), (HabHigh.rad - HabLow.rad) / 2);
                }
                return habWidth.Value;
            }
        }
        Hab? habWidth = null;

        public int GrowthRate { get; set; } = 15;
        public int ColonistsPerResource { get; set; } = 1000;
        public int FactoryOutput { get; set; } = 10;
        public int FactoryCost { get; set; } = 10;
        public int NumFactories { get; set; } = 10;
        public bool FactoriesCostLess { get; set; } = false;
        public int MineOutput { get; set; } = 10;
        public int MineCost { get; set; } = 5;
        public int NumMines { get; set; } = 10;
        public bool ImmuneGrav { get; set; } = false;
        public bool ImmuneTemp { get; set; } = false;
        public bool ImmuneRad { get; set; } = false;
        public ResearchCost ResearchCost { get; set; } = new ResearchCost();
        public bool TechsStartHigh { get; set; } = false;

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
                        // green planet
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
                        // red planet
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