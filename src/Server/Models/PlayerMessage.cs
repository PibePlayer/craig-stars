namespace CraigStars
{
    public class PlayerMessage
    {
        public int PlayerNum { get; set; } = 1;
        public string Message { get; set; } = "";

        public PlayerMessage(int playerNum = 1, string message = "")
        {
            PlayerNum = playerNum;
            Message = message;
        }

        public override string ToString()
        {
            return $"{PlayerNum} - {Message}";
        }
    }
}
