using System.Collections.Generic;

namespace CraigStars
{
    public class TechEngine : TechHullComponent
    {
        private const int MaxWarp = 10;
        public int[] FuelUsage = new int[MaxWarp];
    }
}
