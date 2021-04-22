using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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

        /// <summary>
        /// Stolen from godot because I couldn't unit test it. Bummer
        /// </summary>
        /// <param name="segmentFrom"></param>
        /// <param name="segmentTo"></param>
        /// <param name="circlePosition"></param>
        /// <param name="circleRadius"></param>
        /// <returns></returns>
        public static float SegmentIntersectsCircle(Vector2 segmentFrom, Vector2 segmentTo, Vector2 circlePosition, float circleRadius)
        {
            Vector2 line_vec = segmentTo - segmentFrom;
            Vector2 vec_to_line = segmentFrom - circlePosition;

            // Create a quadratic formula of the form ax^2 + bx + c = 0
            float a, b, c;

            a = line_vec.Dot(line_vec);
            b = 2 * vec_to_line.Dot(line_vec);
            c = vec_to_line.Dot(vec_to_line) - circleRadius * circleRadius;

            // Solve for t.
            float sqrtterm = b * b - 4 * a * c;

            // If the term we intend to square root is less than 0 then the answer won't be real,
            // so it definitely won't be t in the range 0 to 1.
            if (sqrtterm < 0)
            {
                return -1;
            }

            // If we can assume that the line segment starts outside the circle (e.g. for continuous time collision detection)
            // then the following can be skipped and we can just return the equivalent of res1.
            sqrtterm = (float)Math.Sqrt(sqrtterm);
            float res1 = (-b - sqrtterm) / (2 * a);
            float res2 = (-b + sqrtterm) / (2 * a);

            if (res1 >= 0 && res1 <= 1)
            {
                return res1;
            }
            if (res2 >= 0 && res2 <= 1)
            {
                return res2;
            }
            return -1;
        }

        /// <summary>
        /// Returns true if this point is in a circle
        /// </summary>
        /// <param name="point"></param>
        /// <param name="circlePosition"></param>
        /// <param name="circleRadius"></param>
        /// <returns></returns>
        public static bool IsPointInCircle(Vector2 point, Vector2 circlePosition, float circleRadius)
        {
            return point.DistanceSquaredTo(circlePosition) <= circleRadius * circleRadius;
        }

        /// <summary>
        /// Return true if the location is not already in (or close to) another object
        /// </summary>
        /// <param name="loc">The location to check</param>
        /// <param name="otherObjectLocations">The locations of other objects we don't want to be near</param>
        /// <param name="minDistance">The minimum distance required between and any other objects in otherObjectLocations</param>
        /// <returns>True if this location (or near it) is not already in use</returns>
        public static bool IsLocationValid(Vector2 loc, HashSet<Vector2> otherObjectLocations, int minDistance)
        {
            var minDistanceSquared = minDistance * minDistance;
            return !otherObjectLocations.Any(planetLoc => loc.DistanceSquaredTo(planetLoc) < minDistanceSquared);

        }
    }
}

