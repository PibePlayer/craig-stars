using System;
using System.Collections.Generic;
using CraigStars.Client;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The TurnProcessorManager for the game
    /// </summary>
    public class TurnProcessorManager : Node, ITurnProcessorManager
    {
        static CSLog log = LogProvider.GetLogger(typeof(RulesManager));

        [Inject] private ShipDesignerTurnProcessor shipDesignerTurnProcessor;
        [Inject] private FleetCompositionTurnProcessor fleetCompositionTurnProcessor;
        [Inject] private PlanetProductionTurnProcessor planetProductionTurnProcessor;
        [Inject] private ScoutTurnProcessor scoutTurnProcessor;
        [Inject] private ColonyTurnProcessor colonyTurnProcessor;
        [Inject] private BomberTurnProcessor bomberTurnProcessor;
        [Inject] private MineLayerTurnProcessor mineLayerTurnProcessor;
        [Inject] private PopulationRebalancerTurnProcessor populationRebalancerTurnProcessor;

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static TurnProcessorManager instance = new TurnProcessorManager();
        public static TurnProcessorManager Instance
        {
            get
            {
                return instance;
            }
        }

        TurnProcessorManager()
        {
            instance = this;
        }

        public override void _Ready()
        {
            instance = this;
            this.ResolveDependencies();
            base._Ready();

            // Register default turn processors
            RegisterTurnProcessor(shipDesignerTurnProcessor);
            RegisterTurnProcessor(fleetCompositionTurnProcessor);
            RegisterTurnProcessor(planetProductionTurnProcessor);
            RegisterTurnProcessor(scoutTurnProcessor);
            RegisterTurnProcessor(colonyTurnProcessor);
            RegisterTurnProcessor(bomberTurnProcessor);
            RegisterTurnProcessor(mineLayerTurnProcessor);
            RegisterTurnProcessor(populationRebalancerTurnProcessor);
        }

        /// <summary>
        /// A list of all turn processors being managed
        /// </summary>
        public IEnumerable<TurnProcessor> TurnProcessors { get => turnProcessors; }
        List<TurnProcessor> turnProcessors = new List<TurnProcessor>();

        /// <summary>
        /// Dictionary to key turn processors by name
        /// </summary>
        Dictionary<string, TurnProcessor> TurnProcessorsByName { get; set; } = new Dictionary<string, TurnProcessor>();

        public void RegisterTurnProcessor(TurnProcessor processor)
        {
            turnProcessors.Add(processor);
            TurnProcessorsByName.Add(processor.Name, processor);
        }

        public TurnProcessor GetTurnProcessor(string name)
        {
            TurnProcessorsByName.TryGetValue(name, out var processor);
            return processor;
        }

    }
}
