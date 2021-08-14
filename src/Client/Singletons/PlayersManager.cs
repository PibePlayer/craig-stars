using System.Collections.Generic;
using System.Linq;
using CraigStars.Utils;
using Godot;

namespace CraigStars.Singletons
{
    public static class PlayersManager
    {
        static CSLog log = LogProvider.GetLogger(typeof(PlayersManager));

        /// <summary>
        /// This is just a temporary property for development.
        /// </summary>
        public const int DefaultNumPlayers = 2;

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

        /// <summary>
        /// The currently active player for the client
        /// </summary>
        public static Player Me { get; set; }
        
        /// <summary>
        /// The currently active game for the client
        /// </summary>
        public static PublicGameInfo GameInfo{ get; set; }

        public static void Reset()
        {
            RulesManager.Rules.Random.Shuffle(raceNames);
            RulesManager.Rules.Random.Shuffle(playerNames);
        }

        /// <summary>
        /// Create a new list of players for use in a new game, new lobby, etc
        /// </summary>
        /// <param name="numPlayers"></param>
        /// <returns></returns>
        public static List<Player> CreatePlayersForNewGame(int numPlayers = DefaultNumPlayers)
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
