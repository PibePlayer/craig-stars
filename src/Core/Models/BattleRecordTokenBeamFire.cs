using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleRecordTokenBeamFire : BattleRecordTokenFire
    {

        public BattleRecordTokenBeamFire()
        {
        }

        public BattleRecordTokenBeamFire(BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed)
        : base(token, from, to, slot, target, damageDoneShields, damageDoneArmor, tokensDestroyed)
        {
        }

        public override string ToString()
        {
            return $"{Token} fired upon {Target} with {Token.Token.Design.Slots[Slot - 1].HullComponent.Name} for {DamageDoneShields} shield damage, {DamageDoneArmor} armor damage, destroying {TokensDestroyed} ships";
        }

    }
}