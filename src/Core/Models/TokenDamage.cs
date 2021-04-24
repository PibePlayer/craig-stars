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

        public TokenDamage(int damage = 0, int tokensDestroyed = 0)
        {
            this.damage = damage;
            this.shipsDestroyed = tokensDestroyed;
        }

        public static TokenDamage None { get => new TokenDamage(0, 0); }

        public override string ToString()
        {
            return $"{damage} dp, {shipsDestroyed} ships destroyed";
        }

        public override bool Equals(object obj)
        {
            if (obj is TokenDamage tokenDamage)
            {
                return Equals(tokenDamage);
            }
            return false;
        }

        public bool Equals(TokenDamage other)
        {
            return damage == other.damage && shipsDestroyed == other.shipsDestroyed;
        }

        public static bool operator ==(TokenDamage a, TokenDamage b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TokenDamage a, TokenDamage b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return damage.GetHashCode() ^ shipsDestroyed.GetHashCode();
        }
    }
}