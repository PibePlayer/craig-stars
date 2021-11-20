using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// Check each owned planet for a chance to discover a new mineral deposit, increasing the mineral concentration
    /// on the planet.
    /// </summary>
    public class RandomMineralDepositStep : TurnGenerationStep
    {
        private readonly IRulesProvider rulesProvider;
        private Rules Rules => rulesProvider.Rules;

        public RandomMineralDepositStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider) : base(gameProvider, TurnGenerationState.RandomMineralDiscoveryStep)
        {
            this.rulesProvider = rulesProvider;
        }

        public override void Process()
        {
            // give each player a chance to discover a random deposit on one of their planets
            HashSet<int> blessedPlayers = new();
            var depositDiscoveryChance = Rules.RandomEventChances[RandomEventType.MineralDeposit];
            foreach (var planet in OwnedPlanets.Where(planet => !blessedPlayers.Contains(planet.PlayerNum) && depositDiscoveryChance >= Rules.Random.NextDouble()))
            {
                var player = Game.Players[planet.PlayerNum];
                blessedPlayers.Add(player.Num);
                DiscoverMineralDeposit(planet, player);
            }
        }

        internal void DiscoverMineralDeposit(Planet planet, Player player)
        {
            var mineralTypes = Enum.GetValues(typeof(MineralType));
            MineralType mineralType = (MineralType)Rules.Random.Next(mineralTypes.Length);
            int concentrationBonus = Rules.Random.Next(Rules.RandomMineralDepositBonusRange.Item1, Rules.RandomMineralDepositBonusRange.Item2);

            planet.MineralConcentration = planet.MineralConcentration.WithType(mineralType, planet.MineralConcentration[mineralType] + concentrationBonus);
            Message.RandomMineralDeposit(player, planet, mineralType);
        }
    }
}