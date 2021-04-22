using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CraigStars.Utils
{
    public static class UniverseUtils
    {

        /// <summary>
        /// Get all the planets that are within some circle in the universe
        /// </summary>
        /// <param name="planets"></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static IEnumerable<Planet> GetPlanetsWithin(IEnumerable<Planet> planets, Vector2 position, float radius)
        {
            return planets.Where(planet => Utils.IsPointInCircle(planet.Position, position, radius));
        }

    }
}

