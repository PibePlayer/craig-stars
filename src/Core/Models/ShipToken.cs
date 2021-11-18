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
            var shields = Design.Spec.Shield;
            var armor = Design.Spec.Armor;
            var possibleDamageToShields = damage * .5;
            var actualDamageToShields = Math.Min(shields, possibleDamageToShields);
            var remainingDamage = damage - actualDamageToShields;
            var exisitingDamage = Damage * QuantityDamaged;

            Damage = (int)(exisitingDamage + remainingDamage);

            int tokensDestroyed = Math.Min(Quantity, (int)(Damage / armor));
            Quantity = Quantity - tokensDestroyed;

            if (Quantity > 0)
            {
                // Figure out how much damage we have leftover after destroying
                // tokens. This will be applied to the rest of the tokens
                // if we took 100 damage, and we have 40 armor, we lose 2 tokens
                // and have 20 leftover damage to spread across tokens
                var leftoverDamage = Damage - tokensDestroyed * armor;
                Damage = (int)leftoverDamage;
                QuantityDamaged = Quantity;
            }

            return new TokenDamage((int)remainingDamage, tokensDestroyed);
        }

        public TokenDamage ApplyOvergateDamage(float dist, int safeRange, int safeSourceMass, int safeDestMass, int maxMassFactor = 5)
        {

            var rangeDamageFactor = GetStargateRangeDamageFactor(dist, safeRange);
            var massDamageFactor = GetStargateMassDamageFactor(dist, safeSourceMass, safeDestMass, maxMassFactor);

            var totalDamageFactor = Math.Min(.98, massDamageFactor + (1.0 - massDamageFactor) * rangeDamageFactor);

            // apply damage as a percentage of armor to all tokens
            var armor = Design.Spec.Armor;
            int newDamage = (int)Math.Round(totalDamageFactor * Quantity * armor);
            QuantityDamaged = Quantity;
            Damage += newDamage;

            int tokensDestroyed = Math.Min(Quantity, (int)(Damage / armor));

            Quantity = Quantity - tokensDestroyed;
            if (Quantity > 0)
            {
                // Figure out how much damage we have leftover after destroying
                // tokens. This will be applied to the rest of the tokens
                // if we took 100 damage, and we have 40 armor, we lose 2 tokens
                // and have 20 leftover damage to spread across tokens
                var leftoverDamage = Damage - tokensDestroyed * armor;
                Damage = (int)leftoverDamage;
                QuantityDamaged = Quantity;
            }

            return new TokenDamage(newDamage, tokensDestroyed);

        }

        public double GetStargateRangeDamageFactor(float dist, int safeRange)
        {
            var rangeDamageFactor = 0.0;
            if (safeRange == TechHullComponent.InfinteGate || safeRange >= dist)
            {
                // no damage
                rangeDamageFactor = 0;
            }
            else
            {
                rangeDamageFactor = (dist - safeRange) / (4.0 * safeRange);
            }

            return rangeDamageFactor;
        }

        public double GetStargateMassDamageFactor(float dist, int safeSourceMass, int safeDestMass, int maxMassFactor = 5)
        {
            var mass = Design.Spec.Mass;
            var sourceMassDamageFactor = 1.0;
            var destMassDamageFactor = 1.0;
            if (safeSourceMass != TechHullComponent.InfinteGate && safeSourceMass < mass)
            {
                sourceMassDamageFactor = (maxMassFactor * safeSourceMass - mass) / (4.0 * safeSourceMass);
            }
            if (safeDestMass != TechHullComponent.InfinteGate && safeDestMass < mass)
            {
                destMassDamageFactor *= (maxMassFactor * safeDestMass - mass) / (4.0 * safeDestMass);
            }

            return 1 - (sourceMassDamageFactor * destMassDamageFactor);
        }

        /// <summary>
        /// Vanishing% = 100/3*[1-(5*maxMass-mass)^2/(4*maxMass)^2], rounded down to nearest 1%.
        /// where maxMass is the maximum safe mass for the sending gate.
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="safeRange"></param>
        /// <param name="safeSourceMass"></param>
        /// <param name="maxMassFactor"></param>
        /// <returns>The chance this a ship in this </returns>
        public double GetStargateMassVanishingChance(int safeSourceMass, int maxMassFactor = 5)
        {
            var mass = Design.Spec.Mass;
            var vanishingChance = 100.0 / 3 * (1 -
                (double)((maxMassFactor * safeSourceMass - mass) * (maxMassFactor * safeSourceMass - mass)) /
                ((4 * safeSourceMass) * (4 * safeSourceMass))
            );

            // chance as percent
            return Math.Floor(vanishingChance) / 100.0;
        }

        /// <summary>
        /// For distance overgating the probability of ships being lost to the void is roughly equal to the damage divided by 3. 
        /// For example, if the overgating causes 60% damage then there will be a 20% chance of losing the ship.
        /// </summary>
        /// <param name="dist"></param>
        /// <param name="safeRange"></param>
        /// <param name="safeSourceMass"></param>
        /// <param name="maxMassFactor"></param>
        /// <returns></returns>
        public double GetStargateRangeVanishingChance(float dist, int safeRange)
        {
            // chance as percent, rounded down to 1%
            return Math.Floor(100.0 / 3 * GetStargateRangeDamageFactor(dist, safeRange)) / 100.0;
        }
    }
}