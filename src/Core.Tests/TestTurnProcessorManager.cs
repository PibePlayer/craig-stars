
using System.Collections.Generic;

namespace CraigStars.Tests
{
    public class TestTurnProcessorManager : ITurnProcessorManager
    {
        public IEnumerable<TurnProcessor> TurnProcessors { get => turnProcessors; }
        List<TurnProcessor> turnProcessors = new List<TurnProcessor>();

        /// <summary>
        /// Dictionary to key turn processors by name
        /// </summary>
        Dictionary<string, TurnProcessor> TurnProcessorsByName { get; set; } = new Dictionary<string, TurnProcessor>();

        public TestTurnProcessorManager()
        {

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