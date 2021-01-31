using System;
using System.Collections.Generic;

namespace CraigStars
{
    public static class Races
    {
        /// <summary>
        /// Get a regular ol' humanoid
        /// </summary>
        /// <returns></returns>
        public static Race Humanoid
        {
            get => new Race()
            {
                Name = "Humanoid",
                PluralName = "Humanoids",
                PRT = PRT.JoaT,
                HabLow = new Hab(15, 15, 15),
                HabHigh = new Hab(85, 85, 85),

                GrowthRate = 15,
                ColonistsPerResource = 1000,
                FactoryOutput = 10,
                FactoryCost = 10,
                NumFactories = 10,
                FactoriesCostLess = false,
                MineOutput = 10,
                MineCost = 5,
                NumMines = 10
            };
        }

        /// <summary>
        /// Built in Rabbitoid
        /// </summary>
        /// <returns></returns>
        public static Race Rabbitoid
        {
            get => new Race()
            {
                Name = "Rabbitoid",
                PluralName = "Rabbitoids",
                SpendLeftoverPointsOn = SpendLeftoverPointsOn.Defenses,
                PRT = PRT.IT,
                LRTs = new HashSet<LRT>() {
                    LRT.IFE,
                    LRT.TT,
                    LRT.CE,
                    LRT.NAS
                },
                HabLow = new Hab(10, 35, 13),
                HabHigh = new Hab(56, 81, 53),

                GrowthRate = 20,
                ColonistsPerResource = 1000,
                FactoryOutput = 10,
                FactoryCost = 9,
                NumFactories = 17,
                FactoriesCostLess = true,
                MineOutput = 10,
                MineCost = 9,
                NumMines = 10,
                ResearchCost = new ResearchCost()
                {
                    Energy = ResearchCostLevel.Extra,
                    Weapons = ResearchCostLevel.Extra,
                    Propulsion = ResearchCostLevel.Less,
                    Construction = ResearchCostLevel.Standard,
                    Electronics = ResearchCostLevel.Standard,
                    Biotechnology = ResearchCostLevel.Less
                }
            };
        }

        /// <summary>
        /// Built in Insectoid
        /// </summary>
        /// <returns></returns>
        public static Race Insectoid
        {
            get => new Race()
            {
                Name = "Insectoid",
                PluralName = "Insectoids",
                SpendLeftoverPointsOn = SpendLeftoverPointsOn.MineralConcentrations,
                PRT = PRT.WM,
                LRTs = new HashSet<LRT>() {
                    LRT.ISB,
                    LRT.RS,
                    LRT.CE
                },
                HabLow = new Hab(-1, 0, 70),
                HabHigh = new Hab(-1, 100, 100),
                ImmuneGrav = true,

                GrowthRate = 10,
                ColonistsPerResource = 1000,
                FactoryOutput = 10,
                FactoryCost = 10,
                NumFactories = 10,
                FactoriesCostLess = false,
                MineOutput = 9,
                MineCost = 10,
                NumMines = 6,
                ResearchCost = new ResearchCost()
                {
                    Energy = ResearchCostLevel.Less,
                    Weapons = ResearchCostLevel.Less,
                    Propulsion = ResearchCostLevel.Less,
                    Construction = ResearchCostLevel.Less,
                    Electronics = ResearchCostLevel.Standard,
                    Biotechnology = ResearchCostLevel.Extra
                }
            };
        }
    }
}