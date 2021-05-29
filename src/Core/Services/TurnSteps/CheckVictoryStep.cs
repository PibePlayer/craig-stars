using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using Godot;

namespace CraigStars
{
    /// <summary>
    /// Check if any players are victors
    /// Note, this depends on each player having updated scores and player reports
    /// </summary>
    public class CheckVictoryStep : TurnGenerationStep
    {
        public CheckVictoryStep(Game game) : base(game, TurnGenerationState.VictoryCheck) { }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                CheckForVictor(player);
            });

            // we don't declare a victor until some time has passed
            if (Game.YearsPassed >= Game.VictoryConditions.YearsPassed && Game.VictorDeclared)
            {
                SendVictorMessages();
            }
        }

        /// <summary>
        /// Check for a victor, and if found, set the player.Victor flag and Game.VictorDeclared flag
        /// </summary>
        /// <param name="player"></param>
        internal void CheckForVictor(Player player)
        {
            foreach (VictoryConditionType victoryConditionType in Game.VictoryConditions.Conditions)
            {
                switch (victoryConditionType)
                {
                    case VictoryConditionType.OwnPlanets:
                        CheckOwnPlanets(player);
                        break;
                    case VictoryConditionType.AttainTechLevels:
                        CheckAttainTechLevels(player);
                        break;
                    case VictoryConditionType.ExceedScore:
                        CheckExceedScore(player);
                        break;
                    case VictoryConditionType.ExceedSecondPlaceScore:
                        CheckExceedSecondPlaceScore(player);
                        break;
                    case VictoryConditionType.ProductionCapacity:
                        CheckProductionCapacity(player);
                        break;
                    case VictoryConditionType.OwnCapitalShips:
                        CheckOwnCapitalShips(player);
                        break;
                    case VictoryConditionType.HighestScore:
                        CheckHighestScore(player);
                        break;
                    default:
                        throw new System.ArgumentException("Unknown VictoryConditionType  " + victoryConditionType);
                };
            }

            if (player.AchievedVictoryConditions.Count >= Game.VictoryConditions.NumCriteriaRequired && Game.YearsPassed >= Game.VictoryConditions.YearsPassed)
            {
                // we have a victor!
                player.Victor = true;
                Game.VictorDeclared = true;
            }
        }

        internal void CheckOwnPlanets(Player player)
        {
            // i.e. if we own more than 60% of the planets, we have this victory condition
            if (player.Planets.Count >= Game.Planets.Count * (Game.VictoryConditions.OwnPlanets / 100f))
            {
                player.AchievedVictoryConditions.Add(VictoryConditionType.OwnPlanets);
            }
        }

        internal void CheckAttainTechLevels(Player player)
        {
            int numAttained = 0;
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                if (player.TechLevels[field] >= Game.VictoryConditions.AttainTechLevel)
                {
                    numAttained++;
                }
            }
            if (numAttained >= Game.VictoryConditions.AttainTechLevelNumFields)
            {
                player.AchievedVictoryConditions.Add(VictoryConditionType.AttainTechLevels);
            }
        }

        internal void CheckExceedScore(Player player)
        {
            if (player.Score.Score > Game.VictoryConditions.ExceedScore)
            {
                player.AchievedVictoryConditions.Add(VictoryConditionType.ExceedScore);
            }
        }

        internal void CheckExceedSecondPlaceScore(Player player)
        {
            if (Game.Players.Count > 1)
            {
                var scores = Game.Players.OrderByDescending(p => p.Score.Score).Select(p => p.Score.Score).ToList();
                // if player 1 is score 120, player 2 is score 100
                // our score is 20% higher
                if (((float)player.Score.Score / scores[1] - 1) * 100 > Game.VictoryConditions.ExceedSecondPlaceScorePercent)
                {
                    player.AchievedVictoryConditions.Add(VictoryConditionType.ExceedSecondPlaceScore);
                }
            }
        }

        internal void CheckProductionCapacity(Player player)
        {
            var productionCapacity = player.Planets.Sum(planet => planet.ResourcesPerYear);
            if (productionCapacity >= Game.VictoryConditions.ProductionCapacity)
            {
                player.AchievedVictoryConditions.Add(VictoryConditionType.ProductionCapacity);
            }
        }

        internal void CheckOwnCapitalShips(Player player)
        {
            if (player.Score.CapitalShips >= Game.VictoryConditions.OwnCapitalShips)
            {
                player.AchievedVictoryConditions.Add(VictoryConditionType.OwnCapitalShips);
            }
        }

        internal void CheckHighestScore(Player player)
        {
            if (Game.GameInfo.YearsPassed >= Game.VictoryConditions.HighestScoreAfterYears)
            {
                if (player.Score == Game.Players.OrderByDescending(player => player.Score).Select(player => player.Score).ToArray()[0])
                {
                    // our score is the highest after a certain number of years
                    player.AchievedVictoryConditions.Add(VictoryConditionType.HighestScore);
                }
            }
        }


        /// <summary>
        /// Send messages to all victors
        /// </summary>
        internal void SendVictorMessages()
        {
            var victors = Game.Players.Where(p => p.Victor).ToList();

            Game.Players.ForEach(player =>
            {
                victors.ForEach(victor =>
                {
                    // TODO: what happens with multiple victors
                    Message.Victory(player, victor);
                });
            });

        }

    }
}