using Godot;
using System;
using CraigStars.Utils;
using CraigStars.Singletons;

namespace CraigStars.Client
{
    public class MineFieldSummaryContainer : MapObjectSummary<MineFieldSprite>
    {
        static CSLog log = LogProvider.GetLogger(typeof(MineFieldSummaryContainer));

        [Inject] private MineFieldDecayer mineFieldDecayer;

        TextureRect icon;
        Label location;
        Label fieldType;
        Label fieldRadius;
        Label decayRateLabel;
        Label decayRate;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            location = GetNode<Label>("HBoxContainer/GridContainer/Location");
            fieldType = GetNode<Label>("HBoxContainer/GridContainer/FieldType");
            fieldRadius = GetNode<Label>("HBoxContainer/GridContainer/FieldRadius");
            decayRate = GetNode<Label>("HBoxContainer/GridContainer/DecayRate");
            decayRateLabel = GetNode<Label>("HBoxContainer/GridContainer/DecayRateLabel");
            icon = GetNode<TextureRect>("HBoxContainer/Panel/Icon");
        }

        protected override void UpdateControls()
        {
            decayRate.Visible = false;
            decayRateLabel.Visible = false;
            if (MapObject != null)
            {
                location.Text = $"{TextUtils.GetPositionString(MapObject.MineField.Position)}";
                fieldType.Text = $"{EnumUtils.GetLabelForMineFieldType(MapObject.MineField.Type)}";
                fieldRadius.Text = $"{MapObject.MineField.Radius:.} l.y. ({MapObject.MineField.NumMines} mines)";
                if (MapObject.OwnedByMe)
                {
                    decayRate.Visible = true;
                    decayRateLabel.Visible = true;
                    decayRate.Text = $"{mineFieldDecayer.GetDecayRate(MapObject.MineField, Me, Me.AllPlanets)} mines / year";
                }

                switch (MapObject.MineField.Type)
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