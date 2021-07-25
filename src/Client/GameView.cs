using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using CraigStars.Client;

namespace CraigStars
{
    public class GameView : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameView));

        /// <summary>
        /// Information about the game
        /// </summary>
        public PublicGameInfo GameInfo { get; set; }


        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;

        ProductionQueueDialog productionQueueDialog;
        CargoTransferDialog cargoTransferDialog;
        ResearchDialog researchDialog;
        BattlePlansDialog battlePlansDialog;
        TransportPlansDialog transportPlansDialog;
        ReportsDialog reportsDialog;
        TechBrowserDialog techBrowserDialog;
        RaceDesignerDialog raceDesignerDialog;
        ShipDesignerDialog shipDesignerDialog;
        MergeFleetsDialog mergeFleetsDialog;
        BattleViewerDialog battleViewerDialog;
        ScoreDialog scoreDialog;

        public override void _Ready()
        {
            scanner = FindNode("Scanner") as Scanner;
            productionQueueDialog = GetNode<ProductionQueueDialog>("CanvasLayer/ProductionQueueDialog");
            cargoTransferDialog = GetNode<CargoTransferDialog>("CanvasLayer/CargoTransferDialog");
            researchDialog = GetNode<ResearchDialog>("CanvasLayer/ResearchDialog");
            battlePlansDialog = GetNode<BattlePlansDialog>("CanvasLayer/BattlePlansDialog");
            transportPlansDialog = GetNode<TransportPlansDialog>("CanvasLayer/TransportPlansDialog");
            reportsDialog = GetNode<ReportsDialog>("CanvasLayer/ReportsDialog");
            techBrowserDialog = GetNode<TechBrowserDialog>("CanvasLayer/TechBrowserDialog");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("CanvasLayer/RaceDesignerDialog");
            shipDesignerDialog = GetNode<ShipDesignerDialog>("CanvasLayer/ShipDesignerDialog");
            mergeFleetsDialog = GetNode<MergeFleetsDialog>("CanvasLayer/MergeFleetsDialog");
            battleViewerDialog = GetNode<BattleViewerDialog>("CanvasLayer/BattleViewerDialog");
            scoreDialog = GetNode<ScoreDialog>("CanvasLayer/ScoreDialog");

            Signals.ChangeProductionQueuePressedEvent += OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent += OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent += OnResearchDialogRequested;
            Signals.BattlePlansDialogRequestedEvent += OnBattlePlansDialogRequested;
            Signals.TransportPlansDialogRequestedEvent += OnTransportPlansDialogRequested;
            Signals.ReportsDialogRequestedEvent += OnReportsDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent += OnShipDesignerDialogRequested;
            Signals.ScoreDialogRequestedEvent += OnScoreDialogRequested;
            Signals.TechBrowserDialogRequestedEvent += OnTechBrowserDialogRequested;
            Signals.RaceDesignerDialogRequestedEvent += OnRaceDesignerDialogRequested;
            Signals.MergeFleetsDialogRequestedEvent += OnMergeFleetsDialogRequested;
            Signals.BattleViewerDialogRequestedEvent += OnBattleViewerDialogRequested;

            PlayersManager.Me.RunTurnProcessors(TurnProcessorManager.Instance);
            // add the universe to the viewport
            scanner.Init();
        }

        public override void _ExitTree()
        {
            Signals.ChangeProductionQueuePressedEvent -= OnChangeProductionQueue;
            Signals.CargoTransferRequestedEvent -= OnCargoTransferRequested;
            Signals.ResearchDialogRequestedEvent -= OnResearchDialogRequested;
            Signals.BattlePlansDialogRequestedEvent -= OnBattlePlansDialogRequested;
            Signals.TransportPlansDialogRequestedEvent -= OnTransportPlansDialogRequested;
            Signals.ReportsDialogRequestedEvent -= OnReportsDialogRequested;
            Signals.ShipDesignerDialogRequestedEvent -= OnShipDesignerDialogRequested;
            Signals.ScoreDialogRequestedEvent -= OnScoreDialogRequested;
            Signals.TechBrowserDialogRequestedEvent -= OnTechBrowserDialogRequested;
            Signals.RaceDesignerDialogRequestedEvent -= OnRaceDesignerDialogRequested;
            Signals.MergeFleetsDialogRequestedEvent -= OnMergeFleetsDialogRequested;
            Signals.BattleViewerDialogRequestedEvent -= OnBattleViewerDialogRequested;

        }

        void OnTechBrowserDialogRequested()
        {
            techBrowserDialog.PopupCentered();
        }

        void OnRaceDesignerDialogRequested()
        {
            raceDesignerDialog.Race = PlayersManager.Me.Race;
            raceDesignerDialog.PopupCentered();
        }

        void OnReportsDialogRequested()
        {
            reportsDialog.PopupCentered();
        }

        void OnShipDesignerDialogRequested()
        {
            shipDesignerDialog.PopupCentered();
        }

        void OnScoreDialogRequested()
        {
            scoreDialog.PopupCentered();
        }

        void OnResearchDialogRequested()
        {
            researchDialog.PopupCentered();
        }

        void OnBattlePlansDialogRequested()
        {
            battlePlansDialog.PopupCentered();
        }

        void OnTransportPlansDialogRequested()
        {
            transportPlansDialog.PopupCentered();
        }

        void OnCargoTransferRequested(ICargoHolder source, ICargoHolder dest)
        {
            cargoTransferDialog.Source = source;
            cargoTransferDialog.Dest = dest;
            cargoTransferDialog.PopupCentered();
        }

        void OnChangeProductionQueue(PlanetSprite planetSprite)
        {
            productionQueueDialog.Planet = planetSprite.Planet;
            productionQueueDialog.PopupCentered();
        }

        void OnMergeFleetsDialogRequested(FleetSprite sourceFleet)
        {
            mergeFleetsDialog.SourceFleet = sourceFleet;
            mergeFleetsDialog.PopupCentered();
        }

        void OnBattleViewerDialogRequested(BattleRecord battle)
        {
            battleViewerDialog.BattleRecord = battle;
            battleViewerDialog.PopupCentered();
        }

    }
}
