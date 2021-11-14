
using System;
using System.Linq;
using System.Collections.Generic;
using CraigStars.Utils;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// Represents a minefield in the universe
    /// </summary>
    [JsonObject(IsReference = true)]
    public class MineField : MapObject
    {
        public int ReportAge = Unexplored;
        public MineFieldType Type { get; set; } = MineFieldType.Standard;

        public bool Detonate { get; set; }
        public long NumMines
        {
            get => numMines;
            set
            {
                numMines = value;
                radius = (float)Math.Sqrt(numMines);
            }
        }
        long numMines;

        /// <summary>
        /// The radius of this minefield, calculated as the square root of the number of mines
        /// </summary>
        public float Radius { get => radius; }
        float radius;


    }
}
