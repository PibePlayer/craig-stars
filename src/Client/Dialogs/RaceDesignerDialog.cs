using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;


namespace CraigStars.Client
{
    public class RaceDesignerDialog : GameViewDialog
    {
        /// <summary>
        /// The race the editor is editing
        /// </summary>
        /// <value></value>
        public Race Race { get; set; } = Races.Humanoid;

        public string RaceFilename { get; set; }

        /// <summary>
        /// True if this race designer is editable or just readonly
        /// </summary>
        [Export]
        public bool Editable { get; set; } = true;

        // if this is true, UI change events shouldn't update the race because we are loading it
        bool loadingRace;

        /// <summary>
        /// The race that represents what is currently in the editor
        /// </summary>
        Race updatedRace;
        RacePointsCalculator racePointsCalculator = new RacePointsCalculator();

        TabContainer tabContainer;

        Label advantagePoints;

        LineEdit filename;
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
        SpinBox growthRate;
        Label habChancesDescriptionLabel;

        Control annualResourcesContainer;
        SpinBox annualResourcesSpinBox;
        Control planetaryProductionContainer;
        SpinBox colonistsPerResourceSpinBox;
        SpinBox factoryOutputSpinBox;
        SpinBox factoryCostSpinBox;
        SpinBox numFactoriesSpinBox;
        CheckBox factoriesCostLessCheckBox;
        SpinBox mineOutputSpinBox;
        SpinBox mineCostSpinBox;
        SpinBox numMinesSpinBox;

        ResearchCostEditor energyResearchCost;
        ResearchCostEditor weaponsResearchCost;
        ResearchCostEditor propulsionResearchCost;
        ResearchCostEditor constructionResearchCost;
        ResearchCostEditor electronicsResearchCost;
        ResearchCostEditor biotechnologyResearchCost;
        CheckBox techsStartHighCheckBox;


        public override void _Ready()
        {
            base._Ready();
            advantagePoints = (Label)FindNode("AdvantagePoints");

            tabContainer = (TabContainer)FindNode("TabContainer");

            filename = (LineEdit)FindNode("Filename");
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
            growthRate = (SpinBox)FindNode("GrowthRateSpinBox");
            habChancesDescriptionLabel = (Label)FindNode("HabChancesDescriptionLabel");

            // production
            annualResourcesContainer = (Control)FindNode("AnnualResourcesContainer");
            annualResourcesSpinBox = (SpinBox)FindNode("AnnualResourcesSpinBox");
            planetaryProductionContainer = (Control)FindNode("PlanetaryProductionContainer");
            colonistsPerResourceSpinBox = (SpinBox)FindNode("ColonistsPerResourceSpinBox");
            factoryOutputSpinBox = (SpinBox)FindNode("FactoryOutputSpinBox");
            factoryCostSpinBox = (SpinBox)FindNode("FactoryCostSpinBox");
            numFactoriesSpinBox = (SpinBox)FindNode("NumFactoriesSpinBox");
            factoriesCostLessCheckBox = (CheckBox)FindNode("FactoriesCostLessCheckBox");
            mineOutputSpinBox = (SpinBox)FindNode("MineOutputSpinBox");
            mineCostSpinBox = (SpinBox)FindNode("MineCostSpinBox");
            numMinesSpinBox = (SpinBox)FindNode("NumMinesSpinBox");

            // research
            energyResearchCost = (ResearchCostEditor)FindNode("EnergyResearchCost");
            weaponsResearchCost = (ResearchCostEditor)FindNode("WeaponsResearchCost");
            propulsionResearchCost = (ResearchCostEditor)FindNode("PropulsionResearchCost");
            constructionResearchCost = (ResearchCostEditor)FindNode("ConstructionResearchCost");
            electronicsResearchCost = (ResearchCostEditor)FindNode("ElectronicsResearchCost");
            biotechnologyResearchCost = (ResearchCostEditor)FindNode("BiotechnologyResearchCost");
            techsStartHighCheckBox = (CheckBox)FindNode("TechsStartHighCheckBox");

            raceName.Connect("text_changed", this, nameof(OnRaceNameTextChanged));

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
            growthRate.Connect("value_changed", this, nameof(OnGrowthRateChanged));

            annualResourcesSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            colonistsPerResourceSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            factoryOutputSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            factoryCostSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            numFactoriesSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            factoriesCostLessCheckBox.Connect("toggled", this, nameof(OnFactoriesCostLessToggled));
            mineOutputSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            mineCostSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));
            numMinesSpinBox.Connect("value_changed", this, nameof(OnProductionValueChanged));

            energyResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            weaponsResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            propulsionResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            constructionResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            electronicsResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            biotechnologyResearchCost.ResearchCostLevelChangedEvent += OnResearchCostChanged;
            techsStartHighCheckBox.Connect("toggled", this, nameof(OnTechsStartHighCheckBoxToggled));

            SetAsMinsize();
            // Show();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                gravHabEditor.HabChangedEvent -= OnHabChanged;
                tempHabEditor.HabChangedEvent -= OnHabChanged;
                radHabEditor.HabChangedEvent -= OnHabChanged;

                energyResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
                weaponsResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
                propulsionResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
                constructionResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
                electronicsResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
                biotechnologyResearchCost.ResearchCostLevelChangedEvent -= OnResearchCostChanged;
            }
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

            UpdateHabChancesDescription();
            UpdateAdvantagePoints();
        }

        void UpdateHabChancesDescription()
        {
            double habChance = updatedRace.GetHabChance();
            int approximateHabitablePlanetRatio = (int)(1d / habChance);
            if (habChance == 1)
            {
                habChancesDescriptionLabel.Text = $"All planets will be habitable to your race.";
            }
            else if (approximateHabitablePlanetRatio == 1)
            {
                habChancesDescriptionLabel.Text = $"Virtually all planets will be habitable to your race.";
            }
            else
            {
                habChancesDescriptionLabel.Text = $"You can expect that 1 in {approximateHabitablePlanetRatio} planets will be habitable to your race.";
            }
        }

        void PopulateSpendLeftover()
        {
            spendLeftoverPointsOn.Clear();
            foreach (SpendLeftoverPointsOn value in Enum.GetValues(typeof(SpendLeftoverPointsOn)))
            {
                spendLeftoverPointsOn.AddItem(EnumUtils.GetLabelForSpendLeftoverPointsOn(value));
            }
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();
            if (IsVisibleInTree())
            {
                loadingRace = true;
                tabContainer.CurrentTab = 0;
                updatedRace = Race;
                raceName.Text = Race.Name;
                racePluralName.Text = Race.PluralName;

                if (RaceFilename == null || RaceFilename.Empty())
                {
                    filename.Text = Race.PluralName;
                }

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

                // update production view
                annualResourcesContainer.Visible = Race.PRT == PRT.AR;
                planetaryProductionContainer.Visible = Race.PRT != PRT.AR;


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

                // hab
                gravHabEditor.Low = Race.HabLow.grav;
                gravHabEditor.High = Race.HabHigh.grav;
                gravHabEditor.Immune = Race.ImmuneGrav;
                tempHabEditor.Low = Race.HabLow.temp;
                tempHabEditor.High = Race.HabHigh.temp;
                tempHabEditor.Immune = Race.ImmuneTemp;
                radHabEditor.Low = Race.HabLow.rad;
                radHabEditor.High = Race.HabHigh.rad;
                radHabEditor.Immune = Race.ImmuneRad;

                UpdateHabChancesDescription();

                // growth
                growthRate.Value = Race.GrowthRate;

                // production
                colonistsPerResourceSpinBox.Value = Race.PopEfficiency * 100;
                annualResourcesSpinBox.Value = Race.PopEfficiency;
                factoryOutputSpinBox.Value = Race.FactoryOutput;
                factoryCostSpinBox.Value = Race.FactoryCost;
                numFactoriesSpinBox.Value = Race.NumFactories;
                factoriesCostLessCheckBox.Pressed = Race.FactoriesCostLess;
                mineOutputSpinBox.Value = Race.MineOutput;
                mineCostSpinBox.Value = Race.MineCost;
                numMinesSpinBox.Value = Race.NumMines;

                // research cost
                energyResearchCost.Level = Race.ResearchCost[TechField.Energy];
                weaponsResearchCost.Level = Race.ResearchCost[TechField.Weapons];
                propulsionResearchCost.Level = Race.ResearchCost[TechField.Propulsion];
                constructionResearchCost.Level = Race.ResearchCost[TechField.Construction];
                electronicsResearchCost.Level = Race.ResearchCost[TechField.Electronics];
                biotechnologyResearchCost.Level = Race.ResearchCost[TechField.Biotechnology];
                UpdateTechsStartHighLabel(Race);

                loadingRace = false;

                UpdateAdvantagePoints();

            }
        }

        protected override void OnOk()
        {
            if (Editable)
            {
                UpdateRace(Race);
                if (RacesManager.FileExists(filename.Text))
                {
                    CSConfirmDialog.Show(
                        $"A race with the file named: {filename.Text} already exists. Do you want to overwrite it?",
                        () =>
                        {
                            RacesManager.SaveRace(Race, filename.Text);
                            base.OnOk();
                        });
                }
                else
                {
                    RacesManager.SaveRace(Race, filename.Text);
                    base.OnOk();
                }
            }
            else
            {
                // hide without saving or calling our OnOk callback
                Hide();
            }
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

            race.HabLow = new Hab(
                gravHabEditor.Low,
                tempHabEditor.Low,
                radHabEditor.Low
            );
            race.HabHigh = new Hab(
                gravHabEditor.High,
                tempHabEditor.High,
                radHabEditor.High
            );
            race.ImmuneGrav = gravHabEditor.Immune;
            race.ImmuneTemp = tempHabEditor.Immune;
            race.ImmuneRad = radHabEditor.Immune;

            race.GrowthRate = (int)growthRate.Value;

            // production
            race.PopEfficiency = (int)(colonistsPerResourceSpinBox.Value / 100);
            race.FactoryOutput = (int)factoryOutputSpinBox.Value;
            race.FactoryCost = (int)factoryCostSpinBox.Value;
            race.NumFactories = (int)numFactoriesSpinBox.Value;
            race.FactoriesCostLess = factoriesCostLessCheckBox.Pressed;
            race.MineOutput = (int)mineOutputSpinBox.Value;
            race.MineCost = (int)mineCostSpinBox.Value;
            race.NumMines = (int)numMinesSpinBox.Value;

            // research cost
            race.ResearchCost[TechField.Energy] = energyResearchCost.Level;
            race.ResearchCost[TechField.Weapons] = weaponsResearchCost.Level;
            race.ResearchCost[TechField.Propulsion] = propulsionResearchCost.Level;
            race.ResearchCost[TechField.Construction] = constructionResearchCost.Level;
            race.ResearchCost[TechField.Electronics] = electronicsResearchCost.Level;
            race.ResearchCost[TechField.Biotechnology] = biotechnologyResearchCost.Level;
            race.TechsStartHigh = techsStartHighCheckBox.Pressed;

        }

        void UpdateTechsStartHighLabel(Race race)
        {
            techsStartHighCheckBox.Text = $"All 'Costs 75% extra' research fields start at Tech {race.Spec.TechsCostExtraLevel}";
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

        /// <summary>
        /// As a convenience, keep the race name, filename, and plural name in sync
        /// </summary>
        /// <param name="newText"></param>
        void OnRaceNameTextChanged(string newText)
        {
            if (loadingRace) return;
            if (filename.Text == racePluralName.Text)
            {
                filename.Text = newText + "s";
            }
            racePluralName.Text = newText + "s";
        }

        void OnPRTCheckBoxPressed(PRT prt)
        {
            prtDescription.Text = TextUtils.GetDescriptionForPRT(prt);

            if (loadingRace) return;
            updatedRace.PRT = prt;
            UpdateTechsStartHighLabel(updatedRace);
            annualResourcesContainer.Visible = prt == PRT.AR;
            planetaryProductionContainer.Visible = prt != PRT.AR;
            UpdateAdvantagePoints();
        }

        void OnLRTCheckBoxToggled(bool buttonPressed, LRT lrt)
        {
            lrtDescriptionLabel.Text = EnumUtils.GetLabelForLRT(lrt);
            lrtDescription.Text = TextUtils.GetDescriptionForLRT(lrt);

            if (loadingRace) return;

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

        void OnGrowthRateChanged(float value)
        {
            if (loadingRace) return;

            updatedRace.GrowthRate = (int)value;
            UpdateAdvantagePoints();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void OnProductionValueChanged(float value)
        {
            if (loadingRace) return;

            updatedRace.PopEfficiency = (int)(colonistsPerResourceSpinBox.Value / 100);
            updatedRace.FactoryOutput = (int)factoryOutputSpinBox.Value;
            updatedRace.FactoryCost = (int)factoryCostSpinBox.Value;
            updatedRace.NumFactories = (int)numFactoriesSpinBox.Value;
            updatedRace.MineOutput = (int)mineOutputSpinBox.Value;
            updatedRace.MineCost = (int)mineCostSpinBox.Value;
            updatedRace.NumMines = (int)numMinesSpinBox.Value;
            UpdateAdvantagePoints();
        }

        void OnFactoriesCostLessToggled(bool buttonPressed)
        {
            if (loadingRace) return;

            updatedRace.FactoriesCostLess = buttonPressed;
            UpdateAdvantagePoints();
        }

        void OnResearchCostChanged(TechField field, ResearchCostLevel level)
        {
            if (loadingRace) return;
            updatedRace.ResearchCost[field] = level;
            UpdateAdvantagePoints();
        }

        void OnTechsStartHighCheckBoxToggled(bool buttonPressed)
        {
            if (loadingRace) return;
            updatedRace.TechsStartHigh = buttonPressed;
            UpdateAdvantagePoints();
        }
    }
}