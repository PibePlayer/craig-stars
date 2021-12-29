
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
            var techStoreProvider = new TestTechStoreProvider();
            var rulesProvider = new TestRulesProvider();
            var playerTechService = new PlayerTechService(techStoreProvider);
            var playerService = new PlayerService(rulesProvider);
            var planetService = new PlanetService(playerService, playerTechService, rulesProvider);
            var fleetSpecService = new FleetSpecService(rulesProvider);
            var fleetService = new FleetService(fleetSpecService);
            var estimator = new ProductionQueueEstimator(planetService, playerService);
            var shipDesignGenerator = new ShipDesignGenerator(playerTechService, fleetSpecService);
            var cargoTransferer = new CargoTransferer();

            // Register default turn processors
            var shipDesignerTurnProcessor = new ShipDesignerTurnProcessor(shipDesignGenerator, playerTechService, fleetSpecService, techStoreProvider);
            RegisterTurnProcessor(shipDesignerTurnProcessor);
            RegisterTurnProcessor(new FleetCompositionTurnProcessor());
            RegisterTurnProcessor(new PlanetProductionTurnProcessor(planetService, estimator));
            RegisterTurnProcessor(new ScoutTurnProcessor(planetService, fleetService));
            RegisterTurnProcessor(new ColonyTurnProcessor(planetService, fleetService, shipDesignerTurnProcessor, cargoTransferer));
            RegisterTurnProcessor(new BomberTurnProcessor(planetService, fleetService));
            RegisterTurnProcessor(new MineLayerTurnProcessor());
            RegisterTurnProcessor(new PopulationRebalancerTurnProcessor(planetService, fleetService, cargoTransferer));
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