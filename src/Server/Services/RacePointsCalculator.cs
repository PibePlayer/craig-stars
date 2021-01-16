using System;
using System.Collections.Generic;

namespace CraigStars
{
    public class RacePointsCalculator
    {
        // The 'cost' in points of each LRT. A good LRT costs negative, a bad LRT is positive
        static Dictionary<LRT, int> lrtPointCost = new Dictionary<LRT, int>()
        {
            {LRT.IFE,  -235},
            {LRT.TT,  -25},
            {LRT.ARM,  -159},
            {LRT.ISB,  -201},
            {LRT.GR,  40},
            {LRT.UR,  -240},
            {LRT.MA,  -155},
            {LRT.NRSE,  160},
            {LRT.CE,  240},
            {LRT.OBRM,  255},
            {LRT.NAS,  325},
            {LRT.LSP,  180},
            {LRT.BET,  70},
            {LRT.RS,  30},
        };
        static Dictionary<PRT, int> prtPointCost = new Dictionary<PRT, int>()
        {
            {PRT.HE, -40},
            {PRT.SS, -95},
            {PRT.WM, -45},
            {PRT.CA, -10},
            {PRT.IS, 100},
            {PRT.SD, 150},
            {PRT.PP, -120},
            {PRT.IT, -180},
            {PRT.AR, -90},
            {PRT.JoaT, 66},
        };

        /// <summary>
        /// Get the Advantage points for a race
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The advantage points for this race, negative means an invalid race.</returns>
        public int GetAdvantagePoints(Race race, int startingPoints)
        {
            // start off with some constant
            int points = startingPoints;

            int habPoints = (int)(GetHabRangePoints(race) / 2000);

            int growthRateFactor = race.GrowthRate; // use raw growth rate, otherwise
                                                    // HEs pay for GR at 2x
            float grRate = growthRateFactor;

            // update the points based on growth rate
            if (growthRateFactor <= 5)
            {
                points += (6 - growthRateFactor) * 4200;
            }
            else if (growthRateFactor <= 13)
            {
                switch (growthRateFactor)
                {
                    case 6:
                        points += 3600;
                        break;
                    case 7:
                        points += 2250;
                        break;
                    case 8:
                        points += 600;
                        break;
                    case 9:
                        points += 225;
                        break;
                }
                growthRateFactor = growthRateFactor * 2 - 5;
            }
            else if (growthRateFactor < 20)
            {
                growthRateFactor = (growthRateFactor - 6) * 3;
            }
            else
            {
                growthRateFactor = 45;
            }

            points -= (int)(habPoints * growthRateFactor) / 24;

            // give points for off center habs
            int numImmunities = 0;
            for (int habType = 0; habType < 3; habType++)
            {
                if (race.IsImmune(habType))
                {
                    numImmunities++;
                }
                else
                {
                    points += Math.Abs(race.HabCenter[habType] - 50) * 4;
                }
            }

            // multiple immunities are penalized extra
            if (numImmunities > 1)
            {
                points -= 150;
            }

            // determine factory costs
            int operationPoints = race.NumFactories;
            int productionPoints = race.FactoryOutput;

            if (operationPoints > 10 || productionPoints > 10)
            {
                operationPoints -= 9;
                if (operationPoints < 1)
                {
                    operationPoints = 1;
                }
                productionPoints -= 9;
                if (productionPoints < 1)
                {
                    productionPoints = 1;
                }

                // HE penalty, 2 for all PRTs execpt 3 for HE
                int factoryProductionCost = 2;
                if (race.PRT == PRT.HE)
                {
                    factoryProductionCost = 3;
                }

                productionPoints *= factoryProductionCost;

                // additional penalty for two- and three-immune
                if (numImmunities >= 2)
                {
                    points -= (int)((productionPoints * operationPoints) * grRate) / 2;
                }
                else
                {
                    points -= (int)((productionPoints * operationPoints) * grRate) / 9;
                }
            }

            // pop efficiency
            int popEfficiency = race.ColonistsPerResource / 100;
            if (popEfficiency > 25)
                popEfficiency = 25;

            if (popEfficiency <= 7)
                points -= 2400;
            else if (popEfficiency == 8)
                points -= 1260;
            else if (popEfficiency == 9)
                points -= 600;
            else if (popEfficiency > 10)
                points += (popEfficiency - 10) * 120;

            // factory points (AR races have very simple points)
            if (race.PRT == PRT.AR)
            {
                points += 210;
            }
            else
            {
                productionPoints = 10 - race.FactoryOutput;
                int costPoints = 10 - race.FactoryCost;
                operationPoints = 10 - race.NumFactories;
                int tmpPoints = 0;

                if (productionPoints > 0)
                {
                    tmpPoints = productionPoints * 100;
                }
                else
                {
                    tmpPoints = productionPoints * 121;
                }

                if (costPoints > 0)
                {
                    tmpPoints += costPoints * costPoints * -60;
                }
                else
                {
                    tmpPoints += costPoints * -55;
                }

                if (operationPoints > 0)
                {
                    tmpPoints += operationPoints * 40;
                }
                else
                {
                    tmpPoints += operationPoints * 35;
                }

                // limit low factory points
                int llfp = 700;
                if (tmpPoints > llfp)
                {
                    tmpPoints = (tmpPoints - llfp) / 3 + llfp;
                }

                if (operationPoints <= -7)
                {
                    if (operationPoints < -11)
                    {
                        if (operationPoints < -14)
                        {
                            tmpPoints -= 360;
                        }
                        else
                        {
                            tmpPoints += (operationPoints + 7) * 45;
                        }
                    }
                    else
                    {
                        tmpPoints += (operationPoints + 6) * 30;
                    }
                }

                if (productionPoints <= -3)
                {
                    tmpPoints += (productionPoints + 2) * 60;
                }

                points += tmpPoints;

                if (race.FactoriesCostLess)
                {
                    points -= 175;
                }

                // mines
                productionPoints = 10 - race.MineOutput;
                costPoints = 3 - race.MineCost;
                operationPoints = 10 - race.NumMines;
                tmpPoints = 0;

                if (productionPoints > 0)
                {
                    tmpPoints = productionPoints * 100;
                }
                else
                {
                    tmpPoints = productionPoints * 169;
                }

                if (costPoints > 0)
                {
                    tmpPoints -= 360;
                }
                else
                {
                    tmpPoints += costPoints * (-65) + 80;
                }

                if (operationPoints > 0)
                {
                    tmpPoints += operationPoints * 40;
                }
                else
                {
                    tmpPoints += operationPoints * 35;
                }

                points += tmpPoints;
            }

            // prt and lrt point costs
            points += prtPointCost[race.PRT];

            // too many lrts
            int badLRTs = 0;
            int goodLRTs = 0;

            // figure out how many bad vs good lrts we have.
            foreach (LRT lrt in race.LRTs)
            {
                if (lrtPointCost[lrt] >= 0)
                {
                    badLRTs++;
                }
                else
                {
                    goodLRTs++;
                }
                points += lrtPointCost[lrt];
            }

            if (goodLRTs + badLRTs > 4)
            {
                points -= (goodLRTs + badLRTs) * (goodLRTs + badLRTs - 4) * 10;
            }
            if (badLRTs - goodLRTs > 3)
            {
                points -= (badLRTs - goodLRTs - 3) * 60;
            }
            if (goodLRTs - badLRTs > 3)
            {
                points -= (goodLRTs - badLRTs - 3) * 40;
            }

            // No Advanced scanners is penalized in some races
            if (race.HasLRT(LRT.NAS))
            {
                if (race.PRT == PRT.PP)
                {
                    points -= 280;
                }
                else if (race.PRT == PRT.SS)
                {
                    points -= 200;
                }
                else if (race.PRT == PRT.JoaT)
                {
                    points -= 40;
                }
            }

            // Techs
            //
            // Figure out the total number of Extra's, offset by the number of Less's 
            int techcosts = 0;
            for (int i = 0; i < 6; i++)
            {
                ResearchCostLevel rc = race.ResearchCost[i];
                if (rc == ResearchCostLevel.Extra)
                {
                    techcosts--;
                }
                else if (rc == ResearchCostLevel.Less)
                {
                    techcosts++;
                }
            }

            // if we have more less's then extra's, penalize the race
            if (techcosts > 0)
            {
                points -= (techcosts * techcosts) * 130;
                if (techcosts >= 6)
                {
                    points += 1430; // already paid 4680 so true cost is 3250
                }
                else if (techcosts == 5)
                {
                    points += 520; // already paid 3250 so true cost is 2730
                }
            }
            else if (techcosts < 0)
            {
                // if we have more extra's, give the race a bonus that increases as
                // we have more extra's
                int[] scienceCost = new int[] { 150, 330, 540, 780, 1050, 1380 };
                points += scienceCost[(-techcosts) - 1];
                if (techcosts < -4 && race.ColonistsPerResource < 1000)
                {
                    points -= 190;
                }
            }

            if (race.TechsStartHigh)
            {
                points -= 180;
            }

            // ART races get penalized extra for have cheap energy because it gives them such a boost
            if (race.PRT == PRT.AR && race.ResearchCost.Energy == ResearchCostLevel.Less)
            {
                points -= 100;
            }

            return points / 3;
        }

        /// <summary>
        /// Compute the hab range advantage points for this race by generating test planets for a variety
        /// of ranges and using the habitability of those planets
        /// </summary>
        /// <returns>The advantage points for this race's habitability range</returns>
        long GetHabRangePoints(Race race)
        {
            int ttCorrectionFactor = 0;
            bool totalTerraforming;
            double temperatureSum, gravitySum;
            long radiationSum, planetDesirability;
            int terraformOffsetSum, tmpHab;
            int[] terraformOffset = new int[3];

            // setup the starting values for each hab type, and the widths
            // for those
            int[] testHabStart = new Hab();
            int[] testHabWidth = new Hab();

            double points = 0.0;
            totalTerraforming = race.HasLRT(LRT.TT);

            terraformOffset[0] = terraformOffset[1] = terraformOffset[2] = 0;

            int numIterationsGrav = 0;
            int numIterationsRad = 0;
            int numIterationsTemp = 0;

            // set the number of iterations for each hab type.  If we're immune it's just
            // 1 because all the planets in that range will be the same.  Otherwise we loop
            // over the entire hab range in 11 equal divisions (i.e. for Humanoids grav would be 15, 22, 29, etc. all the way to 85)
            if (race.ImmuneGrav)
            {
                numIterationsGrav = 1;
            }
            else
            {
                numIterationsGrav = 11;
            }
            if (race.ImmuneTemp)
            {
                numIterationsTemp = 1;
            }
            else
            {
                numIterationsTemp = 11;
            }
            if (race.ImmuneRad)
            {
                numIterationsRad = 1;
            }
            else
            {
                numIterationsRad = 11;
            }

            // We go through 3 main iterations.  During each the habitability of the test planet
            // varies between the low and high of the hab range for each hab type.  So for a humanoid
            // it goes (15, 15, 15), (15, 15, 22), (15, 15, 29), etc.   Until it's (85, 85, 85)
            // During the various loops the TTCorrectionFactor changes to account for the race's ability
            // to terrform.
            for (int loopIndex = 0; loopIndex < 3; loopIndex++)
            {

                // each main loop gets a different TTCorrectionFactor
                if (loopIndex == 0)
                    ttCorrectionFactor = 0;
                else if (loopIndex == 1)
                    ttCorrectionFactor = totalTerraforming ? 8 : 5;
                else
                    ttCorrectionFactor = totalTerraforming ? 17 : 15;


                // for each hab type, set up the starts and widths
                // for this outer loop
                for (int habType = 0; habType < 3; habType++)
                {
                    // if we're immune, just make the hab values some middle value
                    if (race.IsImmune(habType))
                    {
                        testHabStart[habType] = 50;
                        testHabWidth[habType] = 11;

                    }
                    else
                    {
                        // start at the minimum hab range
                        testHabStart[habType] = race.HabLow[habType] - ttCorrectionFactor;

                        // don't go below 0, that doesnt' make sense for a hab range
                        if (testHabStart[habType] < 0)
                        {
                            testHabStart[habType] = 0;
                        }

                        // get the high range for this hab type
                        tmpHab = race.HabHigh[habType] + ttCorrectionFactor;

                        // don't go over 100, that doesn't make sense
                        if (tmpHab > 100)
                            tmpHab = 100;

                        // figure out the width for this hab type's starting range
                        testHabWidth[habType] = tmpHab - testHabStart[habType];
                    }
                }

                // 3 nested for loops, one for each hab type.  The number of iterations is 11 for non immune habs, or 1 for immune habs
                // this starts iterations for the first hab (gravity)
                gravitySum = 0.0;
                Hab testPlanetHab = new Hab();
                for (int iterationGrav = 0; iterationGrav < numIterationsGrav; iterationGrav++)
                {
                    tmpHab = GetPlanetHabForHabIndex(race, iterationGrav, 0, loopIndex, numIterationsGrav, testHabStart[0], testHabWidth[0], terraformOffset, ttCorrectionFactor);
                    testPlanetHab = testPlanetHab.WithGrav(tmpHab);

                    // go through iterations for temperature
                    temperatureSum = 0.0;
                    for (int iterationTemp = 0; iterationTemp < numIterationsTemp; iterationTemp++)
                    {
                        tmpHab = GetPlanetHabForHabIndex(race, iterationTemp, 1, loopIndex, numIterationsTemp, testHabStart[1], testHabWidth[1], terraformOffset, ttCorrectionFactor);
                        testPlanetHab = testPlanetHab.WithTemp(tmpHab);

                        // go through iterations for radiation
                        radiationSum = 0;
                        for (int iterationRad = 0; iterationRad < numIterationsRad; iterationRad++)
                        {
                            tmpHab = GetPlanetHabForHabIndex(race, iterationRad, 2, loopIndex, numIterationsRad, testHabStart[2], testHabWidth[2], terraformOffset, ttCorrectionFactor);
                            testPlanetHab = testPlanetHab.WithRad(tmpHab);

                            planetDesirability = race.GetPlanetHabitability(testPlanetHab);

                            terraformOffsetSum = terraformOffset[0] + terraformOffset[1] + terraformOffset[2];
                            if (terraformOffsetSum > ttCorrectionFactor)
                            {
                                // bring the planet desirability down by the difference between the terraformOffsetSum and the TTCorrectionFactor
                                planetDesirability -= terraformOffsetSum - ttCorrectionFactor;
                                // make sure the planet isn't negative in desirability
                                if (planetDesirability < 0)
                                    planetDesirability = 0;
                            }
                            planetDesirability *= planetDesirability;

                            // modify the planetDesirability by some factor based on which main loop we're going through
                            switch (loopIndex)
                            {
                                case 0:
                                    planetDesirability *= 7;
                                    break;
                                case 1:
                                    planetDesirability *= 5;
                                    break;
                                default:
                                    planetDesirability *= 6;
                                    break;
                            }

                            radiationSum += planetDesirability;
                        }

                        // The radiationSum is the sum of the planetDesirability for each iteration in numIterationsRad
                        // if we're immune to radiation it'll be the same very loop, so *= by 11
                        if (!race.ImmuneRad)
                        {
                            radiationSum = (radiationSum * testHabWidth[2]) / 100;
                        }
                        else
                        {
                            radiationSum *= 11;
                        }

                        temperatureSum += radiationSum;
                    }

                    // The tempSum is the sum of the radSums
                    // if we're immune to radiation it'll be the same very loop, so *= by 11
                    if (!race.ImmuneTemp)
                    {
                        temperatureSum = (temperatureSum * testHabWidth[1]) / 100;
                    }
                    else
                    {
                        temperatureSum *= 11;
                    }

                    gravitySum += temperatureSum;
                }
                if (!race.ImmuneGrav)
                {
                    gravitySum = (gravitySum * testHabWidth[0]) / 100;
                }
                else
                {
                    gravitySum *= 11;
                }

                points += gravitySum;
            }

            return (long)(points / 10.0 + 0.5);
        }

        /// <summary>
        /// Get the planet hab value (grav, temp or rad) for an iteration of the loop
        /// </summary>
        /// <param name="race">The race we are calculating for</param>
        /// <param name="iterIndex">The index of the iteration loop (1 through 11 usually)</param>
        /// <param name="habType">The index of the loop</param>
        /// <param name="loopIndex">The type of hab for the main outer loop</param>
        /// <param name="numIterations">The numIterations[HabType] for this loop</param>
        /// <param name="testHabStart">The testHabStart for the habType</param>
        /// <param name="testHabWidth">The testHabWidth for the habType</param>
        /// <param name="terraformOffset">The terraformOffset array to set for the habIndex, this happens to account for the race terraforming in the future (I think)</param>
        /// <param name="ttCorrectionFactor">The terraformOffset array to set for the habIndex, this happens to account for the race terraforming in the future (I think)</param>
        /// <returns>The Hab value for the test planet, based on the habIndex, habType, TTCorrectionFactor, etc.</returns>
        private int GetPlanetHabForHabIndex(Race race, int iterIndex, int habType, int loopIndex, int numIterations, int testHabStart, int testHabWidth,
                                            int[] terraformOffset, int ttCorrectionFactor)
        {
            int tmpHab = 0;

            // on the first iteration just use the testHabStart we already defined
            // if we're on a subsequent loop move the hab value along the habitable range of this race
            if (iterIndex == 0 || numIterations <= 1)
            {
                tmpHab = testHabStart;
            }
            else
            {
                tmpHab = (testHabWidth * iterIndex) / (numIterations - 1) + testHabStart;
            }

            // if we on a main loop other than the first one, do some
            // stuff with the terraforming correction factor
            if (loopIndex != 0 && !race.IsImmune(habType))
            {
                int offset = race.HabCenter[habType] - tmpHab;
                if (Math.Abs(offset) <= ttCorrectionFactor)
                {
                    offset = 0;
                }
                else if (offset < 0)
                {
                    offset += ttCorrectionFactor;
                }
                else
                {
                    offset -= ttCorrectionFactor;
                }

                // we set this terraformOffset value for later use
                // when we do the summing
                terraformOffset[habType] = offset;
                tmpHab = race.HabCenter[habType] - offset;
            }

            return tmpHab;
        }

    }

}