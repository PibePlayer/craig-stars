using System;
using CraigStars.Singletons;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Client
{
    public class Toolbar : MarginContainer
    {
        protected Player Me { get => PlayersManager.Me; }

        ToolButton normalViewToolButton;
        ToolButton surfaceMineralsViewToolButton;
        ToolButton mineralConcentrationViewToolButton;
        ToolButton percentViewToolButton;
        ToolButton populationViewToolButton;
        ToolButton planetNamesToolButton;
        ToolButton fleetTokenCountsToolButton;
        ToolButton scannerToolButton;
        ToolButton mineFieldsToolButton;
        ToolButton idleFleetsToolButton;
        SpinBox scannerSpinBox;

        ToolButton networkStatusToolButton;

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
            PlayerRelations,
            TransportPlans,
            ProductionPlans,
            Race,
            TechBrowser,
            Players,
        }

        public override void _Ready()
        {
            commandsMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/CommandsMenuButton").GetPopup();
            plansMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/PlansMenuButton").GetPopup();
            infoMenu = GetNode<MenuButton>("Panel/HBoxContainerRight/InfoMenuButton").GetPopup();
            normalViewToolButton = (ToolButton)FindNode("NormalViewToolButton");
            surfaceMineralsViewToolButton = (ToolButton)FindNode("SurfaceMineralsViewToolButton");
            mineralConcentrationViewToolButton = (ToolButton)FindNode("MineralConcentrationViewToolButton");
            percentViewToolButton = (ToolButton)FindNode("PercentViewToolButton");
            populationViewToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/PopulationViewToolButton");
            planetNamesToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/PlanetNamesToolButton");
            fleetTokenCountsToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/FleetTokenCountsToolButton");
            scannerToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/ScannerToolButton");
            mineFieldsToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/MineFieldsToolButton");
            idleFleetsToolButton = GetNode<ToolButton>("Panel/HBoxContainerLeft/IdleFleetsToolButton");
            scannerSpinBox = GetNode<SpinBox>("Panel/HBoxContainerLeft/ScannerSpinBox");
            networkStatusToolButton = GetNode<ToolButton>("Panel/HBoxContainerRight/NetworkStatusToolButton");

            reportsButton = (Button)FindNode("ReportsButton");
            submitTurnButton = (Button)FindNode("SubmitTurnButton");

            commandsMenu.AddItem("Ship Designer", (int)MenuItem.ShipDesigner);
            commandsMenu.AddItem("Research", (int)MenuItem.Research);
            commandsMenu.AddItem("Player Relations", (int)MenuItem.PlayerRelations);
            plansMenu.AddItem("Battle Plans", (int)MenuItem.BattlePlans);
            plansMenu.AddItem("Transport Plans", (int)MenuItem.TransportPlans);
            plansMenu.AddItem("Production Plans", (int)MenuItem.ProductionPlans);
            infoMenu.AddItem("View Race", (int)MenuItem.Race);
            infoMenu.AddItem("Technology Browser", (int)MenuItem.TechBrowser);
            infoMenu.AddItem("Players", (int)MenuItem.Players);

            normalViewToolButton.Connect("pressed", this, nameof(OnPlanetViewStateToolButtonPressed), new Godot.Collections.Array() { PlanetViewState.Normal });
            surfaceMineralsViewToolButton.Connect("pressed", this, nameof(OnPlanetViewStateToolButtonPressed), new Godot.Collections.Array() { PlanetViewState.SurfaceMinerals });
            mineralConcentrationViewToolButton.Connect("pressed", this, nameof(OnPlanetViewStateToolButtonPressed), new Godot.Collections.Array() { PlanetViewState.MineralConcentration });
            percentViewToolButton.Connect("pressed", this, nameof(OnPlanetViewStateToolButtonPressed), new Godot.Collections.Array() { PlanetViewState.Percent });
            populationViewToolButton.Connect("pressed", this, nameof(OnPlanetViewStateToolButtonPressed), new Godot.Collections.Array() { PlanetViewState.Population });
            planetNamesToolButton.Connect("pressed", this, nameof(OnPlanetNamesToolButtonPressed));
            fleetTokenCountsToolButton.Connect("pressed", this, nameof(OnFleetTokenCountsToolButtonPressed));
            scannerToolButton.Connect("toggled", this, nameof(OnScannerToolButtonToggled));
            mineFieldsToolButton.Connect("toggled", this, nameof(OnMineFieldsToolButtonToggled));
            idleFleetsToolButton.Connect("toggled", this, nameof(OnIdleFleetsToolButtonToggled));
            scannerSpinBox.Connect("value_changed", this, nameof(OnScannerSpinBoxValueChanged));

            reportsButton.Connect("pressed", this, nameof(OnReportsButtonPressed));
            submitTurnButton.Connect("pressed", this, nameof(OnSubmitTurnButtonPressed));

            commandsMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            plansMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));
            infoMenu.Connect("id_pressed", this, nameof(OnMenuItemIdPressed));

            EventManager.GameViewResetEvent += OnGameViewReset;
            NetworkClient.Instance.ServerDisconnectedEvent += OnServerDisconnected;
            NetworkClient.Instance.ServerConnectedEvent += OnServerConnected;
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationPredelete)
            {
                EventManager.GameViewResetEvent -= OnGameViewReset;
                NetworkClient.Instance.ServerDisconnectedEvent -= OnServerDisconnected;
                NetworkClient.Instance.ServerConnectedEvent -= OnServerConnected;
            }
        }

        void OnGameViewReset(PublicGameInfo gameInfo)
        {
            submitTurnButton.Disabled = false;
            normalViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Normal;
            surfaceMineralsViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.SurfaceMinerals;
            mineralConcentrationViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.MineralConcentration;
            percentViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Percent;
            populationViewToolButton.Pressed = Me.UISettings.PlanetViewState == PlanetViewState.Population;
            planetNamesToolButton.Pressed = Me.UISettings.ShowPlanetNames;
            fleetTokenCountsToolButton.Pressed = Me.UISettings.ShowFleetTokenCounts;
            scannerToolButton.Pressed = Me.UISettings.ShowScanners;
            mineFieldsToolButton.Pressed = Me.UISettings.ShowMineFields;
            idleFleetsToolButton.Pressed = Me.UISettings.ShowIdleFleetsOnly;
            scannerSpinBox.Value = Me.UISettings.ScannerPercent;

            // TODO: make this colorblind friendly
            if (this.IsMultiplayer())
            {
                networkStatusToolButton.Visible = true;
                submitTurnButton.Disabled = !NetworkClient.Instance.Connected;
                networkStatusToolButton.HintTooltip = NetworkClient.Instance.Connected ? "Server connected" : "Server disconnected";
                networkStatusToolButton.Modulate = NetworkClient.Instance.Connected ? Colors.Green : Colors.Red;
            }
            else
            {
                networkStatusToolButton.Visible = false;
                submitTurnButton.Disabled = false;
            }
        }

        void OnServerDisconnected()
        {
            networkStatusToolButton.Modulate = Colors.Red;
            networkStatusToolButton.HintTooltip = "Server disconnected";
            submitTurnButton.Disabled = true;
        }

        void OnServerConnected()
        {
            networkStatusToolButton.Modulate = Colors.Green;
            networkStatusToolButton.HintTooltip = "Server connected";
            submitTurnButton.Disabled = false;
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
                case MenuItem.PlayerRelations:
                    EventManager.PublishPlayerRelationsDialogRequestedEvent();
                    break;
                case MenuItem.TransportPlans:
                    EventManager.PublishTransportPlansDialogRequestedEvent();
                    break;
                case MenuItem.ProductionPlans:
                    EventManager.PublishProductionPlansDialogRequestedEvent();
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

        void OnPlanetViewStateToolButtonPressed(PlanetViewState state)
        {
            Me.UISettings.PlanetViewState = state;
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

        void OnFleetTokenCountsToolButtonPressed()
        {
            Me.UISettings.ShowFleetTokenCounts = fleetTokenCountsToolButton.Pressed;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishFleetViewStateUpdatedEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnScannerToolButtonToggled(bool toggled)
        {
            Me.UISettings.ShowScanners = toggled;
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


        void OnMineFieldsToolButtonToggled(bool toggled)
        {
            Me.UISettings.ShowMineFields = toggled;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishViewStateUpdatedEvent();
        }

        void OnIdleFleetsToolButtonToggled(bool toggled)
        {
            Me.UISettings.ShowIdleFleetsOnly = toggled;
            Me.Dirty = true;
            EventManager.PublishPlayerDirtyEvent();
            EventManager.PublishFleetViewStateUpdatedEvent();
            EventManager.PublishPlanetViewStateUpdatedEvent();
        }

        void OnReportsButtonPressed()
        {
            EventManager.PublishReportsDialogRequestedEvent();
        }

        void OnSubmitTurnButtonPressed()
        {
            submitTurnButton.Disabled = true;
            PlayersManager.Me.SubmittedTurn = true;
            EventManager.PublishSubmitTurnRequestedEvent(PlayersManager.Me.GetOrders());
        }

        public override void _Input(InputEvent @event)
        {
            if (!IsInsideTree())
            {
                return;
            }
            if (!submitTurnButton.Disabled && @event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                OnSubmitTurnButtonPressed();
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                EventManager.PublishTechBrowserDialogRequestedEvent();
            }
            if (@event.IsActionPressed("research"))
            {
                EventManager.PublishResearchDialogRequestedEvent();
            }
            if (@event.IsActionPressed("player_relations"))
            {
                EventManager.PublishPlayerRelationsDialogRequestedEvent();
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