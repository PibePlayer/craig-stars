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

        public BattleRecordTokenFire(BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed) : base(token, from)
        {
            To = to;
            Slot = slot;
            Target = target;
            DamageDoneShields = damageDoneShields;
            DamageDoneArmor = damageDoneArmor;
            TokensDestroyed = tokensDestroyed;
        }

        /// <summary>
        /// The ending location of the token
        /// </summary>
        public Vector2 To { get; set; }

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
        public int DamageDoneShields { get; set; }
        public int DamageDoneArmor { get; set; }

        public override string ToString()
        {
            return $"{Token} fired upon {Target} with {Token.Token.Design.Slots[Slot - 1].HullComponent.Name} for {DamageDoneShields} shield damage, {DamageDoneArmor} armor damage, destroying {TokensDestroyed} ships";
        }

    }
}