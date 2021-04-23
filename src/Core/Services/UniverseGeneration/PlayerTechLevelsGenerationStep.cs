using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars;
using static CraigStars.Utils.Utils;

namespace CraigStars.UniverseGeneration
{
    /// <summary>
    /// Initialize players with their default tech levels
    /// </summary>
    public class PlayerTechLevelsGenerationStep : UniverseGenerationStep
    {
        public PlayerTechLevelsGenerationStep(Game game) : base(game, UniverseGenerationState.TechLevels) { }

        public override void Process()
        {
            Game.Players.ForEach(player => player.TechLevels = GetStartingTechLevels(player.Race));
        }

        public TechLevel GetStartingTechLevels(Race race)
        {
            var techLevels = new TechLevel();
            switch (race.PRT)
            {
                case PRT.HE:
                    break;
                case PRT.SS:
                    techLevels.Electronics = 5;
                    break;
                case PRT.WM:
                    techLevels.Weapons = 6;
                    techLevels.Energy = 1;
                    techLevels.Propulsion = 1;
                    break;
                case PRT.CA:
                    techLevels.Energy = 1;
                    techLevels.Weapons = 1;
                    techLevels.Propulsion = 1;
                    techLevels.Construction = 2;
                    techLevels.Biotechnology = 6;
                    break;
                case PRT.IS:
                    break;
                case PRT.SD:
                    techLevels.Propulsion = 2;
                    techLevels.Biotechnology = 2;
                    break;
                case PRT.PP:
                    techLevels.Energy = 4;
                    break;
                case PRT.IT:
                    techLevels.Propulsion = 5;
                    techLevels.Construction = 5;
                    break;
                case PRT.AR:
                    techLevels.Energy = 1;
                    break;
                case PRT.JoaT:
                    techLevels.Energy = 3;
                    techLevels.Weapons = 3;
                    techLevels.Propulsion = 3;
                    techLevels.Construction = 3;
                    techLevels.Electronics = 3;
                    techLevels.Biotechnology = 3;
                    break;
            }

            // if a race has Techs costing exra start high, set the start level to 3
            // for any TechField that is set to research costs extra
            if (race.TechsStartHigh)
            {
                // Jack of All Trades start at 4
                var costsExtraLevel = race.PRT == PRT.JoaT ? 4 : 3;
                foreach (TechField field in Enum.GetValues(typeof(TechField)))
                {
                    var level = techLevels[field];
                    if (race.ResearchCost[field] == ResearchCostLevel.Extra && level < costsExtraLevel)
                    {
                        techLevels[field] = costsExtraLevel;
                    }
                }
            }

            if (race.HasLRT(LRT.IFE) || race.HasLRT(LRT.CE))
            {
                // Improved Fuel Efficiency and Cheap Engines increases propulsion by 1
                techLevels.Propulsion++;
            }

            return techLevels;
        }
    }
}