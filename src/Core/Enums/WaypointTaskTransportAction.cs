namespace CraigStars
{
    /// <summary>
    /// The transport action for Transport waypoint tasks  
    /// </summary>
    public enum WaypointTaskTransportAction
    {
        // No transport task for the specified cargo.
        None,

        // (fuel only) Load or unload fuel until the fleet carries only the exact amount 
        // needed to reach the next waypoint. You can use this task to send a fleet 
        // loaded with fuel to rescue a stranded fleet. The rescue fleet will transfer 
        // only the amount of fuel it can spare without stranding itself.
        LoadOptimal,

        // Load as much of the specified cargo as the fleet can hold.
        LoadAll,

        // Unload all the specified cargo at the waypoint.
        UnloadAll,

        // Load the amount specified only if there is room in the hold.
        LoadAmount,

        // Unload the amount specified only if the fleet is carrying that amount.
        UnloadAmount,

        // Loads up to the specified portion of the cargo hold subject to amount available at waypoint and room left in hold.
        FillPercent,

        // Remain at the waypoint until exactly X % of the hold is filled.
        WaitForPercent,

        // (minerals and colonists only) This command waits until all other loads and unloads are complete, 
        // then loads as many colonists or amount of a mineral as will fit in the remaining space. For example, 
        // setting Load All Germanium, Load Dunnage Ironium, will load all the Germanium that is available, 
        // then as much Ironium as possible. If more than one dunnage cargo is specified, they are loaded in 
        // the order of Ironium, Boranium, Germanium, and Colonists.
        LoadDunnage,
        
        // Load or unload the cargo until the amount on board is the amount specified. 
        // If less than the specified cargo is available, the fleet will not move on.
        SetAmountTo,

        // Load or unload the cargo until the amount at the waypoint is the amount specified. 
        // This order is always carried out to the best of the fleet’s ability that turn but does not prevent the fleet from moving on.
        SetWaypointTo,
    }
}