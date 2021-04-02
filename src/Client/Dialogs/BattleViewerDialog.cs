using CraigStars.Singletons;
using Godot;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    public class BattleViewerDialog : WindowDialog
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BattleViewerDialog));

        [Export]
        public int GridSize { get; set; } = 10;

        [Export]
        public PackedScene BattleGridTokenScene { get; set; }

        // for debugging, we can generate a test battle
        bool useTestBattle = true;

        public BattleRecord BattleRecord { get; set; } = new BattleRecord();

        BattleGridSquare[,] Squares = new BattleGridSquare[10, 10];

        Label roundLabel;
        Label phaseLabel;
        Label actionLabel;
        Label actionRaceLabel;
        Label actionDesignLabel;
        Label actionMoveLabel;
        Container actionAttackLabelContainer;
        Label actionAttackLabel;
        Label actionAttackTargetLabel;
        Label actionAttackLocationLabel;
        Label actionAttackDamageLabel;

        Label selectionLabel;
        Label selectionRaceLabel;
        Label selectionDesignLabel;
        Label selectionInitiative;
        Label selectionMovement;
        Label selectionArmor;
        Label selectionDamage;
        Label selectionShields;

        Button resetBoardButton;
        Button nextActionButton;
        Button nextAttackActionButton;
        Button prevActionButton;

        int currentRound = 0;
        int currentAction = -1;
        int currentPhase = -1;
        int totalRounds = 0;
        int totalPhases = 0;

        BattleGridSquare selectedSquare;
        int selectedTokenIndex = 0;
        BattleGridToken selectedGridToken;
        BattleGridToken actionToken;
        List<BattleGridToken> Tokens = new List<BattleGridToken>();
        Dictionary<Guid, BattleGridToken> GridTokensByGuid { get; set; } = new Dictionary<Guid, BattleGridToken>();

        Button designDetailsButton;
        PopupPanel hullSummaryPopup;
        HullSummary hullSummary;

        public override void _Ready()
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    Squares[y, x] = GetNode<BattleGridSquare>($"MarginContainer/VBoxContainer/BattleContainer/BattleGrid/BattleGridSquare{(x + 1) + y * GridSize}");
                    Squares[y, x].Coordinates = new Vector2(y, x);
                    Squares[y, x].SelectedEvent += OnBattleGridSquareSelected;
                    log.Debug($"Loading square MarginContainer/VBoxContainer/BattleContainer/BattleGrid/BattleGridSquare{(x + 1) + y * GridSize}");
                }
            }

            phaseLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/PhaseRoundContainer/PhaseLabel");
            roundLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/PhaseRoundContainer/RoundLabel");
            actionLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionLabel");
            actionRaceLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionRaceLabel");
            actionDesignLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionDesignLabel");
            actionMoveLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionMoveLabel");
            actionAttackLabelContainer = GetNode<Container>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionAttackLabelContainer");
            actionAttackLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionAttackLabelContainer/ActionAttackLabel");
            actionAttackTargetLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionAttackLabelContainer/ActionAttackTargetLabel");
            actionAttackLocationLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionAttackLabelContainer/ActionAttackLocationLabel");
            actionAttackDamageLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/ActionDetailContainer/VBoxContainer/ActionAttackLabelContainer/ActionAttackDamageLabel");

            selectionLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/SelectionLabel");
            selectionRaceLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/SelectionRaceLabel");
            selectionDesignLabel = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/SelectionDesignLabel");
            selectionInitiative = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/GridContainer/SelectionInitiative");
            selectionMovement = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/GridContainer/SelectionMovement");
            selectionArmor = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/GridContainer/SelectionArmor");
            selectionDamage = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/GridContainer/SelectionDamage");
            selectionShields = GetNode<Label>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/GridContainer/SelectionShields");
            designDetailsButton = GetNode<Button>("MarginContainer/VBoxContainer/BattleContainer/MarginContainer/VBoxContainer/SelectedDetailContainer/VBoxContainer/DesignDetailsButton");

            hullSummaryPopup = GetNode<PopupPanel>("HullSummaryPopup");
            hullSummary = GetNode<HullSummary>("HullSummaryPopup/HullSummary");

            nextActionButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/NextActionButton");
            resetBoardButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/ResetBoardButton");
            prevActionButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/PrevActionButton");
            nextAttackActionButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainerButtons/NextAttackActionButton");

            designDetailsButton.Connect("button_down", this, nameof(OnDesignDetailsButtonDown));
            designDetailsButton.Connect("button_up", this, nameof(OnDesignDetailsButtonUp));
            nextActionButton.Connect("pressed", this, nameof(OnNextActionButtonPressed));
            nextAttackActionButton.Connect("pressed", this, nameof(OnNextAttackActionButtonPressed));
            resetBoardButton.Connect("pressed", this, nameof(OnResetBoardButtonPressed));
            prevActionButton.Connect("pressed", this, nameof(OnPrevActionButtonPressed));
            Connect("visibility_changed", this, nameof(OnVisibilityChanged));

            SetAsMinsize();
            
            // Uncomment this to debug JUSt the battle editor. This generates a test battle with a couple players
            // if (useTestBattle)
            // {
            //     BattleRecord = GenerateTestBattle();
            //     Show();
            // }
        }

        /// <summary>
        /// When the dialog becomes visible, this will setup the board with whatever information we have in the BattleRecord
        /// </summary>
        void OnVisibilityChanged()
        {
            if (Visible)
            {
                totalRounds = BattleRecord.ActionsPerRound.Count;
                totalPhases = BattleRecord.ActionsPerRound.Sum(actionsPerRound => actionsPerRound.Count);

                // clear out previous token nodes
                Tokens.ForEach(token => token.QueueFree());
                Tokens.Clear();
                GridTokensByGuid.Clear();

                // add new tokens
                BattleRecord.Tokens.ForEach(token =>
                {
                    BattleGridToken gridToken = BattleGridTokenScene.Instance() as BattleGridToken;
                    gridToken.Token = token;
                    Tokens.Add(gridToken);
                    GridTokensByGuid[gridToken.Token.Guid] = gridToken;
                });

                ResetBoard();
                UpdateDescriptionFields();
            }
        }

        /// <summary>
        /// When a battle grid is selected, cycle through the tokens and update our display information
        /// </summary>
        /// <param name="square"></param>
        /// <param name="token"></param>
        void OnBattleGridSquareSelected(BattleGridSquare square, BattleGridToken token)
        {
            if (selectedSquare != null)
            {
                if (selectedSquare == square)
                {
                    // cycle through tokens
                    selectedTokenIndex++;
                    if (selectedTokenIndex >= selectedSquare.Tokens.Count)
                    {
                        selectedTokenIndex = 0;
                    }
                }
                else
                {
                    // deselect the old square
                    selectedSquare.Selected = false;

                    // select this square
                    selectedSquare = square;
                    selectedSquare.Selected = true;
                    selectedTokenIndex = 0;
                }
            }
            else
            {
                // select this square
                selectedSquare = square;
                selectedSquare.Selected = true;
                selectedTokenIndex = 0;
            }

            if (selectedSquare.Tokens.Count > 0)
            {
                selectedGridToken = selectedSquare.Tokens[selectedTokenIndex];
                selectedSquare.SelectedTokenIndex = selectedTokenIndex;
            }
            else
            {
                selectedGridToken = null;
            }

            UpdateDescriptionFields();
        }

        /// <summary>
        /// Reset the board to starting positions and clear the state
        /// of all tokens
        /// </summary>
        void ResetBoard()
        {
            currentRound = 0;
            currentAction = -1;
            currentPhase = -1;

            // clear out existing tokens
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    Squares[y, x].ClearTokens();
                }
            }

            foreach (var token in Tokens)
            {
                token.FiringBeam = token.FiringTorpedo = token.TakingDamage = false;
                token.Visible = false;
                var square = Squares[(int)token.Token.StartingPosition.y, (int)token.Token.StartingPosition.x];
                square.AddToken(token);
                OnBattleGridSquareSelected(square, token);
            }
        }

        /// <summary>
        /// Update the various fields like phase number, action details, etc
        /// </summary>
        void UpdateDescriptionFields()
        {
            roundLabel.Text = $"Round {currentRound + 1} of {totalRounds}";

            if (currentPhase == -1)
            {
                phaseLabel.Text = $"{totalPhases} Phases";
                actionLabel.Text = $"{BattleRecord.ActionsPerRound[currentRound].Count} Actions";
            }
            else
            {
                phaseLabel.Text = $"Phase {currentPhase + 1} of {totalPhases}";
                actionLabel.Text = $"Action {currentAction + 1} of {BattleRecord.ActionsPerRound[currentRound].Count}";

                var action = BattleRecord.ActionsPerRound[currentRound][currentAction];
                actionRaceLabel.Text = action.Token.Owner.RacePluralName;
                actionDesignLabel.Text = action.Token.Token.Design.Name;

                actionMoveLabel.Visible = false;
                actionAttackLabelContainer.Visible = false;

                if (action is BattleRecordTokenMove move)
                {
                    actionMoveLabel.Visible = true;
                    actionMoveLabel.Text = $"Moved from ({move.From.x + 1}, {move.From.y + 1}) to ({move.To.x + 1}, {move.To.y + 1})";
                }
                else if (action is BattleRecordTokenBeamFire beamFire)
                {
                    actionAttackLabelContainer.Visible = true;
                    actionAttackLabel.Text = $"attacks the {beamFire.Target.Owner.RacePluralName}";
                    actionAttackTargetLabel.Text = $"{beamFire.Target.Token.Design.Name} * {beamFire.Target.Token.Quantity - beamFire.TokensDestroyed}";
                    actionAttackLocationLabel.Text = $"at ({beamFire.To.x + 1}, {beamFire.To.y + 1}) doing";
                    if (beamFire.DamageDoneShields > 0 && beamFire.DamageDoneArmor > 0)
                    {
                        actionAttackDamageLabel.Text = $"{beamFire.DamageDoneShields} damage to shields and {beamFire.DamageDoneArmor} to armor.";
                    }
                    else if (beamFire.DamageDoneShields > 0)
                    {
                        actionAttackDamageLabel.Text = $"{beamFire.DamageDoneShields} damage to shields.";
                    }
                    else
                    {
                        actionAttackDamageLabel.Text = $"{beamFire.DamageDoneArmor} damage to armor.";
                    }

                }
                else if (action is BattleRecordTokenTorpedoFire torpedoFire)
                {
                    actionAttackLabelContainer.Visible = true;
                    actionAttackLabel.Text = $"attacks the {torpedoFire.Target.Owner.RacePluralName}";
                    actionAttackTargetLabel.Text = $"{torpedoFire.Target.Token.Design.Name} * {torpedoFire.Target.Token.Quantity - torpedoFire.TokensDestroyed}";
                    actionAttackLocationLabel.Text = $"at ({torpedoFire.To.x + 1}, {torpedoFire.To.y + 1}) doing";
                    if (torpedoFire.DamageDoneShields > 0 && torpedoFire.DamageDoneArmor > 0)
                    {
                        actionAttackDamageLabel.Text = $"{torpedoFire.DamageDoneShields} damage to shields and {torpedoFire.DamageDoneArmor} to armor.";
                    }
                    else if (torpedoFire.DamageDoneShields > 0)
                    {
                        actionAttackDamageLabel.Text = $"{torpedoFire.DamageDoneShields} damage to shields.";
                    }
                    else
                    {
                        actionAttackDamageLabel.Text = $"{torpedoFire.DamageDoneArmor} damage to armor.";
                    }

                }
            }


            if (selectedSquare != null)
            {
                selectionLabel.Text = $"Selection: ({selectedSquare.Coordinates.x}, {selectedSquare.Coordinates.y})";
                if (selectedGridToken != null)
                {
                    var selectedToken = selectedGridToken.Token;
                    selectionRaceLabel.Text = selectedToken.Owner.RacePluralName;

                    var design = selectedToken.Token.Design;
                    selectionDesignLabel.Text = design.Name;

                    if (design.Aggregate.HasWeapons)
                    {
                        // todo, add initiative to aggregate..
                        selectionInitiative.Text = $"Initiative: {design.Aggregate.WeaponSlots[0].HullComponent.Initiative}";
                    }
                    else
                    {
                        selectionInitiative.Text = $"Initiative: 0";
                    }

                    selectionMovement.Text = $"Movement: {selectedToken.Token.Design.Aggregate.Movement}";
                    selectionArmor.Text = $"Armor: {selectedToken.Token.Design.Aggregate.Armor}dp";
                    selectionShields.Text = $"Shields: {selectedGridToken.Shields}dp";

                    if (selectedGridToken.DamageArmor > 0)
                    {
                        selectionDamage.Text = $"Damage: {selectedGridToken.QuantityDamaged}@{selectedGridToken.DamageArmor}";
                    }
                    else
                    {
                        selectionDamage.Text = $"Damage: (none)";
                    }
                }
            }
        }

        void OnResetBoardButtonPressed()
        {
            ResetBoard();
            prevActionButton.Disabled = true;
            UpdateDescriptionFields();
        }

        void OnNextAttackActionButtonPressed()
        {
            while (currentPhase < totalPhases - 1)
            {
                OnNextActionButtonPressed();
                var action = BattleRecord.ActionsPerRound[currentRound][currentAction];
                if (action is BattleRecordTokenBeamFire || action is BattleRecordTokenTorpedoFire)
                {
                    break;
                }
            }
        }

        void OnNextActionButtonPressed()
        {
            prevActionButton.Disabled = false;

            if (currentPhase < totalPhases - 1)
            {
                currentPhase++;

                // see if we need to advance the round
                var round = BattleRecord.ActionsPerRound[currentRound];
                if (currentAction < round.Count - 1)
                {
                    currentAction++;
                }
                else
                {
                    currentAction = 0;
                    currentRound++;
                }

                RunAction(BattleRecord.ActionsPerRound[currentRound][currentAction]);
            }

            UpdateDescriptionFields();

            if (currentPhase >= totalPhases - 1)
            {
                nextActionButton.Disabled = true;
            }
        }

        void OnPrevActionButtonPressed()
        {
            nextActionButton.Disabled = false;

            currentPhase--;

            var round = BattleRecord.ActionsPerRound[currentRound];
            var action = BattleRecord.ActionsPerRound[currentRound][currentAction];
            if (currentPhase == -1 || currentAction > 0)
            {
                currentAction--;
            }
            else
            {
                currentRound--;
                currentAction = BattleRecord.ActionsPerRound[currentRound].Count - 1;
            }

            if (currentPhase == -1)
            {
                ResetBoard();
                prevActionButton.Disabled = true;
            }
            else
            {
                ReverseAction(action);
            }

            UpdateDescriptionFields();
        }

        void OnDesignDetailsButtonDown()
        {
            if (selectedGridToken != null)
            {
                var design = selectedGridToken.Token.Token.Design;
                hullSummary.Hull = design.Hull;
                hullSummary.ShipDesign = design;

                // position the summary view on the corner
                var size = RectSize;
                var position = RectGlobalPosition;
                position.x += size.x / 2 - hullSummary.RectSize.x / 2;
                position.y += size.y / 2 - hullSummary.RectSize.y / 2;
                hullSummaryPopup.SetGlobalPosition(position);
                hullSummaryPopup.Show();
            }
        }

        void OnDesignDetailsButtonUp()
        {
            hullSummaryPopup.Hide();
        }

        /// <summary>
        /// Run an action
        /// </summary>
        /// <param name="action"></param>
        void RunAction(BattleRecordTokenAction action)
        {
            if (actionToken != null)
            {
                actionToken.FiringBeam = false;
                actionToken.FiringTorpedo = false;
                actionToken.Target = null;
            }
            var gridToken = GridTokensByGuid[action.Token.Guid];
            actionToken = gridToken;
            gridToken.FiringBeam = false;
            gridToken.FiringTorpedo = false;
            gridToken.Target = null;
            log.Debug($"Running action {action}");
            if (action is BattleRecordTokenMove move)
            {
                var source = Squares[(int)move.From.y, (int)move.From.x];
                var dest = Squares[(int)move.To.y, (int)move.To.x];
                source.RemoveToken(gridToken);
                dest.AddToken(gridToken);
                OnBattleGridSquareSelected(dest, gridToken);
                selectedGridToken = gridToken;
                UpdateDescriptionFields();
            }
            else if (action is BattleRecordTokenBeamFire beamFire)
            {
                var target = GridTokensByGuid[beamFire.Target.Guid];
                gridToken.FiringBeam = true;
                gridToken.Target = target;

                target.DamageArmor += beamFire.DamageDoneArmor;
                target.DamageShields += beamFire.DamageDoneShields;
                target.Quantity -= beamFire.TokensDestroyed;
                if (target.Quantity == 0)
                {
                    Squares[(int)beamFire.To.y, (int)beamFire.To.x].RemoveToken(target);
                }
            }
            else if (action is BattleRecordTokenTorpedoFire torpedoFire)
            {
                var target = GridTokensByGuid[torpedoFire.Target.Guid];
                gridToken.FiringBeam = true;
                gridToken.Target = target;

                target.DamageArmor += torpedoFire.DamageDoneArmor;
                target.DamageShields += torpedoFire.DamageDoneShields;
                target.Quantity -= torpedoFire.TokensDestroyed;
                if (target.Quantity == 0)
                {
                    Squares[(int)torpedoFire.To.y, (int)torpedoFire.To.x].RemoveToken(target);
                }
            }
        }

        void ReverseAction(BattleRecordTokenAction action)
        {
            if (actionToken != null)
            {
                actionToken.FiringBeam = false;
                actionToken.FiringTorpedo = false;
                actionToken.Target = null;
            }
            var gridToken = GridTokensByGuid[action.Token.Guid];
            actionToken = gridToken;

            gridToken.FiringBeam = false;
            gridToken.FiringTorpedo = false;
            gridToken.Target = null;
            log.Debug($"Reversing action {action}");
            if (action is BattleRecordTokenMove move)
            {
                var source = Squares[(int)move.From.y, (int)move.From.x];
                var dest = Squares[(int)move.To.y, (int)move.To.x];
                dest.RemoveToken(gridToken);
                source.AddToken(gridToken);
                OnBattleGridSquareSelected(source, gridToken);
                selectedGridToken = gridToken;
                UpdateDescriptionFields();
            }
            else if (action is BattleRecordTokenBeamFire beamFire)
            {
                var target = GridTokensByGuid[beamFire.Target.Guid];
                target.DamageArmor -= beamFire.DamageDoneArmor;
                target.DamageShields -= beamFire.DamageDoneShields;
                if (target.Quantity == 0 && beamFire.TokensDestroyed > 0)
                {
                    Squares[(int)beamFire.To.y, (int)beamFire.To.x].AddToken(target);
                }
                target.Quantity += beamFire.TokensDestroyed;
            }
            else if (action is BattleRecordTokenTorpedoFire torpedoFire)
            {
                var target = GridTokensByGuid[torpedoFire.Target.Guid];
                target.DamageArmor -= torpedoFire.DamageDoneArmor;
                target.DamageShields -= torpedoFire.DamageDoneShields;
                if (target.Quantity == 0 && torpedoFire.TokensDestroyed > 0)
                {
                    Squares[(int)torpedoFire.To.y, (int)torpedoFire.To.x].AddToken(target);
                }
                target.Quantity += torpedoFire.TokensDestroyed;
            }
        }

        #region Test Battle Setup

        BattleRecord GenerateTestBattle()
        {
            PlayersManager.Instance.SetupPlayers();
            var player1 = PlayersManager.Me;
            // level up our players so they will have designs
            player1.TechLevels = new TechLevel(10, 10, 10, 10, 10, 10);

            // create a second weaker player
            var player2 = PlayersManager.Instance.Players[1] as Player;
            player2.TechLevels = new TechLevel(6, 6, 6, 6, 6, 6);

            var battleEngine = new BattleEngine(RulesManager.Rules);
            var battle = battleEngine.BuildBattle(Battles.GetFleetsForDesignsBattle(
                player1,
                player2,
                new HashSet<string>() { "Destroyer", "Space Station" },
                new HashSet<string>() { "Destroyer", "Scout", "Fuel Transport" }
            ));
            battleEngine.RunBattle(battle);

            return battle.PlayerRecords[PlayersManager.Me];
        }

        #endregion
    }
}