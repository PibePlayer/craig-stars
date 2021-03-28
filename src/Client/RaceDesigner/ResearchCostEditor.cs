using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class ResearchCostEditor : Panel
    {
        public delegate void ResearchCostLevelChanged(TechField field, ResearchCostLevel level);
        public event ResearchCostLevelChanged ResearchCostLevelChangedEvent;
        public void PublishResearchCostLevelChangedEvent() => ResearchCostLevelChangedEvent?.Invoke(Field, Level);


        [Export]
        public TechField Field
        {
            get => field;
            set
            {
                field = value;
                UpdateControls();
            }
        }
        TechField field = TechField.Energy;

        [Export]
        public ResearchCostLevel Level
        {
            get => level;
            set
            {
                level = value;
                UpdateControls();
            }
        }
        ResearchCostLevel level = ResearchCostLevel.Standard;

        Label label;
        CheckBox researchCostExtraCheckBox;
        CheckBox researchCostStandardCheckBox;
        CheckBox researchCostLessCheckBox;

        public override void _Ready()
        {
            label = GetNode<Label>("MarginContainer/VBoxContainer/Label");
            researchCostExtraCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ResearchCostExtraCheckBox");
            researchCostStandardCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ResearchCostStandardCheckBox");
            researchCostLessCheckBox = GetNode<CheckBox>("MarginContainer/VBoxContainer/ResearchCostLessCheckBox");

            researchCostExtraCheckBox.Connect("pressed", this, nameof(OnResearchCostCheckBoxPressed), new Godot.Collections.Array() { ResearchCostLevel.Extra });
            researchCostStandardCheckBox.Connect("pressed", this, nameof(OnResearchCostCheckBoxPressed), new Godot.Collections.Array() { ResearchCostLevel.Standard });
            researchCostLessCheckBox.Connect("pressed", this, nameof(OnResearchCostCheckBoxPressed), new Godot.Collections.Array() { ResearchCostLevel.Less });
        }

        void OnResearchCostCheckBoxPressed(ResearchCostLevel level)
        {
            Level = level;
            PublishResearchCostLevelChangedEvent();
        }

        void UpdateControls()
        {
            if (label != null)
            {
                label.Text = $"{Field.ToString()} Research";
                switch (Level)
                {
                    case ResearchCostLevel.Extra:
                        researchCostExtraCheckBox.Pressed = true;
                        break;
                    case ResearchCostLevel.Standard:
                        researchCostStandardCheckBox.Pressed = true;
                        break;
                    case ResearchCostLevel.Less:
                        researchCostLessCheckBox.Pressed = true;
                        break;
                }
            }
        }
    }
}
