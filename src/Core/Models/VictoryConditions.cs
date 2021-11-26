using System.Collections.Generic;

namespace CraigStars
{
    public enum VictoryConditionType
    {
        OwnPlanets,
        AttainTechLevels,
        ExceedsScore,
        ExceedsSecondPlaceScore,
        ProductionCapacity,
        OwnCapitalShips,
        HighestScoreAfterYears
    }

    /// <summary>
    /// The conditions for victory in the game
    /// </summary>
    public class VictoryConditions
    {
        public HashSet<VictoryConditionType> Conditions { get; set; } = new HashSet<VictoryConditionType>() {
            VictoryConditionType.OwnPlanets,
            VictoryConditionType.AttainTechLevels,
            VictoryConditionType.ExceedsSecondPlaceScore
        };

        public int NumCriteriaRequired { get; set; } = 1;
        public int YearsPassed { get; set; } = 50;
        public int OwnPlanets { get; set; } = 60;
        public int AttainTechLevel { get; set; } = 22;
        public int AttainTechLevelNumFields { get; set; } = 4;
        public int ExceedsScore { get; set; } = 11000;
        public int ExceedsSecondPlaceScore { get; set; } = 100;
        public int ProductionCapacity { get; set; } = 100000;
        public int OwnCapitalShips { get; set; } = 100;
        public int HighestScoreAfterYears { get; set; } = 100;
    }
}