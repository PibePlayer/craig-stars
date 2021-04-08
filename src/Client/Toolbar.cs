using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class Toolbar : MarginContainer
    {
        ToolButton normalViewToolButton;
        ToolButton percentViewToolButton;

        Button techsBrowserButton;
        Button raceDesignerButton;
        Button shipDesignerButton;
        Button researchButton;
        Button battlePlansButton;
        Button reportsButton;
        Button submitTurnButton;

        public override void _Ready()
        {
            normalViewToolButton = (ToolButton)FindNode("NormalViewToolButton");
            percentViewToolButton = (ToolButton)FindNode("PercentViewToolButton");

            techsBrowserButton = (Button)FindNode("TechsBrowserButton");
            raceDesignerButton = (Button)FindNode("RaceDesignerButton");
            shipDesignerButton = (Button)FindNode("ShipDesignerButton");
            researchButton = (Button)FindNode("ResearchButton");
            battlePlansButton = (Button)FindNode("BattlePlansButton");
            reportsButton = (Button)FindNode("ReportsButton");
            submitTurnButton = (Button)FindNode("SubmitTurnButton");

            normalViewToolButton.Connect("pressed", this, nameof(OnNormalViewToolButtonPressed));
            percentViewToolButton.Connect("pressed", this, nameof(OnPercentViewToolButtonPressed));

            techsBrowserButton.Connect("pressed", this, nameof(OnTechBrowserButtonPressed));
            raceDesignerButton.Connect("pressed", this, nameof(OnRaceDesignerButtonPressed));
            shipDesignerButton.Connect("pressed", this, nameof(OnShipDesignerButtonPressed));
            researchButton.Connect("pressed", this, nameof(OnResearchButtonPressed));
            battlePlansButton.Connect("pressed", this, nameof(OnBattlePlansButtonPressed));
            reportsButton.Connect("pressed", this, nameof(OnReportsButtonPressed));
            submitTurnButton.Connect("pressed", this, nameof(OnSubmitTurnButtonPressed));

            Signals.TurnGeneratingEvent += OnTurnGenerating;
            Signals.UnsubmitTurnRequestedEvent += OnUnsubmitTurnButtonPressed;
            Signals.TurnPassedEvent += OnTurnPassed;
        }

        public override void _ExitTree()
        {
            Signals.TurnGeneratingEvent -= OnTurnGenerating;
            Signals.TurnPassedEvent -= OnTurnPassed;
        }

        void EnableSubmitButton()
        {
            submitTurnButton.Disabled = false;
        }

        void OnTurnPassed(PublicGameInfo gameInfo)
        {
            CallDeferred(nameof(EnableSubmitButton));
        }

        void OnTurnGenerating()
        {
            submitTurnButton.Disabled = true;
        }

        void OnUnsubmitTurnButtonPressed(Player player)
        {
            if (player == PlayersManager.Me)
            {
                CallDeferred(nameof(EnableSubmitButton));
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

        void OnTechBrowserButtonPressed()
        {
            Signals.PublishTechBrowserDialogRequestedEvent();
        }

        void OnRaceDesignerButtonPressed()
        {
            Signals.PublishRaceDesignerDialogRequestedEvent();
        }

        void OnShipDesignerButtonPressed()
        {
            Signals.PublishShipDesignerDialogRequestedEvent();
        }

        void OnResearchButtonPressed()
        {
            Signals.PublishResearchDialogRequestedEvent();
        }

        void OnReportsButtonPressed()
        {
            Signals.PublishReportsDialogRequestedEvent();
        }

        void OnBattlePlansButtonPressed()
        {
            Signals.PublishBattlePlansDialogRequestedEvent();
        }

        void OnSubmitTurnButtonPressed()
        {
            Signals.PublishSubmitTurnRequestedEvent(PlayersManager.Me);
        }

        public override void _Input(InputEvent @event)
        {
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
        }

    }
}