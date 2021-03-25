using System;
using System.Collections.Generic;

namespace CraigStars.Utils
{
    public static class Utils
    {

        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this Random rng, List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        /// <summary>
        /// Iterate over items in a list with an index
        /// i.e. items.Each((item, index) => {})
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        /// <summary>
        /// Helper function to round to the nearest 100 (by default)
        /// This is used to ensure we have colonists in counts of 100 after bombings, invasions, etc
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="nearest">The nearest integer to round to, defaults to 100</param>
        /// <returns></returns>
        public static int RoundToNearest(float value, int nearest = 100)
        {
            return (int)(Math.Round(value / nearest) * nearest);
        }

    }
}

