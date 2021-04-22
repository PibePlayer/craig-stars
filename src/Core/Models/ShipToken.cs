using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A stack of a single ShipDesign type in a fleet.  A fleet is made up of many Ship Stacks
    /// </summary>
    public class ShipToken
    {
        [JsonProperty(IsReference = true)]
        public ShipDesign Design { get; set; } = new ShipDesign();
        public int Quantity { get; set; }

        /// <summary>
        /// A stack of tokens needs to know how many of the tokens are damaged and 
        /// what percent that quanity is damaged
        /// </summary>
        /// <value></value>
        public int QuantityDamaged { get; set; }

        /// <summary>
        /// This is the percent the QuanityDamaged tokens are damaged
        /// </summary>
        /// <value></value>
        public float Damage { get; set; }

        public ShipToken() { }

        public ShipToken(ShipDesign design, int quantity)
        {
            Design = design;
            Quantity = quantity;
        }

        public ShipToken(ShipDesign design, int quantity, float damage, int quantityDamaged)
        {
            Design = design;
            Quantity = quantity;
            Damage = damage;
            QuantityDamaged = quantityDamaged;
        }

        /// <summary>
        /// Apply damage to a token, updating quantity damaged and damage amount
        /// </summary>
        /// <param name="damage"></param>
        public TokenDamage ApplyMineDamage(int damage)
        {
            // mines do half damage to shields
            var shields = Design.Aggregate.Shield;
            var armor = Design.Aggregate.Armor;
            var possibleDamageToShields = damage * .5;
            var actualDamageToShields = Math.Min(shields, possibleDamageToShields);
            var remainingDamage = damage - actualDamageToShields;
            var exisitingDamage = Damage * QuantityDamaged;

            var newDamage = exisitingDamage + remainingDamage;

            int tokensDestroyed = Math.Min(Quantity, (int)(newDamage / armor));
            Quantity = Quantity - tokensDestroyed;

            if (Quantity > 0)
            {
                // Figure out how much damage we have leftover after destroying
                // tokens. This will be applied to the rest of the tokens
                // if we took 100 damage, and we have 40 armor, we lose 2 tokens
                // and have 20 leftover damage to spread across tokens
                var leftoverDamage = newDamage - tokensDestroyed * armor;
                Damage = (int)leftoverDamage;
                QuantityDamaged = Quantity;
            }

            return new TokenDamage((int)remainingDamage, tokensDestroyed);
        }

    }
}