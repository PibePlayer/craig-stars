using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Calculate the score for this year for each player
    ///  
    /// Note: this depends on each player having updated player reports
    /// 
    /// Here's how empires score:
    /// Planets:  From 1 to 6 points, scoring 1 point for each 100,000 colonists
    /// Starbases: 3 points each (doesn't include Orbital Forts)
    /// Unarmed Ships: An unarmed ship has a power rating of 0. You receive 1/2 point for each unarmed ship (up to the number of planets you own).
    /// Escort Ships: An escort ship has a power rating greater than 0 and less than 2000. You receive 2 points for each Escort ship (up to the number of planets you own).
    /// Capital Ships A Capital ship has a power rating of greater than 1999.  For each capital ship, you receive points calculated by the following formula:
    /// Points = (8 * #_capital_ships * #_planets) /( #_capital_ships + #_planets)
    ///    For example, if you have 20 capital ships and 30 planets, you receive (8 x 20 x 30) / (20 + 30) or 4.8 points for each ship.
    ///          Tech Levels:  1 point for levels 1-3,
    ///                        2 points for levels 4-6,
    ///                        3 points for levels 7-9,
    ///                        4 points for level 10 and above
    /// Resources: 1 point for every 30 resources
    /// </summary>
    public class CalculateScoreStep : TurnGenerationStep
    {
        private readonly PlanetService planetService;

        public CalculateScoreStep(IProvider<Game> gameProvider, PlanetService planetService) : base(gameProvider, TurnGenerationState.CalculateScoreStep)
        {
            this.planetService = planetService;
        }

        public override void Process()
        {
            Game.Players.ForEach(player =>
            {
                // reset score for this turn
                player.Score = CalculateScore(player);

                // add the score to the history
                player.ScoreHistory.Add(player.Score);

                // update public player scores
                // once a victor is declared, we always show public player scores
                if (Game.GameInfo.ScoresVisible)
                {
                    player.PublicScore = player.Score;
                }
            });
        }

        internal PlayerScore CalculateScore(Player player)
        {
            var score = new PlayerScore();

            // sum up planets
            var playerPlanets = Game.Planets.Where(planet => planet.PlayerNum == player.Num).ToList();
            score.Planets = playerPlanets.Count;
            foreach (var planet in playerPlanets)
            {
                if (planet.HasStarbase)
                {
                    score.Starbases++;
                }
                // Planets:  From 1 to 6 points, scoring 1 point for each 100,000 colonists
                score.Score += Mathf.Clamp(planet.Population / 100000, 1, 6);
                score.Resources += planetService.GetResourcesPerYear(planet, player);
            }

            score.TechLevels = player.TechLevels.Sum();

            var playerFleets = Game.Fleets.Where(fleet => fleet.PlayerNum == player.Num).ToList();
            foreach (var token in playerFleets.SelectMany(fleet => fleet.Tokens))
            {
                var powerRating = token.Design.Spec.PowerRating;
                if (powerRating <= 0)
                {
                    score.UnarmedShips += token.Quantity;
                }
                else if (powerRating > 0 && powerRating < 1999)
                {
                    score.EscortShips += token.Quantity;
                }
                else
                {
                    score.CapitalShips += token.Quantity;
                }
            }

            // Resources: 1 point for every 30 resources
            score.Score += score.Resources / 30;
            // Starbases: 3 points each (doesn't include Orbital Forts)
            score.Score += score.Starbases * 3;
            // Unarmed Ships: You receive 1/2 point for each unarmed ship (up to the number of planets you own).
            score.Score += (int)Mathf.Clamp(score.UnarmedShips * .5f + 5f, 0, score.Planets);
            // Escort Ships: You receive 2 points for each Escort ship (up to the number of planets you own).
            score.Score += (int)Mathf.Clamp(score.EscortShips * 2, 0, score.Planets);
            // Capital Ships (8 * #_capital_ships * #_planets) /( #_capital_ships + #_planets)
            if (score.CapitalShips + score.Planets > 0)
            {
                score.Score += (8 * score.CapitalShips * score.Planets) / (score.CapitalShips + score.Planets);
            }

            // Tech Levels:  1 point for levels 1-3,
            //               2 points for levels 4-6,
            //               3 points for levels 7-9,
            //               4 points for level 10 and above
            foreach (TechField field in Enum.GetValues(typeof(TechField)))
            {
                // var level = ;
                score.Score += player.TechLevels[field] switch
                {
                    int level when level >= 1 && level <= 3 => 1,
                    int level when level >= 4 && level <= 6 => 2,
                    int level when level >= 7 && level <= 9 => 3,
                    int level when level >= 10 => 4,
                    _ => 0,
                };
            }

            return score;
        }
    }
}