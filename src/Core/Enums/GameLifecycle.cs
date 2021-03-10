namespace CraigStars
{
    /// <summary>
    /// The lifecycle of this game
    /// </summary>
    public enum GameLifecycle
    {
        // The game is currently being setup
        Setup,
        // The game is in progress, waiting for players to submit
        WaitingForPlayers,
        // The game is generating a turn
        GeneratingTurn,
    }
}