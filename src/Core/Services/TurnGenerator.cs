using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CraigStars.Singletons;
using log4net;

namespace CraigStars
{
    public class TurnGenerator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TurnGenerator));

        public event Action<TurnGeneratorState> TurnGeneratorAdvancedEvent;
        public void PublishTurnGeneratorAdvancedEvent(TurnGeneratorState state) => TurnGeneratorAdvancedEvent?.Invoke(state);

        Game Game { get; }

        // the steps executed by the TurnGenerator
        List<Step> steps;

        /// <summary>
        /// Stars! Order of Events
        /// <c>
        ///     Scrapping fleets (w/possible tech gain) 
        ///     Waypoint 0 unload tasks 
        ///     Waypoint 0 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 0 load tasks 
        ///     Other Waypoint 0 tasks * 
        ///     MT moves 
        ///     In-space packets move and decay 
        ///     PP packets (de)terraform 
        ///     Packets cause damage 
        ///     Wormhole entry points jiggle 
        ///     Fleets move (run out of fuel, hit minefields (fields reduce as they are hit), stargate, wormhole travel) 
        ///     Inner Strength colonists grow in fleets 
        ///     Mass Packets still in space and Salvage decay 
        ///     Wormhole exit points jiggle 
        ///     Wormhole endpoints degrade/jump 
        ///     SD Minefields detonate (possibly damaging again fleet that hit minefield during movement) 
        ///     Mining 
        ///     Production (incl. research, packet launch, fleet/starbase construction) 
        ///     SS Spy bonus obtained 
        ///     Population grows/dies 
        ///     Packets that just launched and reach their destination cause damage 
        ///     Random events (comet strikes, etc.) 
        ///     Fleet battles (w/possible tech gain) 
        ///     Meet MT 
        ///     Bombing 
        ///     Waypoint 1 unload tasks 
        ///     Waypoint 1 Colonization/Ground Combat resolution (w/possible tech gain) 
        ///     Waypoint 1 load tasks 
        ///     Mine Laying 
        ///     Fleet Transfer 
        ///     CA Instaforming 
        ///     Mine sweeping 
        ///     Starbase and fleet repair 
        ///     Remote Terraforming
        ///     Scan and Update Player Intel
        /// </c>
        /// </summary>
        /// <param name="game"></param>
        public TurnGenerator(Game game)
        {
            FleetWaypointStep waypointStep = new FleetWaypointStep(game, TurnGeneratorState.Waypoint);
            Game = game;
            steps = new List<Step>() {
                waypointStep, // wp0
                new FleetMoveStep(game, TurnGeneratorState.MoveFleets),
                new PlanetMineStep(game, TurnGeneratorState.Mining),
                new PlanetProduceStep(game, TurnGeneratorState.Production),
                new PlayerResearchStep(game, TurnGeneratorState.Research),
                new PlanetGrowStep(game, TurnGeneratorState.Grow),
                new PlanetBombStep(game, TurnGeneratorState.Bomb),
                waypointStep, // wp1
                new PlayerScanStep(game, TurnGeneratorState.Scan),
            };
        }


        /// <summary>
        /// Generate a turn
        /// </summary>
        public void GenerateTurn()
        {
            PublishTurnGeneratorAdvancedEvent(TurnGeneratorState.Scrapping);
            Game.Year++;

            // reset the players for a new turn
            log.Debug("Resetting players");
            Game.Players.ForEach(p =>
            {
                p.SubmittedTurn = false;
                p.Messages.Clear();
                p.ComputeAggregates();
            });

            // execute each turn step
            var ownedPlanets = Game.OwnedPlanets.ToList();
            foreach (var step in steps)
            {
                PublishTurnGeneratorAdvancedEvent(step.State);
                step.Execute(ownedPlanets);
            }

        }

        #region Event Publishers


        #endregion
    }
}