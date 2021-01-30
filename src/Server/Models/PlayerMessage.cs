
using Newtonsoft.Json;

namespace CraigStars
{
    public readonly struct PlayerMessage
    {
        public readonly int playerNum;
        public readonly string message;

        [JsonConstructor]
        public PlayerMessage(int playerNum = 0, string message = "")
        {
            this.playerNum = playerNum;
            this.message = message;
        }

        public override string ToString()
        {
            return $"{playerNum + 1} - {message}";
        }
    }
}
