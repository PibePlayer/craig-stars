using CraigStars.Utils;
using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class HabBar : HBoxContainer
    {
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
        public int HabValue
        {
            get => habValue;
            set
            {
                habValue = value;
                UpdateControls();
            }
        }
        int habValue = 50;

        [Export]
        public int TerraformHabValue
        {
            get => terraformHabValue;
            set
            {
                terraformHabValue = value;
                UpdateControls();
            }
        }
        int terraformHabValue = 50;

        [Export]
        public bool ShowSeparator
        {
            get => showSeparator;
            set
            {
                showSeparator = value;
                UpdateControls();
            }
        }
        bool showSeparator = true;

        Label habLabel;
        Label habValueLabel;
        HSeparator separator;
        Control hab;

        public override void _Ready()
        {
            habLabel = FindNode("HabLabel") as Label;
            habValueLabel = FindNode("HabValueLabel") as Label;
            separator = FindNode("Separator") as HSeparator;
            hab = FindNode("Hab") as Control;

            UpdateControls();
        }

        public override void _Draw()
        {
            if (hab == null || GUIColors == null)
            {
                // these are null when the scene is initialized
                // GD.PrintErr("Hab controls are null");
                return;
            }

            var barColor = Colors.White;
            var valueColor = Colors.White;
            switch (type)
            {
                case HabType.Gravity:
                    barColor = GUIColors.GravColor;
                    valueColor = GUIColors.GravValueColor;
                    break;
                case HabType.Temperature:
                    barColor = GUIColors.TempColor;
                    valueColor = GUIColors.TempValueColor;
                    break;
                case HabType.Radiation:
                    barColor = GUIColors.RadColor;
                    valueColor = GUIColors.RadValueColor;
                    break;
            }
            Rect2 rect = new Rect2(
                new Vector2(hab.RectPosition.x + (Low / 100f * hab.RectSize.x), hab.RectPosition.y),
                new Vector2(hab.RectSize.x * ((High - Low) / 100f), hab.RectSize.y)
            );

            var valueSize = hab.RectSize.y;
            Vector2 valuePosition = new Vector2(hab.RectPosition.x + (HabValue / 100f * hab.RectSize.x), hab.RectPosition.y + hab.RectSize.y / 2);

            // color in a black background with 
            DrawRect(new Rect2(hab.RectPosition, hab.RectSize), Colors.Black, true);
            DrawRect(rect, barColor, true);

            this.DrawDiamondOutline(valuePosition, valueSize, valueColor);
            this.DrawCross(valuePosition, valueSize, valueColor);

            if (TerraformHabValue != HabValue)
            {
                // draw terraform line
                Vector2 terraformValuePosition = new Vector2(hab.RectPosition.x + (TerraformHabValue / 100f * hab.RectSize.x), hab.RectPosition.y + hab.RectSize.y / 2);
                DrawLine(valuePosition, terraformValuePosition, valueColor);
            }
        }

        void UpdateControls()
        {
            if (habLabel == null || habValueLabel == null || separator == null)
            {
                // these are null when the scene is initialized
                // GD.PrintErr("Hab controls are null");
                return;
            }
            switch (type)
            {
                case HabType.Gravity:
                    habLabel.Text = "Gravity";
                    habValueLabel.Text = TextUtils.GetGravString(HabValue);
                    break;
                case HabType.Temperature:
                    habLabel.Text = "Temperature";
                    habValueLabel.Text = TextUtils.GetTempString(HabValue);
                    break;
                case HabType.Radiation:
                    habLabel.Text = "Radiation";
                    habValueLabel.Text = TextUtils.GetRadString(HabValue);
                    break;
            }

            separator.Visible = ShowSeparator;
            separator.RectPosition = new Vector2(hab.RectPosition.x, separator.RectPosition.y);
            separator.RectSize = new Vector2(hab.RectSize.x, separator.RectPosition.y);

            // update so our draw draws
            Update();
        }


    }
}

