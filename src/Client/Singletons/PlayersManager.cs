using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;
using log4net;

namespace CraigStars.Singletons
{
    public class PlayersManager : Node
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayersManager));

        /// <summary>
        /// This is just a temporary property for development.
        /// </summary>
        public const int DefaultNumPlayers = 2;

        /// <summary>
        /// The currently active player
        /// </summary>
        /// <value></value>
        public int ActivePlayer
        {
            get => activePlayer;
            set
            {
                activePlayer = value;
                Me = null;
            }
        }
        int activePlayer = 0;

        public static Color[] PlayerColors { get; } = new Color[] {
            new Color("c33232"),
            new Color("1f8ba7"),
            new Color("43a43e"),
            new Color("8d29cb"),
            new Color("b88628")
        };

        static string[] playerNames = new string[] {
            "Craig",
            "Ted",
            "Joe",
            "Bob",
            "Lance",
            "Elias",
            "Eva",
            "Maeve",
        };

        static string[] raceNames = new string[] {
            "Berserker",
            "Hooveron",
            "American",
            "Ubert",
            "Kurkonian",
            "Mensoid",
            "Ubert",
            "Crusher",
            "House Cat",
            "Bulushi",
            "Ferret",
            "Nee",
            "Golem",
            "Loraxoid",
            "Hicardi",
            "Nairnian",
            "Hawk",
            "Rush'n",
            "Nee",
            "Tritizoid",
        };

        public static Player Me { get; set; }

        /// <summary>
        /// PlayersManager is a singleton
        /// </summary>
        private static PlayersManager instance;
        public static PlayersManager Instance
        {
            get
            {
                return instance;
            }
        }

        PlayersManager()
        {
            instance = this;
        }

        public void Reset()
        {
            Me = null;
            RulesManager.Rules.Random.Shuffle(raceNames);
            RulesManager.Rules.Random.Shuffle(playerNames);
        }

        /// <summary>
        /// Reset the current player to whatever the default is. I.e. if this is a hotseat game, the current player
        /// will be player 1.
        /// /// </summary>
        public static void ResetCurrentPlayer()
        {
            Instance.ActivePlayer = 0;
            Me = null;
        }

        /// <summary>
        /// Setup the initial players list with players based on color
        /// </summary>
        public List<Player> CreatePlayersForNewGame(int numPlayers = DefaultNumPlayers)
        {
            var players = new List<Player>();
            players.AddRange(CreateNewPlayersList(numPlayers));
            players.ForEach(player => Signals.PublishPlayerUpdatedEvent(player));
            return players;
        }

        /// <summary>
        /// Create a new list of players for use in a new game, new lobby, etc
        /// </summary>
        /// <param name="numPlayers"></param>
        /// <returns></returns>
        public static List<Player> CreateNewPlayersList(int numPlayers)
        {
            List<Player> players = new();
            for (var num = 0; num < numPlayers; num++)
            {
                var player = CreateNewPlayer(num);
                players.Add(player);
            }

            return players;
        }

        /// <summary>
        /// Add a new player to PlayersManager
        /// </summary>
        /// <returns></returns>
        public static Player CreateNewPlayer(int num)
        {
            var player = new Player
            {
                Num = num,
                Name = num < playerNames.Length ? playerNames[num] : $"Player {num + 1}",
                Color = num < PlayerColors.Length ? PlayerColors[num] : new Color((float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble(), (float)RulesManager.Rules.Random.NextDouble()),
                AIControlled = num != 0,
                Ready = true,
                TechStore = TechStore.Instance,
                DefaultHullSet = num % 2
            };

            if (player.AIControlled)
            {
                player.Race.Name = num < raceNames.Length ? raceNames[num] : $"Race {num + 1}";
                player.Race.PluralName = player.Race.Name + "s";
            }

            return player;
        }

    }
}
