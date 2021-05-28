using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CraigStars.Singletons;
using log4net;
using Newtonsoft.Json;

namespace CraigStars
{
    /// <summary>
    /// This represents publicly available game information
    /// </summary>
    public class PublicGameInfo
    {
        static CSLog log = LogProvider.GetLogger(typeof(PublicGameInfo));

        public string Name { get; set; } = "A Barefoot Jaywalk";
        public int QuickStartTurns { get; set; } = 0;
        public Size Size { get; set; } = Size.Small;
        public Density Density { get; set; } = Density.Normal;
        public GameStartMode StartMode { get; set; } = GameStartMode.Normal;


        public int Year { get; set; } = 2400;
        public GameMode Mode { get; set; } = GameMode.SinglePlayer;
        public GameLifecycle Lifecycle { get; set; } = GameLifecycle.Setup;
        public Rules Rules { get; set; } = new Rules(0);
        public VictoryConditions VictoryConditions { get; set; } = new VictoryConditions();

        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))]
        public List<PublicPlayerInfo> Players { get; set; } = new List<PublicPlayerInfo>();

    }
}
