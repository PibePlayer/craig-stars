using Godot;
using System.Collections.Generic;
using System.Linq;

using CraigStars.Singletons;
using System;
using log4net;
using log4net.Core;
using log4net.Appender;

namespace CraigStars
{
    public class Game : Node
    {
        /// <summary>
        /// The game node creates a server in single player or host mode
        /// </summary>
        public Server Server { get; set; } = new Server();

        /// <summary>
        /// This is the main view into the universe
        /// </summary>
        Scanner scanner;

        ProductionQueueDialog productionQueueDialog;

        public override void _Ready()
        {
            Server.Init(PlayersManager.Instance.Players, SettingsManager.Settings, TechStore.Instance);

            // add the universe to the viewport
            scanner = FindNode("Scanner") as Scanner;
            scanner.InitMapObjects();

            productionQueueDialog = GetNode<ProductionQueueDialog>("CanvasLayer/ProductionQueueDialog");

            Signals.ChangeProductionQueuePressedEvent += OnChangeProductionQueue;
        }

        public override void _ExitTree()
        {
            Server.Shutdown();
            Signals.ChangeProductionQueuePressedEvent -= OnChangeProductionQueue;
        }

        void OnChangeProductionQueue(PlanetSprite planetSprite)
        {
            productionQueueDialog.Planet = planetSprite.Planet;
            productionQueueDialog.PopupCentered();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("submit_turn"))
            {
                // submit our turn
                Signals.PublishSubmitTurnEvent(PlayersManager.Instance.Me);
            }
            if (@event.IsActionPressed("technology_browser"))
            {
                GetTree().ChangeScene("res://src/Client/ShipDesigner/HullSummary.tscn");
            }
        }

    }
}
