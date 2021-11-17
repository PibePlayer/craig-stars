using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{
    public record StartingFleet(string Name, string HullName, ShipDesignPurpose Purpose) { }
}