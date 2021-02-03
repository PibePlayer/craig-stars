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
    /// The Signals csharp class services as a way to bridge csharp and gdscript until 
    /// everything is rewritten in .net
    /// </summary>
    public static class EventManager
    {

        #region Server Events

        public static event Action<Fleet> FleetBuiltEvent;

        #endregion

        #region Event Publishers

        internal static void PublishFleetBuiltEvent(Fleet fleet)
        {
            FleetBuiltEvent?.Invoke(fleet);
        }

        #endregion

    }
}
