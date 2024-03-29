using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CraigStars
{
    public class Race
    {
        public string Name { get; set; } = "Humanoid";
        public string PluralName { get; set; } = "Humanoids";
        public SpendLeftoverPointsOn SpendLeftoverPointsOn { get; set; }

        [DefaultValue(PRT.JoaT)]
        public PRT PRT { get; set; } = PRT.JoaT;
        public HashSet<LRT> LRTs { get; set; } = new HashSet<LRT>();

        public Hab HabLow { get => habLow; set { habLow = value; habCenter = null; habWidth = null; } }
        Hab habLow = new Hab(15, 15, 15);

        public Hab HabHigh { get => habHigh; set { habHigh = value; habCenter = null; habWidth = null; } }
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
                    habWidth = new Hab((HabHigh.grav - HabLow.grav) / 2, (HabHigh.temp - HabLow.temp) / 2, (HabHigh.rad - HabLow.rad) / 2);
                }
                return habWidth.Value;
            }
        }
        Hab? habWidth = null;

        public int GrowthRate { get; set; } = 15;

        /// <summary>
        /// For normal races, this is the colonists per resource (in kT of colonists)
        /// for AR races, this is the Annual resources sqrt of pop * energy / thing
        /// </summary>
        /// <value></value>
        public int PopEfficiency { get; set; } = 10;
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

        #region Computed Fields

        // These fields are all computed by the RaceService from a race's makeup
        public RaceSpec Spec { get; set; } = new();

        #endregion

        public bool HasLRT(LRT lrt)
        {
            return LRTs.Contains(lrt);
        }

        /// <summary>
        /// Return whether this race is immune to a specific hab, by index
        /// </summary>
        /// <param name="index">The index of the hab, 0 == gravity, 1 == temp, 2 == radiation</param>
        /// <returns>Whether this race is immune to the specific hab type.</returns>
        public bool IsImmune(int index) => index switch
        {
            0 => ImmuneGrav,
            1 => ImmuneTemp,
            2 => ImmuneRad,
            _ => throw new IndexOutOfRangeException($"Index {index} out of range for {this.GetType().ToString()}")
        };

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [JsonIgnore] public bool IsDamagedByRadiation => !(ImmuneRad || HabWidth.rad == 50);

        /// <summary>
        /// Return whether this race is immune to a specific hab, by index
        /// </summary>
        /// <param name="index">The index of the hab, 0 == gravity, 1 == temp, 2 == radiation</param>
        /// <returns>Whether this race is immune to the specific hab type.</returns>
        public bool IsImmune(HabType habType) => habType switch
        {
            HabType.Gravity => ImmuneGrav,
            HabType.Temperature => ImmuneTemp,
            HabType.Radiation => ImmuneRad,
            _ => throw new IndexOutOfRangeException($"{habType} out of range for {this.GetType().ToString()}")
        };

        /// <summary>
        /// Get th
        /// </summary>
        /// <returns></returns>
        public double GetHabChance()
        {
            // do a straight calc of hab width, so if we have a hab with widths of 50, 50% of planets will be habitable
            // so we get (.5 * .5 * .5) = .125, or 1 in 8 planets
            var gravChance = ImmuneGrav ? 1.0 : (HabWidth.grav * 2 / 100.0);
            var tempChance = ImmuneTemp ? 1.0 : (HabWidth.temp * 2 / 100.0);
            var radChance = ImmuneRad ? 1.0 : (HabWidth.rad * 2 / 100.0);
            return gravChance * tempChance * radChance;
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