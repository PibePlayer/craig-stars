using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleRecordTokenFire : BattleRecordTokenAction
    {
        public BattleRecordTokenFire()
        {
        }

        public BattleRecordTokenFire(BattleRecordToken token, Vector2 from, int slot, BattleRecordToken target, int damageDone, int tokensDestroyed) : base(token, from)
        {
            Slot = slot;
            Target = target;
            DamageDone = damageDone;
            TokensDestroyed = tokensDestroyed;
        }

        /// <summary>
        /// The slot with weapons that is firing
        /// </summary>
        public int Slot { get; set; }

        /// <summary>
        /// The target fired upon
        /// </summary>
        /// <value></value>
        [JsonProperty(IsReference = true)]
        public BattleRecordToken Target { get; set; }

        public int TokensDestroyed { get; set; }
        public int DamageDone { get; set; }

        public override string ToString()
        {
            return $"{Token} fired upon {Target} with {Token.Token.Design.Slots[Slot - 1].HullComponent.Name} for {DamageDone} damage, destroying {TokensDestroyed} ships";
        }

    }
}