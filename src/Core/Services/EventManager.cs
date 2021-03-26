using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars.Singletons
{
    /// <summary>
    /// 
    /// </summary>
    public static class EventManager
    {

        #region Server Events

        public static event Action<Fleet> FleetCreatedEvent;
        public static event Action<Fleet> FleetDeletedEvent;
        public static event Action<Planet> PlanetPopulationEmptiedEvent;
        public static event Action<Player, TechField, int> PlayerResearchLevelIncreasedEvent;
        public static event Action<Battle> BattleRunEvent;

        #endregion

        #region Event Publishers

        internal static void PublishFleetCreatedEvent(Fleet fleet) => FleetCreatedEvent?.Invoke(fleet);
        internal static void PublishFleetDeletedEvent(Fleet fleet) => FleetDeletedEvent?.Invoke(fleet);
        internal static void PublishPlanetPopulationEmptiedEvent(Planet planet) => PlanetPopulationEmptiedEvent?.Invoke(planet);
        internal static void PublishPlayerResearchLevelIncreasedEvent(Player player, TechField field, int level) => PlayerResearchLevelIncreasedEvent?.Invoke(player, field, level);
        internal static void PublishBattleRunEvent(Battle battle) => BattleRunEvent?.Invoke(battle);

        #endregion

    }
}
