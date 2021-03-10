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
        static ILog log = LogManager.GetLogger(typeof(PublicGameInfo));

        public String Name { get; set; } = "A Barefoot Jaywalk";
        public int Year { get; set; } = 2400;
        public GameMode Mode { get; set; } = GameMode.SinglePlayer;
        public GameLifecycle Lifecycle { get; set; } = GameLifecycle.Setup;
        public Rules Rules { get; set; } = new Rules();

        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))]
        public List<PublicPlayerInfo> Players { get; set; } = new List<PublicPlayerInfo>();

    }
}
