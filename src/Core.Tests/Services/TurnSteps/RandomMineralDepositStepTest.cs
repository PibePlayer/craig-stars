using Godot;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using FakeItEasy;

using CraigStars.Singletons;
using log4net;
using System.Diagnostics;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System.Threading.Tasks;
using System.Linq;

namespace CraigStars.Tests
{
    [TestFixture]
    public class RandomMineralDepositStepTest
    {
        static CSLog log = LogProvider.GetLogger(typeof(RandomMineralDepositStepTest));

        Game game;
        GameRunner gameRunner;
        RandomMineralDepositStep step;
        IRulesProvider mockRulesProvider;

        [SetUp]
        public void SetUp()
        {
            mockRulesProvider = A.Fake<IRulesProvider>();
            (game, gameRunner) = TestUtils.GetSingleUnitGame();
            step = new RandomMineralDepositStep(gameRunner.GameProvider, mockRulesProvider);
        }

        [Test]
        public void TestProcess()
        {
            var player = game.Players[0];
            var planet = game.Planets[0];

            // start with some base mineral conc
            planet.MineralConcentration = new Mineral(1, 2, 3);

            // make sure our random number generator returns "yes, this planet gets a bonus mineral deposit"
            var mockRules = A.Fake<Rules>();
            var random = A.Fake<Random>();
            A.CallTo(() => mockRulesProvider.Rules).Returns(mockRules);
            mockRules.Random = random;
            mockRules.RandomMineralDepositBonusRange = new Tuple<int, int>(10, 20);
            
            A.CallTo(() => random.NextDouble()).Returns(0); // always discover
            A.CallTo(() => random.Next(3)).Returns((int)MineralType.Ironium); // random mineral is Ironiu
            A.CallTo(() => random.Next(10, 20)).Returns(15); // random amount is 15

            // should permaform one step, and adjust our value up one
            step.Execute(new TurnGenerationContext(), game.OwnedPlanets.ToList());

            // should get a bonus
            Assert.AreEqual(new Mineral(16, 2, 3), planet.MineralConcentration);
            Assert.AreEqual(1, player.Messages.Count);
            Assert.AreEqual(MessageType.RandomMineralDeposit, player.Messages[0].Type);
        }
    }
}