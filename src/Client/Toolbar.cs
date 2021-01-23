using CraigStars.Singletons;
using Godot;
using System;

namespace CraigStars
{
    public class Toolbar : MarginContainer
    {
        ToolButton normalViewToolButton;
        ToolButton percentViewToolButton;

        Button techBrowserButton;
        Button shipDesignerButton;
        Button researchButton;
        Button battlePlansButton;
        Button submitTurnButton;

        public override void _Ready()
        {
            normalViewToolButton = FindNode("NormalViewToolButton") as ToolButton;
            percentViewToolButton = FindNode("PercentViewToolButton") as ToolButton;

            techBrowserButton = FindNode("TechBrowserButton") as Button;
            shipDesignerButton = FindNode("ShipDesignerButton") as Button;
            researchButton = FindNode("ResearchButton") as Button;
            battlePlansButton = FindNode("BattlePlansButton") as Button;
            submitTurnButton = FindNode("SubmitTurnButton") as Button;

            normalViewToolButton.Connect("pressed", this, nameof(OnNormalViewToolButtonPressed));
            percentViewToolButton.Connect("pressed", this, nameof(OnPercentViewToolButtonPressed));

            techBrowserButton.Connect("pressed", this, nameof(OnTechBrowserButtonPressed));
            shipDesignerButton.Connect("pressed", this, nameof(OnShipDesignerButtonPressed));
            researchButton.Connect("pressed", this, nameof(OnResearchButtonPressed));
            battlePlansButton.Connect("pressed", this, nameof(OnBattlePlansButtonPressed));
            submitTurnButton.Connect("pressed", this, nameof(OnSubmitTurnButtonPressed));
        }

        void OnNormalViewToolButtonPressed()
        {
            PlayersManager.Instance.Me.PlanetViewState = PlanetViewState.Normal;
            Signals.PublishPlanetViewStateUpdatedEvent();
        }

        void OnPercentViewToolButtonPressed()
        {
            PlayersManager.Instance.Me.PlanetViewState = PlanetViewState.Percent;
            Signals.PublishPlanetViewStateUpdatedEvent();
        }


        void OnTechBrowserButtonPressed()
        {
            Signals.PublishTechBrowserDialogRequestedEvent();
        }

        void OnShipDesignerButtonPressed()
        {
            Signals.PublishShipDesignerDialogRequestedEvent();
        }

        void OnResearchButtonPressed()
        {
            Signals.PublishResearchDialogRequestedEvent();
        }

        void OnBattlePlansButtonPressed()
        {

        }

        void OnSubmitTurnButtonPressed()
        {
            Signals.PublishSubmitTurnEvent(PlayersManager.Instance.Me);
        }

    }
}