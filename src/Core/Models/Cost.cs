using System;
using Newtonsoft.Json;

namespace CraigStars
{
    public readonly struct Cost
    {
        public readonly int Ironium;
        public readonly int Boranium;
        public readonly int Germanium;
        public readonly int Resources;

        [JsonConstructor]
        public Cost(int ironium = 0, int boranium = 0, int germanium = 0, int resources = 0)
        {
            Ironium = ironium;
            Boranium = boranium;
            Germanium = germanium;
            Resources = resources;
        }

        public Cost(Mineral minerals, int resources = 0)
        {
            Ironium = minerals.Ironium;
            Boranium = minerals.Boranium;
            Germanium = minerals.Germanium;
            Resources = resources;
        }

        public override string ToString()
        {
            return $"Cost i:{Ironium}, b:{Boranium}, g:{Germanium}, r:{Resources}";
        }

        public static Cost Zero { get => new Cost(0, 0, 0, 0); }

        public static Cost operator +(Cost a, Cost b)
        {
            return new Cost(
                a.Ironium + b.Ironium,
                a.Boranium + b.Boranium,
                a.Germanium + b.Germanium,
                a.Resources + b.Resources
            );
        }

        public static Cost operator -(Cost a, Cost b)
        {
            return new Cost(
                a.Ironium - b.Ironium,
                a.Boranium - b.Boranium,
                a.Germanium - b.Germanium,
                a.Resources - b.Resources
            );
        }

        public static Cost operator -(Cost a, Mineral b)
        {
            return new Cost(
                a.Ironium - b.Ironium,
                a.Boranium - b.Boranium,
                a.Germanium - b.Germanium,
                a.Resources
            );
        }

        public static Cost operator *(Cost a, int b)
        {
            return new Cost(
                a.Ironium * b,
                a.Boranium * b,
                a.Germanium * b,
                a.Resources * b
            );
        }

        public static Cost operator *(Cost a, double b)
        {
            return new Cost(
                (int)(a.Ironium * b + .5),
                (int)(a.Boranium * b + .5),
                (int)(a.Germanium * b + .5),
                (int)(a.Resources * b + .5)
            );
        }

        /// <summary>
        /// Divide two costs to find the number of times Cost b fits into Cost a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float operator /(Cost a, Cost b)
        {
            // if either component is 0, we will have at least one
            float newIronium = b.Ironium == 0 ? float.MaxValue : (float)a.Ironium / b.Ironium;
            float newBoranium = b.Boranium == 0 ? float.MaxValue : (float)a.Boranium / b.Boranium;
            float newGermanium = b.Germanium == 0 ? float.MaxValue : (float)a.Germanium / b.Germanium;
            float newResources = b.Resources == 0 ? float.MaxValue : (float)a.Resources / b.Resources;

            // get the minimum number of times b goes into a
            return Math.Min(newResources,
                        Math.Min(newIronium,
                            Math.Min(newBoranium, newGermanium)
                        )
                    );
        }

        public static Cost operator -(Cost a)
        {
            return new Cost(
                -a.Ironium,
                -a.Boranium,
                -a.Germanium,
                -a.Resources
            );
        }

        /// <summary>
        /// Return true if all components of this cost are greater than or equal to scalar
        /// </summary>
        /// <param name="a"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static bool operator >=(Cost a, int scalar)
        {
            return
                a.Ironium >= scalar &&
                a.Boranium >= scalar &&
                a.Germanium >= scalar &&
                a.Resources >= scalar;
        }

        /// <summary>
        /// Return true if all components of this cost are less than or equal to scalar
        /// </summary>
        /// <param name="a"></param>
        /// <param name="scalar"></param>
        /// <returns></returns>
        public static bool operator <=(Cost a, int scalar)
        {
            return
                a.Ironium <= scalar &&
                a.Boranium <= scalar &&
                a.Germanium <= scalar &&
                a.Resources <= scalar;
        }

        public override bool Equals(object obj)
        {
            if (obj is Cost cargo)
            {
                return Equals(cargo);
            }
            return false;
        }

        public bool Equals(Cost other)
        {
            return Ironium == other.Ironium && Boranium == other.Boranium && Germanium == other.Germanium && Resources == other.Resources;
        }

        public static bool operator ==(Cost a, Cost b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Cost a, Cost b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Ironium.GetHashCode() ^ Boranium.GetHashCode() ^ Germanium.GetHashCode() ^ Resources.GetHashCode();
        }

        /// <summary>
        /// Get a copy of this, with updated Ironium
        /// </summary>
        /// <returns></returns>
        public Cost WithIronium(int ironium = 0)
        {
            return new Cost(ironium, Boranium, Germanium, Resources);
        }

        /// <summary>
        /// Get a copy of this, with updated Boranium
        /// </summary>
        /// <returns></returns>
        public Cost WithBoranium(int boranium = 0)
        {
            return new Cost(Ironium, boranium, Germanium, Resources);
        }

        /// <summary>
        /// Get a copy of this, with updated Germanium
        /// </summary>
        /// <returns></returns>
        public Cost WithGermanium(int germanium = 0)
        {
            return new Cost(Ironium, Boranium, germanium, Resources);
        }

        /// <summary>
        /// Get a copy of this, with updated Colonists
        /// </summary>
        /// <returns></returns>
        public Cost WithResources(int resources = 0)
        {
            return new Cost(Ironium, Boranium, Germanium, resources);
        }
    }
}