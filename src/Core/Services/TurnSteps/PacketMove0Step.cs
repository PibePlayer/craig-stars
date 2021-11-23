using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class PacketMove0Step : AbstractPacketMoveStep
    {
        public PacketMove0Step(IProvider<Game> gameProvider, PlanetService planetService, ShipDesignDiscoverer designDiscoverer, IRulesProvider rulesProvider) 
            : base(gameProvider, planetService, designDiscoverer, rulesProvider, TurnGenerationState.PacketMove0Step, 0) { }
    }
}