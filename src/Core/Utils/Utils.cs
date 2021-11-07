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

        /// <summary>
        /// Returns the 2D point on the 2D segment (s1, s2) that is closest to point. The returned point will always be inside the specified segment.
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <param name="s1">The start of the segment</param>
        /// <param name="s2">The end of the segment</param>
        /// <returns></returns>
        public static Vector2 GetClosestPointToSegment2D(Vector2 point, Vector2 s1, Vector2 s2)
        {
            Vector2 p = point - s1;
            Vector2 n = s2 - s1;
            float l2 = n.LengthSquared();
            if (l2 < 1e-20)
            {
                return s1; // Both points are the same, just give any.
            }

            float d = n.Dot(p) / l2;

            if (d <= 0.0)
            {
                return s1; // Before first point.
            }
            else if (d >= 1.0)
            {
                return s2; // After first point.
            }
            else
            {
                return s1 + n * d; // Inside.
            }
        }
    }
}

