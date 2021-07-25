using CraigStars.Singletons;
using CraigStarsTable;
using Godot;
using System;
using System.Linq;

namespace CraigStars
{
    public class ScoreDialog : GameViewDialog
    {
        Button okButton;
        CSTable scoreTable;
        CSTable victoryTable;

        public override void _Ready()
        {
            base._Ready();
            okButton = FindNode("OKButton") as Button;
            scoreTable = GetNode<CSTable>("MarginContainer/VBoxContainer/TabContainer/Player Scores/ScoreTable");
            victoryTable = GetNode<CSTable>("MarginContainer/VBoxContainer/TabContainer/Victory Conditions/VBoxContainer/VictoryTable");

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            okButton.Connect("pressed", this, nameof(OnOk));
        }

        /// <summary>
        /// Our designer dialog is about to show
        /// </summary>
        void OnAboutToShow()
        {
            ResetScoreTable();
            ResetVictoryTable();
        }

        /// <summary>
        /// Called when the popup hides
        /// </summary>
        void OnPopupHide()
        {

        }

        /// <summary>
        /// Just hide the dialog on ok
        /// </summary>
        void OnOk()
        {
            Hide();
        }

        void ResetScoreTable()
        {
            scoreTable.Data.Clear();

            // add the empty column for score type
            scoreTable.Data.AddColumn("", false, align: Label.AlignEnum.Right);
            GameInfo.Players.ForEach(player =>
            {
                scoreTable.Data.AddColumn(player.RacePluralName, false, align: Label.AlignEnum.Right);
            });

            int numPlayers = GameInfo.Players.Count;

            object[] planets = new object[numPlayers + 1];
            object[] starbases = new object[numPlayers + 1];
            object[] unarmedShips = new object[numPlayers + 1];
            object[] escortShips = new object[numPlayers + 1];
            object[] capitalShips = new object[numPlayers + 1];
            object[] techLevels = new object[numPlayers + 1];
            object[] resources = new object[numPlayers + 1];
            object[] score = new object[numPlayers + 1];
            object[] rank = new object[numPlayers + 1];

            planets[0] = "Planets";
            starbases[0] = "Starbases";
            unarmedShips[0] = "Unarmed Ships";
            escortShips[0] = "Escort Ships";
            capitalShips[0] = "Capital Ships";
            techLevels[0] = "Tech Levels";
            resources[0] = "Resources";
            score[0] = "Score";
            rank[0] = "Rank";

            for (int i = 0; i < GameInfo.Players.Count; i++)
            {
                var player = GameInfo.Players[i];
                var index = i + 1;

                if (player == Me || Me.Game.ScoresVisible)
                {
                    var playerScore = player == Me ? Me.Score : player.PublicScore;
                    planets[index] = playerScore.Planets;
                    starbases[index] = playerScore.Starbases;
                    unarmedShips[index] = playerScore.UnarmedShips;
                    escortShips[index] = playerScore.EscortShips;
                    capitalShips[index] = playerScore.CapitalShips;
                    techLevels[index] = playerScore.TechLevels;
                    resources[index] = playerScore.Resources;
                    score[index] = playerScore.Score;
                    rank[index] = playerScore.Rank;
                }
                else
                {
                    planets[index] = "";
                    starbases[index] = "";
                    unarmedShips[index] = "";
                    escortShips[index] = "";
                    capitalShips[index] = "";
                    techLevels[index] = "";
                    resources[index] = "";
                    score[index] = "";
                    rank[index] = "";
                }
            }

            scoreTable.Data.AddRow(planets);
            scoreTable.Data.AddRow(starbases);
            scoreTable.Data.AddRow(unarmedShips);
            scoreTable.Data.AddRow(escortShips);
            scoreTable.Data.AddRow(capitalShips);
            scoreTable.Data.AddRow(techLevels);
            scoreTable.Data.AddRow(resources);
            scoreTable.Data.AddRow(score);
            scoreTable.Data.AddRow(rank);

            scoreTable.ResetTable();
        }

        void ResetVictoryTable()
        {
            victoryTable.Data.Clear();

            // add the empty column for victory condition description
            victoryTable.Data.AddColumn("", false, align: Label.AlignEnum.Right);
            GameInfo.Players.ForEach(player =>
            {
                victoryTable.Data.AddColumn(player.RacePluralName, false, align: Label.AlignEnum.Right);
            });

            int numPlayers = GameInfo.Players.Count;

            object[] OwnPlanets = new object[numPlayers + 1];
            object[] AttainTechLevels = new object[numPlayers + 1];
            object[] ExceedScore = new object[numPlayers + 1];
            object[] ExceedSecondPlaceScore = new object[numPlayers + 1];
            object[] ProductionCapacity = new object[numPlayers + 1];
            object[] OwnCapitalShips = new object[numPlayers + 1];
            object[] HighestScore = new object[numPlayers + 1];

            var victoryConditions = Me.Game.VictoryConditions;
            OwnPlanets[0] = $"Own {((int)(victoryConditions.OwnPlanets / 100f * Me.AllPlanets.Count()))} planets.";
            AttainTechLevels[0] = $"Attain Tech {victoryConditions.AttainTechLevel} in {victoryConditions.AttainTechLevelNumFields} fields.";
            ExceedScore[0] = $"Excced a score of {victoryConditions.ExceedScore}.";
            ExceedSecondPlaceScore[0] = $"Exceed second place score by {victoryConditions.ExceedSecondPlaceScorePercent}%.";
            ProductionCapacity[0] = $"Has a production capacity of {victoryConditions.ProductionCapacity / 1000} thousand.";
            OwnCapitalShips[0] = $"Owns {victoryConditions.OwnCapitalShips} capital ships.";
            HighestScore[0] = $"Has the highest score after {victoryConditions.HighestScoreAfterYears} years.";

            for (int i = 0; i < GameInfo.Players.Count; i++)
            {
                var player = GameInfo.Players[i];
                var index = i + 1;

                OwnPlanets[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.OwnPlanets);
                AttainTechLevels[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.AttainTechLevels);
                ExceedScore[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.ExceedScore);
                ExceedSecondPlaceScore[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.ExceedSecondPlaceScore);
                ProductionCapacity[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.ProductionCapacity);
                OwnCapitalShips[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.OwnCapitalShips);
                HighestScore[index] = player.AchievedVictoryConditions.Contains(VictoryConditionType.HighestScore);

            }

            victoryTable.Data.AddRow(OwnPlanets);
            victoryTable.Data.AddRow(AttainTechLevels);
            victoryTable.Data.AddRow(ExceedScore);
            victoryTable.Data.AddRow(ExceedSecondPlaceScore);
            victoryTable.Data.AddRow(ProductionCapacity);
            victoryTable.Data.AddRow(OwnCapitalShips);
            victoryTable.Data.AddRow(HighestScore);

            victoryTable.ResetTable();
        }
    }
}