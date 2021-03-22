using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;
using System;


namespace CraigStars
{
    public class RaceDesignerDialog : WindowDialog
    {
        /// <summary>
        /// The race the editor is editing
        /// </summary>
        /// <value></value>
        public Race Race { get; set; } = Races.Humanoid;

        /// <summary>
        /// The race that represents what is currently in the editor
        /// </summary>
        Race updatedRace;
        RacePointsCalculator racePointsCalculator = new RacePointsCalculator();

        Button okButton;

        Label advantagePoints;

        LineEdit raceName;
        LineEdit racePluralName;
        OptionButton spendLeftoverPointsOn;

        CheckBox heCheckBox;
        CheckBox ssCheckBox;
        CheckBox wmCheckBox;
        CheckBox caCheckBox;
        CheckBox isCheckBox;
        CheckBox sdCheckBox;
        CheckBox ppCheckBox;
        CheckBox itCheckBox;
        CheckBox arCheckBox;
        CheckBox joatCheckBox;
        Label prtDescription;

        CheckBox ifeCheckBox;
        CheckBox ttCheckBox;
        CheckBox armCheckBox;
        CheckBox isbCheckBox;
        CheckBox grCheckBox;
        CheckBox urCheckBox;
        CheckBox nrseCheckBox;
        CheckBox obrmCheckBox;
        CheckBox nasCheckBox;
        CheckBox lspCheckBox;
        CheckBox betCheckBox;
        CheckBox rsCheckBox;
        CheckBox maCheckBox;
        CheckBox ceCheckBox;
        Label lrtDescriptionLabel;
        Label lrtDescription;

        HabEditor gravHabEditor;
        HabEditor tempHabEditor;
        HabEditor radHabEditor;

        public override void _Ready()
        {
            advantagePoints = (Label)FindNode("AdvantagePoints");

            raceName = (LineEdit)FindNode("RaceName");
            racePluralName = (LineEdit)FindNode("RacePluralName");
            spendLeftoverPointsOn = (OptionButton)FindNode("SpendLeftoverPointsOn");
            PopulateSpendLeftover();

            heCheckBox = (CheckBox)FindNode("HECheckBox");
            ssCheckBox = (CheckBox)FindNode("SSCheckBox");
            wmCheckBox = (CheckBox)FindNode("WMCheckBox");
            caCheckBox = (CheckBox)FindNode("CACheckBox");
            isCheckBox = (CheckBox)FindNode("ISCheckBox");
            sdCheckBox = (CheckBox)FindNode("SDCheckBox");
            ppCheckBox = (CheckBox)FindNode("PPCheckBox");
            itCheckBox = (CheckBox)FindNode("ITCheckBox");
            arCheckBox = (CheckBox)FindNode("ARCheckBox");
            joatCheckBox = (CheckBox)FindNode("JoaTCheckBox");
            prtDescription = (Label)FindNode("PRTDescription");

            // LRTs
            ifeCheckBox = (CheckBox)FindNode("IFECheckBox");
            ttCheckBox = (CheckBox)FindNode("TTCheckBox");
            armCheckBox = (CheckBox)FindNode("ARMCheckBox");
            isbCheckBox = (CheckBox)FindNode("ISBCheckBox");
            grCheckBox = (CheckBox)FindNode("GRCheckBox");
            urCheckBox = (CheckBox)FindNode("URCheckBox");
            nrseCheckBox = (CheckBox)FindNode("NRSECheckBox");
            obrmCheckBox = (CheckBox)FindNode("OBRMCheckBox");
            nasCheckBox = (CheckBox)FindNode("NASCheckBox");
            lspCheckBox = (CheckBox)FindNode("LSPCheckBox");
            betCheckBox = (CheckBox)FindNode("BETCheckBox");
            rsCheckBox = (CheckBox)FindNode("RSCheckBox");
            maCheckBox = (CheckBox)FindNode("MACheckBox");
            ceCheckBox = (CheckBox)FindNode("CECheckBox");
            lrtDescription = (Label)FindNode("LRTDescription");
            lrtDescriptionLabel = (Label)FindNode("LRTDescriptionLabel");

            // hab
            gravHabEditor = (HabEditor)FindNode("GravHabEditor");
            tempHabEditor = (HabEditor)FindNode("TempHabEditor");
            radHabEditor = (HabEditor)FindNode("RadHabEditor");

            okButton = (Button)FindNode("OKButton");

            Connect("visibility_changed", this, nameof(OnVisibilityChanged));
            okButton.Connect("pressed", this, nameof(OnOk));

            heCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.HE });
            ssCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.SS });
            wmCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.WM });
            caCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.CA });
            isCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.IS });
            sdCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.SD });
            ppCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.PP });
            itCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.IT });
            arCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.AR });
            joatCheckBox.Connect("pressed", this, nameof(OnPRTCheckBoxPressed), new Godot.Collections.Array() { PRT.JoaT });

            ifeCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.IFE });
            ttCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.TT });
            armCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.ARM });
            isbCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.ISB });
            grCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.GR });
            urCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.UR });
            nrseCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.NRSE });
            obrmCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.OBRM });
            nasCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.NAS });
            lspCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.LSP });
            betCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.BET });
            rsCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.RS });
            maCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.MA });
            ceCheckBox.Connect("toggled", this, nameof(OnLRTCheckBoxToggled), new Godot.Collections.Array() { LRT.CE });

            gravHabEditor.HabChangedEvent += OnHabChanged;
            tempHabEditor.HabChangedEvent += OnHabChanged;
            radHabEditor.HabChangedEvent += OnHabChanged;
            Show();
        }

        public override void _ExitTree()
        {
            gravHabEditor.HabChangedEvent -= OnHabChanged;
            tempHabEditor.HabChangedEvent -= OnHabChanged;
            radHabEditor.HabChangedEvent -= OnHabChanged;
        }

        void OnHabChanged(HabType type, int low, int high, bool immune)
        {
            updatedRace.HabLow = updatedRace.HabLow.WithType(type, low);
            updatedRace.HabHigh = updatedRace.HabHigh.WithType(type, high);
            switch (type)
            {
                case HabType.Gravity:
                    updatedRace.ImmuneGrav = immune;
                    break;
                case HabType.Temperature:
                    updatedRace.ImmuneTemp = immune;
                    break;
                case HabType.Radiation:
                    updatedRace.ImmuneRad = immune;
                    break;
            }

            UpdateAdvantagePoints();
        }

        void PopulateSpendLeftover()
        {
            spendLeftoverPointsOn.Clear();
            foreach (SpendLeftoverPointsOn value in Enum.GetValues(typeof(SpendLeftoverPointsOn)))
            {
                spendLeftoverPointsOn.AddItem(EnumUtils.GetLabelForSpendLeftoverPointsOn(value));
            }
        }

        void OnVisibilityChanged()
        {
            if (Visible)
            {
                updatedRace = Race;
                raceName.Text = Race.Name;
                racePluralName.Text = Race.PluralName;
                spendLeftoverPointsOn.Selected = (int)Race.SpendLeftoverPointsOn;

                // PRT
                switch (Race.PRT)
                {
                    case PRT.HE:
                        heCheckBox.Pressed = true;
                        break;
                    case PRT.SS:
                        ssCheckBox.Pressed = true;
                        break;
                    case PRT.WM:
                        wmCheckBox.Pressed = true;
                        break;
                    case PRT.CA:
                        caCheckBox.Pressed = true;
                        break;
                    case PRT.IS:
                        isCheckBox.Pressed = true;
                        break;
                    case PRT.SD:
                        sdCheckBox.Pressed = true;
                        break;
                    case PRT.PP:
                        ppCheckBox.Pressed = true;
                        break;
                    case PRT.IT:
                        itCheckBox.Pressed = true;
                        break;
                    case PRT.AR:
                        arCheckBox.Pressed = true;
                        break;
                    case PRT.JoaT:
                        joatCheckBox.Pressed = true;
                        break;
                }

                // LRT
                ifeCheckBox.Pressed = Race.HasLRT(LRT.IFE);
                ttCheckBox.Pressed = Race.HasLRT(LRT.TT);
                armCheckBox.Pressed = Race.HasLRT(LRT.ARM);
                isbCheckBox.Pressed = Race.HasLRT(LRT.ISB);
                grCheckBox.Pressed = Race.HasLRT(LRT.GR);
                urCheckBox.Pressed = Race.HasLRT(LRT.UR);
                nrseCheckBox.Pressed = Race.HasLRT(LRT.NRSE);
                obrmCheckBox.Pressed = Race.HasLRT(LRT.OBRM);
                nasCheckBox.Pressed = Race.HasLRT(LRT.NAS);
                lspCheckBox.Pressed = Race.HasLRT(LRT.LSP);
                betCheckBox.Pressed = Race.HasLRT(LRT.BET);
                rsCheckBox.Pressed = Race.HasLRT(LRT.RS);
                maCheckBox.Pressed = Race.HasLRT(LRT.MA);
                ceCheckBox.Pressed = Race.HasLRT(LRT.CE);

            }
        }

        void OnOk()
        {
            UpdateRace(Race);
            // all done
            Hide();
        }

        void UpdateRace(Race race)
        {
            race.Name = raceName.Text;
            race.PluralName = racePluralName.Text;
            race.SpendLeftoverPointsOn = (SpendLeftoverPointsOn)spendLeftoverPointsOn.Selected;

            if (heCheckBox.Pressed) { race.PRT = PRT.HE; }
            if (ssCheckBox.Pressed) { race.PRT = PRT.SS; }
            if (wmCheckBox.Pressed) { race.PRT = PRT.WM; }
            if (caCheckBox.Pressed) { race.PRT = PRT.CA; }
            if (isCheckBox.Pressed) { race.PRT = PRT.IS; }
            if (sdCheckBox.Pressed) { race.PRT = PRT.SD; }
            if (ppCheckBox.Pressed) { race.PRT = PRT.PP; }
            if (itCheckBox.Pressed) { race.PRT = PRT.IT; }
            if (arCheckBox.Pressed) { race.PRT = PRT.AR; }
            if (joatCheckBox.Pressed) { race.PRT = PRT.JoaT; }

            race.LRTs.Clear();
            if (ifeCheckBox.Pressed) { race.LRTs.Add(LRT.IFE); }
            if (ttCheckBox.Pressed) { race.LRTs.Add(LRT.TT); }
            if (armCheckBox.Pressed) { race.LRTs.Add(LRT.ARM); }
            if (isbCheckBox.Pressed) { race.LRTs.Add(LRT.ISB); }
            if (grCheckBox.Pressed) { race.LRTs.Add(LRT.GR); }
            if (urCheckBox.Pressed) { race.LRTs.Add(LRT.UR); }
            if (nrseCheckBox.Pressed) { race.LRTs.Add(LRT.NRSE); }
            if (obrmCheckBox.Pressed) { race.LRTs.Add(LRT.OBRM); }
            if (nasCheckBox.Pressed) { race.LRTs.Add(LRT.NAS); }
            if (lspCheckBox.Pressed) { race.LRTs.Add(LRT.LSP); }
            if (betCheckBox.Pressed) { race.LRTs.Add(LRT.BET); }
            if (rsCheckBox.Pressed) { race.LRTs.Add(LRT.RS); }
            if (maCheckBox.Pressed) { race.LRTs.Add(LRT.MA); }
            if (ceCheckBox.Pressed) { race.LRTs.Add(LRT.CE); }
        }

        void UpdateAdvantagePoints()
        {
            var points = racePointsCalculator.GetAdvantagePoints(updatedRace, RulesManager.Rules.RaceStartingPoints);
            advantagePoints.Text = $"{points}";
            if (points >= 0)
            {
                advantagePoints.Modulate = Colors.White;
                okButton.Disabled = false;
            }
            else
            {
                okButton.Disabled = true;
                advantagePoints.Modulate = Colors.Red;
            }
        }

        void OnPRTCheckBoxPressed(PRT prt)
        {
            prtDescription.Text = TextUtils.GetDescriptionForPRT(prt);
            updatedRace.PRT = prt;
            UpdateAdvantagePoints();
        }

        void OnLRTCheckBoxToggled(bool buttonPressed, LRT lrt)
        {
            lrtDescriptionLabel.Text = EnumUtils.GetLabelForLRT(lrt);
            lrtDescription.Text = TextUtils.GetDescriptionForLRT(lrt);

            if (buttonPressed)
            {
                updatedRace.LRTs.Add(lrt);
            }
            else
            {
                updatedRace.LRTs.Remove(lrt);
            }

            UpdateAdvantagePoints();
        }
    }
}