using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static CraigStars.Utils.Utils;

namespace CraigStars
{
    public class PacketMove1Step : AbstractPacketMoveStep
    {
        public PacketMove1Step(IProvider<Game> gameProvider, PlanetService planetService, ShipDesignDiscoverer designDiscoverer) 
            : base(gameProvider, planetService, designDiscoverer, TurnGenerationState.PacketMove1Step, 1) { }
    }
}