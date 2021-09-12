using System;
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
    public class BattlePlan : PlayerPlan<BattlePlan>
    {
        public BattlePlan() : base() { }
        public BattlePlan(string name) : base(name) { }

        public BattleTargetType PrimaryTarget { get; set; } = BattleTargetType.ArmedShips;
        public BattleTargetType SecondaryTarget { get; set; } = BattleTargetType.Any;
        public BattleTactic Tactic { get; set; } = BattleTactic.MaximizeDamageRatio;
        public BattleAttackWho AttackWho { get; set; } = BattleAttackWho.EnemiesAndNeutrals;
        public bool DumpCargo { get; set; }

        /// <summary>
        /// Make a clone of this battle plan
        /// </summary>
        /// <returns></returns>
        public override BattlePlan Clone()
        {
            return new BattlePlan()
            {
                Guid = Guid,
                Name = Name,
                PrimaryTarget = PrimaryTarget,
                SecondaryTarget = SecondaryTarget,
                Tactic = Tactic,
                AttackWho = AttackWho,
                DumpCargo = DumpCargo,
            };
        }
    }
}