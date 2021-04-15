
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
        public MineFieldType Type { get; set; }
        public int NumMines { get; set; }
        public int Radius { get; set; }
    }
}
