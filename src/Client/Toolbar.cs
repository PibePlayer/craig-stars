using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class Toolbar : MarginContainer
    {
        protected Player Me { get => PlayersManager.Me; }

        ToolButton normalViewToolButton;
        ToolButton percentViewToolButton;
        ToolButton populationViewToolButton;
        ToolButton planetNamesToolButton;
        ToolButton scannerToolButton;
        SpinBox scannerSpinBox;

        PopupMenu commandsMenu;
        PopupMenu plansMenu;
        PopupMenu infoMenu;
        Button reportsButton;
        Button submitTurnButton;

        enum MenuItem
        {
            ShipDesigner,
            Research,
            BattlePlans,
            TransportPlans,
            ProductionPlans,
            Race,
            TechBrowser,
            Score,
        }

        bool turnGenerating;

        public override void _Ready()
        {
            commandsMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/CommandsMenuButton").GetPopup();
            plansMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/PlansMenuButton").GetPopup();
            infoMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/InfoMenuButton").GetPopup();
            normalViewToolButton = (ToolButton)FindNode("NormalViewToolButton");
            percentViewToolButton = (ToolButton)FindNode("PercentViewToolButton");
            populationViewToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/PopulationViewToolButton");
            planetNamesToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/PlanetNamesToolButton");
            scannerToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/ScannerToolButton");
            scannerSpinBox = GetNode<SpinBox>("Panel/HBoxContainerLeft/ScannerSpinBox");

            reportsButton = (Button)FindNode("ReportsButton");
            submitTurnButton = (Button)FindNode("SubmitTurnButton");

            commandsMenu.AddItem("Ship Designer", (int)MenuItem.ShipDesigner);
            commandsMenu.AddItem("Research", (int)MenuItem.Research);
            plansMenu.AddItem("Battle Plans", (int)MenuItem.BattlePlans);
            plansMenu.AddItem("Transport Plans", (int)MenuItem.TransportPlans);
            plansMenu.AddItem("Production Plans", (int)MenuItem.ProductionPlans);
            infoMenu.AddItem("View Race", (int)MenuItem.Race);
            infoMenu.AddItem("Technology Browser", (int)MenuItem.TechBrowser);
            infoMenu.AddItem("Score", (int)MenuItem.Score);

            normalViewToolButton.Connect("pressed", this, nameof(OnNormalViewToolButtonPressed));
            percentViewToolButton.Connect("pressed", this, nameof(OnPercentViewToolButtonPressed));
            populationViewToolButton.Connect("pressed", this, nameof(OnPopulationViewToolButtonPressed));
            planetNamesToolButton.Connect("pressed", this, nameof(OnPlanetNamesToolButtonPressed));
            scannerToolButton.Connect("pressed", this, nameof(OnScannerToolButtonPressed));
            scannerSpinBox.Connect("value_changed", this, nameof(OnScannerSpinBoxValueChanged));

            reportsButton.Connect("pressed", this, nameof(OnReportsButtonPressed));
            submitTurnButton.Connect("pressed", this, nameof(OnSubmitTurnButtonPressed));

            commandsMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            plansMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            infoMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));

            Signals.GameViewResetEvent += OnGameViewReset;
        }

        public override void _ExitTree()
        {
            Signals.GameViewResetEvent += OnGameViewReset;
        }

        void OnGameViewReset(PublicGameInfo gameInfo)
        {
            normalViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Normal;
            percentViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Percent;
            populationViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Population;
            planetNamesToolButton.Pressed = Me.UISettings.ShowPlanetNames;
            scannerToolButton.Pressed = Me.UISettings.ShowScanners;
            scannerSpinBox.Value = Me.UISettings.ScannerPercent;
        }

        void OnMenuItemIdPressed(int id)
        {
            switch ((MenuItem)id)
            {
                case MenuItem.ShipDesigner:
                    Signals.PublishShipDesignerDialogRequestedEvent();
                    break;
                case MenuItem.BattlePlans:
                    Signals.PublishBattlePlansDialogRequestedEvent();
                    break;
                case MenuItem.Research:
                    Signals.PublishResearchDialogRequestedEvent();
                    break;
                case MenuItem.TransportPlans:
                    Signals.PublishTransportPlansDialogRequestedEvent();
                    break;
                case MenuItem.ProductionPlans:
                    break;
                case MenuItem.Race:
                    Signals.PublishRaceDesignerDialogRequestedEvent();
                    break;
                case MenuItem.TechBrowser:
                    Signals.PublishTechBrowserDialogRequestedEvent();
                    break;
                case MenuItem.Score:
                    Signals.PublishScoreDialogRequestedEvent();
                    break;
            }
        }

        void OnNormalViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Normal;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPercentViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Percent;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPopulationViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Population;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPlanetNamesToolButtonPressed()
        {
            Me.UISettings.ShowPlanetNames = planetNamesToolButton.Pressed;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnScannerToolButtonPressed()
        {
            Me.UISettings.ShowScanners = scannerToolButton.Pressed;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnScannerSpinBoxValueChanged(float value)
        {
            Me.UISettings.ScannerPercent = (int)scannerSpinBox.Value;
            Me.Dirty = true;
            Signals.PublishPlayerDirtyEvent();
            Signals.PublishScannerScaleUpdatedEvent();
        }

        void OnReportsButtonPressed()
        {
            Signals.PublishReportsDialogRequestedEvent();
        }

        void OnSubmitTurnButtonPressed()
        {
            Signals.PublishSubmitTurnRequestedEvent(PlayersManager.Me);
        }

        public override void _Input(InputEvent @event)
        {
            if (turnGenerating)
            {
                return;
            }
            if (!submitTurnButton.Disabled && @event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                submitTurnButton.Disabled = true;
                Signals.PublishSubmitTurnRequestedEvent(PlayersManager.Me);
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                Signals.PublishTechBrowserDialogRequestedEvent();
            }
            if (@event.IsActionPressed("research"))
            {
                Signals.PublishResearchDialogRequestedEvent();
            }
            if (@event.IsActionPressed("battle_plans"))
            {
                Signals.PublishBattlePlansDialogRequestedEvent();
            }
            if (@event.IsActionPressed("ship_designer"))
            {
                Signals.PublishShipDesignerDialogRequestedEvent();
            }
            if (@event.IsActionPressed("score"))
            {
                Signals.PublishScoreDialogRequestedEvent();
            }
        }

    }
}