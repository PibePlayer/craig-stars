using Godot;
using System;

namespace CraigStars.Client
{
    public class VictoryConditionsOptions : VBoxContainer
    {

        SpinBox numCriteriaRequiredSpinBox;
        SpinBox yearsPassedSpinBox;
        SpinBox ownPlanetsSpinBox;
        SpinBox attainTechLevelSpinBox;
        SpinBox attainTechLevelNumFieldsSpinBox;
        SpinBox exceedsScoreSpinBox;
        SpinBox exceedsSecondPlaceScoreSpinBox;
        SpinBox productionCapacitySpinBox;
        SpinBox ownCapitalShipsSpinBox;
        SpinBox highestScoreAfterYearsSpinBox;

        CheckBox ownPlanetsCheckBox;
        CheckBox attainTechLevelsCheckBox;
        CheckBox exceedsScoreCheckBox;
        CheckBox exceedsSecondPlaceScoreCheckBox;
        CheckBox productionCapacityCheckBox;
        CheckBox ownCapitalShipsCheckBox;
        CheckBox highestScoreAfterYearsCheckBox;

        public VictoryConditions VictoryConditions { get; set; } = new();

        [Export]
        public bool Disabled
        {
            get => disabled;
            set
            {
                disabled = value;
                OnVisibilityChanged();
            }
        }
        bool disabled;

        public override void _Ready()
        {
            base._Ready();
            numCriteriaRequiredSpinBox = GetNode<SpinBox>("NumCriteriaRequiredContainer/NumCriteriaRequiredSpinBox");
            yearsPassedSpinBox = GetNode<SpinBox>("YearsPassedContainer/YearsPassedSpinBox");
            ownPlanetsSpinBox = GetNode<SpinBox>("OwnPlanetsContainer/OwnPlanetsSpinBox");
            attainTechLevelSpinBox = GetNode<SpinBox>("AttainTechLevelsContainer/AttainTechLevelSpinBox");
            attainTechLevelNumFieldsSpinBox = GetNode<SpinBox>("AttainTechLevelsContainer/AttainTechLevelNumFieldsSpinBox");
            exceedsScoreSpinBox = GetNode<SpinBox>("ExceedsScoreContainer/ExceedsScoreSpinBox");
            exceedsSecondPlaceScoreSpinBox = GetNode<SpinBox>("ExceedsSecondPlaceScoreContainer/ExceedsSecondPlaceScoreSpinBox");
            productionCapacitySpinBox = GetNode<SpinBox>("ProductionCapacityContainer/ProductionCapacitySpinBox");
            ownCapitalShipsSpinBox = GetNode<SpinBox>("OwnCapitalShipsContainer/OwnCapitalShipsSpinBox");
            highestScoreAfterYearsSpinBox = GetNode<SpinBox>("HighestScoreAfterYearsContainer/HighestScoreAfterYearsSpinBox");

            ownPlanetsCheckBox = GetNode<CheckBox>("OwnPlanetsContainer/OwnPlanetsCheckBox");
            attainTechLevelsCheckBox = GetNode<CheckBox>("AttainTechLevelsContainer/AttainTechLevelsCheckBox");
            exceedsScoreCheckBox = GetNode<CheckBox>("ExceedsScoreContainer/ExceedsScoreCheckBox");
            exceedsSecondPlaceScoreCheckBox = GetNode<CheckBox>("ExceedsSecondPlaceScoreContainer/ExceedsSecondPlaceScoreCheckBox");
            productionCapacityCheckBox = GetNode<CheckBox>("ProductionCapacityContainer/ProductionCapacityCheckBox");
            ownCapitalShipsCheckBox = GetNode<CheckBox>("OwnCapitalShipsContainer/OwnCapitalShipsCheckBox");
            highestScoreAfterYearsCheckBox = GetNode<CheckBox>("HighestScoreAfterYearsContainer/HighestScoreAfterYearsCheckBox");

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

        }

        public VictoryConditions GetVictoryConditions()
        {
            var conditions = new VictoryConditions()
            {
                NumCriteriaRequired = (int)numCriteriaRequiredSpinBox.Value,
                YearsPassed = (int)yearsPassedSpinBox.Value,
                OwnPlanets = (int)ownPlanetsSpinBox.Value,
                AttainTechLevel = (int)attainTechLevelSpinBox.Value,
                AttainTechLevelNumFields = (int)attainTechLevelNumFieldsSpinBox.Value,
                ExceedsScore = (int)exceedsScoreSpinBox.Value,
                ExceedsSecondPlaceScore = (int)exceedsSecondPlaceScoreSpinBox.Value,
                ProductionCapacity = (int)productionCapacitySpinBox.Value,
                OwnCapitalShips = (int)ownCapitalShipsSpinBox.Value,
                HighestScoreAfterYears = (int)highestScoreAfterYearsSpinBox.Value,
            };

            if (ownPlanetsCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.OwnPlanets);
            }
            if (attainTechLevelsCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.AttainTechLevels);
            }
            if (exceedsScoreCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.ExceedsScore);
            }
            if (exceedsSecondPlaceScoreCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.ExceedsSecondPlaceScore);
            }
            if (productionCapacityCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.ProductionCapacity);
            }
            if (ownCapitalShipsCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.OwnCapitalShips);
            }
            if (highestScoreAfterYearsCheckBox.Pressed)
            {
                conditions.Conditions.Add(VictoryConditionType.HighestScoreAfterYears);
            }

            return conditions;
        }

        void OnVisibilityChanged()
        {
            if (IsVisibleInTree() && numCriteriaRequiredSpinBox != null)
            {
                numCriteriaRequiredSpinBox.Editable = !Disabled;
                yearsPassedSpinBox.Editable = !Disabled;
                ownPlanetsSpinBox.Editable = !Disabled;
                attainTechLevelSpinBox.Editable = !Disabled;
                attainTechLevelNumFieldsSpinBox.Editable = !Disabled;
                exceedsScoreSpinBox.Editable = !Disabled;
                exceedsSecondPlaceScoreSpinBox.Editable = !Disabled;
                productionCapacitySpinBox.Editable = !Disabled;
                ownCapitalShipsSpinBox.Editable = !Disabled;
                highestScoreAfterYearsSpinBox.Editable = !Disabled;
                ownPlanetsCheckBox.Disabled = Disabled;
                attainTechLevelsCheckBox.Disabled = Disabled;
                exceedsScoreCheckBox.Disabled = Disabled;
                exceedsSecondPlaceScoreCheckBox.Disabled = Disabled;
                productionCapacityCheckBox.Disabled = Disabled;
                ownCapitalShipsCheckBox.Disabled = Disabled;
                highestScoreAfterYearsCheckBox.Disabled = Disabled;

                numCriteriaRequiredSpinBox.Value = VictoryConditions.NumCriteriaRequired;
                yearsPassedSpinBox.Value = VictoryConditions.YearsPassed;
                ownPlanetsSpinBox.Value = VictoryConditions.OwnPlanets;
                attainTechLevelSpinBox.Value = VictoryConditions.AttainTechLevel;
                attainTechLevelNumFieldsSpinBox.Value = VictoryConditions.AttainTechLevelNumFields;
                exceedsScoreSpinBox.Value = VictoryConditions.ExceedsScore;
                exceedsSecondPlaceScoreSpinBox.Value = VictoryConditions.ExceedsSecondPlaceScore;
                productionCapacitySpinBox.Value = VictoryConditions.ProductionCapacity;
                ownCapitalShipsSpinBox.Value = VictoryConditions.OwnCapitalShips;
                highestScoreAfterYearsSpinBox.Value = VictoryConditions.HighestScoreAfterYears;

                ownPlanetsCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.OwnPlanets);
                attainTechLevelsCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.AttainTechLevels);
                exceedsScoreCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.ExceedsScore);
                exceedsSecondPlaceScoreCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.ExceedsSecondPlaceScore);
                productionCapacityCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.ProductionCapacity);
                ownCapitalShipsCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.OwnCapitalShips);
                highestScoreAfterYearsCheckBox.Pressed = VictoryConditions.Conditions.Contains(VictoryConditionType.HighestScoreAfterYears);
            }
        }

    }
}