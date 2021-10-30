using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;

using CraigStars;
using CraigStars.Singletons;

namespace CraigStars.Tests
{
    [TestFixture]
    public class FleetCompositionTurnProcessorTest
    {
        FleetCompositionTurnProcessor processor = new();

        [Test]
        public void TestProcess()
        {
            Player player = new();
            PublicGameInfo gameInfo = new() { Players = new() { player } };
            processor.Process(gameInfo, player);

            Assert.Greater(player.FleetCompositions.Count, 0);
            Assert.True(player.FleetCompositionsByType.ContainsKey(FleetCompositionType.Bomber));
            Assert.False(player.FleetCompositionsByType.ContainsKey(FleetCompositionType.None));
        }
    }
}