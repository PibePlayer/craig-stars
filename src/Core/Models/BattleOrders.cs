using System.Collections.Generic;

namespace CraigStars
{
    /// <summary>
    /// Fleet's have battle orders assigned made up of 4 parts
    /// * Primary target type
    /// * Secondary target type
    /// * Tactic
    /// * Which players the fleet should attack
    /// </summary>
    public class BattleOrders
    {
        public BattleTargetType PrimaryTarget { get; set; } = BattleTargetType.ArmedShips;
        public BattleTargetType SecondaryTarget { get; set; } = BattleTargetType.Any;
        public BattleTactic Tactic { get; set; } = BattleTactic.MaximizeDamageRatio;
        public BattleAttackWho AttackWho { get; set; } = BattleAttackWho.EnemiesAndNeutrals;
    }
}