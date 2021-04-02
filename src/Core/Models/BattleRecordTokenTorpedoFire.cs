using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A token firing weapons
    /// </summary>
    public class BattleRecordTokenTorpedoFire : BattleRecordTokenFire
    {

        public BattleRecordTokenTorpedoFire()
        {
        }

        public BattleRecordTokenTorpedoFire(BattleRecordToken token, Vector2 from, Vector2 to, int slot, BattleRecordToken target, int damageDoneShields, int damageDoneArmor, int tokensDestroyed, int hits, int misses)
        : base(token, from, to, slot, target, damageDoneShields, damageDoneArmor, tokensDestroyed)
        {
            Hits = hits;
            Misses = misses;
        }

        public int Hits { get; set; }
        public int Misses { get; set; }

        public override string ToString()
        {
            return $"{Token} fired upon {Target} with {Token.Token.Design.Slots[Slot - 1].HullComponent.Name} (hit: {Hits}, missed: {Misses}) for {DamageDoneShields} shield damage, {DamageDoneArmor} armor damage, destroying {TokensDestroyed} ships";
        }

    }
}