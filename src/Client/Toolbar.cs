using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars.Client
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
            Players,
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
            infoMenu.AddItem("Players", (int)MenuItem.Players);

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

            EventManager.GameViewResetEvent += OnGameViewReset;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameViewResetEvent -= OnGameViewReset;
            }
        }

        void OnGameViewReset(PublicGameInfo gameInfo)
        {
            submitTurnButton.Disabled = false;
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
                    EventManager.PublishShipDesignerDialogRequestedEvent();
                    break;
                case MenuItem.BattlePlans:
                    EventManager.PublishBattlePlansDialogRequestedEvent();
                    break;
                case MenuItem.Research:
                    EventManager.PublishResearchDialogRequestedEvent();
                    break;
                case MenuItem.TransportPlans:
                    EventManager.PublishTransportPlansDialogRequestedEvent();
                    break;
                case MenuItem.ProductionPlans:
                    break;
                case MenuItem.Race:
                    EventManager.PublishRaceDesignerDialogRequestedEvent();
                    break;
                case MenuItem.TechBrowser:
                    EventManager.PublishTechBrowserDialogRequestedEvent();
                    break;
                case MenuItem.Players:
                    EventManager.PublishPlayerStatusDialogRequestedEvent();
                    break;
            }
        }

        void OnNormalViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Normal;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPercentViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Percent;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPopulationViewToolButtonPressed()
        {
            Me.UISettings.PlanetViewState = PlanetViewState.Population;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPlanetNamesToolButtonPressed()
        {
            Me.UISettings.ShowPlanetNames = planetNamesToolButton.Pressed;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnScannerToolButtonPressed()
        {
            Me.UISettings.ShowScanners = scannerToolButton.Pressed;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnScannerSpinBoxValueChanged(float value)
        {
            Me.UISettings.ScannerPercent = (int)scannerSpinBox.Value;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishScannerScaleUpdatedEvent();
        }

        void OnReportsButtonPressed()
        {
            EventManager.PublishReportsDialogRequestedEvent();
        }

        void OnSubmitTurnButtonPressed()
        {
            EventManager.PublishSubmitTurnRequestedEvent(PlayersManager.Me);
        }

        public override void _Input(InputEvent @event)
        {
            if (turnGenerating || !IsInsideTree())
            {
                return;
            }
            if (!submitTurnButton.Disabled && @event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                submitTurnButton.Disabled = true;
                EventManager.PublishSubmitTurnRequestedEvent(PlayersManager.Me);
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                EventManager.PublishTechBrowserDialogRequestedEvent();
            }
            if (@event.IsActionPressed("research"))
            {
                EventManager.PublishResearchDialogRequestedEvent();
            }
            if (@event.IsActionPressed("battle_plans"))
            {
                EventManager.PublishBattlePlansDialogRequestedEvent();
            }
            if (@event.IsActionPressed("ship_designer"))
            {
                EventManager.PublishShipDesignerDialogRequestedEvent();
            }
            if (@event.IsActionPressed("player_status"))
            {
                EventManager.PublishPlayerStatusDialogRequestedEvent();
            }
        }

    }
}