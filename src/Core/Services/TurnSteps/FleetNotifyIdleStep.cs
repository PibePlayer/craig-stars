
namespace CraigStars
{
    /// <summary>
    /// Age fleets by one year
    /// </summary>
    public class FleetNotifyIdleStep : TurnGenerationStep
    {
        public FleetNotifyIdleStep(IProvider<Game> gameProvider) : base(gameProvider, TurnGenerationState.FleetNotifyIdleStep) { }

        public override void Process()
        {
            Game.Fleets.ForEach(fleet => NotifyIdleFleet(fleet));
        }

        /// <summary>
        /// Notify the Player if this fleet has completed it's assigned task
        /// </summary>
        /// <param name="fleet"></param>
        void NotifyIdleFleet(Fleet fleet)
        {
            if (fleet.Age != 0 && fleet.Waypoints.Count == 1)
            {
                // some orders are continuous, otherwise this fleet is done
                var wp = fleet.Waypoints[0];
                if (wp.Task != WaypointTask.LayMineField &&
                    wp.Task != WaypointTask.RemoteMining &&
                    wp.Task != WaypointTask.Patrol)
                {
                    if (fleet.IdleTurns == 0)
                    {
                        Message.FleetCompletedAssignedOrders(Game.Players[fleet.PlayerNum], fleet);
                    }
                    fleet.IdleTurns++;
                }
            }
        }
    }
}