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

        public override string ToString()
        {
            return $"Cost i:{Ironium}, b:{Boranium}, g:{Germanium}, r:{Resources}";
        }

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

        /// <summary>
        /// Divide two costs to find the number of times Cost b fits into Cost a
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator /(Cost a, Cost b)
        {
            int newIronium = int.MaxValue;
            int newBoranium = int.MaxValue;
            int newGermanium = int.MaxValue;
            int newResources = int.MaxValue;

            newIronium = b.Ironium > 0 ? a.Ironium / b.Ironium : newIronium;
            newBoranium = b.Boranium > 0 ? a.Boranium / b.Boranium : newBoranium;
            newGermanium = b.Germanium > 0 ? a.Germanium / b.Germanium : newGermanium;
            newResources = b.Resources > 0 ? a.Resources / b.Resources : newResources;

            // get the minimum number of times b goes into a
            return Math.Min(newResources,
                        Math.Min(newIronium,
                            Math.Min(newBoranium, newGermanium)
                        )
                    );
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