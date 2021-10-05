using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using CraigStars.Client;
using System.Threading.Tasks;
using System;

namespace CraigStars
{
    public class GameView : Control
    {
        static CSLog log = LogProvider.GetLogger(typeof(GameView));

        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;
        Control gui;

        ProductionQueueDialog productionQueueDialog;
        CargoTransferDialog cargoTransferDialog;
        ResearchDialog researchDialog;
        BattlePlansDialog battlePlansDialog;
        TransportPlansDialog transportPlansDialog;
        ProductionPlansDialog productionPlansDialog;
        ReportsDialog reportsDialog;
        TechBrowserDialog techBrowserDialog;
        RaceDesignerDialog raceDesignerDialog;
        ShipDesignerDialog shipDesignerDialog;
        MergeFleetsDialog mergeFleetsDialog;
        BattleViewerDialog battleViewerDialog;
        PlayerStatusDialog playerStatusDialog;

        public override void _Ready()
        {
            gui = FindNode("GUI") as Control;
            scanner = FindNode("Scanner") as Scanner;
            productionQueueDialog = GetNode<ProductionQueueDialog>("CanvasLayer/ProductionQueueDialog");
            cargoTransferDialog = GetNode<CargoTransferDialog>("CanvasLayer/CargoTransferDialog");
            researchDialog = GetNode<ResearchDialog>("CanvasLayer/ResearchDialog");
            battlePlansDialog = GetNode<BattlePlansDialog>("CanvasLayer/BattlePlansDialog");
            transportPlansDialog = GetNode<TransportPlansDialog>("CanvasLayer/TransportPlansDialog");
            productionPlansDialog = GetNode<ProductionPlansDialog>("CanvasLayer/ProductionPlansDialog");
            reportsDialog = GetNode<ReportsDialog>("CanvasLayer/ReportsDialog");
            techBrowserDialog = GetNode<TechBrowserDialog>("CanvasLayer/TechBrowserDialog");
            raceDesignerDialog = GetNode<RaceDesignerDialog>("CanvasLayer/RaceDesignerDialog");
            shipDesignerDialog = GetNode<ShipDesignerDialog>("CanvasLayer/ShipDesignerDialog");
            mergeFleetsDialog = GetNode<MergeFleetsDialog>("CanvasLayer/MergeFleetsDialog");
            battleViewerDialog = GetNode<BattleViewerDialog>("CanvasLayer/BattleViewerDialog");
            playerStatusDialog = GetNode<PlayerStatusDialog>("CanvasLayer/PlayerStatusDialog");

            Client.EventManager.ProductionQueueDialogRequestedEvent += OnProductionQueueDialogRequested;
            Client.EventManager.CargoTransferDialogRequestedEvent += OnCargoTransferRequested;
            Client.EventManager.ResearchDialogRequestedEvent += OnResearchDialogRequested;
            Client.EventManager.BattlePlansDialogRequestedEvent += OnBattlePlansDialogRequested;
            Client.EventManager.TransportPlansDialogRequestedEvent += OnTransportPlansDialogRequested;
            Client.EventManager.ProductionPlansDialogRequestedEvent += OnProductionPlansDialogRequested;
            Client.EventManager.ReportsDialogRequestedEvent += OnReportsDialogRequested;
            Client.EventManager.ShipDesignerDialogRequestedEvent += OnShipDesignerDialogRequested;
            Client.EventManager.PlayerStatusDialogRequestedEvent += OnPlayerStatusDialogRequested;
            Client.EventManager.TechBrowserDialogRequestedEvent += OnTechBrowserDialogRequested;
            Client.EventManager.RaceDesignerDialogRequestedEvent += OnRaceDesignerDialogRequested;
            Client.EventManager.MergeFleetsDialogRequestedEvent += OnMergeFleetsDialogRequested;
            Client.EventManager.BattleViewerDialogRequestedEvent += OnBattleViewerDialogRequested;

            Connect("visibility_changed", this, nameof(OnVisibilityChangedAsync));
        }

        Task scannerInitTask;

        async void OnVisibilityChangedAsync()
        {
            if (IsVisibleInTree())
            {
                log.Debug("Resetting scanner");
                PlayersManager.Me.RunTurnProcessors(PlayersManager.GameInfo, TurnProcessorManager.Instance);
                // add the universe to the viewport
                gui.Visible = true;
                RemoveChild(scanner);
                scannerInitTask = Task.Run(() => scanner.Init());
                await scannerInitTask;
                AddChild(scanner);
                CallDeferred(nameof(AfterScannerInit));

                DialogManager.DialogRefCount = 0;
            }
            else
            {
                gui.Visible = false;
            }
        }

        /// <summary>
        /// After the scanner is initialized and added
        /// to the view, update stuff and focus the homeworld
        /// </summary>
        /// <returns></returns>
        async void AfterScannerInit()
        {
            await scannerInitTask;
            scanner.UpdateSprites();
            scanner.FocusHomeworld();
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                Client.EventManager.ProductionQueueDialogRequestedEvent -= OnProductionQueueDialogRequested;
                Client.EventManager.CargoTransferDialogRequestedEvent -= OnCargoTransferRequested;
                Client.EventManager.ResearchDialogRequestedEvent -= OnResearchDialogRequested;
                Client.EventManager.BattlePlansDialogRequestedEvent -= OnBattlePlansDialogRequested;
                Client.EventManager.TransportPlansDialogRequestedEvent -= OnTransportPlansDialogRequested;
                Client.EventManager.ReportsDialogRequestedEvent -= OnReportsDialogRequested;
                Client.EventManager.ShipDesignerDialogRequestedEvent -= OnShipDesignerDialogRequested;
                Client.EventManager.PlayerStatusDialogRequestedEvent -= OnPlayerStatusDialogRequested;
                Client.EventManager.TechBrowserDialogRequestedEvent -= OnTechBrowserDialogRequested;
                Client.EventManager.RaceDesignerDialogRequestedEvent -= OnRaceDesignerDialogRequested;
                Client.EventManager.MergeFleetsDialogRequestedEvent -= OnMergeFleetsDialogRequested;
                Client.EventManager.BattleViewerDialogRequestedEvent -= OnBattleViewerDialogRequested;
            }
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

        void OnPlayerStatusDialogRequested()
        {
            // scoreDialog.PopupCentered();
            playerStatusDialog.PopupCentered();
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

        void OnProductionPlansDialogRequested()
        {
            productionPlansDialog.PopupCentered();
        }

        void OnCargoTransferRequested(ICargoHolder source, ICargoHolder dest)
        {
            cargoTransferDialog.Source = source;
            cargoTransferDialog.Dest = dest;
            cargoTransferDialog.PopupCentered();
        }

        void OnProductionQueueDialogRequested(PlanetSprite planetSprite)
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
