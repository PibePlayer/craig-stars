namespace CraigStars
{
    /// <summary>
    /// The field for A tech
    /// </summary>
    public enum TurnGeneratorState
    {
        WaitingForPlayers,
        Scrapping,
        Waypoint0,
        MovePackets,
        MoveFleets,
        Mining,
        Production,
        Grow,
        Battle,
        Bomb,
        Waypoint1,
        MineLaying,
        Transfer,
        MineSweeping,
        Repair,
        UpdatingPlayers,
        Saving,
        Finished,
    }
}
