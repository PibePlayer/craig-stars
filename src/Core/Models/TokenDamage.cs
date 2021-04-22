using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// A struct representing damage done to a token
    /// </summary>
    public readonly struct TokenDamage
    {
        public readonly int damage;
        public readonly int shipsDestroyed;

        public TokenDamage(int damage, int tokensDestroyed)  {
            this.damage = damage;
            this.shipsDestroyed = tokensDestroyed;
        }
    }
}