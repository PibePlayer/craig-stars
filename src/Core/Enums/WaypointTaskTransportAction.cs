namespace CraigStars
{
    /// <summary>
    /// The transport action for Transport waypoint tasks  
    /// </summary>
    public enum WaypointTaskTransportAction
    {
        None,
        LoadOptimal,
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