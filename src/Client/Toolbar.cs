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

            reportsButton.Connect("pressed", this, nameof(OnReportsButtonPressed));
            submitTurnButton.Connect("pressed", this, nameof(OnSubmitTurnButtonPressed));

            commandsMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            plansMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            infoMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));

            normalViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Normal;
            percentViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Percent;

            Signals.TurnGeneratingEvent += OnTurnGenerating;
            Signals.UnsubmitTurnRequestedEvent += OnUnsubmitTurnButtonPressed;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.TurnGeneratingEvent -= OnTurnGenerating;
            Signals.TurnPassedEvent -= OnTurnPassed;
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

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            CallDeferred(nameof(DeferredTurnPassed));
        }

        void DeferredTurnPassed()
        {
            turnGenerating = false;
            submitTurnButton.Disabled = false;
        }

        void OnTurnGenerating()
        {
            turnGenerating = true;
            submitTurnButton.Disabled = true;
        }

        void OnUnsubmitTurnButtonPressed(Player player)
        {
            if (player == PlayersManager.Me)
            {
                CallDeferred(nameof(DeferredTurnPassed));
            }
        }

        void OnNormalViewToolButtonPressed()
        {
            PlayersManager.Me.UISettings.PlanetViewState = PlanetViewState.Normal;
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPercentViewToolButtonPressed()
        {
            PlayersManager.Me.UISettings.PlanetViewState = PlanetViewState.Percent;
            Signals.PublishPlanetViewStateUpdatedEvent();
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