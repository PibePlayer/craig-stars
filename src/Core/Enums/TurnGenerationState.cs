namespace CraigStars
{
    /// <summary>
    /// The field for A tech
    /// </summary>
    public enum TurnGenerationState
    {
        WaitingForPlayers,
        FleetAge,
        Scrapping,
        Waypoint,
        MovePackets,
        MoveFleets,
        DecaySalvage,
        DecayMineralPackets,
        WormholeJiggle,
        DetonateMines,
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
        CalculatingScore,
        UpdatingPlayers,
        VictoryCheck,
        RunningTurnProcessors,
        Saving,
        Finished,
    }
}
