namespace CraigStars
{
    /// <summary>
    /// The lifecycle of this game
    /// </summary>
    public enum GameState
    {
        // The game is currently being setup
        Setup,
        // The game is in progress, waiting for players to submit
        WaitingForPlayers,
        // The game is generating a turn
        GeneratingTurn,
    }
}