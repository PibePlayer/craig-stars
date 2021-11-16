using System.Linq;
using System.Reflection;
using CraigStars.UniverseGeneration;
using SimpleInjector;

namespace CraigStars
{
    /// <summary>
    /// SimpleInjector Container to autowire game instances
    /// </summary>
    public static class GameRunnerContainer
    {
        public static GameRunner CreateGameRunner(Game game, ITechStore techStore)
        {
            // create a new DI container for this game
            // it will auto inject this game in all the needed turn generators and 
            // other services
            Container container = new Container();
            container.RegisterInstance<IProvider<Game>>(new Provider<Game>(game));
            container.RegisterInstance<IProvider<ITechStore>>(new Provider<ITechStore>(techStore));
            container.RegisterInstance<ITechStore>(techStore);
            container.RegisterInstance<IRulesProvider>(game);
            container.RegisterInstance<Game>(game);

            container.Register<GameRunner>(Lifestyle.Singleton);
            container.Register<RaceService>(Lifestyle.Singleton);
            container.Register<UniverseGenerator>(Lifestyle.Singleton);
            container.Register<TurnGenerator>(Lifestyle.Singleton);
            container.Register<TurnSubmitter>(Lifestyle.Singleton);
            container.Register<PlanetService>(Lifestyle.Singleton);
            container.Register<PlayerService>(Lifestyle.Singleton);
            container.Register<FleetService>(Lifestyle.Singleton);
            container.Register<FleetAggregator>(Lifestyle.Singleton);
            container.Register<Researcher>(Lifestyle.Singleton);
            container.Register<PlayerTechService>(Lifestyle.Singleton);
            container.Register<ShipDesignGenerator>(Lifestyle.Singleton);
            container.Register<InvasionProcessor>(Lifestyle.Singleton);
            container.Register<MineFieldDamager>(Lifestyle.Singleton);
            container.Register<MineFieldDecayer>(Lifestyle.Singleton);
            container.Register<FleetOrderExecutor>(Lifestyle.Singleton);
            container.Register<BattleEngine>(Lifestyle.Singleton);
            container.Register<ProductionQueueEstimator>(Lifestyle.Singleton);

            // register player intel and all discoverers
            container.Register<PlayerIntel>(Lifestyle.Singleton);

            container.Register<PlanetDiscoverer>(Lifestyle.Singleton);
            container.Register<FleetDiscoverer>(Lifestyle.Singleton);
            container.Register<ShipDesignDiscoverer>(Lifestyle.Singleton);
            container.Register<MineFieldDiscoverer>(Lifestyle.Singleton);
            container.Register<MineralPacketDiscoverer>(Lifestyle.Singleton);
            container.Register<SalvageDiscoverer>(Lifestyle.Singleton);
            container.Register<WormholeDiscoverer>(Lifestyle.Singleton);
            container.Register<MysteryTraderDiscoverer>(Lifestyle.Singleton);


            // we need this outside of the TurnStep array
            container.Register<PlayerScanStep>(Lifestyle.Singleton);
            container.Register<PlanetGrowStep>(Lifestyle.Singleton);
            container.Register<WormholeGenerationStep>(Lifestyle.Singleton);

            // universe generation steps
            container.Collection.Register<UniverseGenerationStep>(
                typeof(PlanetGenerationStep),
                typeof(WormholeGenerationStep),
                typeof(PlayerTechLevelsGenerationStep),
                typeof(PlayerPlansGenerationStep),
                typeof(PlayerShipDesignsGenerationStep),
                typeof(PlayerPlanetReportGenerationStep),
                typeof(PlayerHomeworldGenerationStep),
                typeof(PlayerFleetGenerationStep),
                typeof(GameStartModeModifierStep)
            );

            // turn steps
            container.Collection.Register<TurnGenerationStep>(
                typeof(FleetAgeStep),
                typeof(FleetWaypoint0Step),
                typeof(PacketMove0Step),
                typeof(FleetMoveStep),
                typeof(FleetReproduceStep),
                typeof(DecaySalvageStep),
                typeof(DecayPacketsStep),
                typeof(WormholeJiggleStep),
                typeof(DetonateMinesStep),
                typeof(PlanetMineStep),
                typeof(PlanetProductionStep),
                typeof(InstaformStep),
                typeof(PlayerResearchStep),
                typeof(PlanetGrowStep),
                typeof(PacketMove1Step),
                typeof(FleetBattleStep),
                typeof(PlanetBombStep),
                typeof(FleetWaypoint1Step),
                typeof(DecayMinesStep),
                typeof(FleetLayMinesStep),
                typeof(FleetSweepMinesStep),
                typeof(PlayerScanStep),
                typeof(CalculateScoreStep),
                typeof(CheckVictoryStep)
            );

            // turn processors
            container.Register<ShipDesignerTurnProcessor>(Lifestyle.Singleton);
            container.Register<FleetCompositionTurnProcessor>(Lifestyle.Singleton);
            container.Register<PlanetProductionTurnProcessor>(Lifestyle.Singleton);
            container.Register<ScoutTurnProcessor>(Lifestyle.Singleton);
            container.Register<ColonyTurnProcessor>(Lifestyle.Singleton);
            container.Register<BomberTurnProcessor>(Lifestyle.Singleton);
            container.Register<MineLayerTurnProcessor>(Lifestyle.Singleton);
            container.Register<PopulationRebalancerTurnProcessor>(Lifestyle.Singleton);

            container.Verify();

            return container.GetInstance<GameRunner>();
        }

    }
}