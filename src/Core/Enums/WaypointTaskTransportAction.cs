namespace CraigStars
{
    /// <summary>
    /// The transport action for Transport waypoint tasks  
    /// </summary>
    public enum WaypointTaskTransportAction
    {
        None,
        LoadAll,
        UnloadAll,
        LoadAmount,
        UnloadAmount,
        FillPercent,
        WaitForPercent,
        LoadDunnage,
        SetAmountTo,
        SetWaypointTo,
    }
}