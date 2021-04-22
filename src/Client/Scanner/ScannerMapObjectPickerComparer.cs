
using System;
using System.Collections.Generic;

namespace CraigStars
{

    /// <summary>
    /// Compares map objects by type, sorting so we can pick planets, then fleets, then
    /// everything else
    /// </summary>
    public class ScannerMapObjectPickerComparer : IComparer<MapObjectSprite>
    {
        public int Compare(MapObjectSprite x, MapObjectSprite y)
        {
            if (x.GetType() == y.GetType())
            {
                return 0;
            }
            // prioritize planets
            if (x is PlanetSprite)
            {
                return -1;
            }
            if (y is PlanetSprite)
            {
                return 1;
            }

            // next prioritize fleets
            if (x is FleetSprite)
            {
                return -1;
            }
            if (y is FleetSprite)
            {
                return 1;
            }

            // everything else doesn't matter
            return 0;
        }
    }
}