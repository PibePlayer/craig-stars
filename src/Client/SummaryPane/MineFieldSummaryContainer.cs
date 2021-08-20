using Godot;
using System;
using CraigStars.Utils;

namespace CraigStars.Client
{
    public class MineFieldSummaryContainer : Container
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldSummaryContainer));

        MineFieldSprite mineField;
        TextureRect icon;
        Label location;
        Label fieldType;
        Label fieldRadius;
        Label decayRateLabel;
        Label decayRate;

        public override void _Ready()
        {
            base._Ready();
            location = GetNode<Label>("HBoxContainer/GridContainer/Location");
            fieldType = GetNode<Label>("HBoxContainer/GridContainer/FieldType");
            fieldRadius = GetNode<Label>("HBoxContainer/GridContainer/FieldRadius");
            decayRate = GetNode<Label>("HBoxContainer/GridContainer/DecayRate");
            decayRateLabel = GetNode<Label>("HBoxContainer/GridContainer/DecayRateLabel");
            icon = GetNode<TextureRect>("HBoxContainer/Panel/Icon");

            EventManager.MapObjectSelectedEvent += OnMapObjectSelected;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.MapObjectSelectedEvent -= OnMapObjectSelected;
            }
        }

        void OnMapObjectSelected(MapObjectSprite mapObject)
        {
            mineField = mapObject as MineFieldSprite;
            UpdateControls();
        }

        void UpdateControls()
        {
            decayRate.Visible = false;
            decayRateLabel.Visible = false;
            if (mineField != null)
            {
                location.Text = $"{TextUtils.GetPositionString(mineField.MineField.Position)}";
                fieldType.Text = $"{EnumUtils.GetLabelForMineFieldType(mineField.MineField.Type)}";
                fieldRadius.Text = $"{mineField.MineField.Radius:.} l.y. ({mineField.MineField.NumMines} mines)";
                if (mineField.OwnedByMe)
                {
                    decayRate.Visible = true;
                    decayRateLabel.Visible = true;
                    decayRate.Text = $"{mineField.MineField.GetDecayRate()} mines / year";
                }

                switch (mineField.MineField.Type)
                {
                    case MineFieldType.Standard:
                        icon.Texture = ResourceLoader.Load<Texture>("res://assets/gui/minefields/StandardMineField.png");
                        break;
                    case MineFieldType.Heavy:
                        icon.Texture = ResourceLoader.Load<Texture>("res://assets/gui/minefields/HeavyMineField.png");
                        break;
                    case MineFieldType.SpeedBump:
                        icon.Texture = ResourceLoader.Load<Texture>("res://assets/gui/minefields/SpeedBumpMineField.png");
                        break;
                }
            }
        }
    }
}