using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

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
        CheckButton detonateCheckButton;

        public override void _Ready()
        {
            this.ResolveDependencies();
            base._Ready();
            location = GetNode<Label>("VBoxContainer/HBoxContainer/GridContainer/Location");
            fieldType = GetNode<Label>("VBoxContainer/HBoxContainer/GridContainer/FieldType");
            fieldRadius = GetNode<Label>("VBoxContainer/HBoxContainer/GridContainer/FieldRadius");
            decayRate = GetNode<Label>("VBoxContainer/HBoxContainer/GridContainer/DecayRate");
            decayRateLabel = GetNode<Label>("VBoxContainer/HBoxContainer/GridContainer/DecayRateLabel");
            icon = GetNode<TextureRect>("VBoxContainer/HBoxContainer/Panel/Icon");
            detonateCheckButton = GetNode<CheckButton>("VBoxContainer/DetonateCheckButton");

            detonateCheckButton.Connect("toggled", this, nameof(OnDetonateCheckButtonToggled));

        }

        void OnDetonateCheckButtonToggled(bool toggled)
        {
            if (MapObject != null)
            {
                MapObject.MineField.Detonate = toggled;
            }
        }

        protected override void UpdateControls()
        {
            decayRate.Visible = false;
            decayRateLabel.Visible = false;
            detonateCheckButton.Visible = false;
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

                    // SD races can detonate standard minefields
                    detonateCheckButton.Visible = Me.Race.Spec.CanDetonateMineFields && MapObject.MineField.Type == MineFieldType.Standard;
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