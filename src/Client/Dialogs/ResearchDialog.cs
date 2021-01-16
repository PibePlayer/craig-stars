using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{

    public class ResearchDialog : WindowDialog
    {
        Player me;

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
            me = PlayersManager.Instance.Me;
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
                me.Researching = TechField.Energy;
            }
            else if (weaponsCheckBox.Pressed)
            {
                me.Researching = TechField.Weapons;
            }
            else if (propulsionCheckBox.Pressed)
            {
                me.Researching = TechField.Propulsion;
            }
            else if (constructionCheckBox.Pressed)
            {
                me.Researching = TechField.Construction;
            }
            else if (electronicsCheckBox.Pressed)
            {
                me.Researching = TechField.Electronics;
            }
            else if (biotechnologyCheckBox.Pressed)
            {
                me.Researching = TechField.Biotechnology;
            }

            me.ResearchAmount = (int)resourcesBudgetedAmount.Value;
            me.NextResearchField = (NextResearchField)nextFieldToResearchMenuButton.Selected;

            Hide();
        }

        void UpdateControls()
        {
            nextFieldToResearchMenuButton.Select((int)me.NextResearchField);
            switch (me.Researching)
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

            energyLabel.Text = $"{me.TechLevels.Energy}";
            weaponsLabel.Text = $"{me.TechLevels.Weapons}";
            propulsionLabel.Text = $"{me.TechLevels.Propulsion}";
            constructionLabel.Text = $"{me.TechLevels.Construction}";
            electronicsLabel.Text = $"{me.TechLevels.Electronics}";
            biotechnologyLabel.Text = $"{me.TechLevels.Biotechnology}";

            resourcesBudgetedAmount.Value = me.ResearchAmount;

        }
    }

}
