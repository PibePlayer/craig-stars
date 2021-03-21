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

        #endregion

        #region Event Publishers

        internal static void PublishFleetCreatedEvent(Fleet fleet) => FleetCreatedEvent?.Invoke(fleet);
        internal static void PublishFleetDeletedEvent(Fleet fleet) => FleetDeletedEvent?.Invoke(fleet);

        #endregion

    }
}
