using System;
using System.Collections.Generic;
using Godot;

namespace CraigStars.Singletons
{
    /// <summary>
    /// The currently active rules for this game
    /// </summary>
    public class TurnProcessorManager : Node, ITurnProcessorManager
    {
        static CSLog log = LogProvider.GetLogger(typeof(RulesManager));

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

            // Register default turn processors
            RegisterTurnProcessor<ShipDesignerTurnProcessor>();
            RegisterTurnProcessor<FleetCompositionTurnProcessor>();
            RegisterTurnProcessor<PlanetProductionTurnProcessor>();
            RegisterTurnProcessor<ScoutTurnProcessor>();
            RegisterTurnProcessor<ColonyTurnProcessor>();
            RegisterTurnProcessor<BomberTurnProcessor>();
            RegisterTurnProcessor<MineLayerTurnProcessor>();
            RegisterTurnProcessor<PopulationRebalancerTurnProcessor>();
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

        public void RegisterTurnProcessor<T>() where T : TurnProcessor, new()
        {
            var processor = new T();
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
