using System;
using System.Collections.Generic;
using System.Linq;

namespace CraigStars
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractFleetWaypointProcessStep : TurnGenerationStep
    {
        static CSLog log = LogProvider.GetLogger(typeof(AbstractFleetWaypointProcessStep));

        public const string ProcessedWaypointsContextKey = "ProcessedWaypoints";

        private readonly IRulesProvider rulesProvider;
        protected readonly WaypointTask task;

        protected HashSet<Waypoint> processedWaypoints = new HashSet<Waypoint>();
        protected List<MapObject> deletedMapObjects = new();
       
        protected Rules Rules => rulesProvider.Rules;

        public AbstractFleetWaypointProcessStep(IProvider<Game> gameProvider, IRulesProvider rulesProvider, TurnGenerationState state, WaypointTask task) : base(gameProvider, state)
        {
            this.rulesProvider = rulesProvider;
            this.task = task;
        }

        /// <summary>
        /// Make sure we don't process a waypoint task twice.
        /// </summary>
        /// <param name="ownedPlanets"></param>
        public override void PreProcess(List<Planet> ownedPlanets)
        {
            base.PreProcess(ownedPlanets);

            if (Context.Context.TryGetValue(FleetUnloadStep.ProcessedWaypointsContextKey, out var waypoints)
            && waypoints is HashSet<Waypoint> processedWaypointsFromContext)
            {
                // add any processed waypoints from the context
                processedWaypoints.UnionWith(processedWaypointsFromContext);
            }
        }

        /// <summary>
        /// Build FleetWaypointTasks for this WaypointTask type and process it.
        /// </summary>
        public override void Process()
        {
            // Separate our waypoint tasks into groups
            foreach (var task in Game.Fleets
                .Select(fleet => BuildWaypointTasks(fleet, task))
                .Where(t => t != null)
            )
            {
                if (ProcessTask(task))
                {
                    CompleteWaypointForTurn(task.Waypoint);
                }
            }
        }

        /// <summary>
        /// Override this to process a task
        /// </summary>
        /// <param name="task"></param>
        /// <returns>return true if this waypoint was processed and does not need to be processed again.</returns>
        protected abstract bool ProcessTask(FleetWaypointProcessTask task);

        /// <summary>
        /// Override PostProcess() to set processed waypoints to the TurnGeneratorContext
        /// </summary>
        public override void PostProcess()
        {
            base.PostProcess();
            Context.Context[ProcessedWaypointsContextKey] = processedWaypoints;

            // delete any mapobjects that are scheduled for deletion
            deletedMapObjects.ForEach(mo => EventManager.PublishMapObjectDeletedEvent(mo));
        }

        /// <summary>
        /// At the end of processing, tell the game to delete these MapObjects
        /// </summary>
        /// <param name="fleet"></param>
        protected void QueueMapObjectForDeletion(MapObject mapObject)
        {
            deletedMapObjects.Add(mapObject);
        }

        /// <summary>
        /// Add this waypoint to the set of processed waypoints so we don't process it again
        /// </summary>
        /// <param name="wp"></param>
        protected void CompleteWaypointForTurn(Waypoint wp)
        {
            processedWaypoints.Add(wp);
        }

        /// <summary>
        /// Helper method to scrap a fleet whether because of colonization or a scrap fleet order
        /// </summary>
        /// <param name="fleet"></param>
        /// <param name="wp"></param>
        /// <param name="player"></param>
        protected void ScrapFleet(Fleet fleet, Waypoint wp, Player player)
        {
            // create a new cargo instance out of our fleet cost
            Cargo cargo = fleet.Spec.Cost;

            // TODO: handle starbases and better recycling
            // this is 1/3rd for normal races, but UR is more efficient and scraps 45%
            cargo *= Rules.ScrapMineralAmount + player.Race.Spec.ScrapMineralOffset;

            // add in any cargo the fleet was holding
            cargo += fleet.Cargo;
            QueueMapObjectForDeletion(fleet);

            if (wp.Target is Planet planet)
            {
                planet.Cargo += cargo;
                fleet.Scrapped = true;
            }
            else
            {
                var salvage = new Salvage()
                {
                    PlayerNum = fleet.PlayerNum,
                    Name = "Salvage",
                    Position = wp.Position,
                    Cargo = cargo
                };
                EventManager.PublishMapObjectCreatedEvent(salvage);
            }
        }

        /// <summary>
        /// For each waypoint, split it into separate tasks because we process all scrap, 
        /// then unload, then colonize, then load, etc tasks in groups
        /// </summary>
        /// <param name="wp"></param>
        protected FleetWaypointProcessTask BuildWaypointTasks(Fleet fleet, WaypointTask waypointTask)
        {
            if (fleet.Waypoints.Count > 0)
            {
                var wp = fleet.Waypoints[0];
                if (processedWaypoints.Contains(wp))
                {
                    // we already processed this during wp0, don't process it again
                    return null;
                }

                if (waypointTask != wp.Task)
                {
                    return null;
                }

                log.Debug($"{Game.Year}: {fleet.PlayerNum} Building waypoint task for {fleet.Name} at {wp.TargetName} -> {wp.Task}");

                var player = Game.Players[fleet.PlayerNum];

                var waypointProcessTask = new FleetWaypointProcessTask(fleet, wp, player);

                // add these transport task specific "Tasks"
                if (waypointTask == WaypointTask.Transport)
                {
                    foreach (CargoType type in Enum.GetValues(typeof(CargoType)))
                    {
                        waypointProcessTask.AddTask(wp.TransportTasks[type], type);
                    }
                }

                return waypointProcessTask;
            }
            else
            {
                log.Error($"{Game.Year}: {fleet.PlayerNum} {fleet.Name} has 0 waypoints.");
                return null;
            }
        }
    }
}