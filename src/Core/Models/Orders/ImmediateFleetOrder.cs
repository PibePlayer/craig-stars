using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// An immediate fleet order (i.e. the client clicks a merge in the UI, or cargo transfer, etc)
    /// This is the same as a PlayerObjectOrder, but they have to be executed in order 
    /// because fleet A could transfer from fleet B which then transfers to fleet C
    /// </summary>
    public abstract class ImmediateFleetOrder : PlayerObjectOrder
    {
    }
}