using Godot;
using SimpleInjector;
using System;

namespace CraigStars.Singletons
{
    /// <summary>
    /// Dependency Injection into godot nodes
    /// ref: https://playdivinary.com/posts/di-in-godot/
    /// </summary>
    public class DependencyInjectionSystem : Node
    {
        private SimpleInjector.Container container;

        /// <summary>
        /// Special client rules provider. It tries to find the rules from the active game
        /// If that doesn't work it goes to the RulesManager instance. If that doesn't work
        /// It just returns an empty rules object
        /// </summary>
        class ClientRulesProvider : IRulesProvider
        {
            public Rules Rules => PlayersManager.GameInfo?.Rules ?? RulesManager.Rules ?? new Rules(0);
        }

        class ClientTechStoreProvider : IProvider<ITechStore>
        {
            public ITechStore Item => CraigStars.Singletons.TechStore.Instance ?? StaticTechStore.Instance;
        }

        public override void _EnterTree()
        {
            base._EnterTree();
            container = new SimpleInjector.Container();

            var rulesProvider = new ClientRulesProvider();

            // register services
            container.Register<IProvider<ITechStore>, ClientTechStoreProvider>(Lifestyle.Singleton);
            container.Register<IRulesProvider, ClientRulesProvider>(Lifestyle.Singleton);
            container.Register<RaceService>(Lifestyle.Singleton);
            container.Register<PlayerService>(Lifestyle.Singleton);
            container.Register<PlanetService>(Lifestyle.Singleton);
            container.Register<FleetService>(Lifestyle.Singleton);
            container.Register<FleetSpecService>(Lifestyle.Singleton);
            container.Register<MineFieldDecayer>(Lifestyle.Singleton);
            container.Register<ProductionQueueEstimator>(Lifestyle.Singleton);
            container.Register<Researcher>(Lifestyle.Singleton);
            container.Register<PlayerTechService>(Lifestyle.Singleton);

            // turn processors
            container.Register<TurnProcessorRunner>(Lifestyle.Singleton);
            container.Register<ShipDesignGenerator>(Lifestyle.Singleton);
            container.Register<ShipDesignerTurnProcessor>(Lifestyle.Singleton);
            container.Register<FleetCompositionTurnProcessor>(Lifestyle.Singleton);
            container.Register<PlanetProductionTurnProcessor>(Lifestyle.Singleton);
            container.Register<ScoutTurnProcessor>(Lifestyle.Singleton);
            container.Register<ColonyTurnProcessor>(Lifestyle.Singleton);
            container.Register<BomberTurnProcessor>(Lifestyle.Singleton);
            container.Register<MineLayerTurnProcessor>(Lifestyle.Singleton);
            container.Register<PopulationRebalancerTurnProcessor>(Lifestyle.Singleton);

            container.Verify();
        }

        /// <summary>
        /// Resolve a type into a service
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public object Resolve(Type fieldType) => container.GetInstance(fieldType);
    }

}
