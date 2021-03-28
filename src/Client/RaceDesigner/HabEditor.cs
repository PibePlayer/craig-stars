using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class HabEditor : HBoxContainer
    {
        public delegate void HabChanged(HabType type, int low, int high, bool immune);
        public event HabChanged HabChangedEvent;
        public void PublishHabChangedEvent() => HabChangedEvent?.Invoke(Type, Low, High, Immune);


        [Export]
        public GUIColors GUIColors { get; set; } = new GUIColors();

        [Export]
        public HabType Type
        {
            get => type;
            set
            {
                type = value;
                UpdateControls();
            }
        }
        HabType type = HabType.Gravity;

        [Export]
        public int Low
        {
            get => low;
            set
            {
                low = value;
                UpdateControls();
            }
        }
        int low = 15;

        [Export]
        public int High
        {
            get => high;
            set
            {
                high = value;
                UpdateControls();
            }
        }
        int high = 85;

        [Export]
        public bool Immune
        {
            get => immune;
            set
            {
                immune = value;
                UpdateControls();
            }
        }
        bool immune = false;

        Label habLabel;
        Label habValueLabel;

        Bar hab;
        CheckBox immuneCheckBox;
        Button expandButton;
        Button shrinkButton;
        Button leftButton;
        Button rightButton;

        public override void _Ready()
        {
            habLabel = GetNode<Label>("HabLabel");
            habValueLabel = GetNode<Label>("HabValueLabel");
            hab = GetNode<Bar>("VBoxContainer/BarHBoxContainer/Hab");
            immuneCheckBox = GetNode<CheckBox>("VBoxContainer/AdjustHBoxContainer/ImmuneCheckBox");
            expandButton = GetNode<Button>("VBoxContainer/AdjustHBoxContainer/ExpandButton");
            shrinkButton = GetNode<Button>("VBoxContainer/AdjustHBoxContainer/ShrinkButton");
            leftButton = GetNode<Button>("VBoxContainer/BarHBoxContainer/LeftButton");
            rightButton = GetNode<Button>("VBoxContainer/BarHBoxContainer/RightButton");

            immuneCheckBox.Connect("pressed", this, nameof(OnImmuneCheckBoxPressed));
            expandButton.Connect("pressed", this, nameof(OnExpandButtonPressed));
            shrinkButton.Connect("pressed", this, nameof(OnShrinkButtonPressed));
            leftButton.Connect("pressed", this, nameof(OnLeftButtonPressed));
            rightButton.Connect("pressed", this, nameof(OnRightButtonPressed));

            hab.BarChangedEvent += OnBarChanged;

            UpdateControls();
        }

        public override void _ExitTree()
        {
            hab.BarChangedEvent -= OnBarChanged;
        }

        void OnBarChanged(int low, int high)
        {
            Low = low;
            High = high;
            UpdateControls();
            PublishHabChangedEvent();
        }

        void OnImmuneCheckBoxPressed()
        {
            Immune = immuneCheckBox.Pressed;
            UpdateControls();
            PublishHabChangedEvent();
        }

        void OnExpandButtonPressed()
        {
            if (Low > 0)
            {
                Low--;
            }
            if (High < 100)
            {
                High++;
            }
            UpdateControls();
            PublishHabChangedEvent();
        }

        void OnShrinkButtonPressed()
        {
            if (Low < High)
            {
                Low++;
            }
            if (High > Low)
            {
                High--;
            }
            UpdateControls();
            PublishHabChangedEvent();
        }

        void OnLeftButtonPressed()
        {
            if (Low > 0)
            {
                Low--;
                High--;
            }
            UpdateControls();
            PublishHabChangedEvent();
        }

        void OnRightButtonPressed()
        {
            if (High < 100)
            {
                Low++;
                High++;
            }
            UpdateControls();
            PublishHabChangedEvent();
        }

        void UpdateControls()
        {
            if (habLabel == null || habValueLabel == null)
            {
                // these are null when the scene is initialized
                // GD.PrintErr("Hab controls are null");
                return;
            }

            habLabel.Text = type.ToString();
            immuneCheckBox.Text = $"Immune to {type.ToString()}";

            if (immuneCheckBox.Pressed)
            {
                habValueLabel.Text = "N/A";
            }
            else
            {
                switch (type)
                {
                    case HabType.Gravity:
                        habValueLabel.Text = $"{TextUtils.GetGravString(Low)}\nto\n{TextUtils.GetGravString(High)}";
                        hab.BarColor = GUIColors.GravColor;
                        break;
                    case HabType.Temperature:
                        habValueLabel.Text = $"{TextUtils.GetTempString(Low)}\nto\n{TextUtils.GetTempString(High)}";
                        hab.BarColor = GUIColors.TempColor;
                        break;
                    case HabType.Radiation:
                        habValueLabel.Text = $"{TextUtils.GetRadString(Low)}\nto\n{TextUtils.GetRadString(High)}";
                        hab.BarColor = GUIColors.RadColor;
                        break;
                }
            }

            // update so our draw draws
            hab.ShowBar = !immuneCheckBox.Pressed;
            hab.Low = Low;
            hab.High = High;
            hab.Update();
        }


    }
}