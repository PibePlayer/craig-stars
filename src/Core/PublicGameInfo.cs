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
    /// By default, public game info has PublicPlayerInfo objects, but when setting
    /// up a new game, we use the full Player
    /// </summary>
    public class PublicGameInfo : GameSettings<PublicPlayerInfo>
    {

    }

    /// <summary>
    /// This represents publicly available game information
    /// </summary>
    public class GameSettings<T> where T : PublicPlayerInfo
    {
        static CSLog log = LogProvider.GetLogger(typeof(PublicGameInfo));

        public string Name { get; set; } = "A Barefoot Jaywalk";
        public int QuickStartTurns { get; set; } = 0;
        public Size Size { get; set; } = Size.Small;
        public Density Density { get; set; } = Density.Normal;
        public PlayerPositions PlayerPositions { get; set; } = PlayerPositions.Moderate;
        public bool RandomEvents { get; set; } = true;
        public bool ComputerPlayersFormAlliances { get; set; }
        public bool PublicPlayerScores { get; set; } = true;
        public GameStartMode StartMode { get; set; } = GameStartMode.Normal;

        public int Year { get; set; } = 2400;
        public GameMode Mode { get; set; } = GameMode.SinglePlayer;
        public GameLifecycle Lifecycle { get; set; } = GameLifecycle.Setup;
        public Rules Rules { get; set; } = new Rules(0);
        public VictoryConditions VictoryConditions { get; set; } = new VictoryConditions();
        public bool VictorDeclared { get; set; }

        #region Computed Properties

        [JsonIgnore] public bool ScoresVisible { get => PublicPlayerScores && YearsPassed >= Rules.ShowPublicScoresAfterYears || VictorDeclared; }
        [JsonIgnore] public int YearsPassed { get => Year - Rules.StartingYear; }

        #endregion

        [JsonProperty(ItemConverterType = typeof(PublicPlayerInfoConverter))]
        public List<T> Players { get; set; } = new List<T>();

        /// <summary>
        /// Convert a generic GameSettings (with full Player object, probably) to a PublicGameInfo with PublicPlayerInfo list
        /// </summary>
        /// <param name="settings"></param>
        public static implicit operator PublicGameInfo(GameSettings<T> settings)
        {
            return new PublicGameInfo()
            {
                Name = settings.Name,
                QuickStartTurns = settings.QuickStartTurns,
                Size = settings.Size,
                Density = settings.Density,
                PlayerPositions = settings.PlayerPositions,
                RandomEvents = settings.RandomEvents,
                ComputerPlayersFormAlliances = settings.ComputerPlayersFormAlliances,
                PublicPlayerScores = settings.PublicPlayerScores,
                StartMode = settings.StartMode,
                Year = settings.Year,
                Mode = settings.Mode,
                Lifecycle = settings.Lifecycle,
                Rules = settings.Rules,
                VictoryConditions = settings.VictoryConditions,
                VictorDeclared = settings.VictorDeclared,
                Players = settings.Players.Cast<PublicPlayerInfo>().ToList()
            };
        }

    }
}
