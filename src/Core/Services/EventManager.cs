using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public static class EventManager
    {

        #region Server Events

        public static event Action<MapObject> MapObjectCreatedEvent;
        public static event Action<MapObject> MapObjectDeletedEvent;
        public static event Action<Planet> PlanetPopulationEmptiedEvent;
        public static event Action<Player, TechField, int> PlayerResearchLevelIncreasedEvent;
        public static event Action<Battle> BattleRunEvent;

        #endregion

        #region Event Publishers

        internal static void PublishMapObjectCreatedEvent(MapObject mapObject) => MapObjectCreatedEvent?.Invoke(mapObject);
        internal static void PublishMapObjectDeletedEvent(MapObject mapObject) => MapObjectDeletedEvent?.Invoke(mapObject);
        internal static void PublishPlanetPopulationEmptiedEvent(Planet planet) => PlanetPopulationEmptiedEvent?.Invoke(planet);
        internal static void PublishPlayerResearchLevelIncreasedEvent(Player player, TechField field, int level) => PlayerResearchLevelIncreasedEvent?.Invoke(player, field, level);
        internal static void PublishBattleRunEvent(Battle battle) => BattleRunEvent?.Invoke(battle);

        #endregion

    }
}
