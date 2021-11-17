using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CraigStars
{

    public record StartingPlanet(
        int Population,
        float HabPenaltyFactor = 0,
        bool HasStargate = false,
        bool HasMassDriver = false,
        List<StartingFleet> StartingFleets = null
    )
    { }

}