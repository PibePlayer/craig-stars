
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
            var rulesProvider = new TestRulesProvider();
            var playerTechService = new PlayerTechService(new TestTechStoreProvider());
            var planetService = new PlanetService(playerTechService);
            var fleetService = new FleetService();
            var playerService = new PlayerService(rulesProvider);
            var estimator = new ProductionQueueEstimator(planetService, playerService);
            var shipDesignGenerator = new ShipDesignGenerator(playerTechService);

            // Register default turn processors
            var shipDesignerTurnProcessor = new ShipDesignerTurnProcessor(shipDesignGenerator, playerTechService);
            RegisterTurnProcessor(shipDesignerTurnProcessor);
            RegisterTurnProcessor(new FleetCompositionTurnProcessor());
            RegisterTurnProcessor(new PlanetProductionTurnProcessor(planetService, estimator));
            RegisterTurnProcessor(new ScoutTurnProcessor(planetService, fleetService));
            RegisterTurnProcessor(new ColonyTurnProcessor(planetService, fleetService, shipDesignerTurnProcessor));
            RegisterTurnProcessor(new BomberTurnProcessor(planetService, fleetService));
            RegisterTurnProcessor(new MineLayerTurnProcessor());
            RegisterTurnProcessor(new PopulationRebalancerTurnProcessor(planetService, fleetService));
        }

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