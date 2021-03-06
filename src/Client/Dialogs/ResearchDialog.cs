using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{

    public class ResearchDialog : WindowDialog
    {
        Player Me { get => PlayersManager.Me; }

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

        Button okButton;

        public override void _Ready()
        {
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

            nextFieldToResearchMenuButton = FindNode("NextFieldToResearchMenuButton") as OptionButton;

            resourcesBudgetedAmount = FindNode("ResourcesBudgetedAmount") as SpinBox;

            okButton = FindNode("OKButton") as Button;

            foreach (NextResearchField nextResearchField in Enum.GetValues(typeof(NextResearchField)))
            {
                switch (nextResearchField)
                {
                    case NextResearchField.SameField:
                        nextFieldToResearchMenuButton.AddItem("<Same field>");
                        break;
                    case NextResearchField.LowestField:
                        nextFieldToResearchMenuButton.AddItem("<Lowest field>");
                        break;
                    default:
                        nextFieldToResearchMenuButton.AddItem(nextResearchField.ToString());
                        break;
                }
            }


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
            if (energyCheckBox.Pressed)
            {
                Me.Researching = TechField.Energy;
            }
            else if (weaponsCheckBox.Pressed)
            {
                Me.Researching = TechField.Weapons;
            }
            else if (propulsionCheckBox.Pressed)
            {
                Me.Researching = TechField.Propulsion;
            }
            else if (constructionCheckBox.Pressed)
            {
                Me.Researching = TechField.Construction;
            }
            else if (electronicsCheckBox.Pressed)
            {
                Me.Researching = TechField.Electronics;
            }
            else if (biotechnologyCheckBox.Pressed)
            {
                Me.Researching = TechField.Biotechnology;
            }

            Me.ResearchAmount = (int)resourcesBudgetedAmount.Value;
            Me.NextResearchField = (NextResearchField)nextFieldToResearchMenuButton.Selected;

            Hide();
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

        }
    }

}
