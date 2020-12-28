using Godot;
using System;

namespace CraigStars
{
    [Tool]
    public class HabBar : HBoxContainer
    {
        GUIColors guiColors = new GUIColors();

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
            guiColors = GD.Load("res://src/GUI/GUIColors.tres") as GUIColors;
            if (guiColors == null)
            {
                guiColors = new GUIColors();
            }

            UpdateControls();
        }

        public override void _Draw()
        {
            if (hab == null || guiColors == null)
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
                    barColor = guiColors.GravColor;
                    valueColor = guiColors.GravValueColor;
                    break;
                case HabType.Temperature:
                    barColor = guiColors.TempColor;
                    valueColor = guiColors.TempValueColor;
                    break;
                case HabType.Radiation:
                    barColor = guiColors.RadColor;
                    valueColor = guiColors.RadValueColor;
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

            // draw a diamond shape
            DrawPolyline(new Vector2[] {
                new Vector2(valuePosition.x - valueSize / 2, valuePosition.y),
                new Vector2(valuePosition.x, valuePosition.y - valueSize / 2),
                new Vector2(valuePosition.x + valueSize / 2, valuePosition.y),
                new Vector2(valuePosition.x, valuePosition.y + valueSize / 2),
                new Vector2(valuePosition.x - valueSize / 2, valuePosition.y),
            }, valueColor);

            // draw a cross
            DrawLine(
                new Vector2(valuePosition.x - valueSize / 2, valuePosition.y),
                new Vector2(valuePosition.x + valueSize / 2, valuePosition.y),
                valueColor
            );
            DrawLine(
                new Vector2(valuePosition.x, valuePosition.y - valueSize / 2),
                new Vector2(valuePosition.x, valuePosition.y + valueSize / 2),
                valueColor
            );
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
                    habValueLabel.Text = GetGravString(HabValue);
                    break;
                case HabType.Temperature:
                    habLabel.Text = "Temperature";
                    habValueLabel.Text = GetTempString(HabValue);
                    break;
                case HabType.Radiation:
                    habLabel.Text = "Radiation";
                    habValueLabel.Text = GetRadString(HabValue);
                    break;
            }

            separator.Visible = ShowSeparator;
            separator.RectPosition = new Vector2(hab.RectPosition.x, separator.RectPosition.y);
            separator.RectSize = new Vector2(hab.RectSize.x, separator.RectPosition.y);

            // update so our draw draws
            Update();
        }

        internal static String GetGravString(int grav)
        {
            int result, tmp = Math.Abs(grav - 50);
            if (tmp <= 25)
                result = (tmp + 25) * 4;
            else
                result = tmp * 24 - 400;
            if (grav < 50)
                result = 10000 / result;

            double value = result / 100 + (result % 100 / 100.0);
            return $"{value:0.00}g";
        }

        internal static string GetTempString(int temp)
        {
            int result;
            result = (temp - 50) * 4;

            return $"{result}°C";
        }

        internal static string GetRadString(int rad)
        {
            return rad + "mR";
        }

    }
}

