using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars.Client
{

    public class ResearchDialog : GameViewDialog
    {
        static CSLog log = LogProvider.GetLogger(typeof(ResearchDialog));

        Researcher researcher = new Researcher();

        CheckBox energyCheckBox;
        CheckBox weaponsCheckBox;
        CheckBox propulsionCheckBox;
        CheckBox constructionCheckBox;
        CheckBox electronicsCheckBox;
        CheckBox biotechnologyCheckBox;

        Label energyLabel;
        Label weaponsLabel;
        Label propulsionLabel;
        Label constructionLabel;
        Label electronicsLabel;
        Label biotechnologyLabel;

        OptionButton nextFieldToResearchMenuButton;

        SpinBox resourcesBudgetedAmount;

        // descriptive labels
        Label resourcesNeededToCompleteAmountLabel;
        Label estimatedTimeToCompletionAmountLabel;
        Label annualResourcesAmountLabel;
        Label totalResourcesSpentAmountLabel;
        Label nextYearBudgetAmountLabel;

        // future techs
        FutureTechs futureTechs;

        Button okButton;

        public override void _Ready()
        {
            base._Ready();
            energyCheckBox = FindNode("EnergyCheckBox") as CheckBox;
            weaponsCheckBox = FindNode("WeaponsCheckBox") as CheckBox;
            propulsionCheckBox = FindNode("PropulsionCheckBox") as CheckBox;
            constructionCheckBox = FindNode("ConstructionCheckBox") as CheckBox;
            electronicsCheckBox = FindNode("ElectronicsCheckBox") as CheckBox;
            biotechnologyCheckBox = FindNode("BiotechnologyCheckBox") as CheckBox;

            energyLabel = FindNode("EnergyLabel") as Label;
            weaponsLabel = FindNode("WeaponsLabel") as Label;
            propulsionLabel = FindNode("PropulsionLabel") as Label;
            constructionLabel = FindNode("ConstructionLabel") as Label;
            electronicsLabel = FindNode("ElectronicsLabel") as Label;
            biotechnologyLabel = FindNode("BiotechnologyLabel") as Label;

            futureTechs = FindNode("FutureTechs") as FutureTechs;

            nextFieldToResearchMenuButton = FindNode("NextFieldToResearchMenuButton") as OptionButton;
            nextFieldToResearchMenuButton.PopulateOptionButton<NextResearchField>((nextField) => EnumUtils.GetLabelForNextResearchField(nextField));

            resourcesBudgetedAmount = FindNode("ResourcesBudgetedAmount") as SpinBox;

            resourcesNeededToCompleteAmountLabel = FindNode("ResourcesNeededToCompleteAmountLabel") as Label;
            estimatedTimeToCompletionAmountLabel = FindNode("EstimatedTimeToCompletionAmountLabel") as Label;
            annualResourcesAmountLabel = FindNode("AnnualResourcesAmountLabel") as Label;
            totalResourcesSpentAmountLabel = FindNode("TotalResourcesSpentAmountLabel") as Label;
            nextYearBudgetAmountLabel = FindNode("NextYearBudgetAmountLabel") as Label;

            okButton = FindNode("OKButton") as Button;

            resourcesBudgetedAmount.Connect("value_changed", this, nameof(OnResourcesBudgetedAmountValueChanged));

            energyCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));
            weaponsCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));
            propulsionCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));
            constructionCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));
            electronicsCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));
            biotechnologyCheckBox.Connect("pressed", this, nameof(OnTechFieldSelected));

            Connect("about_to_show", this, nameof(OnAboutToShow));
            Connect("popup_hide", this, nameof(OnPopupHide));
            okButton.Connect("pressed", this, nameof(OnOK));

        }

        void OnAboutToShow()
        {
            UpdateControls();
        }

        void OnPopupHide()
        {
            // nothing to do here, we don't want to save
        }

        void OnOK()
        {
            Me.Researching = GetSelectedTechField();

            Me.ResearchAmount = (int)resourcesBudgetedAmount.Value;
            Me.NextResearchField = (NextResearchField)nextFieldToResearchMenuButton.Selected;

            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();

            Hide();
        }

        void OnTechFieldSelected()
        {
            // show new future techs for this field
            futureTechs.UpdateIncomingTechs(GetSelectedTechField());
        }

        /// <summary>
        /// Get the selected tech field from the UI
        /// </summary>
        /// <returns></returns>
        TechField GetSelectedTechField()
        {
            if (energyCheckBox.Pressed)
            {
                return TechField.Energy;
            }
            else if (weaponsCheckBox.Pressed)
            {
                return TechField.Weapons;
            }
            else if (propulsionCheckBox.Pressed)
            {
                return TechField.Propulsion;
            }
            else if (constructionCheckBox.Pressed)
            {
                return TechField.Construction;
            }
            else if (electronicsCheckBox.Pressed)
            {
                return TechField.Electronics;
            }
            else if (biotechnologyCheckBox.Pressed)
            {
                return TechField.Biotechnology;
            }
            else
            {
                log.Error("Couldn't determine selected tech field from controls.");
                return TechField.Energy;
            }
        }

        void OnResourcesBudgetedAmountValueChanged(float value)
        {
            UpdateResearchEstimates();
        }

        void UpdateControls()
        {
            nextFieldToResearchMenuButton.Select((int)Me.NextResearchField);
            switch (Me.Researching)
            {
                case TechField.Energy:
                    energyCheckBox.Pressed = true;
                    break;
                case TechField.Weapons:
                    weaponsCheckBox.Pressed = true;
                    break;
                case TechField.Propulsion:
                    propulsionCheckBox.Pressed = true;
                    break;
                case TechField.Construction:
                    constructionCheckBox.Pressed = true;
                    break;
                case TechField.Electronics:
                    electronicsCheckBox.Pressed = true;
                    break;
                case TechField.Biotechnology:
                    biotechnologyCheckBox.Pressed = true;
                    break;
            }

            energyLabel.Text = $"{Me.TechLevels.Energy}";
            weaponsLabel.Text = $"{Me.TechLevels.Weapons}";
            propulsionLabel.Text = $"{Me.TechLevels.Propulsion}";
            constructionLabel.Text = $"{Me.TechLevels.Construction}";
            electronicsLabel.Text = $"{Me.TechLevels.Electronics}";
            biotechnologyLabel.Text = $"{Me.TechLevels.Biotechnology}";

            resourcesBudgetedAmount.Value = Me.ResearchAmount;

            UpdateResearchEstimates();
            futureTechs.UpdateIncomingTechs(GetSelectedTechField());
        }

        /// <summary>
        /// Update the fields that show budgeted resources for research, estimated time to completion, etc.
        /// </summary>
        void UpdateResearchEstimates()
        {
            if (Me.TechLevels[Me.Researching] >= RulesManager.Rules.MaxTechLevel)
            {
                resourcesNeededToCompleteAmountLabel.Text = "Max Level";
                estimatedTimeToCompletionAmountLabel.Text = "Max Level";
            }
            else
            {
                // get the research amount from the spin control. We use that to estimate to give the player of an idea of
                // the impact of their chnages
                int researchAmount = (int)resourcesBudgetedAmount.Value;

                // Determine how many resources we need to complete the currently selected research
                var field = GetSelectedTechField();
                int resourcesNeededToComplete = researcher.GetTotalCost(Me, field, Me.TechLevels[field]) - Me.TechLevelsSpent[field];
                resourcesNeededToCompleteAmountLabel.Text = $"{resourcesNeededToComplete}";
                var resourcesToSpend = Me.Planets.Sum(p => p.GetResourcesPerYearResearch(researchAmount));
                var totalResources = Me.Planets.Sum(p => p.ResourcesPerYear);
                if (resourcesToSpend <= 0)
                {
                    estimatedTimeToCompletionAmountLabel.Text = "Never";
                }
                else
                {
                    estimatedTimeToCompletionAmountLabel.Text = $"{(int)Math.Ceiling((double)resourcesNeededToComplete / resourcesToSpend)} years";
                }

                annualResourcesAmountLabel.Text = $"{totalResources}";
                totalResourcesSpentAmountLabel.Text = $"{Me.ResearchSpentLastYear}";
                nextYearBudgetAmountLabel.Text = $"{resourcesToSpend}";
            }
        }

    }

}
