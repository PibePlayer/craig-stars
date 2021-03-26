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
        public BattleTargetType PrimaryTarget { get; set; } = BattleTargetType.ArmedShip;
        public BattleTargetType SecondaryTarget { get; set; } = BattleTargetType.UnarmedShips;
        public BattleTactic Tactic { get; set; } = BattleTactic.MaximizeDamage;
        public BattleAttackWho AttackWho { get; set; } = BattleAttackWho.Enemies;
    }
}