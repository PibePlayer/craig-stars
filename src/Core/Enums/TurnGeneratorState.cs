namespace CraigStars
{
    /// <summary>
    /// The field for A tech
    /// </summary>
    public enum TurnGeneratorState
    {
        WaitingForPlayers,
        Scrapping,
        Waypoint,
        MovePackets,
        MoveFleets,
        Mining,
        Production,
        Research,
        Grow,
        Battle,
        Bomb,
        Waypoint1,
        MineLaying,
        Transfer,
        MineSweeping,
        Repair,
        Scan,
        UpdatingPlayers,
        RunningTurnProcessors,
        Saving,
        Finished,
    }
}
